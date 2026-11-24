using TaskFlow.Domain;

Console.WriteLine("=== Teste do Grafo de Dependências ===\n");

var grafo = new GrafoDependencias();

// Criando algumas tarefas fictícias
var tarefas = new List<Tarefa>
{
    new Tarefa { Id = 1, Titulo = "Analisar requisitos", Status = StatusTarefa.Done },
    new Tarefa { Id = 2, Titulo = "Criar modelagem", Status = StatusTarefa.ToDo },
    new Tarefa { Id = 3, Titulo = "Implementar backend", Status = StatusTarefa.ToDo },
    new Tarefa { Id = 4, Titulo = "Criar frontend", Status = StatusTarefa.ToDo },
    new Tarefa { Id = 5, Titulo = "Testes finais", Status = StatusTarefa.ToDo }
};

// Adicionando dependências
Console.WriteLine("Adicionando dependências...");
Console.WriteLine($"2 depende de 1: {grafo.AdicionarDependencia(2, 1)}");
Console.WriteLine($"3 depende de 2: {grafo.AdicionarDependencia(3, 2)}");
Console.WriteLine($"4 depende de 2: {grafo.AdicionarDependencia(4, 2)}");
Console.WriteLine($"5 depende de 3: {grafo.AdicionarDependencia(5, 3)}");
Console.WriteLine($"5 depende de 4: {grafo.AdicionarDependencia(5, 4)}");

// Tentando criar um ciclo (deve falhar)
Console.WriteLine($"\nTentando criar ciclo (1 depende de 5): {grafo.AdicionarDependencia(1, 5)}");

// Testando tarefas prontas
Console.WriteLine("\n=== Tarefas prontas para iniciar ===");
var prontas = grafo.ObterTarefasProntas(tarefas);
foreach (var t in prontas)
{
    Console.WriteLine($"- {t.Id}: {t.Titulo}");
}

// Ordenação topológica
Console.WriteLine("\n=== Ordem sugerida de execução ===");
var ordem = grafo.OrdenacaoTopologica(tarefas);
Console.WriteLine(string.Join(" → ", ordem));

Console.WriteLine("\nTeste finalizado!");

return GrafoDependenciasTestes.Executar();