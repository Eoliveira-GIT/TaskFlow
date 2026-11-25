using System;
using System.Linq;
using TaskFlow.App.MockServices;

namespace TaskFlow.App.Views
{
    public static class MinhasTarefasView
    {
        public static void Renderizar()
        {
            MockDataStore.Inicializar();
            bool voltar = false;

            while (!voltar)
            {
                try { Console.Clear(); } catch { }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(">>> MINHAS TAREFAS <<<\n");
                Console.ResetColor();

                var responsaveis = MockDataStore.Responsaveis().ToList();
                if (responsaveis.Count == 0)
                {
                    Console.WriteLine("Nenhum responsável cadastrado ainda.");
                    Pausar();
                    return;
                }

                Console.WriteLine("Responsáveis disponíveis: " + string.Join(", ", responsaveis));
                Console.Write("\nNome do responsável (Enter para voltar): ");
                var resp = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(resp))
                {
                    voltar = true;
                    break;
                }

                var tarefas = MockDataStore.TarefasDoResponsavel(resp).ToList();
                Console.WriteLine();

                if (tarefas.Count == 0)
                {
                    Console.WriteLine($"Nenhuma tarefa pendente para '{resp}'.");
                    Pausar();
                    continue;
                }

                Console.WriteLine(string.Format("{0,-4} {1,-22} {2,-30} {3,-8} {4,-8} {5,-12}",
                    "ID", "Projeto", "Tarefa", "Prio.", "Prazo", "Status"));
                Console.WriteLine(new string('-', 92));

                foreach (var t in tarefas)
                {
                    var projeto = MockDataStore.ObterProjeto(t.ProjetoId)?.Nome ?? "?";
                    Console.ForegroundColor = MockDataStore.TarefaAtrasada(t) ? ConsoleColor.Red : ConsoleColor.Gray;
                    Console.WriteLine(string.Format("{0,-4} {1,-22} {2,-30} {3,-8} {4,-8} {5,-12}",
                        t.Id,
                        Truncar(projeto, 22),
                        Truncar(t.Titulo, 30),
                        MockDataStore.RotuloPrioridade(t.Prioridade),
                        t.Prazo.ToString("dd/MM"),
                        MockDataStore.RotuloStatus(t.Status)));
                    Console.ResetColor();
                }

                var atrasadas = tarefas.Count(MockDataStore.TarefaAtrasada);
                Console.ForegroundColor = atrasadas > 0 ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"\nTotal: {tarefas.Count} tarefa(s) | Atrasadas: {atrasadas}");
                Console.ResetColor();

                Pausar();
            }
        }

        private static string Truncar(string texto, int max)
        {
            if (texto.Length <= max)
                return texto;
            return texto.Substring(0, max - 1) + "…";
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }
    }
}
