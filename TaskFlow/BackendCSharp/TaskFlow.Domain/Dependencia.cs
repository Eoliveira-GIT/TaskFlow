namespace TaskFlow.Domain;

public class Dependencia
{
    public int Id { get; set; }

    // A tarefa que depende de outra
    public int TarefaId {get; set; }
    public Tarefa? Tarefa {get; set; }

    // A tarefa da qual ela depende (prendecessora)
    public int TarefaDependenciaId {get; set; }
    public Tarefa? Tarefadependencia {get; set; }

    internal class Add
    {
    }
}