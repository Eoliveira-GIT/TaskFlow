using TaskFlow.App.Navigation;

namespace TaskFlow.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var navigator = new MenuNavigator();
            navigator.Iniciar();
        }
    }
}
