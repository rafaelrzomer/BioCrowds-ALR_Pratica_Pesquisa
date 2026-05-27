# BioCrowds — Dinâmicas de Grupo

Implementação em Unity do modelo **BioCrowds** (Bicho et al., 2012), estendida com **dinâmicas de grupo** inspiradas em Musse & Thalmann (1997). Projeto de Prática em Pesquisa — PUCRS / VHLab, sob orientação da Prof.ª Dr.ª Soraia Raupp Musse.

Baseado em <https://github.com/Virtual-Humans-Lab/BioCrowds>.

---

## Visão Geral

BioCrowds simula multidões usando **marcadores espaciais (auxins)** disputados por agentes dentro de um raio de percepção. Movimento é livre de colisões por construção matemática.
---

## Releases (Dev Log)

Cada release está mapeada para uma tag Git (`git tag`) e referencia diretamente uma entrada do **Caderno de Pesquisa** (`context/Caderno de Pesquisa - prática dem pesquisa.md`). Tags publicadas no GitHub aparecem em <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

| Tag | Data (Caderno) | Resumo | Commit |
|---|---|---|---|
| `v0.1.0` | 21/04/2026 | Fork do BioCrowds-GS e cenário do museu adicionado. | `93028ed` |
| `v0.2.0` | 06/05/2026 (5ª reunião) | `Agent.dominance` e `Agent.affinity` com `[Range(0,1)]`. `groupId` em `Agent` e `SpawnArea`. `FindNearbyGroupMembers` + força de coesão na direção do líder. `UpdateGroupLeaders` elege líder por maior `dominance`. | `8707bdc` |
| `v0.3.0` | 08/05/2026 | `World._groupAffinityAverages` (média de afinidade por grupo). `EvaluateGroupProximityAndSwitches` + `ShouldAgentSwitchGroup`: troca de grupo por diferença de afinidade entre grupos próximos. `Agent.SwitchGroup`. | `99efa94` |
| `v0.4.0` | 13/05/2026 | `EvaluateSoloAgentsMeetings` (dois solos próximos com afinidade compatível formam grupo via `_nextGroupId`). `EvaluateSoloAgentsJoiningGroups` (solo entra em grupo próximo afim). `GROUP_SWITCH_GRACE_PERIOD` pós-spawn. `GroupColorManager` singleton + `VisualAgent.ApplyGroupColor`. | `b876990` |
| `v0.5.0` | 14/05/2026 (7ª reunião) | `timeSinceSpawn = 0f` no spawn (grace period passa a valer de verdade). 4 bugs de troca corrigidos (solo duplo-grupo, swap oscilante, reprocesso de solo, proximidade inviável). Coesão por modulação de pesos via `_effectiveGoalDir` (preserva *collision-free*). Throttle de dinâmica de grupo (`GROUP_EVAL_INTERVAL = 5`), `sqrMagnitude`, *early exit*, pooling (`_groupsScratch`, `_agentListPool`, `_soloScratch`). Brilho/escala 1.25× no líder via `VisualAgent.ApplyGroupColor(Color, bool)`. | `9e2cf24` |
| `v0.6.0` | 21/05/2026 | Local avoidance (repulsão de curto alcance) para quebrar formação em fila. Modulação por personalidade em `GetF` e `CalculateVelocity` (dominance, affinity). `WAIT_TIME_MULTIPLIER` em `World`. `GROUP_PROXIMITY_DISTANCE = 15`, `AFFINITY_SWITCH_THRESHOLD = 0.6`, `GROUP_SWITCH_GRACE_PERIOD = 0.1`. | `a33d674` |
| `v0.7.0` | 22/05/2026 (8ª reunião) | `Group.cs` (id, leader, agents, goals) + `GroupManager.cs` (singleton, lista serializada, lookup O(1), `MoveAgent`, `SetLeader`, `DumpToLog`). Sincronização contínua de goals no `WaitStep` (follower copia `CurrentGoalIndex` do líder). Comportamento por distância ao líder via `leaderSyncRadius`. Fallback de líder morto. Afinidade por `SpawnArea` (`affinityMin` / `affinityMax`). Eleição com tenure mínima (`LEADER_MIN_TENURE = 5s`). Diamante procedural acima da cabeça do líder (`VisualAgent`). Tecla `G` → `DumpToLog`. | `7a3b226` |
| `v0.8.0` (próximo) | 27/05/2026 | Remoção de `Debug.Break()` em `SetupWorld` (estava pausando o Editor logo após `_isReady = true`, bloqueando movimento e eleição de líder). `GroupManager.GetOrCreate` agora insere ordenado por `Id`, alinhando `Element N` do Inspector ao `groupId` real. `Assets/Editor/GroupDrawer.cs`: `PropertyDrawer` que pinta o header da lista como `"Grupo {id}"` em vez de `"Element N"`. | _pendente_ |

