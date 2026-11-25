using System;
using System.Globalization;
using TaskFlow.App.Navigation;
using TaskFlow.Application;
using TaskFlow.Domain;
using static TaskFlow.App.Views.Saida;

namespace TaskFlow.App.Views
{
    public class FinanceiroView
    {
        private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");
        private readonly ITaskFlowService _service;

        public FinanceiroView(ITaskFlowService service)
        {
            _service = service;
        }

        public void Renderizar()
        {
            bool voltar = false;

            while (!voltar)
            {
                try { Console.Clear(); } catch { }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(">>> PAINEL FINANCEIRO PESSOAL <<<\n");
                Console.ResetColor();

                Console.WriteLine($"Salário: {Moeda(_service.ObterSalario())}  |  " +
                                  $"Total de contas: {Moeda(_service.ObterTotalContas())}  |  " +
                                  $"A pagar: {Moeda(_service.ObterTotalAPagar())}");
                Console.WriteLine($"Saldo livre (salário - a pagar): {Moeda(_service.ObterSaldoLivre())}  |  " +
                                  $"Saldo após quitar tudo: {Moeda(_service.ObterSaldoAposQuitarTudo())}");
                Console.WriteLine();

                Console.WriteLine(string.Format("{0,-4} {1,-15} | {2,-14} | {3,-10} | {4,-8}", "ID", "Descrição", "Valor", "Status", "Venc."));
                Console.WriteLine(new string('-', 60));
                foreach (var c in _service.ListarContas())
                {
                    Console.ForegroundColor = c.Status == StatusFinanceiro.Paga ? ConsoleColor.Green
                                            : c.Status == StatusFinanceiro.Atrasada ? ConsoleColor.Red
                                            : ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("{0,-4} {1,-15} | {2,-14} | {3,-10} | {4,-8}",
                        c.Id, c.Titulo, Moeda(c.Valor), Rotulo(c.Status), c.DataVencimento.ToString("dd/MM")));
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nA pagar: {Moeda(_service.ObterTotalAPagar())}  |  " +
                                  $"Atrasadas: {_service.ObterContasAtrasadas().Count}");
                Console.ResetColor();

                Console.WriteLine("\n[A] Pagar Conta | [N] Nova Conta | [E] Editar Conta | [R] Remover Conta | [S] Salário | [V] Voltar");
                Console.Write("Comando: ");
                var cmd = Console.ReadLine()?.Trim().ToUpper();

                switch (cmd)
                {
                    case "A": PagarConta(); break;
                    case "N": NovaConta(); break;
                    case "E": EditarConta(); break;
                    case "R": RemoverConta(); break;
                    case "S": DefinirSalario(); break;
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

        private void PagarConta()
        {
            var id = EntradaUsuario.LerInteiro("ID da conta: ");
            if (id == 0)
                return;

            MostrarResultado(_service.PagarConta(id));
        }

        private void NovaConta()
        {
            var titulo = EntradaUsuario.LerTextoObrigatorio("Descrição da conta: ");

            var valor = EntradaUsuario.LerDecimal("Valor (ex: 150,00, Enter para cancelar): ", null);
            if (valor == 0m)
                return;

            var vencimento = EntradaUsuario.LerData("Vencimento (dd/MM/aaaa, Enter = hoje): ", DateTime.Today);

            // Esta tela só cria contas a pagar, então o tipo é fixo.
            MostrarResultado(_service.CriarConta(titulo, valor, vencimento, TipoFinanceiro.Despesa));
        }

        private void EditarConta()
        {
            var id = EntradaUsuario.LerInteiro("ID da conta (Enter para cancelar): ");
            if (id == 0)
                return;

            // Buscamos antes só para mostrar os valores atuais como padrão.
            var conta = _service.ObterConta(id);
            if (conta == null)
            {
                EntradaUsuario.Erro("Conta não encontrada.");
                Pausar();
                return;
            }

            var titulo = EntradaUsuario.LerTextoOpcional($"Descrição [{conta.Titulo}]: ", conta.Titulo);
            var valor = EntradaUsuario.LerDecimal($"Valor (atual {Moeda(conta.Valor)}, Enter mantém): ", conta.Valor);
            var vencimento = EntradaUsuario.LerData($"Vencimento (atual {conta.DataVencimento:dd/MM/yyyy}, Enter mantém): ", conta.DataVencimento);

            MostrarResultado(_service.EditarConta(id, titulo, valor, vencimento));
        }

        private void RemoverConta()
        {
            var id = EntradaUsuario.LerInteiro("ID da conta (Enter para cancelar): ");
            if (id == 0)
                return;

            MostrarResultado(_service.RemoverConta(id));
        }

        private void DefinirSalario()
        {
            var valor = EntradaUsuario.LerDecimal(
                $"Novo salário mensal (atual {Moeda(_service.ObterSalario())}, Enter para cancelar): ",
                null);
            if (valor == 0m)
                return;

            MostrarResultado(_service.DefinirSalario(valor));
        }


        private static string Moeda(decimal valor) => string.Format(CulturaBr, "{0:C}", valor);

        private static string Rotulo(StatusFinanceiro status) => status switch
        {
            StatusFinanceiro.Paga => "Paga",
            StatusFinanceiro.Atrasada => "Atrasada",
            _ => "Pendente"
        };
    }
}