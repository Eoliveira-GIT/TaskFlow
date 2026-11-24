using System;
using System.Collections.Generic;
using System.Linq;
using TaskFlow.App.MockServices;
using TaskFlow.App.Navigation;

namespace TaskFlow.App.Views
{
    public static class KanbanView
    {
        private const int Largura = 24;

        public static void Renderizar(int projetoId)
        {
            MockDataStore.Inicializar();
            var projeto = MockDataStore.ObterProjeto(projetoId);
            if (projeto == null)
            {
                Console.WriteLine("Projeto não encontrado.");
                Console.ReadLine();
                return;
            }

            string? filtroResponsavel = null;
            PrioridadeMock? filtroPrioridade = null;
            string? filtroTexto = null;
            bool voltar = false;

            while (!voltar)
            {
                try { Console.Clear(); } catch { }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($">>> QUADRO KANBAN - {projeto.Nome} <<<\n");
                Console.ResetColor();

                var filtros = new List<string>();
                if (filtroResponsavel != null) filtros.Add($"responsável={filtroResponsavel}");
                if (filtroPrioridade != null) filtros.Add($"prioridade={MockDataStore.RotuloPrioridade(filtroPrioridade.Value)}");
                if (filtroTexto != null) filtros.Add($"busca=\"{filtroTexto}\"");
                if (filtros.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Filtro ativo: " + string.Join(", ", filtros));
                    Console.ResetColor();
                }

                var tarefasProjeto = MockDataStore.Tarefas.Where(t => t.ProjetoId == projetoId);
                if (filtroResponsavel != null)
                    tarefasProjeto = tarefasProjeto.Where(t => string.Equals(t.Responsavel, filtroResponsavel, StringComparison.OrdinalIgnoreCase));
                if (filtroPrioridade != null)
                    tarefasProjeto = tarefasProjeto.Where(t => t.Prioridade == filtroPrioridade.Value);
                if (filtroTexto != null)
                {
                    tarefasProjeto = tarefasProjeto.Where(t =>
                        t.Titulo.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase) ||
                        (t.Responsavel != null && t.Responsavel.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase)));
                }

                ImprimirColunas(tarefasProjeto.ToList());

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nCores: verde=concluída  amarelo=em andamento  magenta=bloqueada  vermelho=atrasada (!)");
                Console.ResetColor();

                Console.WriteLine("\n[N] Nova Tarefa | [M] Mover Status | [E] Editar | [R] Remover | [F] Filtrar | [B] Buscar | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "N":
                        NovaTarefa(projetoId);
                        break;
                    case "M":
                        MoverStatus();
                        break;
                    case "E":
                        EditarTarefa();
                        break;
                    case "R":
                        RemoverTarefa();
                        break;
                    case "F":
                        Console.Write("Filtrar por responsável (Enter = todos): ");
                        var fr = Console.ReadLine()?.Trim();
                        filtroResponsavel = string.IsNullOrWhiteSpace(fr) ? null : fr;

                        Console.Write("Filtrar por prioridade [1] Baixa [2] Média [3] Alta (Enter = todas): ");
                        var fp = Console.ReadLine()?.Trim();
                        filtroPrioridade = fp == "1" ? PrioridadeMock.Baixa
                                         : fp == "2" ? PrioridadeMock.Media
                                         : fp == "3" ? PrioridadeMock.Alta
                                         : (PrioridadeMock?)null;
                        break;
                    case "B":
                        Console.Write("Buscar por texto no título/responsável (Enter limpa): ");
                        var ft = Console.ReadLine()?.Trim();
                        filtroTexto = string.IsNullOrWhiteSpace(ft) ? null : ft;
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

        private static void ImprimirColunas(List<TarefaMock> tarefas)
        {
            var colunas = new[]
            {
                StatusTarefaMock.AFazer,
                StatusTarefaMock.EmAndamento,
                StatusTarefaMock.Concluida,
                StatusTarefaMock.Bloqueada
            };

            Console.WriteLine(string.Join(" | ", new[]
            {
                "[ A FAZER ]".PadRight(Largura),
                "[ EM ANDAMENTO ]".PadRight(Largura),
                "[ CONCLUÍDA ]".PadRight(Largura),
                "[ BLOQUEADA ]".PadRight(Largura)
            }));
            Console.WriteLine(new string('-', Largura * 4 + 9));

            var porStatus = colunas.Select(s => tarefas.Where(t => t.Status == s).ToList()).ToList();
            var maxLinhas = porStatus.Max(l => l.Count);
            if (maxLinhas == 0)
                maxLinhas = 1;

            for (int i = 0; i < maxLinhas; i++)
            {
                var celulas = porStatus.Select(l => i < l.Count ? l[i] : null).ToArray();
                EscreverLinha(celulas);
            }
        }

        private static void EscreverLinha(TarefaMock?[] celulas)
        {
            for (int i = 0; i < celulas.Length; i++)
            {
                var c = celulas[i];
                if (c == null)
                {
                    Console.ResetColor();
                    Console.Write(new string(' ', Largura));
                }
                else
                {
                    Console.ForegroundColor = CorTarefa(c);
                    Console.Write(Truncar(Rotulo(c)).PadRight(Largura));
                    Console.ResetColor();
                }

                if (i < celulas.Length - 1)
                {
                    Console.ResetColor();
                    Console.Write(" | ");
                }
            }
            Console.WriteLine();
        }

        private static string Rotulo(TarefaMock t)
        {
            var s = $"#{t.Id} {t.Titulo}";
            if (!string.IsNullOrWhiteSpace(t.Responsavel))
                s += $" ({t.Responsavel})";
            if (MockDataStore.TarefaAtrasada(t))
                s += " !";
            return s;
        }

        private static ConsoleColor CorTarefa(TarefaMock t)
        {
            if (t.Status == StatusTarefaMock.Bloqueada) return ConsoleColor.Magenta;
            if (MockDataStore.TarefaAtrasada(t)) return ConsoleColor.Red;
            switch (t.Status)
            {
                case StatusTarefaMock.Concluida: return ConsoleColor.Green;
                case StatusTarefaMock.EmAndamento: return ConsoleColor.Yellow;
                default: return ConsoleColor.Gray;
            }
        }

        private static string Truncar(string texto, int max = Largura)
        {
            if (texto.Length <= max)
                return texto;
            return texto.Substring(0, max - 1) + "…";
        }

        private static void NovaTarefa(int projetoId)
        {
            var titulo = EntradaUsuario.LerTextoObrigatorio("Título da tarefa: ");

            var prioridade = LerPrioridade("Prioridade [1] Baixa  [2] Média  [3] Alta (Enter = Média): ", PrioridadeMock.Media);
            var prazo = EntradaUsuario.LerData("Prazo (dd/MM/aaaa, Enter = 7 dias): ", DateTime.Today.AddDays(7));

            Console.Write("Responsável (Enter = sem responsável): ");
            var responsavel = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(responsavel))
                responsavel = null;

            var tarefa = MockDataStore.AdicionarTarefa(projetoId, titulo, prioridade, prazo, responsavel);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Tarefa #{tarefa.Id} '{tarefa.Titulo}' criada ({MockDataStore.RotuloPrioridade(prioridade)}, prazo {prazo:dd/MM/yyyy}, resp: {responsavel ?? "-"}).");
            Console.ResetColor();
            Pausar();
        }

