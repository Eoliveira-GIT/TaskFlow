using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Contrato entre a interface (TaskFlow.App) e o back end.
/// Consultas devolvem dados; comandos que podem falhar devolvem <see cref="Resultado"/>.
/// Este contrato não conhece console nem telas: quem exibe é a View.
/// </summary>
public interface ITaskFlowService
{
    // =====================
    // PROJETOS
    // =====================
    IReadOnlyList<Projeto> ListarProjetos();
    Projeto? ObterProjeto(int id);
    Resultado CriarProjeto(string nome, string? descricao = null);
    Resultado EditarProjeto(int id, string nome, string? descricao);

    /// <summary>Remove o projeto e as tarefas dele. O undo deve restaurar tudo.</summary>
    Resultado RemoverProjeto(int id);

    // =====================
    // TAREFAS
    // =====================
    IReadOnlyList<Tarefa> ListarTarefas(int? projetoId = null);
    Tarefa? ObterTarefa(int id);

    Resultado CriarTarefa(
        int projetoId,
        string titulo,
        string? descricao = null,
        string? responsavel = null,
        int prioridade = 3,
        DateTime? prazo = null);

    Resultado EditarTarefa(
        int id,
        string titulo,
        string? descricao,
        string? responsavel,
        int prioridade,
        DateTime? prazo);

    Resultado MudarStatusTarefa(int id, StatusTarefa novoStatus);
    Resultado RemoverTarefa(int id);

    /// <summary>Tarefas que já podem ser iniciadas (todas as predecessoras concluídas).</summary>
    IReadOnlyList<Tarefa> ObterTarefasProntas();

    /// <summary>Tarefas ordenadas por prioridade e prazo.</summary>
    IReadOnlyList<Tarefa> ObterFilaPrioridade();

    // =====================
    // DEPENDÊNCIAS
    // =====================

    /// <summary>"tarefaId depende de tarefaDependenciaId". Recusa autodependência, duplicada e ciclo.</summary>
    Resultado AdicionarDependencia(int tarefaId, int tarefaDependenciaId);
    Resultado RemoverDependencia(int tarefaId, int tarefaDependenciaId);
    IReadOnlyList<int> ObterPredecessores(int tarefaId);

    /// <summary>Ids das tarefas na ordem sugerida de execução (ordenação topológica).</summary>
    IReadOnlyList<int> ObterOrdemExecucao(int? projetoId = null);

    // =====================
    // FINANCEIRO
    // =====================
    IReadOnlyList<TarefaFinanceira> ListarContas();
    TarefaFinanceira? ObterConta(int id);
    Resultado CriarConta(string titulo, decimal valor, DateTime dataVencimento, TipoFinanceiro tipo);
    Resultado EditarConta(int id, string titulo, decimal valor, DateTime dataVencimento);
    Resultado PagarConta(int id);
    Resultado RemoverConta(int id);

    Resultado DefinirSalario(decimal valor);
    decimal ObterSalario();

    /// <summary>
    /// Saldo livre = salário menos as contas ainda não pagas.
    /// (Regra proposta: confirmar se considera só o mês atual e como tratar o TipoFinanceiro.)
    /// </summary>
    decimal ObterSaldoLivre();

    /// <summary>Contas não pagas com vencimento anterior a hoje.</summary>
    IReadOnlyList<TarefaFinanceira> ObterContasAtrasadas();

    /// <summary>Contas não pagas que vencem nos próximos <paramref name="dias"/> dias.</summary>
    IReadOnlyList<TarefaFinanceira> ObterContasProximasVencimento(int dias = 7);

    // =====================
    // HISTÓRICO / UNDO
    // =====================
    IReadOnlyList<HistoricoAcao> ListarHistorico();
    HistoricoAcao? ObterUltimaAcao();
    Resultado DesfazerUltimaAcao();
}