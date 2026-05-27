# BioCrowds — Dinâmicas de Grupo

Implementação em Unity do modelo **BioCrowds** (Bicho et al., 2012), estendida com **dinâmicas de grupo** inspiradas em Musse & Thalmann (1997). Projeto de Prática em Pesquisa — PUCRS / VHLab, sob orientação da Prof.ª Dr.ª Soraia Raupp Musse.

Baseado em <https://github.com/Virtual-Humans-Lab/BioCrowds>.

---

## Visão Geral

BioCrowds simula multidões usando **marcadores espaciais (auxins)** disputados por agentes dentro de um raio de percepção. Movimento é livre de colisões por construção matemática.

Esta versão adiciona:

- **Hierarquia crowd → groups → agents** com `groupId`, `affinity`, `dominance`.
- **`Group` + `GroupManager`** — registro central de grupos com lista de membros, líder e goals compartilhados (serializado no Inspector).
- **Trocas de grupo emergentes** baseadas em proximidade e diferença de afinidade.
- **Coesão por liderança**: seguidores são puxados em direção ao líder e sincronizam goals dentro de `leaderSyncRadius`.
- **Eleição de líder com tenure mínima** (`LEADER_MIN_TENURE`) — evita troca oscilante de líder.
- **Marcador visual do líder**: diamante (octaedro) procedural giratório acima da cabeça + brilho + escala 1.25×.
- **Afinidade por `SpawnArea`**: faixa `affinityMin / affinityMax` por área.
- **Multiplicador de tempo de espera** (`WAIT_TIME_MULTIPLIER`).
- **Local avoidance**: repulsão de curto alcance para quebrar formação em fila.
- **Modulação por personalidade**: `dominance` aumenta peso de auxins próximos e velocidade; `affinity < 0.5` adiciona variação estocástica.
- **Visualização por cor de grupo** via `GroupColorManager`.
- **Múltiplos métodos de geração de marcadores**: grade regular e dart-throwing (Poisson-disk).
- **Sistema de goals sequenciais** com tempos de espera por agente.
- **Spawn areas** repetíveis ao longo da simulação.

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

### Já entregue

Veja a seção **Releases (Dev Log)** acima — cada release lista os entregáveis com seu commit/tag e a data correspondente no Caderno.

### Curto prazo — pendente

- ⬜ **Reparar `Assets/Prefabs/AgentPrefab.prefab`** — prefab aninhado com `guid: 7dcf00d1126974d4996a7ef29c81ca22` faltando. Correção precisa ser feita pelo Editor Unity (não é seguro editar o YAML do prefab à mão).
- ⬜ **Migrar `Update` → `FixedUpdate`** para desacoplar simulação do frame de render. Mudança ainda pendente; afeta toda a malha de chamadas e merece teste cuidadoso.
- ⬜ **Spatial grid via `CurrentCell ± 1`** para `FindNearbyGroupMembers` e proximidade entre grupos. Ganho proporcional a `N`; só vale para multidões grandes.
- ⬜ **Adicionar `GroupManager` em todas as cenas existentes.** Sem componente na cena, o registro central fica inativo (código tem fallback, mas Inspector / tecla `G` não funcionam).
- ⬜ **Limpeza de grupos vazios** em `GroupManager`. Hoje, grupos cujos membros migraram permanecem na lista (intencional para depuração); revisar antes de medir métricas.

### Médio prazo — testes e métricas (caderno) — **a discutir com o grupo**

> Itens abaixo foram levantados na **8ª reunião com a orientadora (21/05/2026)** e precisam de alinhamento com o grupo antes de virar implementação. Não modificar arquitetura sem decisão coletiva.

- ⬜ **Cenários múltiplos para experimentos.** Duplicar a cena do museu com variações controladas: poucos vs. muitos agentes, afinidades polarizadas vs. uniformes, layout aberto vs. corredor. Gravar vídeo de cada um e anotar métricas.
- ⬜ **Métricas para os experimentos** (orientadora):
  - **Coesão de grupo:** distância média de cada agente ao centróide do seu grupo.
  - **Trocas de grupo** por intervalo de tempo.
  - **Tamanho dos grupos** ao longo da simulação.
- ⬜ **HUD runtime** mostrando métricas ao vivo (separado do Console / Inspector).
- ⬜ **Exportação de dados** (CSV / JSON) para análise externa e gráficos.
- ⬜ **Bateria de testes de variação** (14/05/2026 — *"dois grupos com muita afinidade e dois grupos com afinidades muito distantes, testar variação de comportamentos"*).
- ⬜ **Métricas inspiradas em WebCrowds:** Density Map, Trajectories Map, Simulation Time.
- ⬜ **Interface runtime para ditar grupos e comportamentos** (01/04/2026 — *"Pequena interface para ditar grupos e comportamentos"*). Painel no Play exibindo/editando `groupId`, `affinity`, `dominance`, `isGroupLeader`.
- ⬜ **Seed reproduzível.** Substituir `Random.Range` por RNG inicializado em `World` com seed no Inspector — pré-requisito para comparar runs.
- ⬜ **Estrutura do trabalho/apresentação final** (8ª reunião): Introdução → Trabalhos relacionados → Modelo (o que foi adicionado, parâmetros novos, resultados) → Métricas dos experimentos.

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