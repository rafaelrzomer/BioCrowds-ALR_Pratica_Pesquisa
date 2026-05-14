# BioCrowds — Dinâmicas de Grupo

Implementação em Unity do modelo **BioCrowds** (Bicho et al., 2012), estendida com **dinâmicas de grupo** inspiradas em Musse & Thalmann (1997). Projeto de Prática em Pesquisa — PUCRS / VHLab, sob orientação da Prof.ª Dr.ª Soraia Raupp Musse.

Baseado em <https://github.com/Virtual-Humans-Lab/BioCrowds>.

---

## Visão Geral

BioCrowds simula multidões usando **marcadores espaciais (auxins)** disputados por agentes dentro de um raio de percepção. Movimento é livre de colisões por construção matemática.

Esta versão adiciona:

- **Hierarquia crowd → groups → agents** com `groupId`, `affinity`, `dominance`.
- **Trocas de grupo emergentes** baseadas em proximidade e diferença de afinidade.
- **Coesão por liderança**: não-líderes aplicam força de atração ao líder próximo.
- **Visualização por cor de grupo** via `GroupColorManager`.
- **Múltiplos métodos de geração de marcadores**: grade regular e dart-throwing (Poisson-disk).
- **Sistema de goals sequenciais** com tempos de espera por agente.
- **Spawn areas** repetíveis ao longo da simulação.

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
│   ├── Agent.cs                 # Agente individual + coesão
│   ├── Cell.cs                  # Célula do grid espacial
│   └── Auxin.cs                 # Marcador disputado
├── Scripts/
│   ├── SceneController.cs       # Tecla 1 = LoadWorld, R = Reload
│   ├── SpawnArea.cs             # Área de spawn com groupId
│   ├── SimulationConfiguration.cs
│   ├── GroupColorManager.cs     # Singleton de cores por grupo
│   └── MarkerSpawn/
│       ├── MarkerSpawner.cs
│       ├── RegularGridMarkerSpawner.cs
│       └── DartThrowingMarkerSpawner.cs
├── Visualization/Scripts/VisualAgent.cs
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
| `GROUP_PROXIMITY_DISTANCE` | 1.0 | Raio de detecção entre grupos |
| `GROUP_SWITCH_GRACE_PERIOD` | 1.0 s | Carência pós-spawn para trocar de grupo |
| `AFFINITY_SWITCH_THRESHOLD` | 0.3 | Diferença mínima de afinidade para trocar |

---

## Fundamentação Teórica

| Paper | Papel no projeto |
|---|---|
| Bicho et al. (2012) — *Simulating crowds based on a space colonization algorithm* | Algoritmo base de movimento por marcadores |
| Musse & Thalmann (1997) — *A model of human crowd behavior* | Hierarquia de grupos e troca de afiliação |
| Knob et al. — *Perception of Personality Traits in Crowds of Virtual Humans* | OCEAN (extensão futura) |
| Silva et al. — *WebCrowds* | Inspiração para métricas |

**Equação central** (Bicho et al.):

```
m⃗ = Σ wₖ · (a⃗ₖ − x⃗)
```

Vetor de movimento é o somatório, para cada marcador `k`, do peso `wₖ` vezes o vetor do agente até o marcador. O peso é função do ângulo entre `(goal − agente)` e `(marcador − agente)`.

---

## Repositório e Branches

| Branch | Função |
|---|---|
| `main` | Estável; recebe PRs aprovados |
| `dev-Humberto-Pedro` | Trabalho atual de Humberto + Pedro Idalencio |

PRs vão de `dev-Humberto-Pedro` → `main` em <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa>.

---

## Roadmap

Alinhado ao **Caderno de Pesquisa** do grupo (Google Docs) e às reuniões com a orientadora. Itens marcados ✅ já estão implementados (ver `CLAUDE.md`, Seção 8). Itens ⬜ pendentes seguem em ordem aproximada de prioridade.

### Já entregue (referência)

