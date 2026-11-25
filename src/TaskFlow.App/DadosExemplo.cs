using System;
using System.Linq;
using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.App
{
    /// <summary>
    /// Dados de demonstração, criados pelo próprio ITaskFlowService (as mesmas regras de validação valem aqui).
    /// Os mesmos exemplos que existiam no antigo MockDataStore, agora com dependências reais.
    /// Só é chamado quando o banco está vazio (primeiro uso). Para recomeçar do zero,
    /// apague o arquivo taskflow.db que fica ao lado do executável.
    /// </summary>
    public static class DadosExemplo
    {
        // Banco recém-criado: nenhum projeto, nenhuma conta e salário ainda não definido.
        public static bool BancoVazio(ITaskFlowService service)
            => service.ListarProjetos().Count == 0
               && service.ListarContas().Count == 0
               && service.ObterSalario() == 0m;

        public static void Carregar(ITaskFlowService service)
        {
            var hoje = DateTime.Today;

            service.DefinirSalario(5000m);

            // ---------- Projetos e tarefas ----------
            var taskflow = NovoProjeto(service, "TaskFlow - Sistema de Gestão");
            var configurarDb = NovaTarefa(service, taskflow, "Configurar DB", "Ana", 1, hoje.AddDays(-2));
            var criarGrafo = NovaTarefa(service, taskflow, "Criar Grafo de Dependências", "Bruno", 2, hoje.AddDays(3));
            var telaLogin = NovaTarefa(service, taskflow, "Tela de Login", "Ana", 3, hoje.AddDays(6));
            var kanban = NovaTarefa(service, taskflow, "Kanban View", "Bruno", 4, hoje.AddDays(-5));
            var financeiro = NovaTarefa(service, taskflow, "Painel Financeiro", "Ana", 5, hoje.AddDays(10));

            var aed = NovoProjeto(service, "Estudo AED2 - Estruturas de Dados");
            var revisarGrafos = NovaTarefa(service, aed, "Revisar grafos e DFS", "Bruno", 2, hoje.AddDays(4));
            var exercicios = NovaTarefa(service, aed, "Lista de exercícios", "Ana", 3, hoje.AddDays(8));

            var pessoal = NovoProjeto(service, "Projeto Pessoal - Financeiro");
            var organizar = NovaTarefa(service, pessoal, "Organizar contas do mês", "Carla", 2, hoje.AddDays(1));

            // Status antes das dependências: assim nenhuma regra de "predecessora pendente" é violada.
            service.MudarStatusTarefa(configurarDb, StatusTarefa.InProgress);
            service.MudarStatusTarefa(kanban, StatusTarefa.Done);
            service.MudarStatusTarefa(organizar, StatusTarefa.Review);

            // ---------- Dependências ("A depende de B") ----------
            service.AdicionarDependencia(telaLogin, configurarDb);   // travada: #1 ainda em andamento
            service.AdicionarDependencia(financeiro, criarGrafo);    // travada: #2 a fazer
            service.AdicionarDependencia(financeiro, kanban);        // essa já está concluída
            service.AdicionarDependencia(exercicios, revisarGrafos); // travada

            // ---------- Contas ----------
            service.CriarConta("Aluguel", 1500m, hoje.AddDays(-3), TipoFinanceiro.Despesa);
            service.PagarConta(service.ListarContas().Last().Id);
            service.CriarConta("Luz", 200m, hoje.AddDays(2), TipoFinanceiro.Despesa);
            service.CriarConta("Internet", 100m, hoje.AddDays(5), TipoFinanceiro.Despesa);
            service.CriarConta("Marketplace", 400m, hoje.AddDays(-8), TipoFinanceiro.Despesa); // vencida = atrasada

            // Carga inicial não é ação do usuário: não deve aparecer no histórico nem ser desfeita.
            service.LimparHistorico();
        }

        // O Resultado não traz o ID criado; como os IDs só crescem, o último da lista é o novo.
        private static int NovoProjeto(ITaskFlowService service, string nome)
        {
            service.CriarProjeto(nome);
            return service.ListarProjetos().Last().Id;
        }

        private static int NovaTarefa(ITaskFlowService service, int projetoId, string titulo, string responsavel, int prioridade, DateTime prazo)
        {
            service.CriarTarefa(projetoId, titulo, responsavel: responsavel, prioridade: prioridade, prazo: prazo);
            return service.ListarTarefas().Last().Id;
        }
    }
}
