namespace TaskFlow.App.Services
{
    public interface ITaskFlowService
    {
        void ObterDashboardResumo();
        void ListarProjetos();
        void ExibirKanbanProjeto(int projetoId);
        void ExibirTarefasProntas();
        void DesfazerUltimaAcao();
        void ExibirPainelFinanceiro();
    }
}
