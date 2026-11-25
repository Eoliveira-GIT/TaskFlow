using System;
using System.Linq;
using TaskFlow.Application;
using TaskFlow.App.Views;
using TaskFlow.Domain;

namespace TaskFlow.App.Navigation
{
    public class MenuNavigator
    {
        private bool _emExecucao = true;

        // Uma única instância do serviço para o programa inteiro:
        // todas as telas enxergam os mesmos dados e o mesmo histórico de undo.
        private readonly ITaskFlowService _service;

        public MenuNavigator()
        {
            // Abre (ou cria) o taskflow.db ao lado do executável e carrega o que já foi salvo.
            _service = new TaskFlowService();

            // Exemplos só no primeiro uso; depois disso o banco já tem os dados do usuário.
            if (DadosExemplo.BancoVazio(_service))
                DadosExemplo.Carregar(_service);
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
                if (opcao == null) // fim da entrada (Ctrl+Z ou entrada redirecionada): encerra
                {
                    _emExecucao = false;
                    break;
                }

                try
                {
                    ProcessarOpcao(opcao?.Trim());
                }
                catch (Exception ex)
                {
                    // Um erro inesperado (ex.: banco travado por outro programa) não derruba o sistema.
                    EntradaUsuario.Erro($"\nErro inesperado: {ex.Message}");
                    Saida.Pausar();
                }
            }
        }

        private void ExibirCabecalho()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("        TASKFLOW - GESTÃO & ESTRUTURAS           ");
            Console.WriteLine("=================================================");
            Console.ResetColor();

            // Estado do sistema em uma linha.
            var tarefas = _service.ListarTarefas();
            var pendentes = tarefas.Count(t => t.Status != StatusTarefa.Done);
            var atrasadas = tarefas.Count(Formatacao.Atrasada);
            var travadas = tarefas.Count(t => t.Status != StatusTarefa.Done && _service.ObterPredecessorasPendentes(t.Id).Count > 0);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" {_service.ListarProjetos().Count} projeto(s) | {pendentes} tarefa(s) pendente(s) | " +
                              $"{atrasadas} atrasada(s) | {travadas} travada(s) | {_service.ListarHistorico().Count} ação(ões) no histórico");
            Console.ResetColor();
            Console.WriteLine();
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
                    new DashboardView(_service).Renderizar();
                    break;
                case "2":
                    new ProjetosView(_service).Renderizar();
                    break;
                case "3":
                    new FilaPrioridadeView(_service).Renderizar();
                    break;
                case "4":
                    new GrafoDependenciasView(_service).Renderizar();
                    break;
                case "5":
                    new FinanceiroView(_service).Renderizar();
                    break;
                case "6":
                    new HistoricoView(_service).Renderizar();
                    break;
                case "7":
                    new MinhasTarefasView(_service).Renderizar();
                    break;
                case "0":
                    _emExecucao = false;
                    Console.WriteLine("Encerrando aplicação...");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Opção inválida! Tente novamente.");
                    Console.ResetColor();
                    Saida.Pausar();
                    break;
            }
        }
    }
}
