using System;

namespace TaskFlow.App.Views
{
    public static class FinanceiroView
    {
        public static void Renderizar()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> PAINEL FINANCEIRO PESSOAL <<<\n");
            Console.ResetColor();

            Console.WriteLine(string.Format("{0,-15} | {1,-15} | {2,-12} | {3,-10}", "Descrição", "Valor", "Status", "Data"));
            Console.WriteLine(new string('-', 58));
            Console.WriteLine(string.Format("{0,-15} | {1,-15} | {2,-12} | {3,-10}", "Aluguel", "R$ 1500,00", "Pago", "20/09"));
            Console.WriteLine(string.Format("{0,-15} | {1,-15} | {2,-12} | {3,-10}", "Luz", "R$ 200,00", "Pendente", "25/09"));
            Console.WriteLine(string.Format("{0,-15} | {1,-15} | {2,-12} | {3,-10}", "Internet", "R$ 100,00", "Pendente", "28/09"));
            Console.WriteLine(string.Format("{0,-15} | {1,-15} | {2,-12} | {3,-10}", "Marketplace", "R$ 400,00", "Atrasada", "15/09"));

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSaldo restante: R$ 2.800,00");
            Console.ResetColor();

            Console.WriteLine("\n[A] Pagar | [V] Voltar");
            Console.Write("Comando: ");
            Console.ReadLine();
        }
    }
}
