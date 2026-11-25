using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.App.Views
{
    /// <summary>
    /// Rótulos e formatos usados por todas as telas.
    /// Fica num lugar só para "Concluída", "R$ 1.500,00" etc. saírem iguais em todo o sistema.
    /// </summary>
    public static class Formatacao
    {
        private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

        public static string Moeda(decimal valor) => valor.ToString("C", CulturaBr);

        public static string Status(StatusTarefa status) => status switch
        {
            StatusTarefa.ToDo => "A Fazer",
            StatusTarefa.InProgress => "Em Andamento",
            StatusTarefa.Review => "Em Revisão",
            StatusTarefa.Done => "Concluída",
            StatusTarefa.Bloqueada => "Bloqueada",
            _ => status.ToString()
        };

        // No Domain, 1 é a prioridade mais alta e 5 a mais baixa.
        public static string Prioridade(int prioridade) => prioridade switch
        {
            1 => "1-Urgente",
            2 => "2-Alta",
            3 => "3-Média",
            4 => "4-Baixa",
            5 => "5-Mínima",
            _ => prioridade.ToString()
        };

        public static string StatusConta(StatusFinanceiro status) => status switch
        {
            StatusFinanceiro.Paga => "Paga",
            StatusFinanceiro.Atrasada => "Atrasada",
            _ => "Pendente"
        };

        public static string Prazo(DateTime? prazo, string formato = "dd/MM")
            => prazo.HasValue ? prazo.Value.ToString(formato) : "-";

        public static string Responsavel(Tarefa tarefa)
            => string.IsNullOrWhiteSpace(tarefa.Responsavel) ? "-" : tarefa.Responsavel!;

        // Atrasada = tem prazo, o prazo já passou e ainda não foi concluída.
        public static bool Atrasada(Tarefa tarefa)
            => tarefa.Status != StatusTarefa.Done
               && tarefa.Prazo.HasValue
               && tarefa.Prazo.Value.Date < DateTime.Today;

        public static string ListaIds(IEnumerable<int> ids)
            => string.Join(", ", ids.Select(id => $"#{id}"));

        // Texto curto que explica por que a tarefa pode ou não ser feita agora.
        public static string Situacao(ITaskFlowService service, Tarefa tarefa)
        {
            if (tarefa.Status == StatusTarefa.Done)
                return "Concluída";

            var pendentes = service.ObterPredecessorasPendentes(tarefa.Id);
            if (pendentes.Count > 0)
                return $"Aguarda {ListaIds(pendentes)}";

            if (tarefa.Status == StatusTarefa.Bloqueada)
                return "Bloqueada (manual)";

            return Atrasada(tarefa) ? "Liberada - ATRASADA" : "Liberada";
        }

        public static string Truncar(string texto, int max)
        {
            if (texto.Length <= max)
                return texto;
            return texto.Substring(0, max - 1) + "…";
        }

        public static void LimparTela()
        {
            try { Console.Clear(); } catch { }
        }

        public static void Titulo(string texto, ConsoleColor cor)
        {
            Console.ForegroundColor = cor;
            Console.WriteLine($">>> {texto} <<<\n");
            Console.ResetColor();
        }

        public static void Secao(string texto, ConsoleColor cor)
        {
            Console.ForegroundColor = cor;
            Console.WriteLine($"\n=== {texto} ===");
            Console.ResetColor();
        }
    }
}
