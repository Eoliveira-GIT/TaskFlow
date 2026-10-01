namespace TaskFlow.Domain;

public class FilaPrioridade
{
    private List<Tarefa> _tarefas = new();

    // Adiciona uma tarefa na fila
    public void Enfileirar(Tarefa tarefa)
    {
        _tarefas.Add(tarefa);
        Ordenar();
    }

    // Remover e retorna a tarefa de maior prioridade
    public Tarefa? Desenfileirar()
    {
        if (_tarefas.Count == 0)
        return null;

        var tarefa = _tarefas[0];
        _tarefas.RemoveAt(0);
        return tarefa;
    }

    // Olha a proximo tarefa sem remover
    public Tarefa? OlharProxima()
    {
        if (_tarefas.Count == 0)
        return null;

        return _tarefas[0];
    }
    
    // Remover todas as tarevas ja ordenadas
    public List<Tarefa> ObterTodas()
    {
        return new List<Tarefa>(_tarefas);
    }

    // Osdenar por Prioridade (menor numero = mais prioridade) e depois por Prazo
    private void Ordenar()
    {
       _tarefas = _tarefas
            .OrderBy(t => t.Prioridade)
            .ThenBy(t => t.Prazo ?? DateTime.MaxValue)
            .ToList();
    }
}