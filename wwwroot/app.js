let sessionId = null;
let isAdmin = false;
let userPollingId = null;
let adminQueueInterval = null;
let adminConvInterval = null;
const loginPanel = document.getElementById('loginPanel');
const inputPanel = document.getElementById('inputPanel');
const messagesEl = document.getElementById('messages');
const chatBody = document.getElementById('chat');
const userArea = document.getElementById('userArea');
const adminArea = document.getElementById('adminArea');
const queueList = document.getElementById('queueList');
const adminMessages = document.getElementById('adminMessages');
const adminConversation = document.getElementById('adminConversation');
const adminBack = document.getElementById('adminBack');
const closeBtn = document.getElementById('close');

function createMessageEl(text, cls){
  const wrapper = document.createElement('div');
  wrapper.className = 'message ' + cls;
  const p = document.createElement('div');
  p.textContent = text;
  p.style.whiteSpace = 'pre-wrap';
  wrapper.appendChild(p);
  return wrapper;
}

function appendUserMessage(text){
  const el = createMessageEl(text, 'msg-user');
  messagesEl.appendChild(el);
  chatBody.scrollTop = chatBody.scrollHeight;
}
function appendBotMessage(text){
  const el = createMessageEl(text, 'msg-bot');
  messagesEl.appendChild(el);
  chatBody.scrollTop = chatBody.scrollHeight;
}

function appendAdminMessage(text, fromUser=false){
  const el = createMessageEl(text, fromUser ? 'msg-user' : 'msg-bot');
  adminMessages.appendChild(el);
  adminMessages.scrollTop = adminMessages.scrollHeight;
}

function showTyping(){
  const t = document.createElement('div'); t.className='message msg-bot typing';
  const dots = document.createElement('div'); dots.className='typing';
  for(let i=0;i<3;i++){ const d=document.createElement('div'); d.className='typing-dot'; dots.appendChild(d);} 
  t.appendChild(dots);
  messagesEl.appendChild(t);
  chatBody.scrollTop = chatBody.scrollHeight;
  return t;
}

function removeEl(el){ if(el && el.parentNode) el.parentNode.removeChild(el); }

async function fetchMessagesOnce(){
  if(!sessionId || isAdmin) return null;
  const resp = await fetch(`/api/messages?sessionId=${sessionId}`);
  if(!resp.ok) return null;
  const data = await resp.json();
  return data;
}

function normalizeMessage(m){
  if(!m) return { from: '', text: '' };
  const from = m.Remetente || m.remetente || m.from || '';
  const text = m.Conteudo || m.conteudo || m.content || m.Text || '';
  return { from, text };
}

async function pollMessages(){
  const data = await fetchMessagesOnce();
  if(!data) return;
  // substituir conteúdo mantendo scroll se já no final
  const atBottom = chatBody.scrollHeight - chatBody.clientHeight <= chatBody.scrollTop + 20;
  messagesEl.innerHTML = '';
  (data.messages || []).forEach(m => {
    const nm = normalizeMessage(m);
    const cls = nm.from === 'Usuário' ? 'msg-user' : 'msg-bot';
    const el = createMessageEl(nm.text, cls);
    messagesEl.appendChild(el);
  });
  if(atBottom) chatBody.scrollTop = chatBody.scrollHeight;
  // if server says no longer waiting for human and no assigned agent, stop polling
  if(!data.waitingHuman && !data.assignedAgent){ stopUserPolling(); }
}

function startUserPolling(){
  if(userPollingId) return;
  // poll immediately then interval
  pollMessages();
  userPollingId = setInterval(pollMessages, 2000);
}

function stopUserPolling(){
  if(!userPollingId) return;
  clearInterval(userPollingId);
  userPollingId = null;
}

async function pollAdminQueue(adminSessionId){
  const resp = await fetch(`/api/admin/queue?sessionId=${adminSessionId}`);
  if(!resp.ok) return;
  const data = await resp.json();
  queueList.innerHTML = '';
  data.forEach(item => {
    const li = document.createElement('li');
    li.textContent = `${item.email || item.sessionId} - ${item.priority}`;
    li.dataset.sessionId = item.sessionId;
    li.style.cursor = 'pointer';
    li.addEventListener('click', ()=> openConversation(adminSessionId, item.sessionId));
    queueList.appendChild(li);
  });
}

