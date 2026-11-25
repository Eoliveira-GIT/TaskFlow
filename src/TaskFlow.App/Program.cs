using TaskFlow.App.Navigation;

namespace TaskFlow.App
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sem isso, acentos e emojis aparecem como "?" no console do Windows.
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            MenuNavigator navigator;
            try
            {
                navigator = new MenuNavigator();
            }
            catch (Exception ex)
            {
                // Ex.: taskflow.db aberto em outro programa ou corrompido.
                EntradaUsuario.Erro($"Não foi possível abrir o banco de dados: {ex.Message}");
                Console.WriteLine("Feche outros programas que usem o taskflow.db (ou apague o arquivo para recomeçar) e tente de novo.");
                return;
            }

            navigator.Iniciar();
        }
    }
}
