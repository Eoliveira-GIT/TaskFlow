using TaskFlow.Domain;

namespace TaskFlow.Application;

public class TaskFlowService : ITaskFlowService
{
    // ---- Estado em memória: aqui mora a fonte da verdade ----
    private readonly List<Projeto> _projetos = new();
    private readonly List<Tarefa> _tarefas = new();
    private readonly List<TarefaFinanceira> _contas = new();
    private readonly GrafoDependencias _grafo = new();
    private readonly PilhaUndo _undo = new();
    private decimal _salario;

    // Contadores de id: só avançam, nunca reaproveitam (evita confundir o histórico)
    private int _proximoProjetoId = 1;
    private int _proximaTarefaId = 1;
    private int _proximaContaId = 1;
    private int _proximaAcaoId = 1;

    // =====================================================================
    // UTILIDADES PRIVADAS
    // =====================================================================

    // Todo comando que muda algo chama isto. O "desfazer" é um bilhete (closure)
    // que lembra como reverter. IMPORTANTE: os bilhetes mexem direto nas listas
    // e no grafo e NUNCA chamam métodos públicos, senão registrariam outro undo.
    private void Registrar(string tipo, string descricao, string entidade, int? entidadeId, Action desfazer)
    {
        _undo.Empilhar(new HistoricoAcao
        {
            Id = _proximaAcaoId++,
            TipoAcao = tipo,
            Descricao = descricao,
            EntidadeAfetada = entidade,
            EntidadeId = entidadeId,
            Desfazer = desfazer
        });
    }

