using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TaskFlow.App.MockServices
{
    public static class MockDataStore
    {
        public static decimal Salario { get; private set; } = 5000m;
        public static List<ProjetoMock> Projetos { get; } = new List<ProjetoMock>();
        public static List<TarefaMock> Tarefas { get; } = new List<TarefaMock>();
        public static List<ContaMock> Contas { get; } = new List<ContaMock>();
        public static Stack<AcaoHistorico> Historico { get; } = new Stack<AcaoHistorico>();

        private static int _proximoProjeto = 1;
        private static int _proximaTarefa = 1;
        private static int _proximaConta = 1;
        private static bool _inicializado;

        private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

        public static void Inicializar()
        {
            if (_inicializado)
                return;

            _inicializado = true;
            var hoje = DateTime.Today;

            Projetos.Add(new ProjetoMock { Id = _proximoProjeto++, Nome = "TaskFlow - Sistema de Gestão" });
            Projetos.Add(new ProjetoMock { Id = _proximoProjeto++, Nome = "Estudo AED2 - Estruturas de Dados" });
            Projetos.Add(new ProjetoMock { Id = _proximoProjeto++, Nome = "Projeto Pessoal - Financeiro" });

            Tarefas.Add(new TarefaMock { Id = _proximaTarefa++, ProjetoId = 1, Titulo = "Configurar DB", Responsavel = "Ana", Prioridade = PrioridadeMock.Alta, Status = StatusTarefaMock.EmAndamento, Prazo = hoje.AddDays(-2) });
            Tarefas.Add(new TarefaMock { Id = _proximaTarefa++, ProjetoId = 1, Titulo = "Criar Grafo de Dependências", Responsavel = "Bruno", Prioridade = PrioridadeMock.Alta, Status = StatusTarefaMock.AFazer, Prazo = hoje.AddDays(3) });
            Tarefas.Add(new TarefaMock { Id = _proximaTarefa++, ProjetoId = 1, Titulo = "Tela de Login", Responsavel = "Ana", Prioridade = PrioridadeMock.Media, Status = StatusTarefaMock.Bloqueada, Prazo = hoje.AddDays(6) });
            Tarefas.Add(new TarefaMock { Id = _proximaTarefa++, ProjetoId = 1, Titulo = "Kanban View", Responsavel = "Bruno", Prioridade = PrioridadeMock.Baixa, Status = StatusTarefaMock.Concluida, Prazo = hoje.AddDays(-5) });
            Tarefas.Add(new TarefaMock { Id = _proximaTarefa++, ProjetoId = 1, Titulo = "Painel Financeiro", Responsavel = "Ana", Prioridade = PrioridadeMock.Baixa, Status = StatusTarefaMock.AFazer, Prazo = hoje.AddDays(10) });

            Contas.Add(new ContaMock { Id = _proximaConta++, Descricao = "Aluguel", Valor = 1500m, Status = StatusContaMock.Paga, Vencimento = hoje.AddDays(-3) });
            Contas.Add(new ContaMock { Id = _proximaConta++, Descricao = "Luz", Valor = 200m, Status = StatusContaMock.Pendente, Vencimento = hoje.AddDays(2) });
            Contas.Add(new ContaMock { Id = _proximaConta++, Descricao = "Internet", Valor = 100m, Status = StatusContaMock.Pendente, Vencimento = hoje.AddDays(5) });
            Contas.Add(new ContaMock { Id = _proximaConta++, Descricao = "Marketplace", Valor = 400m, Status = StatusContaMock.Atrasada, Vencimento = hoje.AddDays(-8) });
        }

        public static string FormatarMoeda(decimal valor)
        {
            return string.Format(CulturaBr, "{0:C}", valor);
        }

        // ---------- Projetos ----------
        public static ProjetoMock AdicionarProjeto(string nome)
        {
            var projeto = new ProjetoMock { Id = _proximoProjeto++, Nome = nome };
            Projetos.Add(projeto);
            Registrar($"Projeto #{projeto.Id} '{projeto.Nome}' criado", () => Projetos.Remove(projeto));
            return projeto;
        }

        public static bool EditarProjeto(int projetoId, string novoNome)
        {
            var projeto = Projetos.FirstOrDefault(p => p.Id == projetoId);
            if (projeto == null)
                return false;

            var anterior = projeto.Nome;
            projeto.Nome = novoNome;
            Registrar($"Projeto #{projeto.Id} renomeado de '{anterior}' para '{novoNome}'", () => projeto.Nome = anterior);
            return true;
        }

        public static bool RemoverProjeto(int projetoId)
        {
            var indice = Projetos.FindIndex(p => p.Id == projetoId);
            if (indice < 0)
                return false;

            var projeto = Projetos[indice];
            var tarefasRemovidas = Tarefas.Where(t => t.ProjetoId == projetoId).ToList();
            Tarefas.RemoveAll(t => t.ProjetoId == projetoId);
            Projetos.RemoveAt(indice);

            Registrar($"Projeto #{projeto.Id} '{projeto.Nome}' removido ({tarefasRemovidas.Count} tarefa(s))", () =>
            {
                Projetos.Insert(Math.Min(indice, Projetos.Count), projeto);
                Tarefas.AddRange(tarefasRemovidas);
            });
            return true;
        }

        // ---------- Tarefas ----------
        public static TarefaMock AdicionarTarefa(int projetoId, string titulo, PrioridadeMock prioridade, DateTime prazo, string? responsavel)
        {
            var tarefa = new TarefaMock
            {
                Id = _proximaTarefa++,
                ProjetoId = projetoId,
                Titulo = titulo,
                Responsavel = responsavel,
                Prioridade = prioridade,
                Status = StatusTarefaMock.AFazer,
                Prazo = prazo
            };
            Tarefas.Add(tarefa);
            Registrar($"Tarefa #{tarefa.Id} '{tarefa.Titulo}' criada", () => Tarefas.Remove(tarefa));
            return tarefa;
        }

        public static bool EditarTarefa(int tarefaId, string titulo, PrioridadeMock prioridade, DateTime prazo, string? responsavel)
        {
            var tarefa = Tarefas.FirstOrDefault(t => t.Id == tarefaId);
            if (tarefa == null)
                return false;

            var antigoTitulo = tarefa.Titulo;
            var antigaPrioridade = tarefa.Prioridade;
            var antigoPrazo = tarefa.Prazo;
            var antigoResponsavel = tarefa.Responsavel;

            tarefa.Titulo = titulo;
            tarefa.Prioridade = prioridade;
            tarefa.Prazo = prazo;
            tarefa.Responsavel = responsavel;

            Registrar($"Tarefa #{tarefa.Id} editada", () =>
            {
                tarefa.Titulo = antigoTitulo;
                tarefa.Prioridade = antigaPrioridade;
                tarefa.Prazo = antigoPrazo;
                tarefa.Responsavel = antigoResponsavel;
            });
            return true;
        }

        public static bool MoverStatus(int tarefaId, StatusTarefaMock novoStatus)
        {
            var tarefa = Tarefas.FirstOrDefault(t => t.Id == tarefaId);
            if (tarefa == null)
                return false;

            var anterior = tarefa.Status;
            tarefa.Status = novoStatus;
            Registrar(
                $"Tarefa #{tarefa.Id} movida de '{RotuloStatus(anterior)}' para '{RotuloStatus(novoStatus)}'",
                () => tarefa.Status = anterior);
            return true;
        }

        public static bool RemoverTarefa(int tarefaId)
        {
            var indice = Tarefas.FindIndex(t => t.Id == tarefaId);
            if (indice < 0)
                return false;

            var tarefa = Tarefas[indice];
            Tarefas.RemoveAt(indice);

            Registrar($"Tarefa #{tarefa.Id} '{tarefa.Titulo}' removida", () =>
                Tarefas.Insert(Math.Min(indice, Tarefas.Count), tarefa));
            return true;
        }

        // ---------- Financeiro ----------
        public static void DefinirSalario(decimal novoSalario)
        {
            var anterior = Salario;
            Salario = novoSalario;
            Registrar($"Salário alterado de {FormatarMoeda(anterior)} para {FormatarMoeda(novoSalario)}", () => Salario = anterior);
        }

        public static bool PagarConta(int contaId)
        {
            var conta = Contas.FirstOrDefault(c => c.Id == contaId);
            if (conta == null)
                return false;

            var anterior = conta.Status;
            conta.Status = StatusContaMock.Paga;
            Registrar(
                $"Conta #{conta.Id} '{conta.Descricao}' paga ({FormatarMoeda(conta.Valor)})",
                () => conta.Status = anterior);
            return true;
        }

        public static ContaMock AdicionarConta(string descricao, decimal valor, DateTime vencimento)
        {
            var conta = new ContaMock
            {
                Id = _proximaConta++,
                Descricao = descricao,
                Valor = valor,
                Status = StatusContaMock.Pendente,
                Vencimento = vencimento
            };
            Contas.Add(conta);
            Registrar($"Conta #{conta.Id} '{conta.Descricao}' adicionada ({FormatarMoeda(conta.Valor)})", () => Contas.Remove(conta));
            return conta;
        }

        public static bool EditarConta(int contaId, string descricao, decimal valor, DateTime vencimento)
        {
            var conta = Contas.FirstOrDefault(c => c.Id == contaId);
            if (conta == null)
                return false;

            var antigaDescricao = conta.Descricao;
            var antigoValor = conta.Valor;
            var antigoVencimento = conta.Vencimento;

            conta.Descricao = descricao;
            conta.Valor = valor;
            conta.Vencimento = vencimento;

            Registrar($"Conta #{conta.Id} '{antigaDescricao}' editada", () =>
            {
                conta.Descricao = antigaDescricao;
                conta.Valor = antigoValor;
                conta.Vencimento = antigoVencimento;
            });
            return true;
        }

        public static bool RemoverConta(int contaId)
        {
            var indice = Contas.FindIndex(c => c.Id == contaId);
            if (indice < 0)
                return false;

            var conta = Contas[indice];
            Contas.RemoveAt(indice);

            Registrar($"Conta #{conta.Id} '{conta.Descricao}' removida", () =>
                Contas.Insert(Math.Min(indice, Contas.Count), conta));
            return true;
        }

        // ---------- Histórico / Undo ----------
        public static bool DesfazerUltimaAcao(out string descricao)
        {
            descricao = string.Empty;
            if (Historico.Count == 0)
                return false;

            var acao = Historico.Pop();
            acao.Desfazer();
            descricao = acao.Descricao;
            return true;
        }

        public static List<AcaoHistorico> ObterHistoricoRecente(int max)
        {
            return Historico.Take(max).ToList();
        }

        // ---------- Cálculos ----------
        public static decimal TotalContas() => Contas.Sum(c => c.Valor);

        public static decimal TotalAPagar() => Contas.Where(c => c.Status != StatusContaMock.Paga).Sum(c => c.Valor);

        public static decimal SaldoAposQuitarTudo() => Salario - TotalContas();

        public static decimal SaldoLivre() => Salario - TotalAPagar();

        public static int ContasAtrasadas() => Contas.Count(c => c.Status == StatusContaMock.Atrasada);

        public static bool TarefaAtrasada(TarefaMock tarefa)
        {
            return tarefa.Status != StatusTarefaMock.Concluida && tarefa.Prazo.Date < DateTime.Today;
        }

        public static IEnumerable<TarefaMock> TarefasProntas()
        {
            return Tarefas
                .Where(t => t.Status == StatusTarefaMock.AFazer || t.Status == StatusTarefaMock.EmAndamento)
                .OrderByDescending(t => t.Prioridade)
                .ThenBy(t => t.Prazo);
        }

        public static IEnumerable<TarefaMock> TarefasDoResponsavel(string responsavel)
        {
            return Tarefas
                .Where(t => !string.IsNullOrWhiteSpace(t.Responsavel) &&
                            string.Equals(t.Responsavel, responsavel, StringComparison.OrdinalIgnoreCase) &&
                            t.Status != StatusTarefaMock.Concluida)
                .OrderBy(t => t.Prazo)
                .ThenByDescending(t => t.Prioridade);
        }

        public static IEnumerable<string> Responsaveis()
        {
            return Tarefas
                .Where(t => !string.IsNullOrWhiteSpace(t.Responsavel))
                .Select(t => t.Responsavel!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r);
        }

        public static IEnumerable<TarefaMock> BuscarTarefas(string termo)
        {
            var busca = termo?.Trim();
            if (string.IsNullOrWhiteSpace(busca))
                return Enumerable.Empty<TarefaMock>();

            return Tarefas.Where(t =>
                t.Titulo.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                (t.Responsavel != null && t.Responsavel.Contains(busca, StringComparison.OrdinalIgnoreCase)));
        }

        public static List<(string Responsavel, int Atrasadas)> AtrasadasPorResponsavel()
        {
            return Tarefas
                .Where(TarefaAtrasada)
                .GroupBy(t => string.IsNullOrWhiteSpace(t.Responsavel) ? "(sem responsável)" : t.Responsavel!)
                .Select(g => (Responsavel: g.Key, Atrasadas: g.Count()))
                .OrderByDescending(r => r.Atrasadas)
                .ToList();
        }

        public static ProjetoMock? ObterProjeto(int projetoId) => Projetos.FirstOrDefault(p => p.Id == projetoId);

        // ---------- Rótulos ----------
        public static string RotuloPrioridade(PrioridadeMock prioridade)
        {
            switch (prioridade)
            {
                case PrioridadeMock.Alta: return "ALTA";
                case PrioridadeMock.Media: return "MÉDIA";
                default: return "BAIXA";
            }
        }

        public static string RotuloStatus(StatusTarefaMock status)
        {
            switch (status)
            {
                case StatusTarefaMock.AFazer: return "A Fazer";
                case StatusTarefaMock.EmAndamento: return "Em Andamento";
                case StatusTarefaMock.Concluida: return "Concluída";
                default: return "Bloqueada";
            }
        }

        public static string RotuloStatusConta(StatusContaMock status)
        {
            switch (status)
            {
                case StatusContaMock.Paga: return "Paga";
                case StatusContaMock.Atrasada: return "Atrasada";
                default: return "Pendente";
            }
        }

        private static void Registrar(string descricao, Action desfazer)
        {
            Historico.Push(new AcaoHistorico { Descricao = descricao, Desfazer = desfazer });
        }
    }
}
