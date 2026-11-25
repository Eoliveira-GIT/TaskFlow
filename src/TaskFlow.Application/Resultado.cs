namespace TaskFlow.Application;

/// <summary>
/// Resultado de uma operação que pode falhar (criar, editar, remover, pagar...).
/// Use <see cref="Ok"/> quando deu certo e <see cref="Erro"/> quando falhou,
/// explicando o motivo para a interface exibir.
/// </summary>
public class Resultado
{
    public bool Sucesso { get; }
    public string? Mensagem { get; }

    private Resultado(bool sucesso, string? mensagem = null)
    {
        Sucesso = sucesso;
        Mensagem = mensagem;
    }

    public static Resultado Ok(string? mensagem = null) => new(true, mensagem);

    public static Resultado Erro(string mensagem) => new(false, mensagem);
}