- ✅ Fork e organização do repositório (21/04/2026).
- ✅ Cenário do museu adicionado ao projeto.
- ✅ Campos `dominance` e `affinity` em `Agent` com randomização no `Start()` (06/05/2026).
- ✅ `groupId` em `Agent`, `SpawnArea` e propagação via `World.SpawnNewAgent`.
- ✅ Coesão de grupo: `FindNearbyGroupMembers` + força de atração ao líder (06/05/2026).
- ✅ Eleição de líder por maior `dominance` (`UpdateGroupLeaders`).
- ✅ Média de afinidade por grupo (`GroupAffinityAverages`, 08/05/2026).
- ✅ Troca de grupo por proximidade + diferença de afinidade (`EvaluateGroupProximityAndSwitches`).
- ✅ Agentes solo formam grupo entre si (`EvaluateSoloAgentsMeetings`, 13/05/2026).
- ✅ Agentes solo entram em grupos existentes (`EvaluateSoloAgentsJoiningGroups`).
- ✅ `GROUP_SWITCH_GRACE_PERIOD` pós-spawn (13/05/2026).
- ✅ **Cores por grupo** via `GroupColorManager` + `VisualAgent.ApplyGroupColor` (sugerido em 07/05/2026).
- ✅ **Correção de bugs da troca de grupo (sessão maio/2026):**
  - Bug 1 — `EvaluateSoloAgentsMeetings` movia o mesmo agente solo para múltiplos grupos no mesmo frame; agora checa `HasGroup` antes de processar e quebra após formar par.
  - Bug 2 — `EvaluateGroupSwaps` permitia *swap recíproco oscilatório* entre dois grupos; agora coleta decisões nas duas direções e aplica só o bloco maior.
  - Bug 3 — `EvaluateSoloAgentsJoiningGroups` reprocessava agente já agrupado no mesmo frame; agora pula `soloAgent.HasGroup`.
  - Bug 4 — `GROUP_PROXIMITY_DISTANCE = 1.0` era inviável (exigia 2 pares a ≤ 0.5 m com `agentRadius = 1.0`); ajustado para `3.0` e comentário corrigido.
- ✅ **Coesão de grupo via modulação de pesos (alternativa C — alinhada ao paper OCEAN):** seguidor agora blenda `goalDir` com `leaderDir` em `_effectiveGoalDir`; `GetF(k)` usa essa direção efetiva, então o vetor de movimento continua sendo somatório ponderado dos auxins do agente. Preserva a garantia *collision-free* do BioCrowds (Bicho et al.).
- ✅ **Performance — throttle e `sqrMagnitude`:**
  - `GROUP_EVAL_INTERVAL = 5`: dinâmica de grupo roda 1 a cada 5 simulation steps (em vez de 50 Hz) → ~80% menos custo nessa seção.
  - `Vector3.Distance` substituído por `sqrMagnitude` em `AreGroupsNearby`, `EvaluateSoloAgentsMeetings`, `EvaluateSoloAgentsJoiningGroups`.
  - `AreGroupsNearby` ganhou *early exit* assim que `anyWithinProx && closePairs >= 2`.

### Curto prazo — itens entregues nesta sessão

- ✅ **Marcador visual do líder** (07/05/2026). `VisualAgent.ApplyGroupColor(Color, bool isLeader)` agora aplica brilho (`Color.Lerp → white, 0.4`) e escala (`× 1.25`) quando o agente é líder. `Agent.ApplyGroupColor()` passa a flag; `World.UpdateGroupLeaders` detecta transição de liderança via `_previousLeaders` e re-aplica cor só nos agentes que mudaram de estado.
- ✅ **Analisar `timeSinceSpawn`** (14/05/2026, 7ª reunião). Default de `Agent.timeSinceSpawn` era `2f`, que **bypassava** o grace period (`GROUP_SWITCH_GRACE_PERIOD = 1.0f`) já no spawn. Corrigido para `0f`; `World.SpawnNewAgentInArea` também reseta explicitamente. Agora agentes esperam `GROUP_SWITCH_GRACE_PERIOD` segundos antes de poder trocar de grupo.
- ✅ **Frame rate — 2ª rodada (pooling + iteração por grupo):**
  - Dicionário `_groupsScratch` e pool `_agentListPool` reutilizados a cada eval cycle — zero `new Dictionary` / `new List` por frame nos métodos de grupo.
  - `UpdateGroupAffinities` reaproveita `_groupsScratch` já montado em `UpdateGroupLeaders`.
  - `EvaluateGroupProximityAndSwitches` substitui `groups.Values.ToList()` por iteração index-based sobre array snapshot.
  - `EvaluateGroupSwaps` usa listas pooled `_toSwitchToA` / `_toSwitchToB`.
  - `EvaluateSoloAgentsMeetings` e `EvaluateSoloAgentsJoiningGroups` compartilham `_soloScratch`; o segundo re-filtra in-place os solos que ganharam grupo no primeiro.
- ✅ **Limpeza de warnings `CS0414`:**
  - Removidos `VisualAgent.updated` e `VisualAgent.initialized` (campos não-lidos) e as atribuições correspondentes.
  - `World._maxAgents` preservado (serializado em `Museu.unity`, `Experiments.unity`, `Test.unity`) e silenciado com `#pragma warning disable 0414`.
