using System;
using System.Collections.Generic;
using System.Linq;
using TaskFlow.App.Navigation;
using TaskFlow.Application;
using TaskFlow.Domain;
using static TaskFlow.App.Views.Formatacao;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    public class KanbanView
    {
        private const int Largura = 20;

        private static readonly StatusTarefa[] Colunas =
        {
            StatusTarefa.ToDo,
            StatusTarefa.InProgress,
            StatusTarefa.Review,
            StatusTarefa.Done,
            StatusTarefa.Bloqueada
        };

        private readonly ITaskFlowService _service;

        // Filtros ficam ativos enquanto o quadro estiver aberto.
        private string? _filtroResponsavel;
        private int? _filtroPrioridade;
        private string? _filtroTexto;

        public KanbanView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar(int projetoId)
        {
            bool voltar = false;

            while (!voltar)
            {
                // O projeto pode ter sido removido/renomeado: busca de novo a cada volta.
                var projeto = _service.ObterProjeto(projetoId);
                if (projeto == null)
                {
                    EntradaUsuario.Erro("Projeto não encontrado.");
                    Pausar();
                    return;
                }

                LimparTela();
                Titulo($"QUADRO KANBAN - {projeto.Nome}", ConsoleColor.Yellow);
                MostrarFiltros();

                ImprimirColunas(AplicarFiltros(_service.ListarTarefas(projetoId)));

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nMarcas: ! = atrasada   * = aguardando dependência");
                Console.WriteLine("Cores: verde=concluída  amarelo=em andamento  ciano=revisão  magenta=bloqueada  vermelho=atrasada  cinza-escuro=aguardando");
                Console.ResetColor();

                Console.WriteLine("\n[N] Nova | [M] Mover | [E] Editar | [R] Remover | [D] Detalhes | [F] Filtrar | [B] Buscar | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "N": NovaTarefa(projetoId); break;
                    case "M": MoverStatus(projetoId); break;
                    case "E": EditarTarefa(projetoId); break;
                    case "R": RemoverTarefa(projetoId); break;
                    case "D": Detalhes(projetoId); break;
                    case "F": Filtrar(); break;
                    case "B": Buscar(); break;
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

        // ---------------------------------------------------------------
        // Quadro
        // ---------------------------------------------------------------

        private void MostrarFiltros()
        {
            var filtros = new List<string>();
            if (_filtroResponsavel != null) filtros.Add($"responsável={_filtroResponsavel}");
            if (_filtroPrioridade != null) filtros.Add($"prioridade={Prioridade(_filtroPrioridade.Value)}");
            if (_filtroTexto != null) filtros.Add($"busca=\"{_filtroTexto}\"");
            if (filtros.Count == 0)
                return;

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("Filtro ativo: " + string.Join(", ", filtros));
            Console.ResetColor();
        }

        private List<Tarefa> AplicarFiltros(IEnumerable<Tarefa> tarefas)
        {
            if (_filtroResponsavel != null)
            {
                var responsavel = _filtroResponsavel;
                tarefas = tarefas.Where(t => string.Equals(t.Responsavel, responsavel, StringComparison.OrdinalIgnoreCase));
            }
            if (_filtroPrioridade != null)
            {
                var prioridade = _filtroPrioridade.Value;
                tarefas = tarefas.Where(t => t.Prioridade == prioridade);
            }
            if (_filtroTexto != null)
            {
                var texto = _filtroTexto;
                tarefas = tarefas.Where(t =>
                    t.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    (t.Responsavel != null && t.Responsavel.Contains(texto, StringComparison.OrdinalIgnoreCase)));
            }
            return tarefas.ToList();
        }

        private void ImprimirColunas(List<Tarefa> tarefas)
        {
            Console.WriteLine(string.Join(" | ", Colunas.Select(c => $"[ {Status(c).ToUpper()} ]".PadRight(Largura))));
            Console.WriteLine(new string('-', Largura * Colunas.Length + 3 * (Colunas.Length - 1)));

            var porColuna = Colunas
                .Select(c => tarefas.Where(t => t.Status == c).OrderBy(t => t.Prioridade).ToList())
                .ToList();

            var linhas = Math.Max(1, porColuna.Max(l => l.Count));
            for (int i = 0; i < linhas; i++)
            {
                for (int c = 0; c < porColuna.Count; c++)
                {
                    var lista = porColuna[c];
                    if (i < lista.Count)
                    {
                        var t = lista[i];
                        Console.ForegroundColor = CorTarefa(t);
                        Console.Write(Truncar(RotuloCelula(t), Largura).PadRight(Largura));
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(new string(' ', Largura));
                    }

                    if (c < porColuna.Count - 1)
                        Console.Write(" | ");
                }
                Console.WriteLine();
            }
        }

        private string RotuloCelula(Tarefa t)
        {
            // As marcas vão no início para não sumirem quando o título for truncado.
            var marcas = string.Empty;
            if (Atrasada(t)) marcas += "!";
            if (EstaAguardando(t)) marcas += "*";
            return marcas.Length > 0 ? $"{marcas} #{t.Id} {t.Titulo}" : $"#{t.Id} {t.Titulo}";
        }

        private ConsoleColor CorTarefa(Tarefa t)
        {
            if (t.Status == StatusTarefa.Done) return ConsoleColor.Green;
            if (t.Status == StatusTarefa.Bloqueada) return ConsoleColor.Magenta;
            if (Atrasada(t)) return ConsoleColor.Red;
            if (EstaAguardando(t)) return ConsoleColor.DarkGray;
            return t.Status switch
            {
                StatusTarefa.InProgress => ConsoleColor.Yellow,
                StatusTarefa.Review => ConsoleColor.Cyan,
                _ => ConsoleColor.Gray
            };
        }

        private bool EstaAguardando(Tarefa t)
            => t.Status != StatusTarefa.Done && _service.ObterPredecessorasPendentes(t.Id).Count > 0;

        // ---------------------------------------------------------------
        // Comandos
        // ---------------------------------------------------------------

        private void NovaTarefa(int projetoId)
        {
            var titulo = EntradaUsuario.LerTextoObrigatorio("Título da tarefa: ");
            var descricao = EntradaUsuario.LerTextoOpcional("Descrição (Enter = sem descrição): ", string.Empty);
            var prioridade = EntradaUsuario.LerPrioridade("Prioridade 1 (urgente) a 5 (mínima) (Enter = 3): ", 3);
            var prazo = EntradaUsuario.LerDataOpcional("Prazo dd/MM/aaaa (Enter = sem prazo): ", null);
            var responsavel = EntradaUsuario.LerTextoOpcional("Responsável (Enter = sem responsável): ", string.Empty);

            MostrarResultado(_service.CriarTarefa(projetoId, titulo, descricao, responsavel, prioridade, prazo));
        }

        private void MoverStatus(int projetoId)
        {
            var tarefa = LerTarefaDoProjeto(projetoId);
            if (tarefa == null)
                return;

            Console.WriteLine($"Status atual: {Status(tarefa.Status)}");
            Console.WriteLine("Novo status: [1] A Fazer  [2] Em Andamento  [3] Em Revisão  [4] Concluída  [5] Bloqueada");
            var novo = LerStatus();
            if (novo == null)
                return;

            // O serviço recusa avançar se houver predecessora não concluída e explica o motivo.
            MostrarResultado(_service.MudarStatusTarefa(tarefa.Id, novo.Value));
        }

        private void EditarTarefa(int projetoId)
        {
            var t = LerTarefaDoProjeto(projetoId);
            if (t == null)
                return;

            var titulo = EntradaUsuario.LerTextoOpcional($"Título [{t.Titulo}]: ", t.Titulo);
            var descricao = EntradaUsuario.LerTextoOpcionalOuLimpar(
                $"Descrição [{t.Descricao ?? "-"}] (Enter mantém, '-' apaga): ", t.Descricao);
            var prioridade = EntradaUsuario.LerPrioridade(
                $"Prioridade 1-5 (atual {Prioridade(t.Prioridade)}, Enter mantém): ", t.Prioridade);
            var prazo = EntradaUsuario.LerDataOpcional(
                $"Prazo (atual {Prazo(t.Prazo, "dd/MM/yyyy")}, Enter mantém, '-' tira o prazo): ", t.Prazo);
            var responsavel = EntradaUsuario.LerTextoOpcionalOuLimpar(
                $"Responsável (atual {Responsavel(t)}, Enter mantém, '-' tira): ", t.Responsavel);

            MostrarResultado(_service.EditarTarefa(t.Id, titulo, descricao, responsavel, prioridade, prazo));
        }

        private void RemoverTarefa(int projetoId)
        {
            var t = LerTarefaDoProjeto(projetoId);
            if (t == null)
                return;

            if (!EntradaUsuario.Confirmar($"Remover a tarefa #{t.Id} '{t.Titulo}' e as dependências dela? (s/n): "))
                return;

            MostrarResultado(_service.RemoverTarefa(t.Id));
        }

        private void Detalhes(int projetoId)
        {
            var t = LerTarefaDoProjeto(projetoId);
            if (t == null)
                return;

            var predecessoras = _service.ObterPredecessores(t.Id);
            var sucessoras = _service.ListarTarefas()
                .Where(x => _service.ObterPredecessores(x.Id).Contains(t.Id))
                .Select(x => x.Id)
                .ToList();

            Console.WriteLine();
            Console.WriteLine($"Tarefa #{t.Id}: {t.Titulo}");
            Console.WriteLine($"  Descrição:    {t.Descricao ?? "-"}");
            Console.WriteLine($"  Responsável:  {Responsavel(t)}");
            Console.WriteLine($"  Prioridade:   {Prioridade(t.Prioridade)}");
            Console.WriteLine($"  Prazo:        {Prazo(t.Prazo, "dd/MM/yyyy")}{(Atrasada(t) ? "  (ATRASADA)" : string.Empty)}");
            Console.WriteLine($"  Status:       {Status(t.Status)}");
            Console.WriteLine($"  Depende de:   {(predecessoras.Count == 0 ? "-" : DescreverTarefas(predecessoras))}");
            Console.WriteLine($"  Libera:       {(sucessoras.Count == 0 ? "-" : DescreverTarefas(sucessoras))}");
            Console.WriteLine($"  Situação:     {Situacao(_service, t)}");
            Pausar();
        }

        private void Filtrar()
        {
            Console.Write("Filtrar por responsável (Enter = todos): ");
            var responsavel = Console.ReadLine()?.Trim();
            _filtroResponsavel = string.IsNullOrWhiteSpace(responsavel) ? null : responsavel;

            Console.Write("Filtrar por prioridade 1-5 (Enter = todas): ");
            var texto = Console.ReadLine()?.Trim();
            _filtroPrioridade = int.TryParse(texto, out var p) && p >= 1 && p <= 5 ? p : null;
        }

        private void Buscar()
        {
            Console.Write("Buscar por texto no título/responsável (Enter limpa): ");
            var texto = Console.ReadLine()?.Trim();
            _filtroTexto = string.IsNullOrWhiteSpace(texto) ? null : texto;
        }

        // ---------------------------------------------------------------
        // Auxiliares
        // ---------------------------------------------------------------

        // Só aceita tarefas do projeto aberto no quadro.
        private Tarefa? LerTarefaDoProjeto(int projetoId)
        {
            var id = EntradaUsuario.LerInteiro("ID da tarefa (Enter para cancelar): ");
            if (id == 0)
                return null;

            var tarefa = _service.ObterTarefa(id);
            if (tarefa == null || tarefa.ProjetoId != projetoId)
            {
                EntradaUsuario.Erro("Tarefa não encontrada neste projeto.");
                Pausar();
                return null;
            }
            return tarefa;
        }

        // "#1 Configurar DB (Concluída), #3 Tela de Login (A Fazer)"
        private string DescreverTarefas(IEnumerable<int> ids)
            => string.Join(", ", ids.Select(id =>
            {
                var tarefa = _service.ObterTarefa(id);
                return tarefa == null ? $"#{id} (removida)" : $"#{id} {tarefa.Titulo} ({Status(tarefa.Status)})";
            }));

        private static StatusTarefa? LerStatus()
        {
            while (true)
            {
                Console.Write("Opção (Enter para cancelar): ");
                var op = Console.ReadLine()?.Trim();

                switch (op)
                {
                    case "1": return StatusTarefa.ToDo;
                    case "2": return StatusTarefa.InProgress;
                    case "3": return StatusTarefa.Review;
                    case "4": return StatusTarefa.Done;
                    case "5": return StatusTarefa.Bloqueada;
                    case "":
                    case null:
                        return null;
                    default:
                        EntradaUsuario.Erro("Status inválido. Escolha de 1 a 5.");
                        break;
                }
            }
        }
    }
}
