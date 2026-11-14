namespace TaskFlow.Domain;

public class Tarefa
{
    public int Id {get; set;} 
    public string Titulo {get; set;} = string.Empty;
    public string? Descricao {get; set;}
    public string? Responsavel { get; set;}
    public int Prioridade {get; set;} = 3; // 1 = mais alta, 5 = mais baixa
    public DateTime? Prazo {get; set;}
    public StatusTarefa Status {get; set;} = StatusTarefa.ToDo;

    // Relacionamento com o Projeto
    public int ProjetoId {get; set; }
    public Projeto? Projeto {get; set; }

    // Hierarquia (subtarefas)
    public int? TarefaPaiId {get; set; }      //null = tarefa principal
    public Tarefa? TarefaPai {get; set; }
    public List<Tarefa> Subtarefas {get; set; } = new();
}