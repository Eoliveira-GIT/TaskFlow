using TaskFlow.Domain;

public static class GrafoDependenciasTestes
{
    private static int _falhas;

    public static int Executar()
    {
        Console.WriteLine("\n=== Testes automáticos do GrafoDependencias ===\n");
        _falhas = 0;

        // Regras já funcionando
        var g1 = CriarGrafoDeExemplo();
        Verificar("tarefa não pode depender de si mesma", false, g1.AdicionarDependencia(1, 1));

        var g2 = CriarGrafoDeExemplo();
        Verificar("dependência duplicada é recusada", false, g2.AdicionarDependencia(2, 1));

        // Detecção de ciclo
        var g3 = new GrafoDependencias();
        g3.AdicionarDependencia(2, 1);
        Verificar("ciclo direto: 1 depende de 2 (2 já depende de 1)", false, g3.AdicionarDependencia(1, 2));

        var g4 = CriarGrafoDeExemplo();
        Verificar("ciclo indireto: 1 depende de 5", false, g4.AdicionarDependencia(1, 5));

        var g5 = CriarGrafoDeExemplo();
        Verificar("dependência redundante (não é ciclo): 5 depende de 1", true, g5.AdicionarDependencia(5, 1));

        // O grafo não pode ficar "sujo" depois de recusar um ciclo
        var g6 = CriarGrafoDeExemplo();
        g6.AdicionarDependencia(1, 5);
        Verificar("recusar ciclo não altera o grafo (continua com 5 dependências)", 5, g6.ObterTodas().Count);

        // A ordenação topológica depende do grafo estar sem ciclos
        var g7 = CriarGrafoDeExemplo();
        g7.AdicionarDependencia(1, 5);
        var ordem = g7.OrdenacaoTopologica(CriarTarefas());
        Verificar("ordem de execução tem as 5 tarefas", 5, ordem.Count);
        Verificar("a primeira da ordem é a tarefa 1", 1, ordem.FirstOrDefault());
        Verificar("a última da ordem é a tarefa 5", 5, ordem.LastOrDefault());

        Console.WriteLine(_falhas == 0
            ? "\nTodos os testes passaram."
            : $"\n{_falhas} verificação(ões) falharam.");

        return _falhas == 0 ? 0 : 1;
    }

    // 1 <- 2 <- 3,4 <- 5  (cada seta é "depende de")
    private static GrafoDependencias CriarGrafoDeExemplo()
    {
        var g = new GrafoDependencias();
        g.AdicionarDependencia(2, 1);
        g.AdicionarDependencia(3, 2);
        g.AdicionarDependencia(4, 2);
        g.AdicionarDependencia(5, 3);
        g.AdicionarDependencia(5, 4);
        return g;
    }

    private static List<Tarefa> CriarTarefas() =>
        Enumerable.Range(1, 5)
            .Select(id => new Tarefa { Id = id, Titulo = $"Tarefa {id}", Status = StatusTarefa.ToDo })
            .ToList();

    private static void Verificar<T>(string nome, T esperado, T obtido)
    {
        bool ok = EqualityComparer<T>.Default.Equals(esperado, obtido);
        if (!ok) _falhas++;

        Console.WriteLine(ok
            ? $"[PASSOU] {nome}"
            : $"[FALHOU] {nome} -> esperado: {esperado}, obtido: {obtido}");
    }
}