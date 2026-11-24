# README — Pessoa B (Interface / UI / Navegação)

> Documento das alterações e do estado atual da frente **Pessoa B**.
> Complementa `CONTINUACAO_PESSOA_B.md` (retomada) e `CHECKLIST_FASE1_PessoaB.md` (Fase 1).

## Escopo

Tudo o que a Pessoa B altera fica em `src/TaskFlow.App` (telas, navegação e mocks).
Não foram alterados `TaskFlow.Domain`, `TaskFlow.Application`, `TaskFlow.Tests` nem o contrato `ITaskFlowService`.

---

## O que já estava pronto (Fase 1)

- Menu principal com loop estável e cabeçalho (`Navigation/MenuNavigator.cs`).
- Telas: Dashboard, Projetos, Kanban, Financeiro, Grafo de Dependências e Minhas Tarefas.
- `ITaskFlowService` consumido pelo menu via `TaskFlowMockService`.
- Dados simulados em memória (`MockDataStore` + `MockModels`), com histórico/undo.
- Formulários de nova/editar tarefa, mover status, filtrar, novo/editar projeto, pagar/nova conta, salário e undo.

---

## Alterações desta rodada (prioridade alta da seção 7)

Todas no App, sem dependência da Pessoa A.

### 1. Editar conta (valor/vencimento)
- `MockServices/MockDataStore.cs` → `EditarConta(int, string, decimal, DateTime)` com registro no undo.
- `Views/FinanceiroView.cs` → comando `[E]`, método `EditarConta()`.
- Enter mantém o valor atual; validação de valor e data.

### 2. Remover projeto / tarefa / conta (com undo)
- `MockServices/MockDataStore.cs`:
  - `RemoverProjeto(int)` — remove também as tarefas do projeto e restaura tudo no undo.
  - `RemoverTarefa(int)` — remove e reinsere na posição original no undo.
  - `RemoverConta(int)` — remove e reinsere na posição original no undo.
- `Views/ProjetosView.cs` → comando `[R]`, método `RemoverProjeto()`.
- `Views/KanbanView.cs` → comando `[R]`, método `RemoverTarefa()`.
- `Views/FinanceiroView.cs` → comando `[R]`, método `RemoverConta()`.
- Toda remoção pode ser desfeita em `[6] Histórico e Desfazer`.

### 3. Busca de tarefas por texto
- `MockServices/MockDataStore.cs` → `BuscarTarefas(string)` (título ou responsável).
- `Views/KanbanView.cs` → comando `[B]`, filtro por texto somado aos filtros de responsável/prioridade.
- Enter no `[B]` limpa a busca.

### 4. Dashboard com atrasadas por responsável
- `MockServices/MockDataStore.cs` → `AtrasadasPorResponsavel()`.
- `Views/DashboardView.cs` → nova seção `TAREFAS ATRASADAS POR RESPONSÁVEL` (contagem por pessoa).

---

## Comandos por tela (após as alterações)

| Tela | Comandos |
|---|---|
| Projetos `[2]` | `[N]` novo, `[A]` abrir, `[E]` editar, `[R]` remover, `[V]` voltar |
| Kanban `[2]` → abrir | `[N]` nova, `[M]` mover, `[E]` editar, `[R]` remover, `[F]` filtrar, `[B]` buscar, `[V]` voltar |
| Financeiro `[5]` | `[A]` pagar, `[N]` nova conta, `[E]` editar conta, `[R]` remover conta, `[S]` salário, `[V]` voltar |
| Dashboard `[1]` | Resumo financeiro, urgentes e atrasadas por responsável |
| Undo `[6]` | `[U]` desfazer a última ação |

---

## Como compilar e rodar

Na pasta `TaskFlow-refactor-organizar-estrutura`:

```powershell
dotnet build TaskFlow.sln
dotnet run --project src\TaskFlow.App\TaskFlow.App.csproj
```

Estado verificado: `dotnet build TaskFlow.sln` → **0 avisos, 0 erros**.

Roteiro rápido:
- `5` → `E` (editar conta) → `R` (remover conta) → `6` → `U` (desfazer)
- `2` → `A` (projeto 1) → `B` (buscar) → `R` (remover tarefa) → `V` → `R` (remover projeto) → `6` → `U`
- `1` (dashboard) → conferir seção de atrasadas por responsável

---

## Alterações desta rodada (prioridade média)

- **Confirmação antes do undo** — `[6]` → `[U]` pede `s/n` (`EntradaUsuario.Confirmar`).
- **Validações com nova tentativa** — novo helper `Navigation/EntradaUsuario.cs`:
  - `LerTextoObrigatorio`, `LerTextoOpcional`, `LerInteiro`, `LerDecimal`, `LerData`, `Confirmar`.
  - IDs, valores, datas e textos inválidos mostram mensagem amigável e pedem de novo, sem sair da tela; Enter cancela (ou mantém o valor atual, em edição).
  - Aplicado em `FinanceiroView`, `ProjetosView` e `KanbanView`.
- **Responsável/atraso em mais telas**:
  - `ProjetosView` mostra colunas `Responsáveis` e `Atras.` por projeto.
  - `DashboardView` mostra `Responsável` e `Atras.` nas tarefas urgentes (além da seção por responsável).

---

## Pendências que ainda NÃO dependem da Pessoa A

- [ ] Permitir editar o responsável na listagem "Minhas Tarefas".
- [ ] (Baixa) Busca global fora do Kanban, se desejado.

## Pendências que dependem da Pessoa A

- [ ] `GrafoDependenciasView` consumir o grafo real (`ObterPredecessoras` / `OrdemExecucao`).
- [ ] Adicionar/remover dependências pela UI (precisa de métodos no contrato/`Domain`).
- [ ] Substituir `TaskFlowMockService`/`MockDataStore` pela implementação real do `Application`.
- [ ] Persistência dos dados (hoje tudo é em memória e reseta ao fechar).

---

## Observações

- Dados em memória: criar/editar/remover e undo funcionam apenas durante a execução.
- `GrafoDependenciasView` segue **ilustrativo** (texto fixo), pois o grafo real é do `Domain`.
- Não alterar `Domain`, `Application`, `Tests` nem o contrato `ITaskFlowService`.
