# Commit — Refatoração da Dinâmica de Grupos

> Sessão de 2026-05-12. Branch: `dev-humberto`.
> Objetivo: consolidar o sistema de troca/coesão de grupos em um único caminho, corrigir bugs latentes e preparar o código para as próximas fases de replicação do paper de Musse & Thalmann (1997).

---

## Sumário

1. **Remoção do caminho legado de troca de grupos** (duplicação que rodava todo frame em paralelo ao novo).
2. **Correção de 3 bugs funcionais** que tornavam a eleição de líder, a coesão e a troca de grupo inconsistentes com o paper.
3. **Limpeza de resíduos** (typo em `cycleLenght`, `SpawnArea` fora do namespace, guarda contra `Goal` nulo, comentários desatualizados).

Nenhuma feature nova foi adicionada — apenas estabilização e padronização para as Fases A/B/C descritas no plano de pesquisa.

---

## 1. Remoção do caminho legado de troca de grupos

### Contexto

O `World.cs` executava **dois caminhos paralelos** todo `Update()` que faziam, em essência, o mesmo trabalho:

- **Caminho novo** (mantido):
  `ComputeGroupData` → `DetectApproachingGroupPairs` → `UpdateGroupMembership` → `UpdateLoneAgents` → `UpdateGroupLeaders`
- **Caminho legado** (removido):
  `UpdateGroupAffinities` → `EvaluateGroupProximityAndSwitches` → `EvaluateGroupSwaps` → `ShouldAgentSwitchGroup` → `agent.SwitchGroup`

### Problemas que isso causava

1. **Trabalho redundante.** `UpdateGroupAffinities` recalculava o dicionário que `ComputeGroupData` já tinha preenchido alguns milissegundos antes no mesmo frame.
2. **Critérios divergentes de proximidade.** O caminho novo usava distância entre **centróides** (`GROUP_DETECTION_RADIUS`). O legado usava distância **mínima par-a-par** de agentes (`GROUP_PROXIMITY_DISTANCE`). Em grupos espalhados isso dava resultados totalmente diferentes.
3. **Troca dupla no mesmo frame.** Um agente podia ser trocado pelo caminho novo e logo em seguida ser trocado de volta pelo caminho legado.
4. **Inconsistência de side-effects.** O caminho legado chamava `agent.SwitchGroup` (que zerava `_nearbyGroupMembers`); o novo apenas reatribuía `groupId` direto. Estados internos divergiam.

### O que foi removido

Em `Assets/Code/World.cs`:

- Campo `[SerializeField] private float GROUP_PROXIMITY_DISTANCE = 10.0f;`
- Atributo `[Range(0f, 1f)]` órfão que se colava no `MeshFilter` seguinte (silenciosamente sem efeito, mas suspeito).
- Chamadas `UpdateGroupAffinities()` e `EvaluateGroupProximityAndSwitches()` dentro de `Update()`.
- Métodos: `UpdateGroupAffinities`, `EvaluateGroupProximityAndSwitches`, `AreGroupsNearby`, `EvaluateGroupSwaps`, `ShouldAgentSwitchGroup`.

Em `Assets/Code/Agent.cs`:

- Método `public void SwitchGroup(int newGroupId)` (sem callers após a remoção acima).

### Resultado

Um único caminho de decisão de grupo por frame. Comportamento determinístico, sem trocas conflitantes.

---

## 2. Correção de bugs funcionais da dinâmica de grupos

### Bug A — Líder eleito sem efeito mecânico

**Sintoma:** `UpdateGroupLeaders` marcava `agent.isGroupLeader = true` no agente de maior `dominance`, mas nenhuma parte do `Agent.cs` consultava esse flag. O líder era puramente decorativo (cor mais clara + escala 1.4×).

**Correção:**

- Adicionado em `World.cs`:
  - Campo `private Dictionary<int, Agent> _groupLeaders` (chave = `groupId`, valor = agente líder).
  - Método público `Agent GetGroupLeader(int groupId)` retornando o líder ou `null`.
  - `UpdateGroupLeaders` agora popula `_groupLeaders` no mesmo passo em que marca o flag.
- Adicionado em `Agent.CalculateDirection()` (após o somatório de auxinas):
  ```csharp
  if (HasGroup && !isGroupLeader && _world != null)
  {
      Agent leader = _world.GetGroupLeader(groupId);
      if (leader != null && leader != this)
      {
          Vector3 toLeader = leader.transform.position - transform.position;
          float   distSqr  = toLeader.sqrMagnitude;
          if (distSqr > agentRadius * agentRadius)
              _rotation += groupCohesionStrength * toLeader.normalized * _maxSpeed;
      }
  }
  ```
  Lê-se: "se o agente pertence a um grupo E não é líder E o mundo está disponível, calcula o vetor que aponta do agente para o líder e, se a distância é maior que o raio do agente, adiciona ao vetor de rotação o produto entre a força de coesão, o vetor normalizado e a velocidade máxima."