async function openConversation(adminSessionId, targetSessionId){
  // show conversation area and hide queue
  adminConversation.style.display = 'block';
  document.querySelector('.admin-queue').style.display = 'none';

  // mark active li
  Array.from(queueList.children).forEach(li => li.classList.remove('active'));
  const activeLi = Array.from(queueList.children).find(li => li.dataset.sessionId === targetSessionId);
  if(activeLi) activeLi.classList.add('active');

  // load conversation
  const resp = await fetch(`/api/admin/conversation?adminSessionId=${adminSessionId}&targetSessionId=${targetSessionId}`);
  if(!resp.ok) return;
  const data = await resp.json();
  adminMessages.innerHTML = '';
  data.forEach(m => {
    const nm = normalizeMessage(m);
    appendAdminMessage(nm.text, nm.from === 'Usuário');
  });
  // store current target
  adminArea.dataset.currentTarget = targetSessionId;

  // start polling this conversation every 2s
  if(adminConvInterval) clearInterval(adminConvInterval);
  adminConvInterval = setInterval(()=>{
    // reload conversation
    fetch(`/api/admin/conversation?adminSessionId=${adminSessionId}&targetSessionId=${targetSessionId}`).then(r=>r.json()).then(d=>{
      adminMessages.innerHTML = '';
      d.forEach(m => { const nm = normalizeMessage(m); appendAdminMessage(nm.text, nm.from === 'Usuário'); });
    }).catch(()=>{});
  },2000);
}

async function adminSend(adminSessionId){
  const target = adminArea.dataset.currentTarget;
  if(!target) return alert('Selecione uma conversa');
  const input = document.getElementById('adminMessageInput');
  const text = input.value.trim(); if(!text) return; input.value='';
  const body = { adminSessionId, targetSessionId: target, content: text };
  const resp = await fetch('/api/admin/send', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(body) });
  if(resp.ok){ appendAdminMessage(text, false); }
}

// Login
const loginBtn = document.getElementById('loginBtn');
if(loginBtn) loginBtn.addEventListener('click', async () => {
  const email = document.getElementById('email').value;
  const password = document.getElementById('password').value;
  const resp = await fetch('/api/login', { method: 'POST', headers: { 'Content-Type':'application/json' }, body: JSON.stringify({ email, password }) });
  if (!resp.ok) { alert('Falha no login'); return; }
  const data = await resp.json();
  sessionId = data.sessionId;
  isAdmin = data.isAdmin;

  if(isAdmin){
    // show admin panel
    userArea.style.display = 'none';
    adminArea.style.display = 'block';
    // show queue only
    adminConversation.style.display = 'none';
    document.querySelector('.admin-queue').style.display = 'block';

    // start polling queue
    if(adminQueueInterval) clearInterval(adminQueueInterval);
    pollAdminQueue(sessionId);
    adminQueueInterval = setInterval(()=> pollAdminQueue(sessionId), 2000);
    document.getElementById('adminSendBtn').addEventListener('click', ()=> adminSend(sessionId));
    return;
  }

  // normal user
  messagesEl.innerHTML = '';
  const welcome = data.welcome || {};
  appendBotMessage(welcome.conteudo || welcome.Conteudo || 'Bem vindo');
  loginPanel.style.display = 'none';
  inputPanel.style.display = 'flex';
  // ensure chat area visible
  userArea.querySelector('#chat').style.display = 'block';
  // do NOT start polling here; polling will start only if the session is queued (user requested human)
});

// User send
const sendBtn = document.getElementById('sendBtn');
if(sendBtn) sendBtn.addEventListener('click', async ()=>{
  const input = document.getElementById('messageInput');
  const text = input.value.trim(); if(!text) return; input.value=''; appendUserMessage(text);

  const typing = showTyping();
  const resp = await fetch('/api/message',{ method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ sessionId, content: text }) });
  removeEl(typing);
  if(!resp.ok){ appendBotMessage('Erro na comunicação com o servidor'); return; }
  const result = await resp.json();

  // server returns { message, queued } when appropriate
  if(result.message){
    const msg = result.message;
    const nm = normalizeMessage(msg);
    appendBotMessage(nm.text);
  }
  else if(result.conteudo || result.Conteudo){
    const textOut = result.conteudo || result.Conteudo;
    appendBotMessage(textOut);
  }

  if(result.queued){
    // start polling for messages from admin
    startUserPolling();
  }
});

// Admin Back button
if(adminBack) adminBack.addEventListener('click', ()=>{
  // stop conv polling
  if(adminConvInterval) clearInterval(adminConvInterval);
  adminConversation.style.display = 'none';
  document.querySelector('.admin-queue').style.display = 'block';
  adminArea.dataset.currentTarget = '';
});

// Close button resets to login for user
if(closeBtn) closeBtn.addEventListener('click', ()=>{
  // reset user UI
  userArea.querySelector('#chat').style.display = 'none';
  loginPanel.style.display = 'block';
  inputPanel.style.display = 'none';
  messagesEl.innerHTML = '<div class="welcome">Olá, Eu sou o Klebão<br><small>Por favor, realize o login no chat</small></div>';
  sessionId = null;
  stopUserPolling();
});

// Enter keys
const userInput = document.getElementById('messageInput');
if(userInput) userInput.addEventListener('keydown', (e)=>{ if(e.key==='Enter') document.getElementById('sendBtn').click(); });
const adminInput = document.getElementById('adminMessageInput');
if(adminInput) adminInput.addEventListener('keydown', (e)=>{ if(e.key==='Enter') document.getElementById('adminSendBtn').click(); });
