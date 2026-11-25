using System;
using System.Linq;
using TaskFlow.App.MockServices;
using TaskFlow.App.Navigation;

namespace TaskFlow.App.Views
{
    public static class ProjetosView
    {
        public static void Renderizar()
        {
            MockDataStore.Inicializar();
            bool voltar = false;

            while (!voltar)
            {
                try { Console.Clear(); } catch { }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(">>> PROJETOS CADASTRADOS <<<\n");
                Console.ResetColor();

                Console.WriteLine(string.Format("  {0,-4} {1,-32} {2,-8} {3,-24} {4,-6}", "ID", "Projeto", "Tarefas", "Responsáveis", "Atras."));
                Console.WriteLine(new string('-', 80));
                foreach (var p in MockDataStore.Projetos)
                {
                    var tarefasDoProjeto = MockDataStore.Tarefas.Where(t => t.ProjetoId == p.Id).ToList();
                    var qtd = tarefasDoProjeto.Count;
                    var atrasadas = tarefasDoProjeto.Count(MockDataStore.TarefaAtrasada);
                    var responsaveis = string.Join(", ", tarefasDoProjeto
                        .Where(t => !string.IsNullOrWhiteSpace(t.Responsavel))
                        .Select(t => t.Responsavel!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(r => r));
                    if (string.IsNullOrWhiteSpace(responsaveis))
                        responsaveis = "-";

                    Console.ForegroundColor = atrasadas > 0 ? ConsoleColor.Red : ConsoleColor.Gray;
                    Console.WriteLine(string.Format("  {0,-4} {1,-32} {2,-8} {3,-24} {4,-6}",
                        p.Id, Truncar(p.Nome, 32), qtd, Truncar(responsaveis, 24), atrasadas));
                    Console.ResetColor();
                }

                Console.WriteLine("\n[N] Novo Projeto | [A] Abrir Projeto | [E] Editar Projeto | [R] Remover Projeto | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "N":
                        CriarProjeto();
                        break;
                    case "A":
                        AbrirProjeto();
                        break;
                    case "E":
                        EditarProjeto();
                        break;
                    case "R":
                        RemoverProjeto();
                        break;
                    case "V":
                    case "":
                        voltar = true;
                        break;
                    default:
                        Console.WriteLine("Comando inválido.");
                        Pausar();
                        break;
                }
            }
        }

        private static void CriarProjeto()
        {
            var nome = EntradaUsuario.LerTextoObrigatorio("Nome do novo projeto: ");

            var projeto = MockDataStore.AdicionarProjeto(nome);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Projeto #{projeto.Id} '{projeto.Nome}' criado!");
            Console.ResetColor();
            Pausar();
        }

        private static void EditarProjeto()
        {
            var id = EntradaUsuario.LerInteiro("ID do projeto (Enter para cancelar): ");
            if (id == 0)
                return;

            var projeto = MockDataStore.ObterProjeto(id);
            if (projeto == null)
            {
                EntradaUsuario.Erro("Projeto não encontrado.");
                Pausar();
                return;
            }

            var nome = EntradaUsuario.LerTextoOpcional($"Novo nome [{projeto.Nome}]: ", projeto.Nome);

            MockDataStore.EditarProjeto(id, nome);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Projeto #{id} atualizado para '{nome}'.");
            Console.ResetColor();
            Pausar();
        }

        private static void RemoverProjeto()
        {
            var id = EntradaUsuario.LerInteiro("ID do projeto (Enter para cancelar): ");
            if (id == 0)
                return;

            if (!MockDataStore.RemoverProjeto(id))
            {
                EntradaUsuario.Erro("Projeto não encontrado.");
                Pausar();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Projeto #{id} removido com suas tarefas (use [6] para desfazer).");
            Console.ResetColor();
            Pausar();
        }

        private static void AbrirProjeto()
        {
            var id = EntradaUsuario.LerInteiro("ID do projeto (Enter para cancelar): ");
            if (id == 0)
                return;

            if (MockDataStore.ObterProjeto(id) == null)
            {
                EntradaUsuario.Erro("Projeto não encontrado.");
                Pausar();
                return;
            }

            KanbanView.Renderizar(id);
        }

        private static string Truncar(string texto, int max)
        {
            if (texto.Length <= max)
                return texto;
            return texto.Substring(0, max - 1) + "…";
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }
    }
}
