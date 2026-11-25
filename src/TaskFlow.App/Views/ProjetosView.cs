using System;
using System.Linq;
using TaskFlow.App.Navigation;
using TaskFlow.Application;
using TaskFlow.Domain;
using static TaskFlow.App.Views.Formatacao;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    public class ProjetosView
    {
        private readonly ITaskFlowService _service;

        public ProjetosView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            bool voltar = false;

            while (!voltar)
            {
                LimparTela();
                Titulo("PROJETOS CADASTRADOS", ConsoleColor.Cyan);

                Console.WriteLine(string.Format("  {0,-4} {1,-30} {2,-7} {3,-7} {4,-6} {5}",
                    "ID", "Projeto", "Tarefas", "Concl.", "Atras.", "Responsáveis"));
                Console.WriteLine("  " + new string('-', 84));

                var projetos = _service.ListarProjetos();
                if (projetos.Count == 0)
                    Console.WriteLine("  (nenhum projeto cadastrado)");

                foreach (var p in projetos)
                {
                    var tarefas = _service.ListarTarefas(p.Id);
                    var concluidas = tarefas.Count(t => t.Status == StatusTarefa.Done);
                    var atrasadas = tarefas.Count(Atrasada);
                    var responsaveis = string.Join(", ", tarefas
                        .Where(t => !string.IsNullOrWhiteSpace(t.Responsavel))
                        .Select(t => t.Responsavel!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(r => r));

                    Console.ForegroundColor = atrasadas > 0 ? ConsoleColor.Red : ConsoleColor.Gray;
                    Console.WriteLine(string.Format("  {0,-4} {1,-30} {2,-7} {3,-7} {4,-6} {5}",
                        p.Id, Truncar(p.Nome, 30), tarefas.Count, concluidas, atrasadas,
                        Truncar(responsaveis.Length == 0 ? "-" : responsaveis, 28)));
                    Console.ResetColor();
                }

                Console.WriteLine("\n[N] Novo | [A] Abrir (Kanban) | [E] Editar | [R] Remover | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "N": CriarProjeto(); break;
                    case "A": AbrirProjeto(); break;
                    case "E": EditarProjeto(); break;
                    case "R": RemoverProjeto(); break;
                    case "V":
                    case "":
                    case null: // fim da entrada (Ctrl+Z ou entrada redirecionada)
                        voltar = true;
                        break;
                    default:
                        Console.WriteLine("Comando inválido.");
                        Pausar();
                        break;
                }
            }
        }

        private void CriarProjeto()
        {
            var nome = EntradaUsuario.LerTextoObrigatorio("Nome do novo projeto: ");
            var descricao = EntradaUsuario.LerTextoOpcional("Descrição (Enter = sem descrição): ", string.Empty);

            MostrarResultado(_service.CriarProjeto(nome, descricao));
        }

        private void EditarProjeto()
        {
            var projeto = LerProjeto();
            if (projeto == null)
                return;

            var nome = EntradaUsuario.LerTextoOpcional($"Nome [{projeto.Nome}]: ", projeto.Nome);
            var descricao = EntradaUsuario.LerTextoOpcionalOuLimpar(
                $"Descrição [{projeto.Descricao ?? "-"}] (Enter mantém, '-' apaga): ", projeto.Descricao);

            MostrarResultado(_service.EditarProjeto(projeto.Id, nome, descricao));
        }

        private void RemoverProjeto()
        {
            var projeto = LerProjeto();
            if (projeto == null)
                return;

            var qtd = _service.ListarTarefas(projeto.Id).Count;
            if (!EntradaUsuario.Confirmar($"Remover '{projeto.Nome}' e suas {qtd} tarefa(s)? (s/n): "))
                return;

            MostrarResultado(_service.RemoverProjeto(projeto.Id));
        }

        private void AbrirProjeto()
        {
            var projeto = LerProjeto();
            if (projeto == null)
                return;

            new KanbanView(_service).Renderizar(projeto.Id);
        }

        // Pede o ID e devolve o projeto, ou null se o usuário cancelou ou o ID não existe.
        private Projeto? LerProjeto()
        {
            var id = EntradaUsuario.LerInteiro("ID do projeto (Enter para cancelar): ");
            if (id == 0)
                return null;

            var projeto = _service.ObterProjeto(id);
            if (projeto == null)
            {
                EntradaUsuario.Erro("Projeto não encontrado.");
                Pausar();
            }
            return projeto;
        }
    }
}
