using System;

namespace TaskFlow.App.Views
{
    public static class GrafoDependenciasView
    {
        public static void Renderizar()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(">>> MAPA DE DEPENDÊNCIAS (GRAFO) <<<\n");
            Console.ResetColor();

            Console.WriteLine("Tarefas e suas predecessoras (travamentos):");
            Console.WriteLine();
            Console.WriteLine("  Tarefa #1 (Config DB)       → Sem predecessoras");
            Console.WriteLine("  Tarefa #2 (Criar Grafo)     → Sem predecessoras");
            Console.WriteLine("  Tarefa #3 (Tela Login)      → Depende de #1");
            Console.WriteLine("  Tarefa #4 (Kanban View)     → Depende de #2 e #3");
            Console.WriteLine("  Tarefa #5 (Financeiro)      → Depende de #2");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nLegenda:");
            Console.WriteLine("  ✅ Concluída   ⏳ Em andamento   🔒 Bloqueada");
            Console.ResetColor();

            Console.WriteLine("\n[V] Voltar");
            Console.Write("Comando: ");
            Console.ReadLine();
        }
    }
}
