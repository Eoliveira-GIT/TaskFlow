using System;
using System.Collections.Generic;
using TaskFlow.Application;

namespace TaskFlow.App.MockServices
{
    public class TaskFlowMockService : ITaskFlowService
    {
        public void ObterDashboardResumo()
        {
            Console.WriteLine("[MOCK] Obtendo resumo do dashboard...");
        }

        public void ListarProjetos()
        {
            Console.WriteLine("[MOCK] Listando projetos...");
        }

        public void ExibirKanbanProjeto(int projetoId)
        {
            Console.WriteLine($"[MOCK] Exibindo Kanban do projeto #{projetoId}...");
        }

        public void ExibirTarefasProntas()
        {
            Console.WriteLine("[MOCK] Exibindo tarefas da fila de prioridade...");
        }

        public void DesfazerUltimaAcao()
        {
            Console.WriteLine("[MOCK] Desfazendo última ação via Pilha de Undo...");
        }

        public void ExibirPainelFinanceiro()
        {
            Console.WriteLine("[MOCK] Exibindo painel financeiro...");
        }
    }
}
