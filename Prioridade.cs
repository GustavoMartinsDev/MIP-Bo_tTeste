namespace TestePIM
{
    public static class Prioridade
    {
        public static string ClassificarPrioridade(string conteudo)
        {
            if (conteudo.Contains("carro não liga") || conteudo.Contains("pneu furado") || conteudo.Contains("pane") || conteudo.Contains("roubo") || conteudo.Contains("não consigo retirar o carro") || conteudo.Contains("falha no pagamento") || conteudo.Contains("erro na reserva") || conteudo.Contains("reserva cancelada sozinha") || conteudo.Contains("urgente") || conteudo.Contains("veículo não disponível") || conteudo.Contains("problema grave") || conteudo.Contains("acidente"))
            {
                return "alta";
            }
            else if (conteudo.Contains("atraso na entrega") || conteudo.Contains("problema com ar-condicionado") || conteudo.Contains("erro no valor cobrado") || conteudo.Contains("limpeza ruim") || conteudo.Contains("tanque estava vazio") || conteudo.Contains("veículo diferente do reservado") || conteudo.Contains("aplicativo travando") || conteudo.Contains("lento pra carregar") || conteudo.Contains("dificuldade para localizar agência"))
            {
                return "media";
            }
            else if (conteudo.Contains("como alterar reserva") || conteudo.Contains("não entendi a taxa") || conteudo.Contains("sugestão") || conteudo.Contains("erro de ortografia") || conteudo.Contains("layout confuso") || conteudo.Contains("dúvida sobre planos") || conteudo.Contains("emitir nota"))
            {
                return "baixa";
            }
            else
            {
                return "Não classificada";
            }
        }
    }
}
