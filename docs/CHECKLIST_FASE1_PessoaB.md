# ✅ Documento de Checklist: Etapas e Sub-etapas (Fase 1 - Pessoa B)

## 📋 Visão Geral

Projeto: **TaskFlow** - Gerenciador de Projetos/Tarefas estilo Kanban com controle financeiro e dependências
- **Pessoa A**: Core/Backend (`TaskFlow.Domain`) - Estruturas de dados (GrafoDependencias, FilaPrioridade, PilhaUndo, etc.)
- **Pessoa B**: Interface/UI/Navegação (`TaskFlow.App`) - Telas, UX, Integração

---

## 📝 Etapa 1.1: Configuração do Projeto e Arquitetura de Pastas

- [x] Criar projeto `TaskFlow.App` na solution `TaskFlow.sln` referenciando `TaskFlow.Domain`
- [x] Criar pasta `Views/` para separar cada tela do sistema
- [x] Criar pasta `Navigation/` com gerenciador central de menus e rotas
- [x] Criar pasta `MockServices/` para simular chamadas aos métodos do backend
- [x] Criar pasta `Services/` com contratos de integração (interfaces)

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

## 📁 Estrutura de Pastas Finalizada

```
TaskFlow.sln
│
├── TaskFlow.Domain/
│   └── ... (Pessoa A - Backend/Core)
│
└── TaskFlow.App/
    ├── Program.cs
    ├── TaskFlow.App.csproj
    ├── Services/
    │   └── ITaskFlowService.cs
    ├── Navigation/
    │   └── MenuNavigator.cs
    ├── Views/
    │   ├── DashboardView.cs
    │   ├── KanbanView.cs
    │   ├── FinanceiroView.cs
    │   └── GrafoDependenciasView.cs
    └── MockServices/
        └── TaskFlowMockService.cs
```

---

## 🔜 Próximo Passo

Implementação detalhada dos formulários de entrada de dados e tela financeira unificada com o dashboard.
