# 🚀 TaskFlow — Fase 1 (Pessoa B)

## 📌 Onde está localizado o projeto?
O projeto separado com todas as entregas da **Fase 1 (Pessoa B - Interface, Navegação e Views)** está localizado em:
`c:\Users\Fabrício\Documents\FACULDADE___UCL\Projeto AED2\Fase1_PessoaB`

---

## 📁 Estrutura de Arquivos da Pasta `Fase1_PessoaB`

```text
Fase1_PessoaB/
├── TaskFlow.sln                         # Arquivo de Solução Visual Studio / .NET
├── CHECKLIST_FASE1_PessoaB.md           # Checklist completo com o status das Etapas 1.1 a 1.4
├── README_COMO_EXECUTAR.md              # Este guia de instruções
│
├── TaskFlow.App/                        # Projeto Console App (Aplicação Executável C#)
│   ├── Program.cs                       # Ponto de entrada (Main)
│   ├── TaskFlow.App.csproj              # Configuração do projeto .NET 8.0
│   ├── Navigation/
│   │   └── MenuNavigator.cs             # Gerenciador de menus, loop principal e navegação
│   ├── Views/
│   │   ├── DashboardView.cs             # Resumo financeiro e tarefas no topo
│   │   ├── KanbanView.cs                # Colunas visuais (A Fazer, Em Andamento, Concluída)
│   │   ├── FinanceiroView.cs            # Contas a pagar, salário e saldo
│   │   └── GrafoDependenciasView.cs     # Mapeamento visual de dependências
│   ├── Services/
│   │   └── ITaskFlowService.cs          # Contrato de serviço (Interface para desacoplamento com Pessoa A)
│   └── MockServices/
│       └── TaskFlowMockService.cs       # Stubs/Mocks para simular retornos do backend
│
└── Apresentacao_Web/                    # Painel Visual Interativo em Web (HTML/CSS)
    ├── index.html                       # Página principal de apresentação do projeto
    ├── checklist.html                   # Visualização interativa do checklist
    └── estrutura.html                   # Visualização da arquitetura de pastas
```

---

## ⚙️ Como Executar a Aplicação (Console C#)

### Opção 1: Via Terminal (PowerShell / Prompt de Comando)

1. Abra o **PowerShell** ou **CMD**.
2. Navegue até a pasta do projeto:
   ```powershell
   cd "C:\Users\Fabrício\Documents\FACULDADE___UCL\Projeto AED2\Fase1_PessoaB"
   ```
3. Execute o comando `dotnet run`:
   ```powershell
   dotnet run --project TaskFlow.App\TaskFlow.App.csproj
   ```

### Opção 2: Pelo Visual Studio / VS Code
1. Abra a solução `TaskFlow.sln` no Visual Studio ou VS Code.
2. Defina `TaskFlow.App` como projeto de inicialização (Startup Project).
3. Pressione `F5` ou `Ctrl + F5` para executar.

---

## 🌐 Como Visualizar a Apresentação Web Interativa

Além da aplicação em console, foi disponibilizada uma **Apresentação Web** interativa detalhando a arquitetura, estrutura de pastas e o checklist da Pessoa B.

### Opção 1: Abrir diretamente no navegador
Navegue até a pasta `Fase1_PessoaB\Apresentacao_Web\` e **dê um duplo-clique no arquivo `index.html`**.

### Opção 2: Servidor Local Python
No PowerShell, execute:
```powershell
cd "C:\Users\Fabrício\Documents\FACULDADE___UCL\Projeto AED2\Fase1_PessoaB\Apresentacao_Web"
python -m http.server 8000
```
Em seguida, abra o navegador no endereço: `http://localhost:8000`

---

## 🎯 O que foi entregue na Fase 1 (Pessoa B)?
- ✅ **Etapa 1.1:** Configuração da arquitetura de pastas (`Views`, `Navigation`, `Services`, `MockServices`).
- ✅ **Etapa 1.2:** Menu de navegação resiliente com loop principal e opções interativas.
- ✅ **Etapa 1.3:** Esqueleto de todas as telas (Dashboard, Kanban, Financeiro e Grafo de Dependências).
- ✅ **Etapa 1.4:** Contratos de serviço desacoplados (`ITaskFlowService`) e tratamento de erros visuais.
