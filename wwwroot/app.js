let sessionId = null;
const loginPanel = document.getElementById('loginPanel');
const inputPanel = document.getElementById('inputPanel');
const messagesEl = document.getElementById('messages');
const chatBody = document.getElementById('chat');

function createMessageEl(text, cls, priority){
  const wrapper = document.createElement('div');
  wrapper.className = 'message ' + cls;
  const p = document.createElement('div');
  p.textContent = text;
  p.style.whiteSpace = 'pre-wrap';
  wrapper.appendChild(p);

  if(priority){
    const badge = document.createElement('div');
    badge.className = 'priority-badge ' + (priority === 'alta' ? 'priority-high' : (priority === 'media' ? 'priority-medium' : 'priority-low'));
    badge.textContent = 'Prioridade: ' + (priority === 'Não classificada' ? 'Não classificada' : priority);
    wrapper.appendChild(badge);
  }

  return wrapper;
}

function appendUserMessage(text){
  const el = createMessageEl(text, 'msg-user');
  messagesEl.appendChild(el);
  chatBody.scrollTop = chatBody.scrollHeight;
}
function appendBotMessage(text, priority){
  const el = createMessageEl(text, 'msg-bot', priority);
  messagesEl.appendChild(el);
  chatBody.scrollTop = chatBody.scrollHeight;
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

function parsePriorityFromResponse(obj){
  // tenta extrair prioridade de várias formas
  if(!obj) return null;
  if(obj.prioridade) return obj.prioridade;
  // se EnvioMsg.EnviarBot retornou texto com "Prioridade: ...", extraia
  const text = (obj.conteudo || obj.Conteudo || obj || '').toString();
  const m = text.match(/Prioridade\s*[:\-]?\s*(\w+)/i);
  if(m) return m[1].toLowerCase();
  return null;
}


document.getElementById('loginBtn').addEventListener('click', async () => {
  const email = document.getElementById('email').value;
  const password = document.getElementById('password').value;
  const resp = await fetch('/api/login', { method: 'POST', headers: { 'Content-Type':'application/json' }, body: JSON.stringify({ email, password }) });
  if (!resp.ok) { alert('Falha no login'); return; }
  const data = await resp.json();
  sessionId = data.sessionId;
  messagesEl.innerHTML = '';
  const welcome = data.welcome || data.welcome || {};
  appendBotMessage(welcome.conteudo || welcome.Conteudo || 'Bem vindo');
  loginPanel.style.display = 'none';
  inputPanel.style.display = 'flex';
});


document.getElementById('sendBtn').addEventListener('click', async ()=>{
  const input = document.getElementById('messageInput');
  const text = input.value.trim(); if(!text) return; input.value=''; appendUserMessage(text);

  const typing = showTyping();
  const resp = await fetch('/api/message',{ method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ sessionId, content: text }) });
  removeEl(typing);
  if(!resp.ok){ appendBotMessage('Erro na comunicação com o servidor'); return; }
  const data = await resp.json();
  const priority = parsePriorityFromResponse(data);
  const content = data.conteudo || data.Conteudo || (typeof data === 'string' ? data : JSON.stringify(data));
  appendBotMessage(content, priority);
});

// Enter key
document.getElementById('messageInput').addEventListener('keydown', (e)=>{ if(e.key==='Enter') document.getElementById('sendBtn').click(); });
