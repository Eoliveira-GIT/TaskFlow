using System.Globalization;
using Microsoft.Data.Sqlite;
using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Persistência SQLite. Carrega e salva o estado completo do TaskFlow.
/// O undo (Action) não é persistido — só o histórico descritivo.
/// </summary>
public class SqliteDataStore
{
    private readonly string _connectionString;

    public SqliteDataStore(string? dbPath = null)
    {
        var path = dbPath ?? Path.Combine(AppContext.BaseDirectory, "taskflow.db");
        _connectionString = $"Data Source={path}";
        InicializarSchema();
    }

    private void InicializarSchema()
    {
        using var conn = Abrir();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Projetos (
                Id INTEGER PRIMARY KEY,
                Nome TEXT NOT NULL,
                Descricao TEXT,
                DataCriacao TEXT NOT NULL,
                Ativo INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Tarefas (
                Id INTEGER PRIMARY KEY,
                ProjetoId INTEGER NOT NULL,
                Titulo TEXT NOT NULL,
                Descricao TEXT,
                Responsavel TEXT,
                Prioridade INTEGER NOT NULL,
                Prazo TEXT,
                Status INTEGER NOT NULL,
                TarefaPaiId INTEGER,
                FOREIGN KEY (ProjetoId) REFERENCES Projetos(Id)
            );

            CREATE TABLE IF NOT EXISTS Contas (
                Id INTEGER PRIMARY KEY,
                Titulo TEXT NOT NULL,
                Descricao TEXT,
                Valor TEXT NOT NULL,
                DataVencimento TEXT NOT NULL,
                Tipo INTEGER NOT NULL,
                Status INTEGER NOT NULL,
                DataPagamento TEXT
            );

            CREATE TABLE IF NOT EXISTS Dependencias (
                TarefaId INTEGER NOT NULL,
                TarefaDependenciaId INTEGER NOT NULL,
                PRIMARY KEY (TarefaId, TarefaDependenciaId)
            );

            CREATE TABLE IF NOT EXISTS Config (
                Chave TEXT PRIMARY KEY,
                Valor TEXT
            );

            CREATE TABLE IF NOT EXISTS Historico (
                Id INTEGER PRIMARY KEY,
                DataHora TEXT NOT NULL,
                TipoAcao TEXT NOT NULL,
                Descricao TEXT NOT NULL,
                EntidadeAfetada TEXT,
                EntidadeId INTEGER
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private SqliteConnection Abrir()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    // ===================== LOAD =====================

    public EstadoCarregado Carregar()
    {
        using var conn = Abrir();
        var estado = new EstadoCarregado();

        // Projetos
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, Nome, Descricao, DataCriacao, Ativo FROM Projetos ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                estado.Projetos.Add(new Projeto
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DataCriacao = LerData(reader.GetString(3)),
                    Ativo = reader.GetInt32(4) == 1
                });
            }
        }

        // Tarefas
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, ProjetoId, Titulo, Descricao, Responsavel, Prioridade, Prazo, Status, TarefaPaiId FROM Tarefas ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                estado.Tarefas.Add(new Tarefa
                {
                    Id = reader.GetInt32(0),
                    ProjetoId = reader.GetInt32(1),
                    Titulo = reader.GetString(2),
                    Descricao = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Responsavel = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Prioridade = reader.GetInt32(5),
                    Prazo = reader.IsDBNull(6) ? null : LerData(reader.GetString(6)),
                    Status = (StatusTarefa)reader.GetInt32(7),
                    TarefaPaiId = reader.IsDBNull(8) ? null : reader.GetInt32(8)
                });
            }
        }

        // Contas
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, Titulo, Descricao, Valor, DataVencimento, Tipo, Status, DataPagamento FROM Contas ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                estado.Contas.Add(new TarefaFinanceira
                {
                    Id = reader.GetInt32(0),
                    Titulo = reader.GetString(1),
                    Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Valor = LerDecimal(reader.GetString(3)),
                    DataVencimento = LerData(reader.GetString(4)),
                    Tipo = (TipoFinanceiro)reader.GetInt32(5),
                    Status = (StatusFinanceiro)reader.GetInt32(6),
                    DataPagamento = reader.IsDBNull(7) ? null : LerData(reader.GetString(7))
                });
            }
        }

        // Dependências
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT TarefaId, TarefaDependenciaId FROM Dependencias";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                estado.Dependencias.Add((reader.GetInt32(0), reader.GetInt32(1)));
        }

        // Config (salário + próximos IDs)
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Chave, Valor FROM Config";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var chave = reader.GetString(0);
                var valor = reader.GetString(1);
                switch (chave)
                {
                    case "Salario": estado.Salario = LerDecimal(valor); break;
                    case "ProximoProjetoId": estado.ProximoProjetoId = int.Parse(valor); break;
                    case "ProximaTarefaId": estado.ProximaTarefaId = int.Parse(valor); break;
                    case "ProximaContaId": estado.ProximaContaId = int.Parse(valor); break;
                    case "ProximaAcaoId": estado.ProximaAcaoId = int.Parse(valor); break;
                }
            }
        }

        // Histórico (só descrição — undo fica em memória)
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, DataHora, TipoAcao, Descricao, EntidadeAfetada, EntidadeId FROM Historico ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                estado.Historico.Add(new HistoricoAcao
                {
                    Id = reader.GetInt32(0),
                    DataHora = LerData(reader.GetString(1)),
                    TipoAcao = reader.GetString(2),
                    Descricao = reader.GetString(3),
                    EntidadeAfetada = reader.IsDBNull(4) ? null : reader.GetString(4),
                    EntidadeId = reader.IsDBNull(5) ? null : reader.GetInt32(5)
                });
            }
        }

        return estado;
    }

    // ===================== SAVE (estado completo) =====================

    public void Salvar(
        IEnumerable<Projeto> projetos,
        IEnumerable<Tarefa> tarefas,
        IEnumerable<TarefaFinanceira> contas,
        IEnumerable<Dependencia> dependencias,
        decimal salario,
        int proximoProjetoId,
        int proximaTarefaId,
        int proximaContaId,
        int proximaAcaoId,
        IEnumerable<HistoricoAcao> historico)
    {
        using var conn = Abrir();
        using var tx = conn.BeginTransaction();

        // Limpa e reinsere (simples e seguro para o tamanho do projeto).
        // Filhas antes das mães: Tarefas tem chave estrangeira para Projetos,
        // e apagar um projeto com tarefas ainda no banco viola essa chave.
        Executar(conn, "DELETE FROM Dependencias");
        Executar(conn, "DELETE FROM Tarefas");
        Executar(conn, "DELETE FROM Projetos");
        Executar(conn, "DELETE FROM Contas");
        Executar(conn, "DELETE FROM Config");
        Executar(conn, "DELETE FROM Historico");

        foreach (var p in projetos)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Projetos (Id, Nome, Descricao, DataCriacao, Ativo) VALUES ($id, $nome, $desc, $data, $ativo)";
            cmd.Parameters.AddWithValue("$id", p.Id);
            cmd.Parameters.AddWithValue("$nome", p.Nome);
            cmd.Parameters.AddWithValue("$desc", (object?)p.Descricao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$data", p.DataCriacao.ToString("o"));
            cmd.Parameters.AddWithValue("$ativo", p.Ativo ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        foreach (var t in tarefas)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Tarefas (Id, ProjetoId, Titulo, Descricao, Responsavel, Prioridade, Prazo, Status, TarefaPaiId)
                VALUES ($id, $proj, $titulo, $desc, $resp, $prio, $prazo, $status, $pai)
                """;
            cmd.Parameters.AddWithValue("$id", t.Id);
            cmd.Parameters.AddWithValue("$proj", t.ProjetoId);
            cmd.Parameters.AddWithValue("$titulo", t.Titulo);
            cmd.Parameters.AddWithValue("$desc", (object?)t.Descricao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$resp", (object?)t.Responsavel ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$prio", t.Prioridade);
            cmd.Parameters.AddWithValue("$prazo", t.Prazo.HasValue ? t.Prazo.Value.ToString("o") : DBNull.Value);
            cmd.Parameters.AddWithValue("$status", (int)t.Status);
            cmd.Parameters.AddWithValue("$pai", (object?)t.TarefaPaiId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        foreach (var c in contas)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Contas (Id, Titulo, Descricao, Valor, DataVencimento, Tipo, Status, DataPagamento)
                VALUES ($id, $titulo, $desc, $valor, $venc, $tipo, $status, $pag)
                """;
            cmd.Parameters.AddWithValue("$id", c.Id);
            cmd.Parameters.AddWithValue("$titulo", c.Titulo);
            cmd.Parameters.AddWithValue("$desc", (object?)c.Descricao ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$valor", c.Valor.ToString(CultureInfo.InvariantCulture));
            cmd.Parameters.AddWithValue("$venc", c.DataVencimento.ToString("o"));
            cmd.Parameters.AddWithValue("$tipo", (int)c.Tipo);
            cmd.Parameters.AddWithValue("$status", (int)c.Status);
            cmd.Parameters.AddWithValue("$pag", c.DataPagamento.HasValue ? c.DataPagamento.Value.ToString("o") : DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        foreach (var d in dependencias)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Dependencias (TarefaId, TarefaDependenciaId) VALUES ($t, $d)";
            cmd.Parameters.AddWithValue("$t", d.TarefaId);
            cmd.Parameters.AddWithValue("$d", d.TarefaDependenciaId);
            cmd.ExecuteNonQuery();
        }

        SalvarConfig(conn, "Salario", salario.ToString(CultureInfo.InvariantCulture));
        SalvarConfig(conn, "ProximoProjetoId", proximoProjetoId.ToString());
        SalvarConfig(conn, "ProximaTarefaId", proximaTarefaId.ToString());
        SalvarConfig(conn, "ProximaContaId", proximaContaId.ToString());
        SalvarConfig(conn, "ProximaAcaoId", proximaAcaoId.ToString());

        foreach (var h in historico)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Historico (Id, DataHora, TipoAcao, Descricao, EntidadeAfetada, EntidadeId)
                VALUES ($id, $data, $tipo, $desc, $ent, $entId)
                """;
            cmd.Parameters.AddWithValue("$id", h.Id);
            cmd.Parameters.AddWithValue("$data", h.DataHora.ToString("o"));
            cmd.Parameters.AddWithValue("$tipo", h.TipoAcao);
            cmd.Parameters.AddWithValue("$desc", h.Descricao);
            cmd.Parameters.AddWithValue("$ent", (object?)h.EntidadeAfetada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$entId", (object?)h.EntidadeId ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
    }

    // Números e datas sempre no formato invariante ("1500.50", ISO 8601):
    // o banco continua legível mesmo se o idioma do Windows mudar.
    // A vírgula só aparece em bancos gravados pela versão anterior, que usava o idioma do Windows ("5000,50").
    private static decimal LerDecimal(string texto)
        => texto.Contains(',')
            ? decimal.Parse(texto, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"))
            : decimal.Parse(texto, NumberStyles.Number, CultureInfo.InvariantCulture);

    private static DateTime LerData(string texto)
        => DateTime.Parse(texto, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static void SalvarConfig(SqliteConnection conn, string chave, string valor)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Config (Chave, Valor) VALUES ($k, $v)";
        cmd.Parameters.AddWithValue("$k", chave);
        cmd.Parameters.AddWithValue("$v", valor);
        cmd.ExecuteNonQuery();
    }

    private static void Executar(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}

/// <summary>Dados carregados do banco na inicialização.</summary>
public class EstadoCarregado
{
    public List<Projeto> Projetos { get; } = new();
    public List<Tarefa> Tarefas { get; } = new();
    public List<TarefaFinanceira> Contas { get; } = new();
    public List<(int TarefaId, int DependenciaId)> Dependencias { get; } = new();
    public List<HistoricoAcao> Historico { get; } = new();
    public decimal Salario { get; set; }
    public int ProximoProjetoId { get; set; } = 1;
    public int ProximaTarefaId { get; set; } = 1;
    public int ProximaContaId { get; set; } = 1;
    public int ProximaAcaoId { get; set; } = 1;
}