        private static void MoverStatus()
        {
            var id = EntradaUsuario.LerInteiro("ID da tarefa (Enter para cancelar): ");
            if (id == 0)
                return;

            if (!MockDataStore.Tarefas.Any(t => t.Id == id))
            {
                EntradaUsuario.Erro("Tarefa não encontrada.");
                Pausar();
                return;
            }

            Console.WriteLine("Novo status: [1] A Fazer  [2] Em Andamento  [3] Concluída  [4] Bloqueada");
            var novo = LerStatus();
            if (novo == null)
                return;

            MockDataStore.MoverStatus(id, novo.Value);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Tarefa #{id} movida para '{MockDataStore.RotuloStatus(novo.Value)}'.");
            Console.ResetColor();
            Pausar();
        }

        private static StatusTarefaMock? LerStatus()
        {
            while (true)
            {
                Console.Write("Opção (Enter para cancelar): ");
                var op = Console.ReadLine()?.Trim();

                switch (op)
                {
                    case "1": return StatusTarefaMock.AFazer;
                    case "2": return StatusTarefaMock.EmAndamento;
                    case "3": return StatusTarefaMock.Concluida;
                    case "4": return StatusTarefaMock.Bloqueada;
                    case "":
                    case null:
                        return null;
                    default:
                        EntradaUsuario.Erro("Status inválido. Escolha de 1 a 4.");
                        break;
                }
            }
        }

        private static void RemoverTarefa()
        {
            var id = EntradaUsuario.LerInteiro("ID da tarefa (Enter para cancelar): ");
            if (id == 0)
                return;

            if (!MockDataStore.RemoverTarefa(id))
            {
                EntradaUsuario.Erro("Tarefa não encontrada.");
                Pausar();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Tarefa #{id} removida (use [6] para desfazer).");
            Console.ResetColor();
            Pausar();
        }

        private static void EditarTarefa()
        {
            var id = EntradaUsuario.LerInteiro("ID da tarefa (Enter para cancelar): ");
            if (id == 0)
                return;

            var t = MockDataStore.Tarefas.FirstOrDefault(x => x.Id == id);
            if (t == null)
            {
                EntradaUsuario.Erro("Tarefa não encontrada.");
                Pausar();
                return;
            }

            var titulo = EntradaUsuario.LerTextoOpcional($"Título [{t.Titulo}]: ", t.Titulo);
            var prioridade = LerPrioridade($"Prioridade [1/2/3] (atual {MockDataStore.RotuloPrioridade(t.Prioridade)}, Enter mantém): ", t.Prioridade);
            var prazo = EntradaUsuario.LerData($"Prazo (atual {t.Prazo:dd/MM/yyyy}, Enter mantém): ", t.Prazo);

            Console.Write($"Responsável (atual {t.Responsavel ?? "-"}, Enter mantém, '-' limpa): ");
            var respTxt = Console.ReadLine()?.Trim();
            string? responsavel = t.Responsavel;
            if (!string.IsNullOrWhiteSpace(respTxt))
                responsavel = respTxt == "-" ? null : respTxt;

            MockDataStore.EditarTarefa(id, titulo, prioridade, prazo, responsavel);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Tarefa #{id} atualizada.");
            Console.ResetColor();
            Pausar();
        }

        private static PrioridadeMock LerPrioridade(string prompt, PrioridadeMock atual)
        {
            while (true)
            {
                Console.Write(prompt);
                var txt = Console.ReadLine()?.Trim();

                switch (txt)
                {
                    case "1": return PrioridadeMock.Baixa;
                    case "2": return PrioridadeMock.Media;
                    case "3": return PrioridadeMock.Alta;
                    case "":
                    case null:
                        return atual;
                    default:
                        EntradaUsuario.Erro("Prioridade inválida. Escolha 1, 2 ou 3.");
                        break;
                }
            }
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }
    }
}
