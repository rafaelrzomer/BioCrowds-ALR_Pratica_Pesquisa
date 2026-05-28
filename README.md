# BioCrowds — Dinâmicas de Grupo

Implementação em Unity do modelo **BioCrowds** (Bicho et al., 2012), estendida com **dinâmicas de grupo** inspiradas em Musse & Thalmann (1997). Projeto de Prática em Pesquisa — PUCRS / VHLab, sob orientação da Prof.ª Dr.ª Soraia Raupp Musse.

Baseado em <https://github.com/Virtual-Humans-Lab/BioCrowds>.

---

## Releases (Dev Log)

Resumo por release. **Detalhes completos em [`CHANGELOG.md`](CHANGELOG.md).** Tags publicadas no GitHub: <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

| Tag | Data | Resumo | Commit |
|---|---|---|---|
| `v0.9.0` | 28/05/2026 | Diamante 3D como marcador do líder; remoção de brilho/escala do corpo; `SpawnNewAgent` legado alinhado. | _pendente_ |
| `v0.8.0` | 28/05/2026 | `Debug.Break` removido; grupos ordenados no Inspector; remoção segura de agentes; marcador do líder configurável. | `400b8b9` |
| `v0.7.0` | 22/05/2026 | `Group` + `GroupManager`; sync de goals; tenure de líder; affinity por `SpawnArea`; diamante; tecla `G`. | `7a3b226` |
| `v0.6.0` | 21/05/2026 | Local avoidance; modulação por personalidade; `WAIT_TIME_MULTIPLIER`. | `a33d674` |
| `v0.5.0` | 14/05/2026 | Correção de 4 bugs de troca; coesão por pesos; performance (throttle/pooling); destaque visual do líder. | `9e2cf24` |
| `v0.4.0` | 13/05/2026 | Solos formam/entram em grupos; grace period; cores por grupo. | `b876990` |
| `v0.3.0` | 08/05/2026 | Média de afinidade por grupo; troca de grupo por afinidade. | `99efa94` |
| `v0.2.0` | 06/05/2026 | `dominance`/`affinity`/`groupId`; coesão; eleição de líder. | `8707bdc` |
| `v0.1.0` | 21/04/2026 | Fork do BioCrowds-GS + cena do museu. | `93028ed` |

Como criar uma nova release: ver [`CHANGELOG.md`](CHANGELOG.md#como-criar-uma-nova-release).

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
| 🚧 | Em andamento na sessão atual |
| ⏳ | Próximo na fila (curto prazo) |
| 📊 | Médio prazo — testes e métricas (a discutir com o grupo) |
| 🔬 | Longo prazo — pesquisa e extensões |
| ⚠️ | Bloqueado / requer Editor Unity ou decisão coletiva |

### ✅ Já entregue

Histórico detalhado por release em [`CHANGELOG.md`](CHANGELOG.md). Resumo das versões na tabela **Releases (Dev Log)** acima.

### ⏳ Curto prazo — pendente

| Status | Item | Notas |
|:---:|---|---|
| ⏳ | Migrar `Update` → `FixedUpdate` | Desacopla simulação do frame de render. Afeta toda a malha de chamadas — **adiado**: alto risco, precisa de sessão dedicada com teste runtime. |
| ⏳ | Spatial grid via `CurrentCell ± 1` | Acelera `FindNearbyGroupMembers` e proximidade entre grupos. **Adiado**: só vale para multidões grandes; mantém O(N²) simples por ora. |
| ✅ | Adicionar `GroupManager` em todas as cenas | Resolvido via auto-bootstrap: `World.Awake` cria um `GroupManager` se nenhum existir na cena. |
| ✅ | Limpeza de grupos vazios em `GroupManager` | `GroupManager.PruneEmptyGroups()` chamado ao fim de cada eval cycle em `World`. |

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
