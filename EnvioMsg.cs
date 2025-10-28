using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TestePIM
{
    public static class EnvioMsg
    {
        public static Mensagem EnviarUsuario(string conteudo)
        {
            Mensagem msg = new()
            {
                Remetente = "Usuario",
                Conteudo = conteudo,
                // Prioridade não será mais usada
                PrioridadeMsg = null
            };


            return msg;
        }

        public static Mensagem EnviarBot(string conteudoUser)
        {
            // Chama integração com IA e devolve o texto da resposta
            var iaResposta = EnviarParaIA(conteudoUser);

            Mensagem msgBot = new()
            {
                Remetente = "Klebao",
                // Conteúdo será exatamente o que a IA retornou
                Conteudo = iaResposta,
                PrioridadeMsg = null
            };


            return msgBot;
        }

        private static string EnviarParaIA(string prompt)
        {
            // Prioridade de provedores:
            // 1) Local (LOCAL_AI_URL) — recomendado se você rodar um servidor local (text-generation-webui, Ollama, etc.)
            // 2) Hugging Face Inference API (HUGGINGFACE_API_KEY + HUGGINGFACE_MODEL)
            // 3) OpenAI (OPENAI_API_KEY) — fallback

            // 1) Local endpoint
            var localUrl = Environment.GetEnvironmentVariable("LOCAL_AI_URL");
            if (!string.IsNullOrWhiteSpace(localUrl))
            {
                try
                {
                    using var client = new HttpClient();
                    var reqObj = new
                    {
                        prompt = prompt,
                        max_tokens = 1024
                    };

                    var json = JsonSerializer.Serialize(reqObj);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resp = client.PostAsync(localUrl, content).GetAwaiter().GetResult();
                    if (!resp.IsSuccessStatusCode)
                    {
                        var err = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        return "[Erro LocalAI] " + resp.StatusCode + " - " + err;
                    }

                    var respStr = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    // tenta extrair texto de vários formatos comuns
                    try
                    {
                        using var doc = JsonDocument.Parse(respStr);
                        if (doc.RootElement.TryGetProperty("text", out var t)) return t.GetString() ?? respStr;
                        if (doc.RootElement.TryGetProperty("generated_text", out var gt)) return gt.GetString() ?? respStr;
                        if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
                        {
                            var first = data[0];
                            if (first.TryGetProperty("generated_text", out var g2)) return g2.GetString() ?? respStr;
                            if (first.TryGetProperty("text", out var g3)) return g3.GetString() ?? respStr;
                        }
                    }
                    catch { }

                    // fallback: retorna o corpo como texto
                    return respStr;
                }
                catch (Exception ex)
                {
                    return "[Erro LocalAI] " + ex.Message;
                }
            }

            // 2) Hugging Face
            var hfKey = Environment.GetEnvironmentVariable("HUGGINGFACE_API_KEY");
            if (!string.IsNullOrWhiteSpace(hfKey))
            {
                var model = Environment.GetEnvironmentVariable("HUGGINGFACE_MODEL") ?? "gpt2"; // ajuste conforme necessário
                try
                {
                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", hfKey);

                    var reqObj = new
                    {
                        inputs = prompt,
                        parameters = new { max_new_tokens = 512 }
                    };

                    var json = JsonSerializer.Serialize(reqObj);
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var url = $"https://api-inference.huggingface.co/models/{model}";
                    var resp = client.PostAsync(url, content).GetAwaiter().GetResult();
                    var respStr = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    if (!resp.IsSuccessStatusCode)
                        return "[Erro HuggingFace] " + resp.StatusCode + " - " + respStr;

                    // A resposta pode ser array de objetos com generated_text ou string
                    try
                    {
                        using var doc = JsonDocument.Parse(respStr);
                        if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                        {
                            var first = doc.RootElement[0];
                            if (first.TryGetProperty("generated_text", out var gen)) return gen.GetString() ?? respStr;
                            if (first.TryGetProperty("text", out var txt)) return txt.GetString() ?? respStr;
                        }
                        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            if (doc.RootElement.TryGetProperty("generated_text", out var gen2)) return gen2.GetString() ?? respStr;
                        }
                    }
                    catch { }

                    return respStr;
                }
                catch (Exception ex)
                {
                    return "[Erro HuggingFace] " + ex.Message;
                }
            }

            // 3) OpenAI fallback
            var openaiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(openaiKey))
            {
                return "[Erro] Nenhum provedor configurado. Defina LOCAL_AI_URL ou HUGGINGFACE_API_KEY ou OPENAI_API_KEY.";
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openaiKey);

                var requestObj = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[] {
                        new { role = "system", content = "Voce e um assistente para uma locadora de veiculos. Responda de forma curta, objetiva e em portugues." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 600,
                    temperature = 0.2
                };

                var json = JsonSerializer.Serialize(requestObj);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = client.PostAsync("https://api.openai.com/v1/chat/completions", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                {
                    var err = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return $"[Erro OpenAI] Status: {resp.StatusCode}. {err}";
                }

                var respStr = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(respStr);
                if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    if (message.TryGetProperty("content", out var contentElem))
                    {
                        return contentElem.GetString() ?? "";
                    }
                }

                return "[Erro] Resposta da OpenAI no formato inesperado.";
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna fallback informativo
                return "[Erro] Falha ao chamar OpenAI: " + ex.Message;
            }
        }
    }
}
