using System.Runtime.Intrinsics.X86;

namespace TestePIM
{
    public class Conversa
    {
        public List<Mensagem> Mensagens { get; } = new List<Mensagem>();


        public void AdicionarMensagem(Mensagem mensagem)
        {
            Mensagens.Add(mensagem);
        }

        public Mensagem GerarResposta(string conteudoUser)
        {

            if (conteudoUser != "sair")
            {
                if (conteudoUser == "1" || conteudoUser == "ajuda")
                {
                    Mensagem msgAjuda = Chamado.GerarMensagemAjuda();
                    Mensagens.Add(msgAjuda);
                    return msgAjuda;
                }
                else
                {
                    Mensagem msgBot = new Mensagem
                    {
                        Remetente = "Klebão",
                        Conteudo = "Não deu pra entender oque você disse. Escolha uma das opções que eu apresentei, assim eu posso te ajudar melhor!"
                    };

                    Mensagens.Add(msgBot);
                    return msgBot;
                }
            }
            else
            {
                Mensagem msgEncerramento = Chamado.GerarMensagemEncerramento();

                Mensagens.Add(msgEncerramento);
                return msgEncerramento;
            }
        }

        public Mensagem GerarRespostaAjuda(string conteudoUser)
        {
            Mensagem msgBot = new Mensagem
            {
                Remetente = "Klebão",
                Conteudo = EnvioMsg.EnviarBot(conteudoUser).Conteudo
            };
            Mensagens.Add(msgBot);
            return msgBot;
        }

        public Mensagem GerarRespostaFaq(string conteudoUser)
        {
            if (conteudoUser == "1")
            {
                Mensagem msgBot = new Mensagem
                {
                    Remetente = "Klebão",
                    Conteudo = "Para alugar um veículo, você precisa apresentar CNH válida, documento de identidade e cartão de crédito em seu nome para caução.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!"
                };

                return msgBot;
            }
            else if (conteudoUser == "2")
            {
                Mensagem msgBot = new Mensagem
                {
                    Remetente = "Klebão",
                    Conteudo = "Não. O cartão de crédito é obrigatório para garantir a pré-autorização (caução), que cobre possíveis danos ou multas durante o período de locação.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!"
                };

                return msgBot;
            }
            else if (conteudoUser == "3")
            {
                Mensagem msgBot = new Mensagem
                {
                    Remetente = "Klebão",
                    Conteudo = "A idade mínima é de 21 anos, com pelo menos 2 anos de habilitação (CNH). Algumas categorias de veículos exigem idade superior.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!"
                };

                return msgBot;
            }
            else if (conteudoUser == "4")
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "A caução é um valor bloqueado no cartão de crédito durante o aluguel.Ela não é cobrada, apenas reservada e liberada após a devolução do carro, caso não haja pendências.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" };

                return msgBot;
            }
            else if (conteudoUser == "5")
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Sim! É possível devolver o carro em outra unidade da locadora, mas pode haver uma taxa adicional. O valor depende da distância entre as filiais.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" };

                return msgBot;
            }
            else if (conteudoUser == "6")
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Entre em contato imediatamente com o suporte 24h da locadora. O número está no contrato e no adesivo colado no para-brisa. Não tente consertar o carro por conta própria.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" }   ;

                return msgBot;
            }
            else if (conteudoUser == "7")
            {
                    Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Sim, desde que o motorista adicional também apresente CNH válida. É cobrada uma taxa extra por condutor adicional.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" };

                return msgBot;
            }
            else if (conteudoUser == "8")
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Depende do plano contratado. Oferecemos tanto quilometragem livre, quanto planos com limite diário de quilômetros (com custo extra por km excedente).\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" };

                return msgBot;
            }
            else if (conteudoUser == "9")
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Aceitamos cartões de crédito e débito das principais bandeiras.Pagamentos em dinheiro são aceitos apenas para valores de diárias, não para caução.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" };

                return msgBot;
            }
            else if (conteudoUser == "10")
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Sim! Basta entrar em contato com a locadora antes do fim do contrato.O valor será ajustado conforme a nova data de devolução e disponibilidade do veículo.\n\nAgora, vou voltar pra primeira mensagem, pra caso você queira mais alguma ajuda!" };

                return msgBot;
            }
            else
            {
                Mensagem msgBot = new Mensagem { Remetente = "Klebão", Conteudo = "Poxa, eu só consigo te ajudar com as perguntas que eu mostrei pra você. Vamos fazer assim, vou voltar pra primeira mensagem, e aí você seleciona a opção de 'Ajuda' pra me contar melhor do que você precisa.\n" };

                return msgBot;
            }

        }
    }
}
