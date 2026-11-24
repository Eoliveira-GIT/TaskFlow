namespace TaskFlow.Domain;

public class HistoricoAcao
{
    public int Id {get; set; }
    public DateTime DataHora {get; set; } = DateTime.Now;
    public string TipoAcao {get; set; } = string.Empty;   // Ex: "Criar", "Editar", "Excluir", "MudarStatus"
    public string Descricao { get; set; } = string.Empty;  //Ex: "Tarefa 'Estudar C# ' criada"
    public string? EntidadeAfetada {get; set; }            // Ex: "Tarefa", "projeto", "TarefaFinanceira"
    public int? EntidadeId {get; set; }                   // Id da tarefa/projeto afetado
    public string? dadosAnteriores {get; set; }          // (opcional) para conseguir fazer Undo depois
}