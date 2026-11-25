using System;
using TaskFlow.Application;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    public class HistoricoView
    {
        private readonly ITaskFlowService _service;

        public HistoricoView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            bool voltar = false;

            while (!voltar)
            {
                try { Console.Clear(); } catch { }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(">>> HISTÓRICO DE AÇÕES <<<\n");
                Console.ResetColor();

                var historico = _service.ListarHistorico();
                if (historico.Count == 0)
                {
                    Console.WriteLine("Nenhuma ação registrada ainda.");
                }
                else
                {
                    foreach (var acao in historico)
                    {
                        // Ações vindas do banco não guardam como ser desfeitas.
                        var sessaoAnterior = acao.Desfazer == null;
                        Console.ForegroundColor = sessaoAnterior ? ConsoleColor.DarkGray : ConsoleColor.Gray;
                        Console.WriteLine($"  • {acao.DataHora:dd/MM HH:mm}  {acao.Descricao}{(sessaoAnterior ? "  (sessão anterior)" : string.Empty)}");
                        Console.ResetColor();
                    }
                }

                // A "última ação" é a que o desfazer vai reverter: mostrar isso evita surpresa.
                var ultima = _service.ObterUltimaAcao();
                if (ultima != null && ultima.Desfazer != null)
                {
                    Console.WriteLine($"\n[D] Desfazer \"{ultima.Descricao}\" | [V] Voltar");
                }
                else
                {
                    if (ultima != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("\nSó dá para desfazer ações feitas desde que o programa foi aberto.");
                        Console.ResetColor();
                    }
                    Console.WriteLine("\n[V] Voltar");
                }

                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "D":
                        MostrarResultado(_service.DesfazerUltimaAcao());
                        break;
                    case "V":
                    case "":
                    case null: // fim da entrada (Ctrl+Z ou entrada redirecionada)
                        voltar = true;
                        break;
                    default:
                        Console.WriteLine("Comando inválido.");
                        Pausar();
                        break;
                }
            }
        }
    }
}