**Efeito:** o líder agora *de fato* atrai os membros do grupo. O campo `groupCohesionStrength` deixa de ser inerte.

---

### Bug B — Troca de grupo não atualizava a rota

**Sintoma:** quando um agente trocava de `groupId` via `UpdateGroupMembership` ou entrava em um grupo via `UpdateLoneAgents`, apenas o inteiro `groupId` mudava. O `goalsList`, `Goal`, `goalIndex` e `goalsWaitList` continuavam apontando para a rota da `SpawnArea` original. Resultado: o agente "trocava de grupo" visualmente mas continuava indo para o objetivo antigo, em direção contrária ao novo grupo.

**Correção:**

- Adicionados getters/setters em `Agent.cs` para o índice de objetivo (estava `private`):
  ```csharp
  public int  GetGoalIndex()       => goalIndex;
  public void SetGoalIndex(int v)  => goalIndex = v;
  ```
- Novo helper em `World.cs`:
  ```csharp
  private void SyncAgentToGroup(Agent agent, int newGroupId)
  ```
  Que faz:
  1. Atualiza `agent.groupId` e zera `isGroupLeader`.
  2. Procura o líder do grupo destino via `GetGroupLeader`.
  3. Se não houver líder eleito ainda, faz fallback para o primeiro agente do grupo destino.
  4. Copia `goalsList`, `goalsWaitList`, `Goal`, `removeWhenGoalReached` e `goalIndex` do agente-template.
- Reordenação do `Update()` para que `UpdateGroupLeaders` rode **antes** de `UpdateGroupMembership`/`UpdateLoneAgents`. Assim, quando um agente entra em um grupo, já existe um líder válido para herdar a rota.
- Substituídas as atribuições diretas (`agent.groupId = X`) em `UpdateGroupMembership` e `UpdateLoneAgents` por chamadas a `SyncAgentToGroup`.

**Efeito:** trocar de grupo passa a significar de fato seguir o mesmo objetivo do grupo. A rota nova é recalculada pelo NavMesh dentro do intervalo padrão (`UPDATE_NAVMESH_INTERVAL = 1.0f`), o que é tolerável (atraso máximo de 1 segundo de simulação).

---

### Bug C — `UpdateGroupMembership` iterava todos os agentes desnecessariamente

**Sintoma:** o loop interno percorria `_agents` inteiro para cada par de grupos próximos, mas tratava apenas os agentes pertencentes àquele par. Lone agents (`groupId == -1`) eram analisados em vão e seriam tratados depois por `UpdateLoneAgents`.

**Correção:**

- Adicionado `continue` no início do loop:
  ```csharp
  if (agent.groupId != groupA && agent.groupId != groupB) continue;
  ```
- Removido o ramo morto `else if (agent.groupId == groupB)` (agora um simples `else` cobre o caso, já que outros valores foram filtrados).

**Efeito:** menos branching, intenção mais clara, custo computacional reduzido em cenários com muitos lone agents.

---

## 3. Limpeza de resíduos

Mesmo sem bloquear nada, esses pontos foram corrigidos para evitar problemas futuros:

### 3.1 `SpawnArea` fora do namespace

**Antes:** `SpawnArea` era declarada no escopo global, enquanto `Agent`, `World`, `Auxin`, `Cell` estavam em `namespace Biocrowds.Core`. Funcionava porque `SceneController` já fazia `using Biocrowds.Core` mas a `SpawnArea` resolvia por *fallback* do escopo global.

**Depois:** `SpawnArea` movido para `namespace Biocrowds.Core`. `using UnityEngine.Serialization` adicionado para o `[FormerlySerializedAs]` (ver 3.2). Nenhuma outra mudança de API necessária — `SceneController` continua compilando porque já importava o namespace.

### 3.2 Typo `cycleLenght` → `cycleLength`

**Antes:** o campo de cooldown do spawn cíclico tinha um typo (`Lenght` em vez de `Length`) que se propagava por todos os prefabs e cenas serializadas.

**Depois:** renomeado para `cycleLength`, com `[FormerlySerializedAs("cycleLenght")]` para que o Unity mapeie automaticamente os valores antigos serializados (`Assets/Prefabs/SpawnAreaPrefab.prefab`, `Assets/Scenes/Experiments.unity`, `Assets/Scenes/TerrainOffset.unity` e outros 250+ pontos). Nenhuma cena precisa ser reaberta manualmente.

### 3.3 Guarda contra `Goal == null` em `Agent.Start`

**Antes:** `_goalPosition = Goal.transform.position;` lançaria `NullReferenceException` se uma `SpawnArea` fosse configurada com `initialAgentsGoalList` vazio ou com referência quebrada.

