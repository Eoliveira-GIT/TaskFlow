using Microsoft.Data.Sqlite;
using TaskFlow.Application;
using TaskFlow.Domain;

/// <summary>
/// Testes do TaskFlowService com um banco SQLite temporário:
/// regras de negócio, undo e se os dados voltam iguais depois de fechar e reabrir.
/// </summary>
public static class TaskFlowServiceTestes
{
    private static int _falhas;

    public static int Executar()
    {
        Console.WriteLine("\n=== Testes automáticos do TaskFlowService (com SQLite) ===\n");
        _falhas = 0;

        var banco = Path.Combine(Path.GetTempPath(), $"taskflow_teste_{Guid.NewGuid():N}.db");
        try
        {
            TestarRegrasEUndo(banco);
            TestarPersistencia(banco);
        }
        catch (Exception ex)
        {
            _falhas++;
            Console.WriteLine($"[FALHOU] exceção inesperada: {ex}");
        }
        finally
        {
            // O pool de conexões mantém o arquivo aberto no Windows; libera antes de apagar.
            SqliteConnection.ClearAllPools();
            try { File.Delete(banco); } catch { /* arquivo temporário: não é erro de teste */ }
        }

        Console.WriteLine(_falhas == 0
            ? "\nTodos os testes do serviço passaram."
            : $"\n{_falhas} verificação(ões) do serviço falharam.");

        return _falhas == 0 ? 0 : 1;
    }

    // Estado deixado no banco para o teste de persistência conferir.
    private static int _projetoId, _t1, _t2, _t3, _contaId, _qtdHistorico;

    private static void TestarRegrasEUndo(string banco)
    {
        ITaskFlowService s = new TaskFlowService(banco);
        Verificar("banco novo começa vazio", 0, s.ListarProjetos().Count);

        Verificar("criar projeto", true, s.CriarProjeto("Projeto Teste").Sucesso);
        Verificar("projeto sem nome é recusado", false, s.CriarProjeto("   ").Sucesso);
        _projetoId = s.ListarProjetos().Last().Id;

        s.CriarTarefa(_projetoId, "Tarefa A", prioridade: 2);
        _t1 = s.ListarTarefas().Last().Id;
        s.CriarTarefa(_projetoId, "Tarefa B", prioridade: 1, prazo: DateTime.Today.AddDays(-1));
        _t2 = s.ListarTarefas().Last().Id;
        s.CriarTarefa(_projetoId, "Tarefa C", prioridade: 3);
        _t3 = s.ListarTarefas().Last().Id;
        Verificar("prioridade fora de 1-5 é recusada", false, s.CriarTarefa(_projetoId, "X", prioridade: 9).Sucesso);

        // Dependências: B depende de A; C depende de B
        Verificar("adicionar dependência B→A", true, s.AdicionarDependencia(_t2, _t1).Sucesso);
        Verificar("adicionar dependência C→B", true, s.AdicionarDependencia(_t3, _t2).Sucesso);
        Verificar("ciclo A→C é recusado", false, s.AdicionarDependencia(_t1, _t3).Sucesso);
        Verificar("autodependência é recusada", false, s.AdicionarDependencia(_t1, _t1).Sucesso);
        Verificar("dependência duplicada é recusada", false, s.AdicionarDependencia(_t2, _t1).Sucesso);
        Verificar("B aguarda A", _t1, s.ObterPredecessorasPendentes(_t2).FirstOrDefault());

        var ordem = s.ObterOrdemExecucao(_projetoId);
        Verificar("ordem topológica começa por A", _t1, ordem.FirstOrDefault());
        Verificar("ordem topológica termina em C", _t3, ordem.LastOrDefault());

        // Bloqueio por dependência
        Verificar("B não avança com A pendente", false, s.MudarStatusTarefa(_t2, StatusTarefa.InProgress).Sucesso);
        Verificar("concluir A", true, s.MudarStatusTarefa(_t1, StatusTarefa.Done).Sucesso);
        Verificar("B avança depois de A concluída", true, s.MudarStatusTarefa(_t2, StatusTarefa.InProgress).Sucesso);
        Verificar("fila não traz concluídas", false, s.ObterFilaPrioridade().Any(t => t.Id == _t1));
        Verificar("fila começa pela prioridade 1 (B)", _t2, s.ObterFilaPrioridade().First().Id);

        // Undo de remover projeto: tarefas e dependências voltam, na mesma ordem
        var antes = s.ListarTarefas().Select(t => t.Id).ToList();
        Verificar("remover projeto", true, s.RemoverProjeto(_projetoId).Sucesso);
        Verificar("tarefas saem com o projeto", 0, s.ListarTarefas().Count);
        Verificar("desfazer remoção", true, s.DesfazerUltimaAcao().Sucesso);
        Verificar("tarefas voltam na mesma ordem", string.Join(",", antes), string.Join(",", s.ListarTarefas().Select(t => t.Id)));
        Verificar("dependência C→B volta", _t2, s.ObterPredecessores(_t3).FirstOrDefault());

        // Financeiro
        Verificar("definir salário com centavos", true, s.DefinirSalario(5000.50m).Sucesso);
        s.CriarConta("Luz", 199.90m, DateTime.Today.AddDays(-1), TipoFinanceiro.Despesa);
        _contaId = s.ListarContas().Last().Id;
        Verificar("conta vencida aparece como atrasada", 1, s.ObterContasAtrasadas().Count);
        Verificar("pagar conta", true, s.PagarConta(_contaId).Sucesso);
        Verificar("pagar de novo é recusado", false, s.PagarConta(_contaId).Sucesso);
        Verificar("desfazer pagamento", true, s.DesfazerUltimaAcao().Sucesso);
        Verificar("conta volta a não paga", StatusFinanceiro.Atrasada, s.ObterConta(_contaId)!.Status);
        Verificar("total a pagar", 199.90m, s.ObterTotalAPagar());

        _qtdHistorico = s.ListarHistorico().Count;
    }

