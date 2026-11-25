# ✅ Documento de Checklist: Etapas e Sub-etapas (Fase 1 - Pessoa B)

## 📋 Visão Geral

Projeto: **TaskFlow** - Gerenciador de Projetos/Tarefas estilo Kanban com controle financeiro e dependências
- **Pessoa A**: Core/Backend (`TaskFlow.Domain`, `TaskFlow.Application`, `TaskFlow.Tests`) - Estruturas de dados (GrafoDependencias, FilaPrioridade, PilhaUndo, etc.)
- **Pessoa B**: Interface/UI/Navegação (`TaskFlow.App`) - Telas, UX, Integração

> Estrutura atual (pós-refactor): `src/TaskFlow.Domain`, `src/TaskFlow.Application`, `src/TaskFlow.App`, `tests/TaskFlow.Tests`, `docs/`.
> O contrato `ITaskFlowService` vive em `src/TaskFlow.Application` (namespace `TaskFlow.Application`).

---

## 📝 Etapa 1.1: Configuração do Projeto e Arquitetura de Pastas

- [x] Criar projeto `TaskFlow.App` na solution `TaskFlow.sln` referenciando `TaskFlow.Application`
- [x] Criar pasta `Views/` para separar cada tela do sistema
- [x] Criar pasta `Navigation/` com gerenciador central de menus e rotas
- [x] Criar pasta `MockServices/` para simular chamadas aos métodos do backend
- [x] Contrato `ITaskFlowService` movido para `src/TaskFlow.Application` (namespace `TaskFlow.Application`)

---

## 📝 Etapa 1.2: Menu Principal e Fluxo de Navegação (UX)

- [x] Implementar loop principal do sistema sem travamento
- [x] Desenhar cabeçalho com estado atual do sistema e notificações rápidas
- [x] Criar opções de acesso direto:
  - [x] 1. Dashboard Geral (Visão Unificada)
  - [x] 2. Gestão de Projetos e Tarefas (Kanban)
  - [x] 3. Área Financeira Pessoal (Contas e Salário)
  - [x] 4. Fila de Execução ("O que fazer agora?" - Prioridades)
  - [x] 5. Histórico e Desfazer Ação (Undo via Pilha)
  - [x] 0. Sair

---

## 📝 Etapa 1.3: Esqueleto das Telas Principais (Views)

- [x] **Tela Dashboard:** Resumo financeiro (Salário vs. Contas) + Tarefas urgentes no topo
- [x] **Tela de Projetos:** Listagem de projetos cadastrados e submenu para abrir projeto
- [x] **Tela do Projeto (Kanban):** Colunas visuais (`A Fazer`, `Em Andamento`, `Concluída`, `Bloqueada`)
- [x] **Tela Financeira:** Tabela de contas a pagar, status (`Pendente`/`Paga`/`Atrasada`) e saldo restante
- [x] **Visualização de Dependências:** Listagem simplificada de tarefas e suas travas (predecessoras)

---

## 📝 Etapa 1.4: Contratos de Integração (Adapters/Interfaces)

- [x] Definir interfaces de serviço (`ITaskFlowService`) para tela não depender de implementação concreta
- [x] Validar feedbacks visuais de erro amigáveis ao usuário

---

## 📝 Etapa 1.5: Finalização da Interface (formulários e telas funcionais)

- [x] `MockDataStore` in-memory (projetos, tarefas, contas, histórico) na camada `TaskFlow.App`
- [x] **Formulário: Nova Tarefa** (título, prioridade, prazo) no Kanban
- [x] **Formulário: Mover Status** de tarefa entre colunas
- [x] **Formulário: Novo Projeto** e **Abrir Projeto** (`ProjetosView`)
- [x] **Formulário: Pagar Conta** e **Nova Conta** (`FinanceiroView`)
- [x] **Dashboard** consumindo dados do store (resumo financeiro coerente + tarefas urgentes)
- [x] **Fila de Prioridade** calculada a partir do store (prioridade + prazo)
- [x] **Undo** funcional (desfaz criação/movimentação/pagamento via pilha de ações)
- [x] **Editar Projeto** (`[E]` em `ProjetosView`) e **Editar Tarefa** (`[E]` no Kanban)
- [x] **Cadastro de Salário** (`[S]` no Financeiro) + cálculo de saldo livre / após quitar tudo
- [x] **Campo Responsável** (nova/editar tarefa) + tela **Minhas Tarefas** (`[7]`, por responsável)
- [x] **Filtros no Kanban** por responsável e por prioridade
- [x] **Alertas visuais**: cores por status e destaque `!` para tarefas atrasadas
- [x] **Histórico completo** (últimas 10 ações) com desfazer a partir do menu (`[6]`)
- [x] Build limpo (`dotnet build TaskFlow.sln`) e fluxos testados

---

## 📁 Estrutura de Pastas Finalizada

```
TaskFlow.sln
│
├── src/
│   ├── TaskFlow.Domain/            # Pessoa A - Backend/Core
│   │
│   ├── TaskFlow.Application/       # Pessoa A - Contrato
│   │   └── ITaskFlowService.cs
│   │
│   └── TaskFlow.App/               # Pessoa B - Interface
│       ├── Program.cs
│       ├── TaskFlow.App.csproj
│       ├── Navigation/
│       │   └── MenuNavigator.cs
│       ├── Views/
│       │   ├── DashboardView.cs
│       │   ├── ProjetosView.cs
│       │   ├── KanbanView.cs
│       │   ├── FinanceiroView.cs
│       │   ├── MinhasTarefasView.cs
│       │   └── GrafoDependenciasView.cs
│       └── MockServices/
│           ├── TaskFlowMockService.cs
│           ├── MockModels.cs
│           └── MockDataStore.cs
│
├── tests/
│   └── TaskFlow.Tests/             # Pessoa A
│
└── docs/
    ├── CHECKLIST_FASE1_PessoaB.md
    ├── CONTINUACAO_PESSOA_A.md
    └── Apresentacao_Web/
```

---

## 🔜 Próximo Passo

A Pessoa B concluiu a Fase 1 e a finalização da interface. O que resta depende da Pessoa A
(contrato devolvendo dados, serviços na camada `Application`, integração com o Domain real e correção
da detecção de ciclo no grafo). Ver o documento **`docs/CONTINUACAO_PESSOA_A.md`** para o plano de
validação e prosseguimento.
