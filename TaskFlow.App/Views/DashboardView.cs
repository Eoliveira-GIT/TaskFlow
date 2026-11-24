using System;
using System.Linq;
using TaskFlow.App.MockServices;

namespace TaskFlow.App.Views
{
    public static class DashboardView
    {
        public static void Renderizar()
        {
            MockDataStore.Inicializar();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(">>> DASHBOARD GERAL - VISÃO UNIFICADA <<<\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== 💰 RESUMO FINANCEIRO ===");
            Console.ResetColor();
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Salário estimado:", MockDataStore.FormatarMoeda(MockDataStore.Salario)));
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Total de contas:", MockDataStore.FormatarMoeda(MockDataStore.TotalContas())));
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "A pagar:", MockDataStore.FormatarMoeda(MockDataStore.TotalAPagar())));
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Saldo após quitar tudo:", MockDataStore.FormatarMoeda(MockDataStore.SaldoAposQuitarTudo())));
            var atrasadas = MockDataStore.ContasAtrasadas();
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Status:", atrasadas > 0 ? $"⚠️ {atrasadas} conta(s) atrasada(s)" : "✅ Em dia"));

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n=== ⚡ TAREFAS URGENTES (TOPO) ===");
            Console.ResetColor();
            Console.WriteLine(string.Format("  {0,-4} {1,-30} {2,-10} {3,-14} {4,-14} {5,-6}", "#", "Descrição", "Prioridade", "Responsável", "Status", "Atras."));
            Console.WriteLine(new string('-', 84));

            var urgentes = MockDataStore.TarefasProntas().Take(5).ToList();
            if (urgentes.Count == 0)
            {
                Console.WriteLine("  (nenhuma tarefa pendente)");
            }
            else
            {
                foreach (var t in urgentes)
                {
                    var responsavel = string.IsNullOrWhiteSpace(t.Responsavel) ? "-" : t.Responsavel;
                    var atraso = MockDataStore.TarefaAtrasada(t) ? "SIM" : "-";
                    Console.ForegroundColor = MockDataStore.TarefaAtrasada(t) ? ConsoleColor.Red : ConsoleColor.Gray;
                    Console.WriteLine(string.Format("  {0,-4} {1,-30} {2,-10} {3,-14} {4,-14} {5,-6}",
                        t.Id, Truncar(t.Titulo, 30), MockDataStore.RotuloPrioridade(t.Prioridade), Truncar(responsavel!, 14),
                        MockDataStore.RotuloStatus(t.Status), atraso));
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n=== ⚠️ TAREFAS ATRASADAS POR RESPONSÁVEL ===");
            Console.ResetColor();

            var atrasadasPorPessoa = MockDataStore.AtrasadasPorResponsavel();
            if (atrasadasPorPessoa.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✅ Nenhuma tarefa atrasada.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(string.Format("  {0,-24} {1,-10}", "Responsável", "Atrasadas"));
                Console.WriteLine(new string('-', 36));
                foreach (var (responsavel, total) in atrasadasPorPessoa)
                    Console.WriteLine(string.Format("  {0,-24} {1,-10}", responsavel, total));
            }

            Console.WriteLine("\nPressione Enter para voltar...");
            Console.ReadLine();
        }

        private static string Truncar(string texto, int max)
        {
            if (texto.Length <= max)
                return texto;
            return texto.Substring(0, max - 1) + "…";
        }
    }
}
