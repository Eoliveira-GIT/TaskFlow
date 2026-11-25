using System;
using System.Linq;
using TaskFlow.Application;
using static TaskFlow.App.Views.Formatacao;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    public class DashboardView
    {
        private readonly ITaskFlowService _service;

        public DashboardView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            LimparTela();
            Titulo("DASHBOARD GERAL - VISÃO UNIFICADA", ConsoleColor.Cyan);

            // ---------- Financeiro ----------
            Secao("💰 RESUMO FINANCEIRO", ConsoleColor.Green);
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Salário:", Moeda(_service.ObterSalario())));
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Total de contas:", Moeda(_service.ObterTotalContas())));
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "A pagar:", Moeda(_service.ObterTotalAPagar())));
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Saldo após quitar tudo:", Moeda(_service.ObterSaldoAposQuitarTudo())));
            var contasAtrasadas = _service.ObterContasAtrasadas().Count;
            Console.WriteLine(string.Format("  {0,-24} {1,-18}", "Status:",
                contasAtrasadas > 0 ? $"⚠️ {contasAtrasadas} conta(s) atrasada(s)" : "✅ Em dia"));

            // ---------- Tarefas urgentes (topo da fila de prioridade) ----------
            Secao("⚡ TAREFAS URGENTES (TOPO DA FILA)", ConsoleColor.Magenta);
            Console.WriteLine(string.Format("  {0,-4} {1,-26} {2,-10} {3,-12} {4,-13} {5,-6} {6}",
                "#", "Tarefa", "Prioridade", "Responsável", "Status", "Prazo", "Situação"));
            Console.WriteLine("  " + new string('-', 96));

            var urgentes = _service.ObterFilaPrioridade().Take(5).ToList();
            if (urgentes.Count == 0)
                Console.WriteLine("  (nenhuma tarefa pendente)");

            foreach (var t in urgentes)
            {
                Console.ForegroundColor = Atrasada(t) ? ConsoleColor.Red : ConsoleColor.Gray;
                Console.WriteLine(string.Format("  {0,-4} {1,-26} {2,-10} {3,-12} {4,-13} {5,-6} {6}",
                    t.Id, Truncar(t.Titulo, 26), Prioridade(t.Prioridade), Truncar(Responsavel(t), 12),
                    Status(t.Status), Prazo(t.Prazo), Situacao(_service, t)));
                Console.ResetColor();
            }

            // ---------- Atrasadas por responsável ----------
            Secao("⚠️ TAREFAS ATRASADAS POR RESPONSÁVEL", ConsoleColor.Red);
            var porPessoa = _service.ListarTarefas()
                .Where(Atrasada)
                .GroupBy(Responsavel)
                .Select(g => (Responsavel: g.Key, Total: g.Count()))
                .OrderByDescending(x => x.Total)
                .ToList();

            if (porPessoa.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ✅ Nenhuma tarefa atrasada.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(string.Format("  {0,-24} {1,-10}", "Responsável", "Atrasadas"));
                Console.WriteLine("  " + new string('-', 34));
                foreach (var (responsavel, total) in porPessoa)
                    Console.WriteLine(string.Format("  {0,-24} {1,-10}", responsavel, total));
            }

            // ---------- Travadas por dependência ----------
            var travadas = _service.ListarTarefas()
                .Where(t => _service.ObterPredecessorasPendentes(t.Id).Count > 0)
                .Count();
            Console.WriteLine($"\nTarefas travadas por dependência: {travadas} (detalhes em [4] Grafo de Dependências)");

            Pausar();
        }
    }
}
