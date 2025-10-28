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
            string conteudoPadrao = "Fala, chefe! Klebão na área. Bora resolver seu problema rapidão?";
            string conteudoAlternativo = "Se tem problema, chama o Klebão. Se não tem, chama também que eu gosto de bater um papo.";

            string conteudoEscolhido = Random.Shared.Next(2) == 0
                ? conteudoPadrao
                : conteudoAlternativo;

            return new Mensagem
            {
                Remetente = "Klebão",
                Conteudo = $"{conteudoEscolhido}\n\n"+
                @"Me diz aí do que você precisa:

1 - Ajuda 
2 - FAQ (Perguntas Frequentes)
0 - Sair"
            };

        }

        // Método que monta a mensagem de Ajuda
        public static Mensagem GerarMensagemAjuda()
        {
            // Conteúdo da mensagem (string)
            string conteudoAjuda = "Agora, me dá mais detalhes sobre o seu problema.";

            // Criar a instância do tipo Mensagem e inicializar suas propriedades
            Mensagem msgAjuda = new()
            {
                Remetente = "Klebão",
                Conteudo = conteudoAjuda,
            };

            // Retornar o objeto msgAjuda
            return msgAjuda;
        }

        // Método que monta a mensagem de FAQ
        public static Mensagem GerarMensagemFAQ()
        {
            // Conteúdo da mensagem (string)
            string conteudoFAQ = "Bora lá! Dá uma olhadinha nessas perguntas, uma delas pode ser a sua:\n\n" +
                "1. Quais documentos preciso para alugar um carro?\n" +
                "2. Posso alugar um carro sem cartão de crédito?\n" +
                "3. Qual é a idade mínima para alugar um carro?\n" +
                "4. Como funciona a caução do aluguel?\n" +
                "5. Posso devolver o carro em outra cidade?\n" +
                "6. O que devo fazer em caso de acidente ou pane?\n" +
                "7. Posso adicionar outro motorista ao contrato?\n" +
                "8. Há limite de quilometragem nos carros alugados?\n" +
                "9. Quais formas de pagamento são aceitas?\n" +
                "10. Posso estender o período do aluguel?\n\n" +
                "É só mandar o número da pergunta que você gostaria de saber mais.";

            Mensagem msgFAQ = new()
            {
                Remetente = "Klebão",
                Conteudo = conteudoFAQ,

            };

            return msgFAQ;
        }

        // Método que monta a mensagem ao encerrar o chamado (conteúdo aleatório)
        public static Mensagem GerarMensagemEncerramento()
        {
            string conteudoPadrao = "Se precisar, já sabe: chama o Klebão que nóis desenrola.";
            string conteudoAlternativo = "Foi um prazerzão! Vai com Deus e com o carro certo, hein?";


            string conteudoEscolhido = Random.Shared.Next(2) == 0
                ? conteudoPadrao
                : conteudoAlternativo;

            Mensagem msgEncerramento = new()
            {
                Remetente = "Klebão",
                Conteudo = conteudoEscolhido,

            };

            return msgEncerramento;
        }
    }
}