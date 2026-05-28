# Changelog — BioCrowds Dinâmicas de Grupo

Histórico detalhado de mudanças (Dev Log). Cada release está mapeada para uma tag Git (`git tag`) e referencia a entrada correspondente do **Caderno de Pesquisa** (`context/Caderno de Pesquisa - prática dem pesquisa.md`).

Resumo enxuto no [README](README.md#releases-dev-log). Tags publicadas aparecem em <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e [Versionamento Semântico](https://semver.org/lang/pt-BR/).

---

## [v0.9.0] — 28/05/2026 (9ª reunião) — _pendente de tag_

- Marcador do líder trocado por **modelo 3D** (prefab *Sims green diamond*) via `_leaderMarkerPrefab`.
- Removidos brilho e escala 1.25× do corpo do líder (`_highlightLeaderBody`, `_leaderBrighten`, `_leaderScale` apagados) — líder marcado **apenas** pelo diamante.
- `SpawnNewAgent` legado alinhado a `SpawnNewAgentInArea` (define `groupId`, `timeSinceSpawn`, `dominance`, `affinity`).
- Confirmado: affinity do spawner já é aplicada corretamente (sem sobrescrita em `Agent.Start`). Log de spawn opcional gated por `DEBUG_LOG_GROUP_CHANGES`.

> ⚠️ **Aviso (config de cena):** SpawnArea com `affinityMin/affinityMax = [0, 1]` gera afinidade aleatória total — os membros desse grupo **não** têm coesão de afinidade entre si. Para um grupo coeso, aperte o range no Inspector da SpawnArea (ex.: `[0.4, 0.5]`). Na `Museu.unity`, o grupo 0 está com `[0, 1]`.

## [v0.8.0] — 28/05/2026 — `400b8b9`

- Remoção de `Debug.Break()` em `SetupWorld` (pausava o Editor logo após `_isReady = true`, bloqueando movimento e eleição de líder).
- `GroupManager.GetOrCreate` insere ordenado por `Id`, alinhando `Element N` do Inspector ao `groupId` real.
- `Assets/Editor/GroupDrawer.cs`: `PropertyDrawer` pinta o header da lista como `"Grupo {id}"`.
- Remoção segura de agentes: `World` chama `GroupManager.RemoveAgent` antes de `Destroy` (evita NRE por refs Unity-null em `Group.Agents` / `Leader`).
- Diamante do líder usa a cor exata do grupo (sem lavar para branco) e re-tinge ao trocar de grupo.
- Marcador do líder configurável no Inspector de `VisualAgent` (`_showLeaderMarker`, `_leaderMarkerPrefab` opcional com fallback procedural, `_tintMarkerWithGroupColor`, altura/escala/rotação).

## [v0.7.0] — 22/05/2026 (8ª reunião) — `7a3b226`

- `Group.cs` (id, leader, agents, goals) + `GroupManager.cs` (singleton, lista serializada, lookup O(1), `MoveAgent`, `SetLeader`, `DumpToLog`).
- Sincronização contínua de goals no `WaitStep` (follower copia `CurrentGoalIndex` do líder).
- Comportamento por distância ao líder via `leaderSyncRadius` (sincroniza dentro do raio; vai em direção ao líder fora dele).
- Fallback de líder morto (follower age com `goalsList` próprio até nova eleição).
- Afinidade por `SpawnArea` (`affinityMin` / `affinityMax`); bug do `Agent.Start()` que sobrescrevia o valor corrigido.
- Eleição com tenure mínima (`LEADER_MIN_TENURE = 5s`).
- Diamante procedural acima da cabeça do líder em `VisualAgent` (mesh cacheado estaticamente).
- Tecla `G` → `GroupManager.DumpToLog()`.

## [v0.6.0] — 21/05/2026 — `a33d674`

- Local avoidance (repulsão de curto alcance) para quebrar formação em fila.
- Modulação por personalidade em `GetF` e `CalculateVelocity` (`dominance`, `affinity`).
- `WAIT_TIME_MULTIPLIER` em `World` — agentes esperam mais em cada goal.
- Ajustes: `GROUP_PROXIMITY_DISTANCE = 15`, `AFFINITY_SWITCH_THRESHOLD = 0.6`, `GROUP_SWITCH_GRACE_PERIOD = 0.1`.

## [v0.5.0] — 14/05/2026 (7ª reunião) — `9e2cf24`

- `timeSinceSpawn = 0f` no spawn (grace period passa a valer de verdade).
- 4 bugs de troca corrigidos:
  - `EvaluateSoloAgentsMeetings` não move o mesmo solo para múltiplos grupos no mesmo frame.
  - `EvaluateGroupSwaps` sem swap recíproco oscilatório (coleta nos dois sentidos e aplica só o bloco maior).
  - `EvaluateSoloAgentsJoiningGroups` não reprocessa agente já agrupado.
  - `GROUP_PROXIMITY_DISTANCE` ajustado (1.0 era inviável com `agentRadius = 1.0`).
- Coesão por modulação de pesos via `_effectiveGoalDir` em `GetF` (preserva *collision-free* do BioCrowds).
- Coesão escalada por `1/√groupSize` (anti-jam em grupos grandes).
- Performance: throttle `GROUP_EVAL_INTERVAL = 5`, `sqrMagnitude`, *early exit*, pooling (`_groupsScratch`, `_agentListPool`, `_soloScratch`).
- Marcador visual do líder: brilho (`Color.Lerp → white, 0.4`) + escala `× 1.25` via `ApplyGroupColor(Color, bool isLeader)`.
- Diagnóstico `DEBUG_LOG_GROUP_CHANGES` com contadores por eval cycle.
- Eliminação de viés contra grupo de menor `id` (desempate aleatório; solo escolhe grupo mais afim, não o primeiro).

## [v0.4.0] — 13/05/2026 — `b876990`

- `EvaluateSoloAgentsMeetings`: dois solos próximos com afinidade compatível formam grupo via `_nextGroupId`.
- `EvaluateSoloAgentsJoiningGroups`: solo entra em grupo próximo afim.
- `GROUP_SWITCH_GRACE_PERIOD` pós-spawn.
- Cores por grupo via `GroupColorManager` singleton + `VisualAgent.ApplyGroupColor`.

## [v0.3.0] — 08/05/2026 — `99efa94`

- `World._groupAffinityAverages` (média de afinidade por grupo).
- `EvaluateGroupProximityAndSwitches` + `ShouldAgentSwitchGroup`: troca de grupo por diferença de afinidade entre grupos próximos.
- `Agent.SwitchGroup`.

## [v0.2.0] — 06/05/2026 (5ª reunião) — `8707bdc`

- `Agent.dominance` e `Agent.affinity` com `[Range(0,1)]`.
- `groupId` em `Agent` e `SpawnArea`; propagação via `World.SpawnNewAgent`.
- Coesão de grupo: `FindNearbyGroupMembers` + força de atração na direção do líder.
- Eleição de líder por maior `dominance` (`UpdateGroupLeaders`).

## [v0.1.0] — 21/04/2026 — `93028ed`

- Fork do BioCrowds-GS e cenário do museu adicionado ao projeto.

---

## Como criar uma nova release

```bash
# Anotada na branch atual (dev-Humberto-Pedro)
git tag -a v0.X.0 -m "Resumo da release"

# Publicar no GitHub (cria a entrada em Releases)
git push origin v0.X.0
```

No GitHub, abra <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases/new>, selecione a tag e cole a entrada correspondente deste changelog.
