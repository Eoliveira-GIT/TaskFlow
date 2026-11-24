using System;
using TaskFlow.App.Services;
using TaskFlow.App.MockServices;
using TaskFlow.App.Views;

namespace TaskFlow.App.Navigation
{
    public class MenuNavigator
    {
        private bool _emExecucao = true;
        private readonly ITaskFlowService _service;

        public MenuNavigator()
        {
            _service = new TaskFlowMockService();
            MockDataStore.Inicializar();
        }

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
            Console.WriteLine(" [6] ↩️  Histórico e Desfazer Ação (Undo)");
            Console.WriteLine(" [7] 👤 Minhas Tarefas (por responsável)");
            Console.WriteLine(" [0] 🚪 Sair");
        }

        private void ProcessarOpcao(string? opcao)
        {
            try { Console.Clear(); } catch { }
            switch (opcao)
            {
                case "1":
                    _service.ObterDashboardResumo();
                    break;
                case "2":
                    _service.ListarProjetos();
                    break;
                case "3":
                    _service.ExibirTarefasProntas();
                    Pausar();
                    break;
                case "4":
                    GrafoDependenciasView.Renderizar();
                    break;
                case "5":
                    _service.ExibirPainelFinanceiro();
                    break;
                case "6":
                    _service.DesfazerUltimaAcao();
                    Pausar();
                    break;
                case "7":
                    MinhasTarefasView.Renderizar();
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
