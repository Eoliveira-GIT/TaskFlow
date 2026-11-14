using System;
using TaskFlow.App.Views;

namespace TaskFlow.App.Navigation
{
    public class MenuNavigator
    {
        private bool _emExecucao = true;

        public void Iniciar()
        {
            while (_emExecucao)
            {
                try { Console.Clear(); } catch { }
                ExibirCabecalho();
                ExibirMenuOpcoes();

                Console.Write("\nEscolha uma opção: ");
                string? opcao = Console.ReadLine();

                ProcessarOpcao(opcao);
            }
        }

        private void ExibirCabecalho()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("        TASKFLOW - GESTÃO & ESTRUTURAS           ");
            Console.WriteLine("=================================================");
            Console.ResetColor();
        }

        private void ExibirMenuOpcoes()
        {
            Console.WriteLine(" [1] 📊 Dashboard Geral");
            Console.WriteLine(" [2] 📁 Projetos & Quadro Kanban");
            Console.WriteLine(" [3] ⚡ O que Fazer Agora? (Fila de Prioridade)");
            Console.WriteLine(" [4] 🔗 Grafo de Dependências");
            Console.WriteLine(" [5] 💰 Painel Financeiro Pessoal");
            Console.WriteLine(" [6] ↩️  Desfazer Última Ação (Undo)");
            Console.WriteLine(" [0] 🚪 Sair");
        }

        private void ProcessarOpcao(string? opcao)
        {
            try { Console.Clear(); } catch { }
            switch (opcao)
            {
                case "1":
                    DashboardView.Renderizar();
                    break;
                case "2":
                    KanbanView.Renderizar();
                    break;
                case "3":
                    Console.WriteLine("--- TAREFAS PRONTAS (FILA DE PRIORIDADE) ---");
                    Pausar();
                    break;
                case "4":
                    Console.WriteLine("--- MAPA DE DEPENDÊNCIAS (GRAFO) ---");
                    GrafoDependenciasView.Renderizar();
                    Pausar();
                    break;
                case "5":
                    FinanceiroView.Renderizar();
                    break;
                case "6":
                    Console.WriteLine("Desfazendo última ação via Pilha de Undo...");
                    Pausar();
                    break;
                case "0":
                    _emExecucao = false;
                    Console.WriteLine("Encerrando aplicação...");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Opção inválida! Tente novamente.");
                    Console.ResetColor();
                    Pausar();
                    break;
            }
        }

        private void Pausar()
        {
            Console.WriteLine("\nPressione Enter para voltar ao menu...");
            Console.ReadLine();
        }
    }
}