    private static string? LimparTexto(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    // "Atrasada" é derivada da data, não digitada: conta não paga e vencida.
    // Chamado nas consultas para o Status nunca ficar velho (ex.: após editar o vencimento).
    private void SincronizarAtrasos()
    {
        foreach (var c in _contas.Where(c => c.Status != StatusFinanceiro.Paga))
            c.Status = c.DataVencimento.Date < DateTime.Today
                ? StatusFinanceiro.Atrasada
                : StatusFinanceiro.Pendente;
    }

    // =====================================================================
    // PROJETOS
    // =====================================================================

    public IReadOnlyList<Projeto> ListarProjetos() => _projetos.ToList();

    public Projeto? ObterProjeto(int id) => _projetos.FirstOrDefault(p => p.Id == id);

    public Resultado CriarProjeto(string nome, string? descricao)
    {
        // 1. validar
        if (string.IsNullOrWhiteSpace(nome))
            return Resultado.Erro("O nome do projeto é obrigatório.");

        // 2. executar
        var projeto = new Projeto
        {
            Id = _proximoProjetoId++,
            Nome = nome.Trim(),
            Descricao = LimparTexto(descricao)
        };
        _projetos.Add(projeto);

        // 3. registrar como desfazer
        Registrar("Criar", $"Projeto #{projeto.Id} '{projeto.Nome}' criado", "Projeto", projeto.Id,
            () => _projetos.Remove(projeto));

        // 4. devolver
        return Resultado.Ok($"Projeto #{projeto.Id} '{projeto.Nome}' criado.");
    }

    public Resultado EditarProjeto(int id, string nome, string? descricao)
    {
        var projeto = ObterProjeto(id);
        if (projeto == null)
            return Resultado.Erro("Projeto não encontrado.");
        if (string.IsNullOrWhiteSpace(nome))
            return Resultado.Erro("O nome do projeto é obrigatório.");

        var nomeAntigo = projeto.Nome;
        var descricaoAntiga = projeto.Descricao;

        projeto.Nome = nome.Trim();
        projeto.Descricao = LimparTexto(descricao);

        Registrar("Editar", $"Projeto #{id} renomeado de '{nomeAntigo}' para '{projeto.Nome}'", "Projeto", id,
            () =>
            {
                projeto.Nome = nomeAntigo;
                projeto.Descricao = descricaoAntiga;
            });

        return Resultado.Ok($"Projeto #{id} atualizado.");
    }

    public Resultado RemoverProjeto(int id)
    {
        var indice = _projetos.FindIndex(p => p.Id == id);
        if (indice < 0)
            return Resultado.Erro("Projeto não encontrado.");

        var projeto = _projetos[indice];
        var tarefasRemovidas = _tarefas.Where(t => t.ProjetoId == id).ToList();

        // Dependências que envolvem essas tarefas saem junto (e voltam no undo)
        var dependenciasRemovidas = new List<Dependencia>();
        foreach (var t in tarefasRemovidas)
            dependenciasRemovidas.AddRange(_grafo.RemoverDependenciasTarefa(t.Id));

        _tarefas.RemoveAll(t => t.ProjetoId == id);
        _projetos.RemoveAt(indice);

        Registrar("Excluir", $"Projeto #{id} '{projeto.Nome}' removido ({tarefasRemovidas.Count} tarefa(s))", "Projeto", id,
            () =>
            {
                _projetos.Insert(Math.Min(indice, _projetos.Count), projeto);
                _tarefas.AddRange(tarefasRemovidas);
                foreach (var d in dependenciasRemovidas)
                    _grafo.AdicionarDependencia(d.TarefaId, d.TarefaDependenciaId);
            });

        return Resultado.Ok($"Projeto #{id} removido com {tarefasRemovidas.Count} tarefa(s).");
    }

    // =====================================================================
    // TAREFAS
    // =====================================================================

    public IReadOnlyList<Tarefa> ListarTarefas(int? projetoId)
        => _tarefas.Where(t => projetoId == null || t.ProjetoId == projetoId).ToList();

    public Tarefa? ObterTarefa(int id) => _tarefas.FirstOrDefault(t => t.Id == id);

    public Resultado CriarTarefa(int projetoId, string titulo, string? descricao, string? responsavel, int prioridade, DateTime? prazo)
    {
        if (ObterProjeto(projetoId) == null)
            return Resultado.Erro("Projeto não encontrado.");
        if (string.IsNullOrWhiteSpace(titulo))
            return Resultado.Erro("O título da tarefa é obrigatório.");
        if (prioridade < 1 || prioridade > 5)
            return Resultado.Erro("A prioridade deve estar entre 1 (mais alta) e 5 (mais baixa).");

        var tarefa = new Tarefa
        {
            Id = _proximaTarefaId++,
            ProjetoId = projetoId,
            Titulo = titulo.Trim(),
            Descricao = LimparTexto(descricao),
            Responsavel = LimparTexto(responsavel),
            Prioridade = prioridade,
            Prazo = prazo,
            Status = StatusTarefa.ToDo
        };
        _tarefas.Add(tarefa);

        Registrar("Criar", $"Tarefa #{tarefa.Id} '{tarefa.Titulo}' criada", "Tarefa", tarefa.Id,
            () => _tarefas.Remove(tarefa));

        return Resultado.Ok($"Tarefa #{tarefa.Id} '{tarefa.Titulo}' criada.");
    }

    public Resultado EditarTarefa(int id, string titulo, string? descricao, string? responsavel, int prioridade, DateTime? prazo)
    {
        var tarefa = ObterTarefa(id);
        if (tarefa == null)
            return Resultado.Erro("Tarefa não encontrada.");
        if (string.IsNullOrWhiteSpace(titulo))
            return Resultado.Erro("O título da tarefa é obrigatório.");
        if (prioridade < 1 || prioridade > 5)
            return Resultado.Erro("A prioridade deve estar entre 1 (mais alta) e 5 (mais baixa).");

        var tituloAntigo = tarefa.Titulo;
        var descricaoAntiga = tarefa.Descricao;
        var responsavelAntigo = tarefa.Responsavel;
        var prioridadeAntiga = tarefa.Prioridade;
        var prazoAntigo = tarefa.Prazo;

        tarefa.Titulo = titulo.Trim();
        tarefa.Descricao = LimparTexto(descricao);
        tarefa.Responsavel = LimparTexto(responsavel);
        tarefa.Prioridade = prioridade;
        tarefa.Prazo = prazo;

        Registrar("Editar", $"Tarefa #{id} '{tituloAntigo}' editada", "Tarefa", id,
            () =>
            {
                tarefa.Titulo = tituloAntigo;
                tarefa.Descricao = descricaoAntiga;
                tarefa.Responsavel = responsavelAntigo;
                tarefa.Prioridade = prioridadeAntiga;
                tarefa.Prazo = prazoAntigo;
            });

        return Resultado.Ok($"Tarefa #{id} atualizada.");
    }

    public Resultado MudarStatusTarefa(int id, StatusTarefa novoStatus)
    {
        var tarefa = ObterTarefa(id);
        if (tarefa == null)
            return Resultado.Erro("Tarefa não encontrada.");

        if (tarefa.Status == novoStatus)
            return Resultado.Ok("A tarefa já estava neste status.");

        // Regra das dependências: não avança se alguma predecessora não estiver concluída
        var avanca = novoStatus is StatusTarefa.InProgress or StatusTarefa.Review or StatusTarefa.Done;
        if (avanca && !_grafo.PodeIniciar(id, _tarefas))
            return Resultado.Erro("Há tarefas predecessoras ainda não concluídas.");

        var anterior = tarefa.Status;
        tarefa.Status = novoStatus;

        Registrar("MudarStatus", $"Tarefa #{id} movida de '{anterior}' para '{novoStatus}'", "Tarefa", id,
            () => tarefa.Status = anterior);

        return Resultado.Ok($"Tarefa #{id} movida para '{novoStatus}'.");
    }

    public Resultado RemoverTarefa(int id)
    {
        var indice = _tarefas.FindIndex(t => t.Id == id);
        if (indice < 0)
            return Resultado.Erro("Tarefa não encontrada.");

        var tarefa = _tarefas[indice];
        var dependenciasRemovidas = _grafo.RemoverDependenciasTarefa(id);
        _tarefas.RemoveAt(indice);

        Registrar("Excluir", $"Tarefa #{id} '{tarefa.Titulo}' removida", "Tarefa", id,
            () =>
            {
                _tarefas.Insert(Math.Min(indice, _tarefas.Count), tarefa);
                foreach (var d in dependenciasRemovidas)
                    _grafo.AdicionarDependencia(d.TarefaId, d.TarefaDependenciaId);
            });

        return Resultado.Ok($"Tarefa #{id} removida.");
    }

    // Tarefas que podem começar agora (predecessoras concluídas). A regra mora no grafo.
    public IReadOnlyList<Tarefa> ObterTarefasProntas() => _grafo.ObterTarefasProntas(_tarefas);

    // Fila: menor número = mais prioridade; empate resolvido pelo prazo. Concluídas ficam de fora.
    public IReadOnlyList<Tarefa> ObterFilaPrioridade()
    {
        var fila = new FilaPrioridade();
        foreach (var t in _tarefas.Where(t => t.Status != StatusTarefa.Done))
            fila.Enfileirar(t);
        return fila.ObterTodas();
    }

    // =====================================================================
    // DEPENDÊNCIAS
    // =====================================================================

    public Resultado AdicionarDependencia(int tarefaId, int tarefaDependenciaId)
    {
        if (ObterTarefa(tarefaId) == null || ObterTarefa(tarefaDependenciaId) == null)
            return Resultado.Erro("Tarefa não encontrada.");
        if (tarefaId == tarefaDependenciaId)
            return Resultado.Erro("Uma tarefa não pode depender de si mesma.");
        if (_grafo.ObterPredecessores(tarefaId).Contains(tarefaDependenciaId))
            return Resultado.Erro("Essa dependência já existe.");

        // O grafo devolve só true/false; já descartamos autodependência e duplicata,
        // então a única recusa que sobra é o ciclo.
        if (!_grafo.AdicionarDependencia(tarefaId, tarefaDependenciaId))
            return Resultado.Erro("Essa dependência criaria um ciclo.");

        Registrar("Criar", $"Tarefa #{tarefaId} agora depende da #{tarefaDependenciaId}", "Dependencia", tarefaId,
            () => _grafo.RemoverDependencia(tarefaId, tarefaDependenciaId));

        return Resultado.Ok($"Tarefa #{tarefaId} agora depende da #{tarefaDependenciaId}.");
    }

    public Resultado RemoverDependencia(int tarefaId, int tarefaDependenciaId)
    {
        if (!_grafo.RemoverDependencia(tarefaId, tarefaDependenciaId))
            return Resultado.Erro("Dependência não encontrada.");

        Registrar("Excluir", $"Dependência removida: #{tarefaId} não depende mais da #{tarefaDependenciaId}", "Dependencia", tarefaId,
            () => _grafo.AdicionarDependencia(tarefaId, tarefaDependenciaId));

        return Resultado.Ok("Dependência removida.");
    }

    public IReadOnlyList<int> ObterPredecessores(int tarefaId) => _grafo.ObterPredecessores(tarefaId);

    public IReadOnlyList<int> ObterOrdemExecucao(int? projetoId)
    {
        var tarefas = _tarefas.Where(t => projetoId == null || t.ProjetoId == projetoId).ToList();
        var ids = tarefas.Select(t => t.Id).ToHashSet();

        // O DFS do grafo pode passar por tarefas de outros projetos: filtramos pelo escopo pedido.
        return _grafo.OrdenacaoTopologica(tarefas).Where(ids.Contains).ToList();
    }

    // =====================================================================
    // FINANCEIRO
    // =====================================================================

    public IReadOnlyList<TarefaFinanceira> ListarContas()
    {
        SincronizarAtrasos();
        return _contas.ToList();
    }

    public TarefaFinanceira? ObterConta(int id)
    {
        SincronizarAtrasos();
        return _contas.FirstOrDefault(c => c.Id == id);
    }

    public Resultado CriarConta(string titulo, decimal valor, DateTime dataVencimento, TipoFinanceiro tipo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            return Resultado.Erro("O título da conta é obrigatório.");
        if (valor <= 0)
            return Resultado.Erro("O valor deve ser maior que zero.");

        var conta = new TarefaFinanceira
        {
            Id = _proximaContaId++,
            Titulo = titulo.Trim(),
            Valor = valor,
            DataVencimento = dataVencimento,
            Tipo = tipo,
            Status = StatusFinanceiro.Pendente
        };
        _contas.Add(conta);

        Registrar("Criar", $"Conta #{conta.Id} '{conta.Titulo}' criada", "TarefaFinanceira", conta.Id,
            () => _contas.Remove(conta));

        return Resultado.Ok($"Conta #{conta.Id} '{conta.Titulo}' criada.");
    }

    public Resultado EditarConta(int id, string titulo, decimal valor, DateTime dataVencimento)
    {
        var conta = _contas.FirstOrDefault(c => c.Id == id);
        if (conta == null)
            return Resultado.Erro("Conta não encontrada.");
        if (string.IsNullOrWhiteSpace(titulo))
            return Resultado.Erro("O título da conta é obrigatório.");
        if (valor <= 0)
            return Resultado.Erro("O valor deve ser maior que zero.");

        var tituloAntigo = conta.Titulo;
        var valorAntigo = conta.Valor;
        var vencimentoAntigo = conta.DataVencimento;

        conta.Titulo = titulo.Trim();
        conta.Valor = valor;
        conta.DataVencimento = dataVencimento;

        Registrar("Editar", $"Conta #{id} '{tituloAntigo}' editada", "TarefaFinanceira", id,
            () =>
            {
                conta.Titulo = tituloAntigo;
                conta.Valor = valorAntigo;
                conta.DataVencimento = vencimentoAntigo;
            });

        return Resultado.Ok($"Conta #{id} atualizada.");
    }

    public Resultado PagarConta(int id)
    {
        var conta = _contas.FirstOrDefault(c => c.Id == id);
        if (conta == null)
            return Resultado.Erro("Conta não encontrada.");
        if (conta.Status == StatusFinanceiro.Paga)
            return Resultado.Erro("Essa conta já está paga.");

        var statusAnterior = conta.Status;
        var pagamentoAnterior = conta.DataPagamento;

        conta.Status = StatusFinanceiro.Paga;
        conta.DataPagamento = DateTime.Now;

        Registrar("Pagar", $"Conta #{id} '{conta.Titulo}' paga", "TarefaFinanceira", id,
            () =>
            {
                conta.Status = statusAnterior;
                conta.DataPagamento = pagamentoAnterior;
            });

        return Resultado.Ok($"Conta #{id} marcada como paga.");
    }

    public Resultado RemoverConta(int id)
    {
        var indice = _contas.FindIndex(c => c.Id == id);
        if (indice < 0)
            return Resultado.Erro("Conta não encontrada.");

        var conta = _contas[indice];
        _contas.RemoveAt(indice);

        Registrar("Excluir", $"Conta #{id} '{conta.Titulo}' removida", "TarefaFinanceira", id,
            () => _contas.Insert(Math.Min(indice, _contas.Count), conta));

        return Resultado.Ok($"Conta #{id} removida.");
    }

    public Resultado DefinirSalario(decimal valor)
    {
        if (valor <= 0)
            return Resultado.Erro("O salário deve ser maior que zero.");

        var anterior = _salario;
        _salario = valor;

        Registrar("Editar", $"Salário alterado de {anterior:C} para {valor:C}", "Salario", null,
            () => _salario = anterior);

        return Resultado.Ok("Salário atualizado.");
    }

    public decimal ObterSalario() => _salario;

    // Regra adotada: salário menos as DESPESAS ainda não pagas (receitas não entram).
    // Não filtra por mês. Se quiserem, é só mudar aqui, num lugar só.
    public decimal ObterSaldoLivre()
        => _salario - _contas
            .Where(c => c.Tipo == TipoFinanceiro.Despesa && c.Status != StatusFinanceiro.Paga)
            .Sum(c => c.Valor);

    public IReadOnlyList<TarefaFinanceira> ObterContasAtrasadas()
    {
        SincronizarAtrasos();
        return _contas.Where(c => c.Status == StatusFinanceiro.Atrasada).ToList();
    }

    public IReadOnlyList<TarefaFinanceira> ObterContasProximasVencimento(int dias)
    {
        var hoje = DateTime.Today;
        var limite = hoje.AddDays(dias);

        return _contas
            .Where(c => c.Status != StatusFinanceiro.Paga
                        && c.DataVencimento.Date >= hoje
                        && c.DataVencimento.Date <= limite)
            .OrderBy(c => c.DataVencimento)
            .ToList();
    }

    // =====================================================================
    // HISTÓRICO / UNDO
    // =====================================================================

    public IReadOnlyList<HistoricoAcao> ListarHistorico() => _undo.ObterTodas();

    public HistoricoAcao? ObterUltimaAcao() => _undo.OlharTopo();

    public Resultado DesfazerUltimaAcao()
    {
        var acao = _undo.Desempilhar();
        if (acao == null)
            return Resultado.Erro("Não há nenhuma ação para desfazer.");
        if (acao.Desfazer == null)
            return Resultado.Erro("Essa ação não pode ser desfeita.");

        acao.Desfazer();
        return Resultado.Ok($"Desfeito: {acao.Descricao}");
    }
}