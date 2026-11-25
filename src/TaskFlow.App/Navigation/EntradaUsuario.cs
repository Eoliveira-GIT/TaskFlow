using System;
using System.Globalization;

namespace TaskFlow.App.Navigation
{
    public static class EntradaUsuario
    {
        private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

        // Aceita "05/03/2026" e também "5/3/2026".
        private static readonly string[] FormatosData = { "dd/MM/yyyy", "d/M/yyyy" };

        public static void Erro(string mensagem)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(mensagem);
            Console.ResetColor();
        }

        public static string LerTextoObrigatorio(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var linha = Console.ReadLine();
                if (linha == null) // fim da entrada: sem isso, o laço pediria o texto para sempre
                    throw new EndOfStreamException("A entrada terminou antes de o texto ser digitado.");

                var texto = linha.Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                    return texto;

                Erro("Entrada inválida. Digite novamente.");
            }
        }

        public static string LerTextoOpcional(string prompt, string atual)
        {
            Console.Write(prompt);
            var texto = Console.ReadLine()?.Trim();
            return string.IsNullOrWhiteSpace(texto) ? atual : texto;
        }

        public static int LerInteiro(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var texto = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                    return 0;

                if (int.TryParse(texto, out var valor) && valor > 0)
                    return valor;

                Erro("Valor inválido. Digite um número positivo ou Enter para cancelar.");
            }
        }

        public static decimal LerDecimal(string prompt, decimal? atual)
        {
            while (true)
            {
                Console.Write(prompt);
                var texto = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                    return atual ?? 0m;

                // Com vírgula, lê no padrão brasileiro ("1.500,00" = 1500).
                // Sem vírgula, aceita ponto como decimal ("150.50" = 150,5).
                var cultura = texto.Contains(',') ? CulturaBr : CultureInfo.InvariantCulture;
                if (decimal.TryParse(texto, NumberStyles.Number, cultura, out var valor) && valor > 0)
                    return valor;

                Erro("Valor inválido. Digite um número maior que zero.");
            }
        }

        public static DateTime LerData(string prompt, DateTime atual)
        {
            while (true)
            {
                Console.Write(prompt);
                var texto = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                    return atual;

                if (DateTime.TryParseExact(texto, FormatosData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                    return data;

                Erro("Data inválida. Use o formato dd/MM/aaaa (ou Enter para manter).");
            }
        }

        // Enter mantém o valor atual; "-" apaga (devolve null).
        public static string? LerTextoOpcionalOuLimpar(string prompt, string? atual)
        {
            Console.Write(prompt);
            var texto = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return atual;

            return texto == "-" ? null : texto;
        }

        // Para prazos, que podem não existir: Enter mantém o atual, "-" deixa sem prazo.
        public static DateTime? LerDataOpcional(string prompt, DateTime? atual)
        {
            while (true)
            {
                Console.Write(prompt);
                var texto = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                    return atual;

                if (texto == "-")
                    return null;

                if (DateTime.TryParseExact(texto, FormatosData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                    return data;

                Erro("Data inválida. Use dd/MM/aaaa, Enter para manter ou '-' para ficar sem prazo.");
            }
        }

        // Prioridade do Domain: 1 (urgente) a 5 (mínima). Enter mantém a atual.
        public static int LerPrioridade(string prompt, int atual)
        {
            while (true)
            {
                Console.Write(prompt);
                var texto = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(texto))
                    return atual;

                if (int.TryParse(texto, out var valor) && valor >= 1 && valor <= 5)
                    return valor;

                Erro("Prioridade inválida. Digite de 1 (urgente) a 5 (mínima).");
            }
        }

        public static bool Confirmar(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var texto = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (texto == "s" || texto == "sim")
                    return true;

                if (string.IsNullOrEmpty(texto) || texto == "n" || texto == "nao" || texto == "não")
                    return false;

                Erro("Responda 's' para sim ou 'n' para não.");
            }
        }
    }
}
