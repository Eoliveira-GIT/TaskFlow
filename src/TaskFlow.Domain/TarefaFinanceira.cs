namespace TaskFlow.Domain;

public class TarefaFinanceira
{
    public int Id {get; set; }
    public string Titulo {get; set; } = string.Empty;     // Ex: "Conta de luz", "Salario"
    public string? Descricao {get; set; }
    public decimal Valor {get; set; }
    public DateTime DataVencimento {get; set; }
    public TipoFinanceiro Tipo {get; set; }            // Receita ou Despesa
    public StatusFinanceiro Status {get; set; } = StatusFinanceiro.Pendente;

    public DateTime? DataPagamento { get; set; }        // Quando foi pago
}