### Como criar uma nova release

```bash
# Anotada na branch atual (dev-Humberto-Pedro)
git tag -a v0.X.0 -m "Resumo da release"

# Publicar no GitHub (cria a entrada em Releases)
git push origin v0.X.0
```

No GitHub, abra <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases/new>, selecione a tag e cole o resumo da entrada correspondente do Caderno.

---

## Requisitos

| Item | Valor |
|---|---|
| Engine | Unity **2020.3.33f1 (LTS)** — versão obrigatória |
| Linguagem | C# 8 |
| Plataforma | Windows / macOS / Linux (Editor) |

---

## Estrutura do Repositório

```
Assets/
├── Code/                        # Núcleo BioCrowds (Biocrowds.Core)
│   ├── World.cs                 # Orquestrador + lógica de grupos
│   ├── Agent.cs                 # Agente individual + coesão + avoidance
│   ├── Cell.cs                  # Célula do grid espacial
│   ├── Auxin.cs                 # Marcador disputado
│   ├── Group.cs                 # Classe Group (id, leader, agents, goals)
│   └── GroupManager.cs          # Registro central (Inspector + tecla G)
├── Scripts/
│   ├── SceneController.cs       # Tecla 1 = LoadWorld, R = Reload
│   ├── SpawnArea.cs             # Área de spawn com groupId + affinityMin/Max
│   ├── SimulationConfiguration.cs
│   ├── GroupColorManager.cs     # Singleton de cores por grupo
│   └── MarkerSpawn/
│       ├── MarkerSpawner.cs
│       ├── RegularGridMarkerSpawner.cs
│       └── DartThrowingMarkerSpawner.cs
├── Visualization/Scripts/VisualAgent.cs  # Cor, brilho do líder, diamante
├── Prefabs/Agents/
└── Scenes/                      # Cena do museu
```

---

## Controles da Simulação

| Tecla | Ação |
|---|---|
| `1` | `world.LoadWorld()` — gera células, marcadores e agentes |
| `R` | Recarrega a cena ativa |
| `2` | Debug `SpawnArea` (loga ponto aleatório) |
| `G` | `GroupManager.DumpToLog()` — loga grupos, líderes e membros no Console |

A **Game View** precisa ter foco do teclado.

---

## Parâmetros Principais (`World.cs`)

| Campo | Default | Função |
|---|---|---|
| `SIMULATION_TIME_STEP` | 0.02 | Passo fixo da simulação |
| `AGENT_RADIUS` | 1.0 | Raio do agente |
| `AUXIN_RADIUS` | 0.1 | Raio do marcador |
| `AUXIN_DENSITY` | 0.5 | Densidade de marcadores |
| `ALLOW_GROUP_CHANGES` | true | Master switch das trocas de grupo |
| `GROUP_PROXIMITY_DISTANCE` | 15.0 | Raio de detecção entre grupos |
| `GROUP_SWITCH_GRACE_PERIOD` | 0.1 s | Carência pós-spawn para trocar de grupo |
| `AFFINITY_SWITCH_THRESHOLD` | 0.6 | Diferença mínima de afinidade para trocar |
| `LEADER_MIN_TENURE` | 5.0 s | Tempo mínimo de um líder no cargo antes de poder ser substituído |
| `GROUP_EVAL_INTERVAL` | 5 | Roda dinâmica de grupos a cada N simulation steps |
| `WAIT_TIME_MULTIPLIER` | 1.0 | Multiplica `goalsWaitList[i]` (subir = agentes esperam mais em cada goal) |

**`SpawnArea.cs`:**

| Campo | Default | Função |
|---|---|---|
| `groupId` | -1 | Grupo dos agentes spawnados (`-1` = solo) |
| `affinityMin` / `affinityMax` | 0 / 1 | Faixa de `Random.Range` para `affinity` dos agentes spawnados |

**`Agent.cs`:**

| Campo | Default | Função |
|---|---|---|
| `leaderSyncRadius` | 6.0 | Multiplica `agentRadius`; dentro desse raio o seguidor sincroniza goal com o líder |
| `groupCohesionStrength` | 0.3 | Peso da força de coesão (escalada por `1/√groupSize` em tempo de execução) |

