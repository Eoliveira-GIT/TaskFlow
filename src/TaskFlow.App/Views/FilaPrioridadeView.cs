using System;
using System.Linq;
using TaskFlow.Application;
using TaskFlow.Domain;
using static TaskFlow.App.Views.Formatacao;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    /// <summary>"O que fazer agora?": fila de prioridade do Domain + regra das dependências.</summary>
    public class FilaPrioridadeView
    {
        private readonly ITaskFlowService _service;

        public FilaPrioridadeView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            LimparTela();
            Titulo("O QUE FAZER AGORA? (FILA DE PRIORIDADE)", ConsoleColor.Magenta);
            Console.WriteLine("Ordem: prioridade (1 = urgente) e, no empate, o prazo mais próximo.\n");

            var fila = _service.ObterFilaPrioridade();
            if (fila.Count == 0)
            {
                Console.WriteLine("  (nenhuma tarefa pendente)");
                Pausar();
                return;
            }

            Console.WriteLine(string.Format("  {0,-3} {1,-4} {2,-26} {3,-18} {4,-10} {5,-6} {6,-13} {7}",
                "Nº", "#", "Tarefa", "Projeto", "Prioridade", "Prazo", "Status", "Situação"));
            Console.WriteLine("  " + new string('-', 106));

            int posicao = 1;
            foreach (var t in fila)
            {
                var projeto = _service.ObterProjeto(t.ProjetoId)?.Nome ?? "?";
                Console.ForegroundColor = CorLinha(t);
                Console.WriteLine(string.Format("  {0,-3} {1,-4} {2,-26} {3,-18} {4,-10} {5,-6} {6,-13} {7}",
                    posicao++, t.Id, Truncar(t.Titulo, 26), Truncar(projeto, 18), Prioridade(t.Prioridade),
                    Prazo(t.Prazo), Status(t.Status), Situacao(_service, t)));
                Console.ResetColor();
            }

            // A sugestão é a primeira da fila que pode mesmo ser trabalhada agora.
            var proxima = fila.FirstOrDefault(PodeTrabalhar);
            Console.WriteLine();
            if (proxima != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"➜ Sugestão: #{proxima.Id} {proxima.Titulo} ({Prioridade(proxima.Prioridade)}, prazo {Prazo(proxima.Prazo)})");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Nenhuma tarefa liberada: todas aguardam dependências ou estão bloqueadas.");
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nTarefas que aguardam dependências ficam na fila, mas não são sugeridas até as predecessoras serem concluídas.");
            Console.ResetColor();

            Pausar();
        }

        private bool PodeTrabalhar(Tarefa t)
            => t.Status != StatusTarefa.Bloqueada && _service.ObterPredecessorasPendentes(t.Id).Count == 0;

        private ConsoleColor CorLinha(Tarefa t)
        {
            if (!PodeTrabalhar(t)) return ConsoleColor.DarkGray;
            if (Atrasada(t)) return ConsoleColor.Red;
            return ConsoleColor.Gray;
        }
    }
}
