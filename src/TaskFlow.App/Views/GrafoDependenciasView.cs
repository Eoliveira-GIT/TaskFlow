using System;
using System.Linq;
using TaskFlow.App.Navigation;
using TaskFlow.Application;
using TaskFlow.Domain;
using static TaskFlow.App.Views.Formatacao;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    /// <summary>
    /// Mostra o grafo real do Domain (via ITaskFlowService): quem depende de quem,
    /// quais tarefas estão travadas e a ordem sugerida de execução (ordenação topológica).
    /// </summary>
    public class GrafoDependenciasView
    {
        private readonly ITaskFlowService _service;

        // null = todos os projetos
        private int? _projetoId;

        public GrafoDependenciasView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            bool voltar = false;

            while (!voltar)
            {
                // Se o projeto escolhido foi removido, volta para "todos".
                if (_projetoId != null && _service.ObterProjeto(_projetoId.Value) == null)
                    _projetoId = null;

                LimparTela();
                Titulo("MAPA DE DEPENDÊNCIAS (GRAFO)", ConsoleColor.Magenta);
                Console.WriteLine($"Escopo: {NomeEscopo()}");

                MostrarDependencias();
                MostrarOrdemExecucao();

                Console.WriteLine("\n[A] Adicionar dependência | [R] Remover dependência | [P] Escolher projeto | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "A": AdicionarDependencia(); break;
                    case "R": RemoverDependencia(); break;
                    case "P": EscolherProjeto(); break;
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
        // Exibição
        // ---------------------------------------------------------------

        private void MostrarDependencias()
        {
            Secao("TAREFAS E SUAS PREDECESSORAS", ConsoleColor.Cyan);

            var tarefas = _service.ListarTarefas(_projetoId).OrderBy(t => t.Id).ToList();
            if (tarefas.Count == 0)
            {
                Console.WriteLine("  (nenhuma tarefa neste escopo)");
                return;
            }

            foreach (var t in tarefas)
            {
                var predecessoras = _service.ObterPredecessores(t.Id);
                var pendentes = _service.ObterPredecessorasPendentes(t.Id);

                string estado;
                if (t.Status == StatusTarefa.Done)
                {
                    estado = "✅ CONCLUÍDA";
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (pendentes.Count > 0)
                {
                    estado = $"🔒 TRAVADA (aguarda {ListaIds(pendentes)})";
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    estado = predecessoras.Count > 0 ? "🔓 LIBERADA (dependências concluídas)" : "🔓 LIBERADA";
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.WriteLine($"  #{t.Id,-3} {Truncar(t.Titulo, 30),-30} [{Status(t.Status)}]  {estado}");
                Console.ResetColor();

                if (predecessoras.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("        ← sem predecessoras");
                    Console.ResetColor();
                    continue;
                }

                foreach (var predId in predecessoras)
                {
                    var pred = _service.ObterTarefa(predId);
                    var concluida = pred?.Status == StatusTarefa.Done;
                    var descricao = pred == null
                        ? $"#{predId} (removida)"
                        : $"#{pred.Id} {Truncar(pred.Titulo, 30)} [{Status(pred.Status)}]{OutroProjeto(pred)}";

                    Console.ForegroundColor = concluida ? ConsoleColor.DarkGreen : ConsoleColor.DarkYellow;
                    Console.WriteLine($"        ← depende de {descricao} {(concluida ? "✓" : "✗")}");
                    Console.ResetColor();
                }
            }
        }

        private void MostrarOrdemExecucao()
        {
            Secao("ORDEM SUGERIDA DE EXECUÇÃO (ORDENAÇÃO TOPOLÓGICA)", ConsoleColor.Cyan);

            var ordem = _service.ObterOrdemExecucao(_projetoId);
            if (ordem.Count == 0)
            {
                Console.WriteLine("  (nenhuma tarefa neste escopo)");
                return;
            }

            int passo = 1;
            foreach (var id in ordem)
            {
                var t = _service.ObterTarefa(id);
                if (t == null)
                    continue;

                Console.ForegroundColor = t.Status == StatusTarefa.Done ? ConsoleColor.DarkGray : ConsoleColor.Gray;
                Console.WriteLine($"  {passo++,2}. #{t.Id,-3} {Truncar(t.Titulo, 34),-34} [{Status(t.Status)}]");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n  Cada tarefa aparece depois de todas as que ela depende. Concluídas ficam em cinza.");
            Console.ResetColor();
        }

        // ---------------------------------------------------------------
        // Comandos
        // ---------------------------------------------------------------

        private void AdicionarDependencia()
        {
            Console.WriteLine("\nA tarefa escolhida primeiro só poderá avançar quando a segunda estiver concluída.");
            var tarefaId = EntradaUsuario.LerInteiro("ID da tarefa que vai ESPERAR (Enter para cancelar): ");
            if (tarefaId == 0)
                return;

            var predecessoraId = EntradaUsuario.LerInteiro($"#{tarefaId} depende de qual tarefa? ID (Enter para cancelar): ");
            if (predecessoraId == 0)
                return;

            // O serviço valida: tarefas existentes, autodependência, duplicada e ciclo.
            MostrarResultado(_service.AdicionarDependencia(tarefaId, predecessoraId));
        }

        private void RemoverDependencia()
        {
            var tarefaId = EntradaUsuario.LerInteiro("\nID da tarefa que espera (Enter para cancelar): ");
            if (tarefaId == 0)
                return;

            var predecessoras = _service.ObterPredecessores(tarefaId);
            if (predecessoras.Count == 0)
            {
                Console.WriteLine($"A tarefa #{tarefaId} não depende de nenhuma outra.");
                Pausar();
                return;
            }

            Console.WriteLine($"#{tarefaId} depende de: {ListaIds(predecessoras)}");
            var predecessoraId = EntradaUsuario.LerInteiro("Remover a dependência de qual ID? (Enter para cancelar): ");
            if (predecessoraId == 0)
                return;

            MostrarResultado(_service.RemoverDependencia(tarefaId, predecessoraId));
        }

        private void EscolherProjeto()
        {
            Console.WriteLine();
            foreach (var p in _service.ListarProjetos())
                Console.WriteLine($"  [{p.Id}] {p.Nome}");

            var id = EntradaUsuario.LerInteiro("ID do projeto (Enter = todos os projetos): ");
            if (id == 0)
            {
                _projetoId = null;
                return;
            }

            if (_service.ObterProjeto(id) == null)
            {
                EntradaUsuario.Erro("Projeto não encontrado.");
                Pausar();
                return;
            }

            _projetoId = id;
        }

        // ---------------------------------------------------------------
        // Auxiliares
        // ---------------------------------------------------------------

        private string NomeEscopo()
            => _projetoId == null
                ? "todos os projetos"
                : $"projeto #{_projetoId} - {_service.ObterProjeto(_projetoId.Value)?.Nome}";

        // Dependências podem cruzar projetos; avisa quando a predecessora é de outro.
        private string OutroProjeto(Tarefa predecessora)
        {
            if (_projetoId == null || predecessora.ProjetoId == _projetoId)
                return string.Empty;

            var nome = _service.ObterProjeto(predecessora.ProjetoId)?.Nome ?? "?";
            return $" (projeto {Truncar(nome, 20)})";
        }
    }
}
