using System;
using TaskFlow.App.Services;
using TaskFlow.App.Navigation;
using TaskFlow.App.Views;

namespace TaskFlow.App.MockServices
{
    public class TaskFlowMockService : ITaskFlowService
    {
        public void ObterDashboardResumo()
        {
            DashboardView.Renderizar();
        }

        public void ListarProjetos()
        {
            ProjetosView.Renderizar();
        }

        public void ExibirKanbanProjeto(int projetoId)
        {
            KanbanView.Renderizar(projetoId);
        }

        public void ExibirTarefasProntas()
        {
            MockDataStore.Inicializar();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(">>> FILA DE PRIORIDADE - TAREFAS PRONTAS <<<\n");
            Console.ResetColor();
            Console.WriteLine("Ordenado por: Prioridade (Alta > Média > Baixa) + Prazo\n");

            int i = 1;
            foreach (var t in MockDataStore.TarefasProntas())
            {
                var resp = string.IsNullOrWhiteSpace(t.Responsavel) ? "-" : t.Responsavel;
                var atraso = MockDataStore.TarefaAtrasada(t) ? "  ! ATRASADA" : string.Empty;
                Console.WriteLine($"  {i++}. [{MockDataStore.RotuloPrioridade(t.Prioridade)}] " +
                                  $"#{t.Id} {t.Titulo} - Prazo: {t.Prazo:dd/MM} | Resp: {resp} " +
                                  $"({MockDataStore.RotuloStatus(t.Status)}){atraso}");
            }

            if (i == 1)
                Console.WriteLine("  (nenhuma tarefa pronta)");

            Console.WriteLine("\nLegenda: ✅ Pronta  ⏳ Aguardando  🔒 Bloqueada  ! Atrasada");
        }

        public void DesfazerUltimaAcao()
        {
            MockDataStore.Inicializar();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> HISTÓRICO E DESFAZER (UNDO) <<<\n");
            Console.ResetColor();

            var historico = MockDataStore.ObterHistoricoRecente(10);
            if (historico.Count == 0)
            {
                Console.WriteLine("Nenhuma ação registrada.");
                return;
            }

            Console.WriteLine("Últimas ações (mais recente primeiro):");
            for (int i = 0; i < historico.Count; i++)
                Console.WriteLine($"  {i + 1}. {historico[i].Descricao}");
            Console.WriteLine();

            Console.WriteLine("[U] Desfazer última ação  |  [V] Voltar");
            Console.Write("Comando: ");
            var cmd = Console.ReadLine()?.Trim().ToUpper();

            if (cmd == "U")
            {
                if (!EntradaUsuario.Confirmar("Confirma desfazer a última ação? (s/n): "))
                {
                    Console.WriteLine("Operação cancelada.");
                    return;
                }

                if (MockDataStore.DesfazerUltimaAcao(out var descricao))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✓ Desfeito: {descricao}");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine("Voltando...");
            }
        }

        public void ExibirPainelFinanceiro()
        {
            FinanceiroView.Renderizar();
        }
    }
}