**Depois:**
```csharp
if (Goal == null)
{
    Debug.LogWarning($"{name} spawnado sem Goal. Verifique SpawnArea.initialAgentsGoalList.");
    _goalPosition = transform.position;
}
else
{
    _goalPosition = Goal.transform.position;
}
```

Falha silenciosa eliminada, sem mudar o comportamento padrão.

### 3.4 Comentários do `Update()` realinhados

Após a reordenação dos passos (líder agora vem antes da troca), os comentários numerados ainda referenciavam "Passos 4 e 5", o que não correspondia mais à sequência real. Renumerados para refletir a ordem atual.

---

## Arquivos alterados

| Arquivo | Natureza da alteração |
|---|---|
| `Assets/Code/World.cs` | Remoção do caminho legado, novo dict `_groupLeaders`, `GetGroupLeader`, `SyncAgentToGroup`, reordenação do `Update`, fix no loop de `UpdateGroupMembership`. |
| `Assets/Code/Agent.cs` | Remoção de `SwitchGroup`, `GetGoalIndex`/`SetGoalIndex`, guarda em `Start`, coesão real em `CalculateDirection`. |
| `Assets/Scripts/SpawnArea.cs` | Movido para `namespace Biocrowds.Core`, `cycleLenght` → `cycleLength` com `[FormerlySerializedAs]`. |

Nenhum prefab, cena ou `.meta` foi modificado manualmente.

---

## Conformidade com Musse & Thalmann (1997)

O paper define três níveis na hierarquia: **crowd → groups → agents**, com o grupo carregando a rota coletiva e o líder ditando direção. Antes desta refatoração:

- O grupo existia no nome (`groupId`) mas a rota era do agente.
- O líder existia no nome (`isGroupLeader`) mas não influenciava ninguém.

Depois desta refatoração:

- A rota é **herdada do líder** no momento da troca.
- O líder **atrai mecanicamente** os membros via coesão.
- A eleição do líder acontece **antes** das trocas, garantindo consistência.

Isso ainda não é uma replicação completa do paper — faltam:

- Dominância como fator de absorção inter-grupo (não só de liderança intra-grupo).
- Matriz de relação grupo-a-grupo.
- Status emocional do grupo.
- Comportamento de *splitting*.
- Reprodutibilidade (seed fixa) e métricas exportáveis.

Esses itens estão planejados como Fase A (validação) e Fase B (núcleo do paper) e serão tratados nos próximos commits.

---

## Próximos passos (não incluídos neste commit)

**Fase A — Validação experimental:**
1. `Random.InitState(SEED)` em `World.Awake` para reprodutibilidade.
2. Campos `groupAffinityMean` e `groupAffinitySpread` em `SpawnArea` para gerar agentes do mesmo grupo com afinidades coerentes (hoje todas aleatórias, o que torna a média do grupo um ruído).
3. Logger CSV gravando contagem de trocas, tempo médio em grupo, distância intra-grupo.

**Fase B — Núcleo do paper:**
4. Regra de absorção por dominância em encontros de grupos.
5. Matriz `_groupRelations` definindo afinidade/hostilidade par-a-par.
6. `_groupEmotion` modulando `_maxSpeed` e `groupCohesionStrength`.
7. *Splitting* de grupos grandes ou com variância alta de afinidade.

**Fase C — Suporte experimental:**
8. Cenários reproduzíveis do paper (encontro de 2 grupos, fluxo bidirecional, gargalo).
9. `OnDrawGizmos` para `GROUP_DETECTION_RADIUS` e labels de debug.
10. Diferenciar `LONE_AGENT_JOIN_THRESHOLD` de `AFFINITY_SWITCH_THRESHOLD`.

---

## Mensagem sugerida para o commit

```
refactor(groups): unify group-switching path and fix leader/cohesion/route bugs

- Remove duplicated legacy switching path in World.Update (UpdateGroupAffinities,
  EvaluateGroupProximityAndSwitches and helpers). Single path now:
  ComputeGroupData → UpdateGroupLeaders → DetectApproachingGroupPairs →
  UpdateGroupMembership → UpdateLoneAgents.
- Add Dictionary<int, Agent> _groupLeaders and World.GetGroupLeader for
  agent-side queries.
- Add SyncAgentToGroup helper that copies goalsList/Goal/goalIndex/waitList
  from the destination group leader, fixing the bug where group switches did
  not update the route.
- Implement actual cohesion in Agent.CalculateDirection: non-leaders are pulled
  toward the elected leader using groupCohesionStrength.
- Skip lone agents inside UpdateGroupMembership (they are handled by
  UpdateLoneAgents).
- Move SpawnArea into namespace Biocrowds.Core.
- Rename SpawnArea.cycleLenght → cycleLength with FormerlySerializedAs to
  preserve existing serialized values.
- Guard Agent.Start against null Goal.
```