---

## Fundamentação Teórica

| Paper | Papel no projeto |
|---|---|
| Bicho et al. (2012) — *Simulating crowds based on a space colonization algorithm* | Algoritmo base de movimento por marcadores |
| Musse & Thalmann (1997) — *A model of human crowd behavior* | Hierarquia de grupos e troca de afiliação |
| Knob et al. — *Perception of Personality Traits in Crowds of Virtual Humans* | OCEAN (extensão futura) |
| Silva et al. — *WebCrowds* | Inspiração para métricas |

**Equação central** (Bicho et al.):

$$\vec{m} = \sum_{k=1}^{n} w_k \cdot (\vec{a}_k - \vec{x})$$

Vetor de movimento $\vec{m}$ é o somatório, para cada marcador $k$, do peso $w_k$ vezes o vetor do agente $\vec{x}$ até o marcador $\vec{a}_k$. O peso é função do ângulo entre $(\text{goal} - \text{agente})$ e $(\text{marcador} - \text{agente})$.

---

## Repositório e Branches

| Branch | Função |
|---|---|
| `main` | Estável; recebe PRs aprovados |
| `dev-Humberto-Pedro` | Trabalho atual de Humberto + Pedro Idalencio |

PRs vão de `dev-Humberto-Pedro` → `main` em <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa>.

---

## Roadmap

Alinhado ao **Caderno de Pesquisa** do grupo (Google Docs) e às reuniões com a orientadora.

**Legenda de status:**

| Marcador | Significado |
|:---:|---|
| ✅ | Entregue — ver seção **Releases (Dev Log)** acima |
| 🚧 | Em andamento na sessão atual |
| ⏳ | Próximo na fila (curto prazo) |
| 📊 | Médio prazo — testes e métricas (a discutir com o grupo) |
| 🔬 | Longo prazo — pesquisa e extensões |
| ⚠️ | Bloqueado / requer Editor Unity ou decisão coletiva |

### ✅ Já entregue

Resumo cronológico. Detalhes técnicos por commit/tag em **Releases (Dev Log)** acima.

