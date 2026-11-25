using TaskFlow.Domain;

namespace TaskFlow.Application;

public class TaskFlowService : ITaskFlowService
{
    // ---- Estado em memória: aqui mora a fonte da verdade ----
    private readonly List<Projeto> _projetos = new();
    private readonly List<Tarefa> _tarefas = new();
    private readonly List<TarefaFinanceira> _contas = new();
    private readonly GrafoDependencias _grafo = new();
    private readonly PilhaUndo _undo = new();
    private decimal _salario;

    private int _proximoProjetoId = 1;
    private int _proximaTarefaId = 1;
    private int _proximaContaId = 1;
    private int _proximaAcaoId = 1;

    // ===== PROJETOS =====
    public IReadOnlyList<Projeto> ListarProjetos() => throw new NotImplementedException();
    public Projeto? ObterProjeto(int id) => throw new NotImplementedException();
    public Resultado CriarProjeto(string nome, string? descricao) => throw new NotImplementedException();
    public Resultado EditarProjeto(int id, string nome, string? descricao) => throw new NotImplementedException();
    public Resultado RemoverProjeto(int id) => throw new NotImplementedException();

    // ===== TAREFAS =====
    public IReadOnlyList<Tarefa> ListarTarefas(int? projetoId) => throw new NotImplementedException();
    public Tarefa? ObterTarefa(int id) => throw new NotImplementedException();
    public Resultado CriarTarefa(int projetoId, string titulo, string? descricao, string? responsavel, int prioridade, DateTime? prazo) => throw new NotImplementedException();
    public Resultado EditarTarefa(int id, string titulo, string? descricao, string? responsavel, int prioridade, DateTime? prazo) => throw new NotImplementedException();
    public Resultado MudarStatusTarefa(int id, StatusTarefa novoStatus) => throw new NotImplementedException();
    public Resultado RemoverTarefa(int id) => throw new NotImplementedException();
    public IReadOnlyList<Tarefa> ObterTarefasProntas() => throw new NotImplementedException();
    public IReadOnlyList<Tarefa> ObterFilaPrioridade() => throw new NotImplementedException();

    // ===== DEPENDÊNCIAS =====
    public Resultado AdicionarDependencia(int tarefaId, int tarefaDependenciaId) => throw new NotImplementedException();
    public Resultado RemoverDependencia(int tarefaId, int tarefaDependenciaId) => throw new NotImplementedException();
    public IReadOnlyList<int> ObterPredecessores(int tarefaId) => throw new NotImplementedException();
    public IReadOnlyList<int> ObterOrdemExecucao(int? projetoId) => throw new NotImplementedException();

    // ===== FINANCEIRO =====
    public IReadOnlyList<TarefaFinanceira> ListarContas() => throw new NotImplementedException();
    public TarefaFinanceira? ObterConta(int id) => throw new NotImplementedException();
    public Resultado CriarConta(string titulo, decimal valor, DateTime dataVencimento, TipoFinanceiro tipo) => throw new NotImplementedException();
    public Resultado EditarConta(int id, string titulo, decimal valor, DateTime dataVencimento) => throw new NotImplementedException();
    public Resultado PagarConta(int id) => throw new NotImplementedException();
    public Resultado RemoverConta(int id) => throw new NotImplementedException();
    public Resultado DefinirSalario(decimal valor) => throw new NotImplementedException();
    public decimal ObterSalario() => throw new NotImplementedException();
    public decimal ObterSaldoLivre() => throw new NotImplementedException();
    public IReadOnlyList<TarefaFinanceira> ObterContasAtrasadas() => throw new NotImplementedException();
    public IReadOnlyList<TarefaFinanceira> ObterContasProximasVencimento(int dias) => throw new NotImplementedException();

    // ===== HISTÓRICO / UNDO =====
    public IReadOnlyList<HistoricoAcao> ListarHistorico() => throw new NotImplementedException();
    public HistoricoAcao? ObterUltimaAcao() => throw new NotImplementedException();
    public Resultado DesfazerUltimaAcao() => throw new NotImplementedException();
}