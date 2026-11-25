using System;
using System.Linq;
using TaskFlow.Application;
using TaskFlow.Domain;
using static TaskFlow.App.Views.Formatacao;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    public class MinhasTarefasView
    {
        private readonly ITaskFlowService _service;

        public MinhasTarefasView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            while (true)
            {
                LimparTela();
                Titulo("MINHAS TAREFAS", ConsoleColor.Cyan);

                var responsaveis = _service.ListarTarefas()
                    .Where(t => !string.IsNullOrWhiteSpace(t.Responsavel))
                    .Select(t => t.Responsavel!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(r => r)
                    .ToList();

                if (responsaveis.Count == 0)
                {
                    Console.WriteLine("Nenhum responsável cadastrado ainda.");
                    Pausar();
                    return;
                }

                Console.WriteLine("Responsáveis disponíveis: " + string.Join(", ", responsaveis));
                Console.Write("\nNome do responsável (Enter para voltar): ");
                var nome = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(nome))
                    return;

                // Pendentes do responsável, do prazo mais próximo para o mais distante (sem prazo por último).
                var tarefas = _service.ListarTarefas()
                    .Where(t => string.Equals(t.Responsavel, nome, StringComparison.OrdinalIgnoreCase)
                                && t.Status != StatusTarefa.Done)
                    .OrderBy(t => t.Prazo ?? DateTime.MaxValue)
                    .ThenBy(t => t.Prioridade)
                    .ToList();

                Console.WriteLine();
                if (tarefas.Count == 0)
                {
                    Console.WriteLine($"Nenhuma tarefa pendente para '{nome}'.");
                    Pausar();
                    continue;
                }

                Console.WriteLine(string.Format("{0,-4} {1,-20} {2,-26} {3,-10} {4,-6} {5,-13} {6}",
                    "ID", "Projeto", "Tarefa", "Prioridade", "Prazo", "Status", "Situação"));
                Console.WriteLine(new string('-', 100));

                foreach (var t in tarefas)
                {
                    var projeto = _service.ObterProjeto(t.ProjetoId)?.Nome ?? "?";
                    Console.ForegroundColor = Atrasada(t) ? ConsoleColor.Red : ConsoleColor.Gray;
                    Console.WriteLine(string.Format("{0,-4} {1,-20} {2,-26} {3,-10} {4,-6} {5,-13} {6}",
                        t.Id, Truncar(projeto, 20), Truncar(t.Titulo, 26), Prioridade(t.Prioridade),
                        Prazo(t.Prazo), Status(t.Status), Situacao(_service, t)));
                    Console.ResetColor();
                }

                var atrasadas = tarefas.Count(Atrasada);
                Console.ForegroundColor = atrasadas > 0 ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"\nTotal: {tarefas.Count} tarefa(s) | Atrasadas: {atrasadas}");
                Console.ResetColor();

                Pausar();
            }
        }
    }
}
