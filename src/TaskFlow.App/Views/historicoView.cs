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
                        Console.WriteLine($"  • {acao.Descricao}");
                }

                // A "última ação" é a que o desfazer vai reverter: mostrar isso evita surpresa.
                var ultima = _service.ObterUltimaAcao();
                if (ultima != null)
                    Console.WriteLine($"\n[D] Desfazer \"{ultima.Descricao}\" | [V] Voltar");
                else
                    Console.WriteLine("\n[V] Voltar");

                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "D":
                        MostrarResultado(_service.DesfazerUltimaAcao());
                        break;
                    case "V":
                    case "":
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