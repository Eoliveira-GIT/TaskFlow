namespace TaskFlow.App.MockServices
{
    public enum PrioridadeMock
    {
        Baixa = 0,
        Media = 1,
        Alta = 2
    }

    public enum StatusTarefaMock
    {
        AFazer = 0,
        EmAndamento = 1,
        Concluida = 2,
        Bloqueada = 3
    }

    public enum StatusContaMock
    {
        Pendente = 0,
        Paga = 1,
        Atrasada = 2
    }

    public class ProjetoMock
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class TarefaMock
    {
        public int Id { get; set; }
        public int ProjetoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Responsavel { get; set; }
        public PrioridadeMock Prioridade { get; set; }
        public StatusTarefaMock Status { get; set; }
        public System.DateTime Prazo { get; set; }
    }

    public class ContaMock
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public StatusContaMock Status { get; set; }
        public System.DateTime Vencimento { get; set; }
    }

    public class AcaoHistorico
    {
        public string Descricao { get; set; } = string.Empty;
        public System.Action Desfazer { get; set; } = () => { };
    }
}