    private static void TestarPersistencia(string banco)
    {
        // Simula fechar e abrir o programa: nova instância lendo o mesmo arquivo.
        ITaskFlowService s = new TaskFlowService(banco);

        Verificar("[reabrir] projeto continua lá", "Projeto Teste", s.ObterProjeto(_projetoId)?.Nome);
        Verificar("[reabrir] 3 tarefas", 3, s.ListarTarefas().Count);
        Verificar("[reabrir] status salvo", StatusTarefa.Done, s.ObterTarefa(_t1)?.Status);
        Verificar("[reabrir] prazo salvo", DateTime.Today.AddDays(-1), s.ObterTarefa(_t2)?.Prazo);
        Verificar("[reabrir] dependência salva", _t1, s.ObterPredecessores(_t2).FirstOrDefault());
        Verificar("[reabrir] salário com centavos", 5000.50m, s.ObterSalario());
        Verificar("[reabrir] valor da conta", 199.90m, s.ObterConta(_contaId)?.Valor);
        Verificar("[reabrir] histórico salvo", _qtdHistorico, s.ListarHistorico().Count);

        var maisRecente = s.ObterUltimaAcao()?.Descricao ?? "";
        Verificar("[reabrir] topo do histórico é a ação mais recente", true, maisRecente.Contains("Conta #"));

        // Ação de sessão anterior não pode ser desfeita, e não some do histórico
        Verificar("[reabrir] undo de sessão anterior é recusado", false, s.DesfazerUltimaAcao().Sucesso);
        Verificar("[reabrir] histórico intacto após a recusa", _qtdHistorico, s.ListarHistorico().Count);

        // IDs continuam de onde pararam
        s.CriarProjeto("Outro");
        Verificar("[reabrir] novo ID não repete", _projetoId + 1, s.ListarProjetos().Last().Id);

        s.LimparHistorico();
        ITaskFlowService s2 = new TaskFlowService(banco);
        Verificar("[reabrir] limpar histórico é salvo", 0, s2.ListarHistorico().Count);
    }

    private static void Verificar<T>(string nome, T esperado, T obtido)
    {
        bool ok = EqualityComparer<T>.Default.Equals(esperado, obtido);
        if (!ok) _falhas++;

        Console.WriteLine(ok
            ? $"[PASSOU] {nome}"
            : $"[FALHOU] {nome} -> esperado: {esperado}, obtido: {obtido}");
    }
}
