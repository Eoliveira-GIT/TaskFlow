namespace TaskFlow.Domain;

public class PilhaUndo
{
    private Stack<HistoricoAcao> _pilha = new();

    // Empilha uma ação (quando o Usuario faz algo)
    public void Empilhar(HistoricoAcao acao)
    {
        _pilha.Push(acao);
    }

    // Desempilha a ultima ação (para desfazer)
    public HistoricoAcao? Desempilhar()
    {
        if (_pilha.Count == 0)
        return null;

        return _pilha.Pop();
    }

    // Olha a ultima ação sem remover
    public HistoricoAcao? OlharTopo()
    {
        if (_pilha.Count == 0)
        return null; 

        return _pilha.Peek();
    }

    // Quantidade de ações na pilha
    public int Quantidade => _pilha.Count;

    // Limpa a pilha
    public void Limpar()
    {
        _pilha.Clear();
    }
    
    // Mais recente primeiro (e a ordem natural de enumeração de uma Stack)
    public List<HistoricoAcao> ObterTodas() => _pilha.ToList();

    /// Como reverter esta ação. Fica só em memória (não será persistido).
    public Action? Desfazer { get; set; }
}