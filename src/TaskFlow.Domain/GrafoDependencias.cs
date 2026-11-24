namespace TaskFlow.Domain;

public class GrafoDependencias
{
    // Lista de todas as dependencias
    private List<Dependencia> _dependencias = new();

    // ==================================================================
    // 1. Adiciona uma dependencia (A depende de B) Com detecção de ciclo.
    // ====================================================================
    public bool AdicionarDependencia(int tarefaId, int tarefaDependenciaId)
    {
        // Não pode depender de si mesma
        if (tarefaId == tarefaDependenciaId)
            return false;

        
        // Verificar se ja existe essa dependecia
        if(_dependencias.Any(d => d.TarefaId == tarefaId && d.TarefaDependenciaId == tarefaDependenciaId))
            return false;

        
        // Verificar se criar essa dependencia formaria um ciclo
        if (FormariaCiclo(tarefaId, tarefaDependenciaId))
            return false;

        var novaDependencia = new Dependencia
        {
            TarefaId = tarefaId,
            TarefaDependenciaId = tarefaDependenciaId
        };

        _dependencias.Add(novaDependencia);
        return true;
    }

    // ================================
    // 2. Detectar se formaria ciclo
    // ================================
    private bool FormariaCiclo(int tarefaId, int tarefaDependenciaId)
    {
        // Simular a adição e vereficar se consegue chegar de volta
        var visitados = new HashSet<int>();
       return ExisteCaminho(tarefaId, tarefaDependenciaId, visitados);
    }

    private bool ExisteCaminho(int atual, int destino, HashSet<int> visitados)
    {
        if (atual == destino)
            return true;

        if (visitados.Contains(atual))
            return false;

        visitados.Add(atual);

        var proximos = _dependencias
            .Where(d => d.TarefaDependenciaId == atual)
            .Select(d => d.TarefaId);

        foreach (var proximo in proximos)
        {
            if (ExisteCaminho(proximo, destino, visitados))
                return true;
        }

        return false;
    }

    //========================================
    // 3. Obter predecessores de uma tarefa
    //========================================
    public List<int> ObterPredecessores(int tarefraId)
    {
        return _dependencias
            .Where(d => d.TarefaId == tarefraId)
            .Select(d => d.TarefaDependenciaId)
            .ToList();
    }

    //===============================================
    // 4. Verificar se uma tarefa pode ser iniciada
    // (todas as predecessores estão concluidas)
    //===============================================
    public bool PodeIniciar(int tarefaId, List<Tarefa> todasTarefas)
    {
        var predecessores = ObterPredecessores(tarefaId);

        if (predecessores.Count == 0)
            return true;   // Não tem dependencia, pode iniciar

        foreach (var predId in predecessores)
        {
            var tarefaPred = todasTarefas.FirstOrDefault(t => t.Id == predId);
            if (tarefaPred == null || tarefaPred.Status != StatusTarefa.Done)
                return false;
        }

        return true;
    }

    //=========================================
    // 5. Listar tarefas prontas para iniciar
    //=========================================
    public List<Tarefa> ObterTarefasProntas(List<Tarefa> todasTarefas)
    {
        return todasTarefas
            .Where(t => t.Status == StatusTarefa.ToDo || t.Status == StatusTarefa.Bloqueada)
            .Where(t => PodeIniciar(t.Id, todasTarefas))
            .ToList();
    }

    //======================================================
    // 6. Ordenação topologica (ordem sugerida de execução)
    //======================================================
    public List<int> OrdenacaoTopologica(List<Tarefa> todasTarefas)
    {
        var resultado = new List<int>();
        var visitados = new HashSet<int>();
        var emPrecessamento = new HashSet<int>();

        foreach (var tarefa in todasTarefas)
        {
            if (!visitados.Contains(tarefa.Id))
            {
                if (!DfsTopologico(tarefa.Id, visitados, emProcessamento: emPrecessamento, resultado: resultado, todasTarefas: todasTarefas))
                {
                    // Ciclo detectado (não deveria acontecer se AdicionarDependencia estiver correto)
                    return new List<int>();
                }
            }
        }

        resultado.Reverse(); // A ordem do DFS sai invertida
        return resultado;
    }

    private bool DfsTopologico(int atual, HashSet<int> visitados, HashSet<int> emProcessamento, List<int> resultado, List<Tarefa> todasTarefas)
    {
        if (emProcessamento.Contains(atual))
            return false;  // Ciclo

        if (visitados.Contains(atual))
            return true;

        emProcessamento.Add(atual);

       var sucessores = _dependencias
            .Where(d => d.TarefaDependenciaId == atual)
            .Select(d => d.TarefaId);


        foreach (var sucessor in sucessores)
        {
            if (!DfsTopologico(sucessor, visitados, emProcessamento, resultado, todasTarefas))
            return false;
        }

        emProcessamento.Remove(atual);
        visitados.Add(atual);
        resultado.Add(atual);

        return true;
    }

    //==============================
    // Metodos auxiliares
    //============================== 
    public List<Dependencia> ObterTodas()
    {
        return new List<Dependencia>(_dependencias);
    }

    public void Limpar()
    {
        _dependencias.Clear();
    }
}