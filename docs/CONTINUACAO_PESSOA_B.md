# 🧭 Retomada — Pessoa B (Interface/UI)

> **Leia este arquivo primeiro.** Ele resume o objetivo do projeto, o estado atual,
> onde mexer e o que falta, para você continuar **sem precisar reanalisar todo o projeto**.
> Documentos irmãos: `CHECKLIST_FASE1_PessoaB.md` (checklist) e `CONTINUACAO_PESSOA_A.md` (integração/gate).

---

## 1. Objetivo do projeto

**TaskFlow** — gerenciador de projetos/tarefas estilo Kanban, com controle financeiro pessoal
e dependências entre tarefas (grafo). Projeto da faculdade dividido em duas frentes:

- **Pessoa A** — Core/Backend: `TaskFlow.Domain`, `TaskFlow.Application` (contrato), `TaskFlow.Tests`.
  Estruturas de dados (grafo de dependências, fila de prioridade, pilha de undo, etc.).
- **Pessoa B (você)** — Interface/UI/Navegação: **somente** `src/TaskFlow.App`
  (telas, UX, menu, integração).

> ⚠️ **Regra de ouro:** não alterar `TaskFlow.Domain`, `TaskFlow.Application` nem `TaskFlow.Tests`.
> O contrato `ITaskFlowService` (6 métodos `void`) é fixo; mudá-lo exige acordo A+B.

---

## 2. Onde trabalhar (estrutura canônica)

Use **sempre** a pasta refatorada:

```
TaskFlow-refactor-organizar-estrutura/
├── src/
│   ├── TaskFlow.Domain/          # Pessoa A — NÃO MEXER
│   ├── TaskFlow.Application/     # Pessoa A — ITaskFlowService (NÃO MEXER)
│   └── TaskFlow.App/             # ✅ Pessoa B (SEU território)
├── tests/TaskFlow.Tests/         # Pessoa A — NÃO MEXER
├── docs/                         # Documentação
└── TaskFlow.sln
```

> A pasta `Fase1_PessoaB/` é uma cópia antiga (flat) e **não** é o alvo. Ignore-a.

---

## 3. Estado atual (o que já está pronto)

- ✅ Build limpo: `dotnet build TaskFlow.sln` → **0 avisos, 0 erros**.
- ✅ Menu e navegação estáveis; telas consumindo `TaskFlowMockService` (que implementa `ITaskFlowService`).
- ✅ Dados simulados em memória via `MockDataStore` (camada App), sem depender do backend real.
- ✅ Fluxos testados manualmente: nova tarefa, mover status, editar, pagar conta, salário, undo, filtros, "minhas tarefas".

### Funcionalidades implementadas na Pessoa B
| Tela / Recurso | Status | Observação |
|---|---|---|
| Dashboard Geral `[1]` | ✅ | Resumo financeiro + tarefas urgentes |
| Projetos & Kanban `[2]` | ✅ | Novo/abrir/editar projeto; colunas visuais |
| Kanban — Nova tarefa | ✅ | título, prioridade, prazo, **responsável** |
| Kanban — Mover status | ✅ | A Fazer / Em Andamento / Concluída / Bloqueada |
| Kanban — **Editar tarefa** `[E]` | ✅ | mantém valores atuais se Enter |
| Kanban — **Filtros** `[F]` | ✅ | por responsável e/ou prioridade |
| Kanban — **Alertas visuais** | ✅ | cores por status + `!` para atrasadas |
| Fila de Prioridade `[3]` | ✅ | ordena por prioridade + prazo, mostra responsável/atraso |
| Grafo de Dependências `[4]` | ⚠️ | **ilustrativo** (dependências fixas em texto; grafo real é do Domain) |
| Financeiro `[5]` | ✅ | pagar/nova conta, **salário `[S]`**, saldo livre e após quitar tudo |
| Histórico/Undo `[6]` | ✅ | lista as 10 últimas ações e desfaz a última |
| Minhas Tarefas `[7]` | ✅ | tarefas pendentes por responsável, com atraso em destaque |