| Status | Release | Data (Caderno) | Item |
|:---:|:---:|:---:|---|
| ✅ | `v0.1.0` | 21/04/2026 | Fork do BioCrowds-GS e cenário do museu adicionado ao projeto. |
| ✅ | `v0.2.0` | 06/05/2026 | Campos `dominance` e `affinity` em `Agent` com `[Range(0,1)]`. |
| ✅ | `v0.2.0` | 06/05/2026 | `groupId` em `Agent` e `SpawnArea`; propagação via `World.SpawnNewAgent`. |
| ✅ | `v0.2.0` | 06/05/2026 | Coesão de grupo: `FindNearbyGroupMembers` + força de atração ao líder. |
| ✅ | `v0.2.0` | 06/05/2026 | Eleição de líder por maior `dominance` (`UpdateGroupLeaders`). |
| ✅ | `v0.3.0` | 08/05/2026 | Média de afinidade por grupo (`World.GroupAffinityAverages`). |
| ✅ | `v0.3.0` | 08/05/2026 | Troca de grupo por proximidade + diferença de afinidade (`EvaluateGroupProximityAndSwitches`, `ShouldAgentSwitchGroup`, `Agent.SwitchGroup`). |
| ✅ | `v0.4.0` | 13/05/2026 | Agentes solo formam grupo entre si (`EvaluateSoloAgentsMeetings` + `_nextGroupId`). |
| ✅ | `v0.4.0` | 13/05/2026 | Agentes solo entram em grupos existentes (`EvaluateSoloAgentsJoiningGroups`). |
| ✅ | `v0.4.0` | 13/05/2026 | `GROUP_SWITCH_GRACE_PERIOD` pós-spawn. |
| ✅ | `v0.4.0` | 13/05/2026 | Cores por grupo via `GroupColorManager` singleton + `VisualAgent.ApplyGroupColor`. |
| ✅ | `v0.5.0` | 14/05/2026 | Correção: `EvaluateSoloAgentsMeetings` não move o mesmo solo para múltiplos grupos no mesmo frame. |
| ✅ | `v0.5.0` | 14/05/2026 | Correção: `EvaluateGroupSwaps` sem swap recíproco oscilatório (coleta nos dois sentidos e aplica só o bloco maior). |
| ✅ | `v0.5.0` | 14/05/2026 | Correção: `EvaluateSoloAgentsJoiningGroups` não reprocessa agente já agrupado. |
| ✅ | `v0.5.0` | 14/05/2026 | `GROUP_PROXIMITY_DISTANCE` ajustado (1.0 era inviável com `agentRadius = 1.0`). |
| ✅ | `v0.5.0` | 14/05/2026 | `Agent.timeSinceSpawn = 0f` no spawn — grace period passa a valer. |
| ✅ | `v0.5.0` | 14/05/2026 | Coesão via modulação de pesos (`_effectiveGoalDir` em `GetF`); preserva *collision-free* do BioCrowds. |
| ✅ | `v0.5.0` | 14/05/2026 | Coesão escalada por `1/√groupSize` (anti-jam em grupos grandes). |
| ✅ | `v0.5.0` | 14/05/2026 | Throttle `GROUP_EVAL_INTERVAL = 5`, `sqrMagnitude`, *early exit*, pooling (`_groupsScratch`, `_agentListPool`, `_soloScratch`). |
| ✅ | `v0.5.0` | 14/05/2026 | Marcador visual do líder: brilho (`Color.Lerp → white, 0.4`) + escala `× 1.25` via `ApplyGroupColor(Color, bool isLeader)`. |
| ✅ | `v0.5.0` | 14/05/2026 | Diagnóstico `DEBUG_LOG_GROUP_CHANGES` com contadores por eval cycle. |
| ✅ | `v0.5.0` | 14/05/2026 | Eliminação de viés contra grupo de menor `id` (desempate aleatório; solo escolhe grupo mais afim, não o primeiro). |
| ✅ | `v0.6.0` | 21/05/2026 | Local avoidance (repulsão de curto alcance) — quebra formação em fila. |
| ✅ | `v0.6.0` | 21/05/2026 | Modulação por personalidade em `GetF` e `CalculateVelocity` (`dominance`, `affinity`). |
| ✅ | `v0.6.0` | 21/05/2026 | `WAIT_TIME_MULTIPLIER` em `World` — agentes esperam mais em cada goal. |
| ✅ | `v0.7.0` | 22/05/2026 | `Group.cs` (id, leader, agents, goals) + `GroupManager.cs` (singleton, lista serializada, lookup O(1), `MoveAgent`, `SetLeader`, `DumpToLog`). |
| ✅ | `v0.7.0` | 22/05/2026 | Sincronização contínua de goals: follower copia `CurrentGoalIndex` do líder em `WaitStep`. |
| ✅ | `v0.7.0` | 22/05/2026 | Comportamento por distância ao líder via `leaderSyncRadius` (sincroniza dentro do raio; vai em direção ao líder fora dele). |
| ✅ | `v0.7.0` | 22/05/2026 | Fallback de líder morto (follower age com `goalsList` próprio). |
| ✅ | `v0.7.0` | 22/05/2026 | Afinidade por `SpawnArea` (`affinityMin` / `affinityMax`); bug do `Agent.Start()` que sobrescrevia o valor corrigido. |
| ✅ | `v0.7.0` | 22/05/2026 | Eleição com tenure mínima (`LEADER_MIN_TENURE = 5s`). |
| ✅ | `v0.7.0` | 22/05/2026 | Diamante procedural acima da cabeça do líder em `VisualAgent` (mesh cacheado estaticamente). |
| ✅ | `v0.7.0` | 22/05/2026 | Tecla `G` → `GroupManager.DumpToLog()`. |
| ✅ | `v0.8.0` | 27/05/2026 | `Debug.Break()` em `SetupWorld` removido — destravar movimento e eleição de líder. |
| ✅ | `v0.8.0` | 27/05/2026 | `GroupManager.GetOrCreate` insere ordenado por `Id` — `Element N` do Inspector alinhado ao `groupId`. |
| ✅ | `v0.8.0` | 27/05/2026 | `Assets/Editor/GroupDrawer.cs` — `PropertyDrawer` pinta header como `"Grupo {id}"`. |

### ⏳ Curto prazo — pendente

| Status | Item | Notas |
|:---:|---|---|
| ⚠️ | Reparar `Assets/Prefabs/AgentPrefab.prefab` | Prefab aninhado com `guid: 7dcf00d1126974d4996a7ef29c81ca22` faltando. Correção precisa ser feita pelo Editor Unity (não é seguro editar o YAML à mão). |
| ⏳ | Migrar `Update` → `FixedUpdate` | Desacopla simulação do frame de render. Afeta toda a malha de chamadas — requer teste cuidadoso. |
| ⏳ | Spatial grid via `CurrentCell ± 1` | Acelera `FindNearbyGroupMembers` e proximidade entre grupos. Ganho proporcional a `N`; vale para multidões grandes. |
| ⏳ | Adicionar `GroupManager` em todas as cenas | Sem o componente, o registro central fica inativo (código tem fallback, mas Inspector / tecla `G` não funcionam). |
| ⏳ | Limpeza de grupos vazios em `GroupManager` | Grupos cujos membros migraram permanecem na lista (intencional para depuração); revisar antes de medir métricas. |

