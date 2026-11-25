using System;
using TaskFlow.App.Navigation;
using TaskFlow.Application;

namespace TaskFlow.App.Views
{
    /// <summary>Helpers de saída no console compartilhados pelas telas.</summary>
    public static class Saida
    {
        // Todo comando termina igual: sucesso em verde, erro com a mensagem do serviço.
        public static void MostrarResultado(Resultado resultado)
        {
            if (resultado.Sucesso)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {resultado.Mensagem}");
                Console.ResetColor();
            }
            else
            {
                EntradaUsuario.Erro(resultado.Mensagem ?? "Não foi possível concluir a operação.");
            }
            Pausar();
        }

        public static void Pausar()
        {
            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }
    }
}