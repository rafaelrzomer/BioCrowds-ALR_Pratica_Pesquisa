# BioCrowds — Notas de Atualização

Mudanças por versão. Cada patch está mapeado para uma tag Git e para uma entrada do **Caderno de Pesquisa** (`context/Caderno de Pesquisa - prática dem pesquisa.md`).

Resumo enxuto no [README](README.md#releases-dev-log). Tags publicadas: <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

Convenções: `valor antigo ⇒ valor novo` para ajustes numéricos. 🆕 novo · 🔧 ajuste · 🐛 correção · ⚡ performance · 🎨 visual.

---

## Patch v0.9.0 — 28/05/2026 · 9ª reunião · _pendente de tag_

### Líder
- 🆕 Marcador = prefab 3D (*Sims green diamond*) via `_leaderMarkerPrefab`.
- 🔧 Escala do corpo do líder: `1.25× ⇒ 1.0×` (removida).
- 🔧 Brilho do corpo do líder: `Lerp→white 0.4 ⇒ removido`. Líder marcado só pelo diamante.
- Campos `_highlightLeaderBody` / `_leaderBrighten` / `_leaderScale` removidos.

### Spawn
- 🔧 `SpawnNewAgent` legado define `groupId`, `timeSinceSpawn`, `dominance`, `affinity` (igual a `SpawnNewAgentInArea`).
- ✅ Affinity do spawner aplicada corretamente (sem sobrescrita em `Agent.Start`). Log opcional gated por `DEBUG_LOG_GROUP_CHANGES`.

### Sistemas de Grupo
- 🆕 Auto-bootstrap: `World.Awake` cria um `GroupManager` se nenhum existir na cena.
- 🆕 `GroupManager.PruneEmptyGroups()` remove grupos sem membros ao fim de cada eval cycle.

> ⚠️ **Config de cena:** SpawnArea com `affinityMin/affinityMax = [0, 1]` gera afinidade aleatória total — membros sem coesão de afinidade. Para grupo coeso, aperte o range no Inspector (ex.: `[0.4, 0.5]`). Na `Museu.unity`, o grupo 0 está com `[0, 1]`.

---

## Patch v0.8.0 — 28/05/2026 · `400b8b9`

### Correções
- 🐛 `Debug.Break()` removido de `SetupWorld` — destrava movimento e eleição de líder.
- 🐛 `World` chama `GroupManager.RemoveAgent` antes de `Destroy` — fim das NRE por refs Unity-null em `Group.Agents` / `Leader`.

### Sistemas de Grupo
- 🔧 `GroupManager.GetOrCreate` insere ordenado por `Id` — `Element N` alinhado ao `groupId`.
- 🆕 `Assets/Editor/GroupDrawer.cs`: header da lista vira `"Grupo {id}"`.

### Líder
- 🆕 Marcador configurável no Inspector de `VisualAgent`: `_showLeaderMarker`, `_leaderMarkerPrefab` (opcional, fallback procedural), `_tintMarkerWithGroupColor`, altura / escala / rotação.
- 🎨 Diamante usa a cor exata do grupo (sem lavar para branco) e re-tinge ao trocar de grupo.

---

## Patch v0.7.0 — 22/05/2026 · 8ª reunião · `7a3b226`

### Sistemas de Grupo
- 🆕 `Group.cs` (id, leader, agents, goals).
- 🆕 `GroupManager.cs` (singleton, lista serializada, lookup O(1), `MoveAgent`, `SetLeader`, `DumpToLog`).
- 🆕 Tecla `G` → `GroupManager.DumpToLog()`.

### Líder & Followers
- 🆕 Sincronização contínua de goals no `WaitStep` (follower copia `CurrentGoalIndex` do líder).
- 🆕 Comportamento por distância via `leaderSyncRadius` (sincroniza dentro do raio; persegue o líder fora dele).
- 🆕 Fallback de líder morto: follower age com `goalsList` próprio até nova eleição.
- 🆕 Eleição com tenure mínima: `LEADER_MIN_TENURE = 5s`.

### Visual & Spawn
- 🆕 Diamante procedural acima da cabeça do líder (mesh cacheado).
- 🆕 Afinidade por `SpawnArea` (`affinityMin` / `affinityMax`).
- 🐛 `Agent.Start()` não sobrescreve mais a affinity do spawner.

---

## Patch v0.6.0 — 21/05/2026 · `a33d674`

### Comportamento
- 🆕 Local avoidance: repulsão de curto alcance entre agentes próximos (quebra formação em fila).
- 🆕 Modulação por personalidade em `GetF` e `CalculateVelocity` (`dominance`, `affinity`).

### Tuning
- 🆕 `WAIT_TIME_MULTIPLIER` em `World`.
- 🔧 `GROUP_PROXIMITY_DISTANCE`: `⇒ 15`.
- 🔧 `AFFINITY_SWITCH_THRESHOLD`: `⇒ 0.6`.
- 🔧 `GROUP_SWITCH_GRACE_PERIOD`: `⇒ 0.1`.

---

## Patch v0.5.0 — 14/05/2026 · 7ª reunião · `9e2cf24`

### Correções de Troca
- 🐛 `EvaluateSoloAgentsMeetings`: não move o mesmo solo para vários grupos no mesmo frame.
- 🐛 `EvaluateGroupSwaps`: fim do swap recíproco oscilatório (aplica só o bloco maior).
- 🐛 `EvaluateSoloAgentsJoiningGroups`: não reprocessa agente já agrupado.
- 🐛 `GROUP_PROXIMITY_DISTANCE` corrigido (1.0 era inviável com `agentRadius = 1.0`).
- 🐛 `timeSinceSpawn`: `2f ⇒ 0f` no spawn — grace period passa a valer.

### Coesão
- 🆕 Coesão por modulação de pesos via `_effectiveGoalDir` em `GetF` (preserva *collision-free*).
- 🔧 Coesão escalada por `1/√groupSize` (anti-jam em grupos grandes).

### Performance
- 🔧 `GROUP_EVAL_INTERVAL`: `50 Hz ⇒ 1 a cada 5 steps`.
- ⚡ `Vector3.Distance ⇒ sqrMagnitude` + *early exit* nos testes de proximidade.
- ⚡ Pooling: `_groupsScratch`, `_agentListPool`, `_soloScratch` (zero alocação por frame).

### Visual & Diagnóstico
- 🎨 Destaque do líder: brilho (`Lerp→white 0.4`) + escala `1.0× ⇒ 1.25×`.
- 🆕 `DEBUG_LOG_GROUP_CHANGES` com contadores por eval cycle.
- 🐛 Fim do viés contra grupo de menor `id` (desempate aleatório; solo escolhe grupo mais afim).

---

## Patch v0.4.0 — 13/05/2026 · `b876990`

### Sistemas de Grupo
- 🆕 `EvaluateSoloAgentsMeetings`: dois solos próximos e afins formam grupo (`_nextGroupId`).
- 🆕 `EvaluateSoloAgentsJoiningGroups`: solo entra em grupo próximo afim.
- 🆕 `GROUP_SWITCH_GRACE_PERIOD` pós-spawn.

### Visual
- 🆕 Cores por grupo via `GroupColorManager` + `VisualAgent.ApplyGroupColor`.

---

## Patch v0.3.0 — 08/05/2026 · `99efa94`

### Sistemas de Grupo
- 🆕 `World._groupAffinityAverages` (média de afinidade por grupo).
- 🆕 `EvaluateGroupProximityAndSwitches` + `ShouldAgentSwitchGroup`: troca por diferença de afinidade.
- 🆕 `Agent.SwitchGroup`.

---

## Patch v0.2.0 — 06/05/2026 · 5ª reunião · `8707bdc`

### Atributos
- 🆕 `Agent.dominance` e `Agent.affinity` com `[Range(0,1)]`.
- 🆕 `groupId` em `Agent` e `SpawnArea`; propagação via `World.SpawnNewAgent`.

### Sistemas de Grupo
- 🆕 Coesão de grupo: `FindNearbyGroupMembers` + atração na direção do líder.
- 🆕 Eleição de líder por maior `dominance` (`UpdateGroupLeaders`).

---

## Patch v0.1.0 — 21/04/2026 · `93028ed`

- 🆕 Fork do BioCrowds-GS.
- 🆕 Cenário do museu adicionado ao projeto.

---

## Como criar uma nova release

```bash
# Anotada na branch atual (dev-Humberto-Pedro)
git tag -a v0.X.0 -m "Resumo da release"

# Publicar no GitHub (cria a entrada em Releases)
git push origin v0.X.0
```

No GitHub, abra <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases/new>, selecione a tag e cole a entrada correspondente destas notas.
