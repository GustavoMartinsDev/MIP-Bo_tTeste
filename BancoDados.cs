using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TestePIM
{
    public class Banco
    {
        // String de conexão. O @ permite pular linha
        public const string StringConexao =
                                @"Server=Localhost;
                                Database=Pim;
                                TrustServerCertificate=True;
                                Integrated Security=True;";

        private SqlConnectionStringBuilder _builder = null!;
        private bool _conectado = false;

        public bool Conectado { get { return _conectado; } }


        public Banco() // Construtor. Inicializa o objeto
        {
            try
            {
                _builder = new SqlConnectionStringBuilder(StringConexao);

                using (var conexao = new SqlConnection(_builder.ConnectionString))
                {
                    conexao.Open(); // Abre conexão
                    _conectado = true;
                    Console.WriteLine("Conexão bem sucedida!");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Falha na tentativa de conexão! ");
                _conectado = false;
            }
        }

        public void ExecutarComandoSql(string comandoSql)
        {
            try
            {
                if (_conectado)
                {
                    using (var conexao = new SqlConnection(_builder.ConnectionString))
                    {
                        conexao.Open();
                        using (var cmd = new SqlCommand(comandoSql, conexao))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                    throw new Exception("Conexão não estabelecida!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public DataTable? ExecutarConsultaSql(string consulta)
        {
            try
            {
                using (var conexao = new SqlConnection(_builder.ConnectionString))
                {
                    conexao.Open();
                    using (var sqlcmd = new SqlCommand(consulta, conexao))
                    {
                        var adaptador = new SqlDataAdapter(sqlcmd);
                        var tabela = new DataTable();
                        adaptador.Fill(tabela);
                        return tabela;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}