### 📊 Médio prazo — testes e métricas (Caderno)

> Itens levantados na **8ª reunião com a orientadora (21/05/2026)**. ⚠️ Requerem alinhamento com o grupo antes de virar implementação — não modificar arquitetura sem decisão coletiva.

| Status | Item | Notas |
|:---:|---|---|
| 📊 | Cenários múltiplos para experimentos | Duplicar a cena do museu com variações controladas: poucos vs. muitos agentes, afinidades polarizadas vs. uniformes, layout aberto vs. corredor. Gravar vídeo e anotar métricas. |
| 📊 | Métricas para os experimentos | **Coesão de grupo:** distância média ao centróide do grupo. **Trocas de grupo** por intervalo. **Tamanho dos grupos** ao longo da simulação. |
| 📊 | HUD runtime de métricas | Painel ao vivo, separado do Console / Inspector. |
| 📊 | Exportação de dados | CSV / JSON para análise externa e gráficos. |
| 📊 | Bateria de testes de variação | Caderno 14/05/2026 — *"dois grupos com muita afinidade e dois grupos com afinidades muito distantes, testar variação de comportamentos"*. |
| 📊 | Métricas inspiradas em WebCrowds | Density Map, Trajectories Map, Simulation Time. |
| 📊 | Interface runtime para ditar grupos e comportamentos | Caderno 01/04/2026. Painel no Play exibindo/editando `groupId`, `affinity`, `dominance`, `isGroupLeader`. |
| 📊 | Seed reproduzível | Substituir `Random.Range` por RNG inicializado em `World` com seed no Inspector — pré-requisito para comparar runs. |
| 📊 | Estrutura do trabalho/apresentação final | 8ª reunião: Introdução → Trabalhos relacionados → Modelo (o que foi adicionado, parâmetros novos, resultados) → Métricas dos experimentos. |

### 🔬 Longo prazo — pesquisa e extensões

| Status | Item | Notas |
|:---:|---|---|
| 🔬 | Pontos de densidade e caminhos preferenciais | Caderno 01/04/2026 — *"Pontos de densidade, caminhos específicos (caminhos futuros)"*. Identificar gargalos no cenário do museu. |
| 🔬 | Aplicação a eventos culturais / museus | Caderno 01/04/2026 — objetivo aplicado. `SpawnAreas` e `Goals` como salas/corredores/saídas; evitar aglomerações e preservar liberdade de movimento. |
| 🔬 | Comparação quantitativa `ALLOW_GROUP_CHANGES` on/off | Validar empiricamente o impacto da dinâmica de grupo. |
| 🔬 | Otimização O(N²) → O(N log N) | Loops de proximidade via grid espacial ou KD-tree (relacionado ao ponto de frame rate). |
| 🔬 | `POISSON_DISK_SAMPLING` spawner | Enum em `SimulationConfiguration.cs` declarado, mas sem classe concreta. |
| 🔬 | Personalidade OCEAN (Knob et al.) | Adicionar `openness, conscientiousness, extraversion, agreeableness, neuroticism` ao `Agent`. Modular o peso $w_k$ por Extraversion: $w'_{k,i} = \delta_i \cdot w_{k,i} \cdot E_i + (1 - \delta_i) \cdot (1 - E_i)$. |

### Questões de pesquisa norteadoras (Caderno, 01/04/2026)

1. Como os agentes mudam de grupo em meio a outras pessoas?
2. Quando os agentes se aproximam, como eles trocam de grupo?
3. Qual a distância e diferença de afinidade necessária para disparar uma troca de grupo?

**Objetivo geral:** avaliar dinâmicas de grupo no espaço e como elas funcionam emergentemente, com aplicação em organização de eventos culturais (cenário do museu).

---

## WebCrowds

Versão WebCrowds disponível em <https://github.com/Virtual-Humans-Lab/BioCrowds-GS/tree/WebCrowds>.

---

## Licença e Créditos

Trabalho acadêmico sobre a base do **VHLab — PUCRS** (<https://www.inf.pucrs.br/vhlab/>).
