using System;
using System.Linq;
using TaskFlow.App.MockServices;
using TaskFlow.App.Navigation;

namespace TaskFlow.App.Views
{
    public static class FinanceiroView
    {
        public static void Renderizar()
        {
            MockDataStore.Inicializar();
            bool voltar = false;

            while (!voltar)
            {
                try { Console.Clear(); } catch { }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(">>> PAINEL FINANCEIRO PESSOAL <<<\n");
                Console.ResetColor();

                Console.WriteLine($"Salário: {MockDataStore.FormatarMoeda(MockDataStore.Salario)}  |  " +
                                  $"Total de contas: {MockDataStore.FormatarMoeda(MockDataStore.TotalContas())}  |  " +
                                  $"A pagar: {MockDataStore.FormatarMoeda(MockDataStore.TotalAPagar())}");
                Console.WriteLine($"Saldo livre (salário - a pagar): {MockDataStore.FormatarMoeda(MockDataStore.SaldoLivre())}  |  " +
                                  $"Saldo após quitar tudo: {MockDataStore.FormatarMoeda(MockDataStore.SaldoAposQuitarTudo())}");
                Console.WriteLine();

                Console.WriteLine(string.Format("{0,-4} {1,-15} | {2,-14} | {3,-10} | {4,-8}", "ID", "Descrição", "Valor", "Status", "Venc."));
                Console.WriteLine(new string('-', 60));
                foreach (var c in MockDataStore.Contas)
                {
                    Console.ForegroundColor = c.Status == StatusContaMock.Paga ? ConsoleColor.Green
                                            : c.Status == StatusContaMock.Atrasada ? ConsoleColor.Red
                                            : ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("{0,-4} {1,-15} | {2,-14} | {3,-10} | {4,-8}",
                        c.Id, c.Descricao, MockDataStore.FormatarMoeda(c.Valor),
                        MockDataStore.RotuloStatusConta(c.Status), c.Vencimento.ToString("dd/MM")));
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nA pagar: {MockDataStore.FormatarMoeda(MockDataStore.TotalAPagar())}  |  " +
                                  $"Atrasadas: {MockDataStore.ContasAtrasadas()}");
                Console.ResetColor();

                Console.WriteLine("\n[A] Pagar Conta | [N] Nova Conta | [E] Editar Conta | [R] Remover Conta | [S] Salário | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "A":
                        PagarConta();
                        break;
                    case "N":
                        NovaConta();
                        break;
                    case "E":
                        EditarConta();
                        break;
                    case "R":
                        RemoverConta();
                        break;
                    case "S":
                        DefinirSalario();
                        break;
                    case "V":
                    case "":
                        voltar = true;
                        break;
                    default:
                        Console.WriteLine("Comando inválido.");
                        Pausar();
                        break;
                }
            }
        }

        private static void PagarConta()
        {
            var id = EntradaUsuario.LerInteiro("ID da conta: ");
            if (id == 0)
                return;

            if (!MockDataStore.PagarConta(id))
            {
                EntradaUsuario.Erro("Conta não encontrada.");
                Pausar();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Conta #{id} marcada como Paga.");
            Console.ResetColor();
            Pausar();
        }

        private static void NovaConta()
        {
            var descricao = EntradaUsuario.LerTextoObrigatorio("Descrição da conta: ");

            var valor = EntradaUsuario.LerDecimal("Valor (ex: 150,00, Enter para cancelar): ", null);
            if (valor == 0m)
                return;

            var vencimento = EntradaUsuario.LerData("Vencimento (dd/MM/aaaa, Enter = hoje): ", DateTime.Today);

            var conta = MockDataStore.AdicionarConta(descricao, valor, vencimento);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Conta #{conta.Id} '{conta.Descricao}' criada ({MockDataStore.FormatarMoeda(conta.Valor)}).");
            Console.ResetColor();
            Pausar();
        }

        private static void EditarConta()
        {
            var id = EntradaUsuario.LerInteiro("ID da conta (Enter para cancelar): ");
            if (id == 0)
                return;

            var conta = MockDataStore.Contas.FirstOrDefault(c => c.Id == id);
            if (conta == null)
            {
                EntradaUsuario.Erro("Conta não encontrada.");
                Pausar();
                return;
            }

            var descricao = EntradaUsuario.LerTextoOpcional($"Descrição [{conta.Descricao}]: ", conta.Descricao);
            var valor = EntradaUsuario.LerDecimal($"Valor (atual {MockDataStore.FormatarMoeda(conta.Valor)}, Enter mantém): ", conta.Valor);
            var vencimento = EntradaUsuario.LerData($"Vencimento (atual {conta.Vencimento:dd/MM/yyyy}, Enter mantém): ", conta.Vencimento);

            MockDataStore.EditarConta(id, descricao, valor, vencimento);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Conta #{id} atualizada.");
            Console.ResetColor();
            Pausar();
        }

        private static void RemoverConta()
        {
            var id = EntradaUsuario.LerInteiro("ID da conta (Enter para cancelar): ");
            if (id == 0)
                return;

            if (!MockDataStore.RemoverConta(id))
            {
                EntradaUsuario.Erro("Conta não encontrada.");
                Pausar();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Conta #{id} removida (use [6] para desfazer).");
            Console.ResetColor();
            Pausar();
        }

        private static void DefinirSalario()
        {
            var valor = EntradaUsuario.LerDecimal(
                $"Novo salário mensal (atual {MockDataStore.FormatarMoeda(MockDataStore.Salario)}, Enter para cancelar): ",
                null);
            if (valor == 0m)
                return;

            MockDataStore.DefinirSalario(valor);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Salário atualizado para {MockDataStore.FormatarMoeda(valor)}.");
            Console.ResetColor();
            Pausar();
        }

        private static void Pausar()
        {
            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }
    }
}