---

## 4. Mapa de arquivos da Pessoa B

```
src/TaskFlow.App/
├── Program.cs                      # Ponto de entrada (inicia MenuNavigator)
├── Navigation/
│   ├── MenuNavigator.cs            # Menu principal + roteamento das opções 0..7
│   └── EntradaUsuario.cs           # Validação de entrada com nova tentativa (novo)
├── Views/
│   ├── DashboardView.cs            # Resumo financeiro + tarefas urgentes
│   ├── ProjetosView.cs             # Lista/cria/edita/abre projetos
│   ├── KanbanView.cs               # Quadro, nova/editar/mover/filtrar tarefas
│   ├── FinanceiroView.cs           # Contas, pagar, nova conta, salário
│   ├── MinhasTarefasView.cs        # Tarefas pendentes por responsável
│   └── GrafoDependenciasView.cs    # Grafo ilustrativo (estático)
└── MockServices/
    ├── MockModels.cs               # ProjetoMock, TarefaMock, ContaMock, enums, AcaoHistorico
    ├── MockDataStore.cs            # Store in-memory: CRUD, undo, cálculos, rótulos
    └── TaskFlowMockService.cs      # Implementa ITaskFlowService (delega para as Views)
```

---

## 5. Referência rápida — `MockDataStore` (API)

Não precisa reler o arquivo; os métodos públicos são:

**Seed / util**
- `Inicializar()` — popula dados iniciais (idempotente).
- `FormatarMoeda(decimal)` — `R$ 1.234,56`.

**Projetos**
- `AdicionarProjeto(string nome)`
- `EditarProjeto(int id, string novoNome)` → `bool`
- `ObterProjeto(int id)` → `ProjetoMock?`

**Tarefas**
- `AdicionarTarefa(int projetoId, string titulo, PrioridadeMock prioridade, DateTime prazo, string? responsavel)` → `TarefaMock`
- `EditarTarefa(int id, string titulo, PrioridadeMock prioridade, DateTime prazo, string? responsavel)` → `bool`
- `MoverStatus(int id, StatusTarefaMock novoStatus)` → `bool`
- `TarefasProntas()` → tarefas `AFazer`/`EmAndamento` ordenadas por prioridade+prazo
- `TarefasDoResponsavel(string resp)` → pendentes do responsável (ordem por prazo+prioridade)
- `Responsaveis()` → nomes distintos
- `TarefaAtrasada(TarefaMock)` → `bool` (não concluída e prazo < hoje)

**Financeiro**
- `DefinirSalario(decimal)`
- `AdicionarConta(string descricao, decimal valor, DateTime vencimento)`
- `PagarConta(int id)` → `bool`
- `TotalContas()`, `TotalAPagar()`, `SaldoLivre()` (= salário − a pagar), `SaldoAposQuitarTudo()` (= salário − total), `ContasAtrasadas()`

**Histórico / Undo**
- `DesfazerUltimaAcao(out string descricao)` → `bool`
- `ObterHistoricoRecente(int max)` → lista (mais recente primeiro)

**Rótulos**
- `RotuloPrioridade`, `RotuloStatus`, `RotuloStatusConta`

> `Salario` tem `private set` — altere só via `DefinirSalario` (registra no histórico/undo).
> Coleções públicas: `Projetos`, `Tarefas`, `Contas`, `Historico`.

---

## 6. Como compilar, rodar e testar

Shell é **PowerShell 5.1** — evite `&&`, `ls -la`, `head`, `2>/dev/null`. Use `;` e `workdir`.

```powershell
# na pasta TaskFlow-refactor-organizar-estrutura
dotnet build TaskFlow.sln
dotnet run --project src\TaskFlow.App\TaskFlow.App.csproj
```

