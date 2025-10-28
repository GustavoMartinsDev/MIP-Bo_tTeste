namespace TestePIM
{
    public class Chamado
    {
        // Propriedade pública que contém a mensagem criada ao iniciar o chamado
        public Mensagem MensagemBoasVindas { get; }
        public Mensagem FAQ { get; }
        public Conversa novaConversa = new();

        // Construtor: cria e atribui a mensagem de boas-vindas
        public Chamado()
        {
            MensagemBoasVindas = GerarMensagemBoasVindas();
            FAQ = GerarMensagemFAQ();

            novaConversa.AdicionarMensagem(MensagemBoasVindas);
        }

        // Método que monta a mensagem ao iniciar o chamado (conteúdo aleatório)
        private static Mensagem GerarMensagemBoasVindas()
        {
            string conteudoPadrao = "Fala, chefe! Klebao na area. Bora resolver seu problema rapidao?";
            string conteudoAlternativo = "Se tem problema, chama o Klebao. Se nao tem, chama tambem que eu gosto de bater um papo.";

            string conteudoEscolhido = Random.Shared.Next(2) == 0
                ? conteudoPadrao
                : conteudoAlternativo;

            return new Mensagem
            {
                Remetente = "Klebao",
                Conteudo = $"{conteudoEscolhido}\n\n"+
                @"Me diz ai do que voce precisa:

1 - Ajuda 
2 - FAQ (Perguntas Frequentes)
0 - Sair"
            };

        }

        // Método que monta a mensagem de Ajuda
        public static Mensagem GerarMensagemAjuda()
        {
            // Conteúdo da mensagem (string)
            string conteudoAjuda = "Agora, me da mais detalhes sobre o seu problema.";

            // Criar a instância do tipo Mensagem e inicializar suas propriedades
            Mensagem msgAjuda = new()
            {
                Remetente = "Klebao",
                Conteudo = conteudoAjuda,
            };

            // Retornar o objeto msgAjuda
            return msgAjuda;
        }

        // Método que monta a mensagem de FAQ
        public static Mensagem GerarMensagemFAQ()
        {
            // Conteúdo da mensagem (string)
            string conteudoFAQ = "Bora la! Da uma olhadinha nessas perguntas, uma delas pode ser a sua:\n\n" +
                "1. Quais documentos preciso para alugar um carro?\n" +
                "2. Posso alugar um carro sem cartao de credito?\n" +
                "3. Qual e a idade minima para alugar um carro?\n" +
                "4. Como funciona a caucao do aluguel?\n" +
                "5. Posso devolver o carro em outra cidade?\n" +
                "6. O que devo fazer em caso de acidente ou pane?\n" +
                "7. Posso adicionar outro motorista ao contrato?\n" +
                "8. Ha limite de quilometragem nos carros alugados?\n" +
                "9. Quais formas de pagamento sao aceitas?\n" +
                "10. Posso estender o periodo do aluguel?\n\n" +
                "E so mandar o numero da pergunta que voce gostaria de saber mais.";

            Mensagem msgFAQ = new()
            {
                Remetente = "Klebao",
                Conteudo = conteudoFAQ,

            };

            return msgFAQ;
        }

        // Método que monta a mensagem ao encerrar o chamado (conteúdo aleatório)
        public static Mensagem GerarMensagemEncerramento()
        {
            string conteudoPadrao = "Se precisar, ja sabe: chama o Klebao que nois desenrola.";
            string conteudoAlternativo = "Foi um prazerzao! Vai com Deus e com o carro certo, hein?";


            string conteudoEscolhido = Random.Shared.Next(2) == 0
                ? conteudoPadrao
                : conteudoAlternativo;

            Mensagem msgEncerramento = new()
            {
                Remetente = "Klebao",
                Conteudo = conteudoEscolhido,

            };

            return msgEncerramento;
        }
    }
}