- ✅ **Eliminação do viés contra grupo de menor id:**
  - `EvaluateGroupSwaps` agora resolve empates de migração com `Random.value < 0.5f` em vez de sempre favorecer `group1 → group2`. Grupos de id baixo (que aparecem primeiro no dicionário) deixam de ser sempre os "doadores" em ties.
  - `EvaluateSoloAgentsJoiningGroups` agora varre **todos** os grupos próximos e escolhe o de **menor `affinityDifference`** dentro do threshold, em vez de fazer `break` no primeiro compatível. Solos entram no grupo mais afim, não no de menor id.
- ✅ **Coesão escalada por tamanho do grupo (anti-jam):**
  - `World._groupSizes` populado a cada eval cycle + accessor público `GetGroupSize(int)`.
  - `Agent.CalculateDirection` agora aplica `effectiveCohesion = groupCohesionStrength * (1 / sqrt(groupSize))`. Grupos maiores aplicam menos puxão por agente, evitando o "traffic jam" onde muitos seguidores disputam os mesmos auxins na trajetória do líder.
- ✅ **Diagnóstico opcional de trocas de grupo:**
  - Campo `DEBUG_LOG_GROUP_CHANGES` (bool) no Inspector de `World`. Quando ligado, loga por eval cycle: `[Groups] cycle: swaps=X newGroups=Y soloJoins=Z groups=N agents=M` (só nos ciclos em que algo aconteceu).
  - Contadores internos `_switchesThisCycle`, `_newGroupsThisCycle`, `_soloJoinsThisCycle`.

### Curto prazo — pendente

- ⬜ **Reparar `Assets/Prefabs/AgentPrefab.prefab`** — prefab aninhado com `guid: 7dcf00d1126974d4996a7ef29c81ca22` faltando. Correção precisa ser feita pelo Editor Unity (não é seguro editar o YAML do prefab à mão).
- ⬜ **Migrar `Update` → `FixedUpdate`** para desacoplar simulação do frame de render. Mudança ainda pendente; afeta toda a malha de chamadas e merece teste cuidadoso.
- ⬜ **Spatial grid via `CurrentCell ± 1`** para `FindNearbyGroupMembers` e proximidade entre grupos. Ganho proporcional a `N`; só vale para multidões grandes.

### Médio prazo — testes e métricas (caderno)

- ⬜ **Bateria de testes de variação** (14/05/2026 — *"dois grupos com muita afinidade e dois grupos com afinidades muito distantes, testar variação de comportamentos"*). Configurar cenas com afinidades controladas e comparar resultados.
- ⬜ **Inspeção visual e métricas** (01/04/2026). Logging de eventos: número de trocas de grupo, formações solo→solo, ingressos solo→grupo, tempo médio em grupo, distância média intra-grupo.
- ⬜ **Métricas inspiradas em WebCrowds:** Density Map, Trajectories Map, Simulation Time.
- ⬜ **Interface runtime para ditar grupos e comportamentos** (01/04/2026 — *"Pequena interface para ditar grupos e comportamentos"*). Painel no Play exibindo/editando `groupId`, `affinity`, `dominance`, `isGroupLeader`.
- ⬜ **Seed reproduzível.** Substituir `Random.Range` nos `Start()` de `Agent` por RNG inicializado em `World` com seed no Inspector — pré-requisito para comparar runs.
- ⬜ **Exportação de dados** (CSV / JSON) das métricas para análise externa.

### Longo prazo — pesquisa e extensões

- ⬜ **Pontos de densidade e caminhos preferenciais** (01/04/2026 — *"Pontos de densidade, caminhos específicos (caminhos futuros)"*). Identificar gargalos no cenário do museu.
- ⬜ **Aplicação a eventos culturais / museus** (01/04/2026 — objetivo aplicado). Cena do museu com `SpawnAreas` e `Goals` representando salas, corredores e saídas; uso da simulação para evitar aglomerações e preservar liberdade de movimento.
- ⬜ **Comparação quantitativa** entre execuções com `ALLOW_GROUP_CHANGES` ligado e desligado.
- ⬜ **Otimização O(N²) → O(N log N)** dos loops de proximidade — grid espacial ou KD-tree (relacionado ao ponto de frame rate).
- ⬜ **POISSON_DISK_SAMPLING spawner.** Enum em `SimulationConfiguration.cs` mas sem classe concreta.
- ⬜ **Personalidade OCEAN** (Knob et al., extensão futura). Adicionar `openness, conscientiousness, extraversion, agreeableness, neuroticism` ao `Agent` e modular o peso `wₖ` por Extraversion:

  ```
  w'ₖ,ᵢ = δᵢ · wₖ,ᵢ · Eᵢ + (1 − δᵢ) · (1 − Eᵢ)
  ```

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