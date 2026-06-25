# BioCrowds — Dinâmicas de Grupo

Implementação em Unity do modelo **BioCrowds** (Bicho et al., 2012), estendida com **dinâmicas de grupo** inspiradas em Musse & Thalmann (1997). Projeto de Prática em Pesquisa — PUCRS / VHLab, sob orientação da Prof.ª Dr.ª Soraia Raupp Musse.

Baseado em <https://github.com/Virtual-Humans-Lab/BioCrowds>.

---

## Releases (Dev Log)

Resumo por release. **Detalhes completos em [`CHANGELOG.md`](CHANGELOG.md).** Tags publicadas no GitHub: <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

| Tag | Data | Resumo | Commit |
|---|---|---|---|
| `v0.10.0` | 23/06/2026 | Métricas: snapshot público + **HUD runtime** (`MetricsHUD`, tecla `M`); CSV na raiz (`Metrics/`); seed reproduzível; métrica **tempo médio em grupo**; flag `ALLOW_GROUP_CHANGES` no CSV/HUD; script Python de gráficos (`tools/plot_metrics.py`). Correções de movimento: NRE do NavMesh, afundamento no chão (lock XZ), flicker (RNG por-frame removido). | _pendente_ |
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
│   ├── MetricsLogger.cs         # Grava CSV (groups + summary) na raiz do projeto
│   ├── MetricsHUD.cs            # HUD runtime de métricas (OnGUI, tecla M)
│   ├── AgentInspectorHUD.cs     # Inspetor por-agente: clique p/ ver/editar grupo e atributos (tecla I)
│   ├── TimeController.cs        # Controle de tempo: pausa/acelera a sim (teclas P, [, ], \)
│   └── MarkerSpawn/
│       ├── MarkerSpawner.cs
│       ├── RegularGridMarkerSpawner.cs
│       └── DartThrowingMarkerSpawner.cs
├── Visualization/Scripts/VisualAgent.cs  # Cor, brilho do líder, diamante
├── Prefabs/Agents/
└── Scenes/                      # Cena do museu

