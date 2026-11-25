namespace TaskFlow.Domain;

public class Projeto
{
    public int Id {get; set; }
    public string Nome {get; set;} = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set;} = DateTime.Now;
    public bool Ativo { get; set;} = true;


    // Lista de tarefas do projeto
    public List<Tarefa> Tarefas { get; set;} = new();
}
