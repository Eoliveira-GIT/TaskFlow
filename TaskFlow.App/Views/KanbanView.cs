using System;

namespace TaskFlow.App.Views
{
    public static class KanbanView
    {
        public static void Renderizar()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(">>> QUADRO KANBAN (PROJETO SELECIONADO) <<<\n");
            Console.ResetColor();

            Console.WriteLine(string.Format("{0,-20} | {1,-20} | {2,-20}", "[ A FAZER ]", "[ EM ANDAMENTO ]", "[ CONCLUÍDO ]"));
            Console.WriteLine(new string('-', 66));

            Console.WriteLine(string.Format("{0,-20} | {1,-20} | {2,-20}", "#1 Config DB", "#2 Criar Grafo", "#0 Setup Solution"));
            Console.WriteLine(string.Format("{0,-20} | {1,-20} | {2,-20}", "#3 Tela Login*", "", ""));

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n* Tarefa #3 possui dependências bloqueantes (ver no Grafo).");
            Console.ResetColor();

            Console.WriteLine("\nAções: [N] Nova Tarefa | [M] Mover Status | [V] Voltar");
            Console.Write("Comando: ");
            Console.ReadLine();
        }
    }
}