tools/
├── plot_metrics.py             # Gera gráficos (PNG) dos CSVs de métricas (pandas+matplotlib)
├── build_xlsx.py               # Gera relatório .xlsx (Excel) com gráficos nativos, tabelas e fórmulas (pandas+xlsxwriter)
├── compare_runs.py             # Sobrepõe uma métrica (ex.: numStuck, numGroups) de várias runs num gráfico
├── plot_trajectories.py        # Mapa de trajetórias + mapa de densidade (heatmap) do positions.csv
└── report.bat                  # Windows: duplo-clique → roda build_xlsx + plot_metrics no run mais recente
```

---

## Controles da Simulação

| Tecla | Ação |
|---|---|
| `1` | `world.LoadWorld()` — gera células, marcadores e agentes |
| `R` | Recarrega a cena ativa |
| `2` | Debug `SpawnArea` (loga ponto aleatório) |
| `G` | `GroupManager.DumpToLog()` — loga grupos, líderes e membros no Console |
| `M` | Liga/desliga a **HUD de métricas** (`MetricsHUD`) |
| `I` | Liga/desliga o **Inspetor de agente** (`AgentInspectorHUD`) — clique num agente para ver/editar `groupId`, `affinity`, `dominance`, `isGroupLeader` |
| `P` | Pausa / retoma a simulação (`TimeController`) |
| `[` / `]` | Diminui / aumenta a velocidade da simulação (0.25× … 4×) |
| `\` | Volta a velocidade para 1× (normal) |

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
| ✅ | Realizado / Completo |
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
| ⏳ | Definição e montagem do **cenário complexo** | 10ª reunião (11/06/2026). Cenário de demonstração que evidencie evolução de grupos por afinidade. Base para os experimentos do artigo. |
| ✅ | Exportação de dados (CSV) | `MetricsLogger.cs` cria **um diretório por run** em `Metrics/<prefix>_<timestamp>/` na **raiz do projeto** (no Editor; em build, pasta do executável), com `groups.csv` (time, groupId, groupSize, coesão, afinidade média, desvio, tempo médio em grupo) e `summary.csv` (time, agentes/grupos/solos, trocas, flag `ALLOW_GROUP_CHANGES`). Também grava cópias `*_excel.csv` no formato pt-BR (`;` e `,`) p/ abrir no Excel com duplo-clique. Amostra a cada eval cycle. Pasta `Metrics/` ignorada pelo git. |

### 📊 Médio prazo — testes e métricas (Caderno)

> Itens levantados na **8ª reunião com a orientadora (21/05/2026)**. ⚠️ Requerem alinhamento com o grupo antes de virar implementação — não modificar arquitetura sem decisão coletiva.

| Status | Item | Notas |
|:---:|---|---|
| 🚧 | Cenários múltiplos para experimentos | Cenas `Cena#6A`/`Cena#6B`/`Sociograma` já criadas em `Assets/Scenes/CenasTeste/`. Falta variar parâmetros de forma controlada (poucos vs. muitos agentes, afinidades polarizadas vs. uniformes, layout aberto vs. corredor), gravar vídeo e anotar métricas. Inclui o **cenário complexo** da 10ª reunião. **Adicionar `MetricsLogger` a cada cena.** |
| 🚧 | Sociograma | Cena `Sociograma.unity` criada (merge da `main`). Falta reproduzir o sociograma do trabalho original (Musse & Thalmann) para os resultados do artigo. |
| ✅ | Métricas para os experimentos | Implementadas no `MetricsLogger.cs`: **coesão de grupo** (distância média ao centróide — coluna CSV `cohesion`; **rotulada nos gráficos como _Dispersão_: menor = mais coeso**), **trocas de grupo** (intervalo + acumuladas), **tamanho dos grupos**, **nº de grupos/solos**, **desvio-padrão de afinidade** (proxy de coesão social), **tempo médio em grupo** (`timeInGroup` por agente, agregado por grupo) e a flag **`ALLOW_GROUP_CHANGES`** (0/1) no summary. |
| ✅ | Gráficos a partir dos CSVs | `tools/plot_metrics.py` (pandas+matplotlib): localiza o run mais recente em `Metrics/`, gera PNGs + `dashboard.png` (população, trocas, dispersão/afinidade/tamanho/tempo por grupo) em `Metrics/<run>/plots/`. Uso: `python tools/plot_metrics.py` (opções `--run`, `--dpi`). |
| ✅ | Relatório Excel (.xlsx) | `tools/build_xlsx.py` (pandas+xlsxwriter): monta `Metrics/<run>/relatorio.xlsx` com tabelas formatadas, **gráficos nativos do Excel** (editáveis), **fórmulas** (KPIs MAX/AVERAGE), aba por métrica de grupo (pivot tempo×grupo) e aba **Config** (parâmetros da run). Uso: `python tools/build_xlsx.py`. |
| ✅ | Métrica de jam (agentes travados) | `numStuck` no summary + HUD + linha "Travados" no gráfico de população: conta agentes que **não estão esperando** mas estão com velocidade `< STUCK_SPEED_THRESHOLD` (≈parados). Quantifica gridlock/densidade. |
| ✅ | Rastreabilidade de parâmetros | `config.csv` por run (`Metrics/<run>/`) com seed, `MAX_AGENTS`, thresholds etc.; HUD mostra seed + maxAg. Liga qual run veio de quais parâmetros. |
| ✅ | Comparação entre runs | `tools/compare_runs.py`: sobrepõe uma métrica do summary (ex.: `numStuck`, `numGroups`, `totalSwitches`) de várias runs num gráfico → `Metrics/comparisons/`. Uso: `python tools/compare_runs.py --metric numStuck --last 3`. |
| ✅ | HUD runtime de métricas | `MetricsHUD.cs` (OnGUI, tecla `M`): painel ao vivo lendo o snapshot público do `World` (tempo, nº de agentes/grupos/solos, trocas total+ciclo, **agentes travados**, **seed/maxAg**, e por grupo tamanho/coesão/afinidade±desvio). Atualiza a cada eval cycle, independente do CSV. |
| ✅ | Seed reproduzível | `World` expõe `USE_SEED` + `RANDOM_SEED` no Inspector; `Awake` chama `Random.InitState(RANDOM_SEED)` antes de qualquer spawn. Como `UnityEngine.Random` é global, fixa toda a população inicial (afinidade, dominância, posições de marcadores). Pré-requisito para comparar runs. |
| 📊 | Estrutura do trabalho/apresentação final | 8ª reunião: Introdução → Trabalhos relacionados → Modelo (o que foi adicionado, parâmetros novos, resultados) → Métricas dos experimentos. Detalhamento do artigo na seção **Artigo** acima. |
| 📊 | Spatial grid via `CurrentCell ± 1` | Acelera `FindNearbyGroupMembers` e proximidade entre grupos. **Adiado**: só vale para multidões grandes; mantém O(N²) simples por ora. |
| 📊 | Bateria de testes de variação | Caderno 14/05/2026 — *"dois grupos com muita afinidade e dois grupos com afinidades muito distantes, testar variação de comportamentos"*. |
| ✅ | Mapas de densidade e trajetórias (WebCrowds) | `MetricsLogger` grava `positions.csv` (time, agentId, x, z, groupId) por run; `tools/plot_trajectories.py` gera **mapa de trajetórias** (caminho de cada agente, cor = grupo) e **mapa de densidade** (heatmap 2D) em `Metrics/<run>/plots/`. |
| ✅ | Controle de tempo da simulação | `TimeController.cs` (teclas `P` pausa, `[`/`]` velocidade, `\` 1×): a sim avança em passos fixos; `World.SimSpeed` controla passos/frame (0.25×–4×), `World.SimPaused` congela. |
| ✅ | Interface runtime para ditar grupos e comportamentos | Caderno 01/04/2026. `AgentInspectorHUD.cs` (OnGUI, tecla `I`): clique seleciona o agente (por proximidade na tela, sem precisar de Collider) e abre painel para **ver/editar** `affinity`, `dominance` (sliders), `groupId` (via `SwitchGroup`, atualiza GroupManager+cor) e `isGroupLeader` (toggle transitório — `UpdateGroupLeaders` pode reverter). Mostra também `goalIndex`, `isWaiting`, nº de vizinhos do grupo e idade; toggle **câmera segue** o agente selecionado. |
### 🔬 Longo prazo — pesquisa e extensões

| Status | Item | Notas |
|:---:|---|---|
| 🔬 | Pontos de densidade e caminhos preferenciais | Caderno 01/04/2026 — *"Pontos de densidade, caminhos específicos (caminhos futuros)"*. Os **mapas** já saem de `tools/plot_trajectories.py` (densidade + trajetórias); falta a **análise** (identificar gargalos no museu a partir deles). |
| 🔬 | Aplicação a eventos culturais / museus | Caderno 01/04/2026 — objetivo aplicado. `SpawnAreas` e `Goals` como salas/corredores/saídas; evitar aglomerações e preservar liberdade de movimento. |
| 🔬 | Comparação quantitativa `ALLOW_GROUP_CHANGES` on/off | Validar empiricamente o impacto da dinâmica de grupo. Estado gravado no CSV (`groupChangesEnabled`) + HUD; `tools/compare_runs.py` já sobrepõe runs on×off. Falta rodar as duas baterias e escrever a análise. |
| 🔬 | Otimização O(N²) → O(N log N) | Loops de proximidade via grid espacial ou KD-tree (relacionado ao ponto de frame rate). |
| 🔬 | `POISSON_DISK_SAMPLING` spawner | Enum em `SimulationConfiguration.cs` declarado, mas sem classe concreta. |
| 🔬 | Personalidade OCEAN (Knob et al.) | Adicionar `openness, conscientiousness, extraversion, agreeableness, neuroticism` ao `Agent`. Modular o peso $w_k$ por Extraversion: $w'_{k,i} = \delta_i \cdot w_{k,i} \cdot E_i + (1 - \delta_i) \cdot (1 - E_i)$. |
| 🔬 | Emoções dos agentes | Estado afetivo por agente (ex.: medo/estresse em aglomeração, conforto em grupo afim) que module velocidade, coesão e propensão à troca de grupo. Complementa OCEAN (traço estável) com estado dinâmico; conecta às linhas de densidade/aglomeração e à literatura de emoção em multidões (VHLab). |

### ⚠️ Limitações conhecidas das dinâmicas de grupo (investigado em 25/06/2026)

> **Importante para o artigo:** as métricas mostram que, com os parâmetros atuais, **a troca de grupo quase nunca acontece** (`totalSwitches → 0`) e **os grupos ficam sempre com 2 agentes**. Não é bug de código — é consequência do desenho atual do modelo. **Não alterar sem decisão do grupo.**

| Sintoma | Causa raiz | Onde |
|---|---|---|
| **Troca de grupo ≈ 0** (`totalSwitches` reto em zero) | `ShouldAgentSwitchGroup` exige que o outro grupo seja um encaixe **estritamente melhor** (`newDiff < currentDiff`). Mas `currentAvg` **inclui o próprio agente**: num grupo de 2, o agente é "metade" da própria média → `currentDiff = |a−b|/2` é minúsculo → praticamente impossível outro grupo ser melhor. | `World.ShouldAgentSwitchGroup` |
| **Grupos sempre de tamanho 2** | `EvaluateSoloAgentsMeetings` (parear 2 solos) roda **antes** de `EvaluateSoloAgentsJoiningGroups`; com `GROUP_PROXIMITY_DISTANCE = 15`, solos viram par antes de engrossar grupos. Entrar em grupo exige `≥ 2` membros perto (restritivo). **Não existe merge de grupos.** | `World.Update` (ordem) |
| **"Coesão" parece invertida** | A métrica é **distância ao centróide** (maior = mais espalhado). Gráficos/relatório rotulam como **"Dispersão (menor = mais coeso)"**. | scripts `tools/` |

**Impacto nas perguntas de pesquisa (abaixo):** as perguntas 2 e 3 dependem de a troca ocorrer. Para a troca emergir, seria preciso (a decidir com o grupo): excluir o próprio agente da média ao medir `currentDiff`, reordenar/afrouxar a entrada em grupos, baixar `AFFINITY_SWITCH_THRESHOLD`, e/ou adicionar fusão de grupos. **Nenhuma dessas mudanças foi aplicada** — apenas documentada.

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
