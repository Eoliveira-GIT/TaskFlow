using System;
using System.Globalization;

namespace TaskFlow.App.Navigation
{
    public static class EntradaUsuario
    {
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
                var texto = Console.ReadLine()?.Trim();
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
                var texto = Console.ReadLine()?.Trim().Replace(',', '.');

                if (string.IsNullOrWhiteSpace(texto))
                    return atual ?? 0m;

                if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) && valor > 0)
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

                if (DateTime.TryParseExact(texto, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                    return data;

                Erro("Data inválida. Use o formato dd/MM/aaaa (ou Enter para manter).");
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
