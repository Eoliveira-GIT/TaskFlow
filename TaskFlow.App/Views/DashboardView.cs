using System;

namespace TaskFlow.App.Views
{
    public static class DashboardView
    {
        public static void Renderizar()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(">>> DASHBOARD GERAL - VISÃO UNIFICADA <<<\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=== 💰 RESUMO FINANCEIRO ===");
            Console.ResetColor();
            Console.WriteLine("  Salário estimado:    R$ 5.000,00");
            Console.WriteLine("  Contas a pagar:      R$ 3.200,00");
            Console.WriteLine("  Saldo disponível:    R$ 1.800,00");
            Console.WriteLine("  Status:              ✅ Em dia");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n=== ⚡ TAREFAS URGENTES (TOPO) ===");
            Console.ResetColor();
            Console.WriteLine("  [!] #2 Criar Grafo de Dependências - ALTA PRIORIDADE");
            Console.WriteLine("  [!] #1 Configurar DB - PRONTO");
            Console.WriteLine("  [ ] #3 Tela Login - BLOQUEADA (depende de #1)");

            Console.WriteLine("\nPressione Enter para voltar...");
            Console.ReadLine();
        }
    }
}