Roteiro rápido de teste manual:
- `2` → `A` (abrir projeto 1) → `N` (nova tarefa) → `E` (editar) → `F` (filtrar) → `M` (mover) → `V`
- `5` → `A` (pagar) → `N` (nova conta) → `S` (salário) → `V`
- `7` → digitar responsável (ex.: `Ana`) → ver pendências/atrasos
- `6` → `U` (desfazer última ação)
- `1` → dashboard

> O projeto `tests/TaskFlow.Tests` é um **console** (não xUnit): rode com
> `dotnet run --project tests\TaskFlow.Tests\TaskFlow.Tests.csproj` (é da Pessoa A).

---

## 7. O que ainda pode ser feito (escopo Pessoa B)

Prioridade **alta** (fecha lacunas da UI, tudo no App) — **concluído** (ver `README_PESSOA_B.md`):
- [x] **Editar conta** (valor/vencimento) — `[E]` no Financeiro + `MockDataStore.EditarConta`.
- [x] **Remover** projeto / tarefa / conta (com registro no undo) — `[R]` em cada tela + `Remover*` no store.
- [x] **Busca** de tarefas por texto no Kanban — `[B]` no Kanban + `MockDataStore.BuscarTarefas`.
- [x] Melhorar **Dashboard**: responsáveis e contagem de atrasadas por pessoa — `AtrasadasPorResponsavel`.

Prioridade **média** — **concluído** (ver `README_PESSOA_B.md`):
- [x] Confirmação antes de desfazer (`[U]` pede "s/n").
- [x] Validações de entrada mais amigáveis — `Navigation/EntradaUsuario.cs` (repete em vez de sair).
- [x] Exibir responsável/atraso também em Projetos e Dashboard.

Prioridade **baixa / depende da Pessoa A**:
- [ ] `GrafoDependenciasView` consumir o grafo real (após integração).
- [ ] Adicionar/remover **dependências** pela UI (precisa de `ObterPredecessoras`/`AdicionarDependencia`).

---

## 8. Integração futura com a Pessoa A (quando o contrato fechar)

Hoje as Views leem do `MockDataStore`. Quando a Pessoa A validar o novo contrato
(ver `CONTINUACAO_PESSOA_A.md`, seção 3), a Pessoa B deverá:
1. Trocar `TaskFlowMockService` por uma implementação que chama o serviço real (`Application`).
2. Fazer as Views consumirem dados retornados (não mais `MockDataStore`).
3. Remover `MockDataStore`/`MockModels` da camada App.
4. Revalidar todos os fluxos.

> Não iniciar essa troca antes do contrato validado — é o maior risco de retrabalho.

Mapeamento de tipos (mock → Domain) está em `CONTINUACAO_PESSOA_A.md`, seção 2.
Atenção: `Tarefa.Prioridade` no Domain é `int` (1 = mais alta) e `StatusTarefa` tem
`Bloqueado`/`Bloqueada` duplicados — decisões da Pessoa A.

---

## 9. Restrições e observações

- Não alterar Domain/Application/Tests nem o contrato `ITaskFlowService`.
- Dados são **em memória**: tudo reseta ao fechar o app (persistência é da Pessoa A).
- `GrafoDependenciasView` é **ilustrativo** (texto fixo), não reflete o store.
- Bug de **detecção de ciclo** no `GrafoDependencias` é da Pessoa A (não corrigir aqui).
- Não existe `.git` local nas pastas baixadas; o commit é feito pelo usuário no clone dele.

---

## 10. Commit (apenas arquivos da Pessoa B)

```powershell
# na raiz do clone com git
git add src/TaskFlow.App docs
git commit -m "Pessoa B: edicao projeto/tarefa, salario, responsavel, filtros, minhas tarefas, historico e alertas visuais"
git push
```

---

## 11. TL;DR para retomar

1. Trabalhe só em `src/TaskFlow.App`.
2. `dotnet build TaskFlow.sln` deve ficar em 0 erros.
3. UI completa e funcional sobre `MockDataStore`; falta integração real (depende da Pessoa A).
4. Próximas melhorias de UI estão na seção 7.
5. Contrato e integração: ver `CONTINUACAO_PESSOA_A.md`.
