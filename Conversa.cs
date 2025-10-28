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
                        Remetente = "Klebao",
                        Conteudo = "Nao deu pra entender oque voce disse. Escolha uma das opcoes que eu apresentei, assim eu posso te ajudar melhor!"
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
                Remetente = "Klebao",
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
                    Remetente = "Klebao",
                    Conteudo = "Para alugar um veiculo, voce precisa apresentar CNH valida, documento de identidade e cartao de credito em seu nome para caucao.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!"
                };

                return msgBot;
            }
            else if (conteudoUser == "2")
            {
                Mensagem msgBot = new Mensagem
                {
                    Remetente = "Klebao",
                    Conteudo = "Nao. O cartao de credito e obrigatorio para garantir a pre-autorizacao (caucao), que cobre possiveis danos ou multas durante o periodo de locacao.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!"
                };

                return msgBot;
            }
            else if (conteudoUser == "3")
            {
                Mensagem msgBot = new Mensagem
                {
                    Remetente = "Klebao",
                    Conteudo = "A idade minima e de 21 anos, com pelo menos 2 anos de habilitacao (CNH). Algumas categorias de veiculos exigem idade superior.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!"
                };

                return msgBot;
            }
            else if (conteudoUser == "4")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "A caucao e um valor bloqueado no cartao de credito durante o aluguel. Ela nao e cobrada, apenas reservada e liberada apos a devolucao do carro, caso nao haja pendencias.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else if (conteudoUser == "5")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Sim! E possivel devolver o carro em outra unidade da locadora, mas pode haver uma taxa adicional. O valor depende da distancia entre as filiais.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else if (conteudoUser == "6")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Entre em contato imediatamente com o suporte 24h da locadora. O numero esta no contrato e no adesivo colado no para-brisa. Nao tente consertar o carro por conta propria.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else if (conteudoUser == "7")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Sim, desde que o motorista adicional tambem apresente CNH valida. E cobrada uma taxa extra por condutor adicional.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else if (conteudoUser == "8")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Depende do plano contratado. Oferecemos tanto quilometragem livre, quanto planos com limite diario de quilometros (com custo extra por km excedente).\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else if (conteudoUser == "9")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Aceitamos cartoes de credito e debito das principais bandeiras. Pagamentos em dinheiro sao aceitos apenas para valores de diarias, nao para caucao.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else if (conteudoUser == "10")
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Sim! Basta entrar em contato com a locadora antes do fim do contrato. O valor sera ajustado conforme a nova data de devolucao e disponibilidade do veiculo.\n\nAgora, vou voltar pra primeira mensagem, pra caso voce queira mais alguma ajuda!" 
                };

                return msgBot;
            }
            else
            {
                Mensagem msgBot = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Poxa, eu so consigo te ajudar com as perguntas que eu mostrei pra voce. Vamos fazer assim, vou voltar pra primeira mensagem, e ai voce seleciona a opcao de 'Ajuda' pra me contar melhor do que voce precisa.\n" 
                };

                return msgBot;
            }

        }
    }
}
