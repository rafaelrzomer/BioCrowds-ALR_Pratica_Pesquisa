# BioCrowds — Dinâmicas de Grupo

Implementação em Unity do modelo **BioCrowds** (Bicho et al., 2012), estendida com **dinâmicas de grupo** inspiradas em Musse & Thalmann (1997). Projeto de Prática em Pesquisa — PUCRS / VHLab, sob orientação da Prof.ª Dr.ª Soraia Raupp Musse.

Baseado em <https://github.com/Virtual-Humans-Lab/BioCrowds>.

---

## Releases (Dev Log)

Resumo por release. **Detalhes completos em [`CHANGELOG.md`](CHANGELOG.md).** Tags publicadas no GitHub: <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

| Tag | Data | Resumo | Commit |
|---|---|---|---|
| `v0.10.0` | 25/06/2026 | **Métricas:** HUD runtime (`M`), CSV por run (+cópias pt-BR `*_excel.csv` e `config.csv`), tempo médio em grupo, jam (`numStuck`), dispersão, flag `ALLOW_GROUP_CHANGES`, seed reproduzível. **Tooling Python:** gráficos (`plot_metrics`), relatório `.xlsx` (`build_xlsx`), comparador de runs (`compare_runs`), mapas de densidade/trajetória (`plot_trajectories`), `report.bat`. **Runtime:** inspetor de agente (`I`), controle de tempo (`P`/`[`/`]`/`\`). **Correções:** NavMesh NRE, afundamento (lock XZ), flicker (RNG por-frame). | `b590c13`, `a11dac4`, `f447a2e`, `5877ac6`, `194c44c`, `8307b8b` |
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

## Métricas e Relatórios

Cada run grava um diretório `Metrics/<prefix>_<timestamp>/` na **raiz do projeto** (só no Editor; em build vai pra pasta do `.exe`). A pasta `Metrics/` é ignorada pelo git.

| Arquivo | Conteúdo |
|---|---|
| `summary.csv` | 1 linha por amostra: tempo, nº de agentes/grupos/solos, trocas, jam (`numStuck`), flag `ALLOW_GROUP_CHANGES` |
| `groups.csv` | 1 linha por grupo por amostra: tamanho, dispersão (dist. ao centróide), afinidade média/desvio, tempo em grupo |
| `positions.csv` | posição (x,z) de cada agente por amostra — base dos mapas de trajetória/densidade |
| `config.csv` | parâmetros da run (seed, `MAX_AGENTS`, thresholds) — rastreabilidade |
| `*_excel.csv` | cópias no formato pt-BR (`;` e `,`) — abrem no Excel com **duplo-clique** |

> CSVs padrão (`,` / `.`) são para pandas e os scripts. As cópias `*_excel.csv` (`;` / `,`) abrem direto no Excel pt-BR. O CSV padrão também abre no Excel via **Dados → De Texto/CSV** (delimitador vírgula, local Inglês-EUA).

### Gerar relatórios

Requer Python: `pip install pandas matplotlib xlsxwriter`.

- **Windows (1 clique):** duplo-clique em **`tools/report.bat`** → gera tudo do run mais recente.
- **Manual** (run mais recente por padrão; ou `--run <pasta>`):
  ```bash
  python tools/build_xlsx.py         # relatorio.xlsx: tabelas + gráficos nativos do Excel + fórmulas
  python tools/plot_metrics.py       # PNGs por métrica + dashboard.png
  python tools/plot_trajectories.py  # mapa de trajetórias + mapa de densidade (heatmap)
  python tools/compare_runs.py --metric numStuck --last 3   # compara N runs numa métrica
  ```

Saídas: `relatorio.xlsx` em `Metrics/<run>/`, PNGs em `Metrics/<run>/plots/`, comparações em `Metrics/comparisons/`.

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

### 📊 Médio prazo — testes e métricas (Caderno)

> Itens levantados na **8ª reunião com a orientadora (21/05/2026)**. ⚠️ Requerem alinhamento com o grupo antes de virar implementação — não modificar arquitetura sem decisão coletiva.

| Status | Item | Notas |
|:---:|---|---|
| 🚧 | Cenários múltiplos para experimentos | Cenas `Cena#6A`/`Cena#6B`/`Sociograma` já criadas em `Assets/Scenes/CenasTeste/`. Falta variar parâmetros de forma controlada (poucos vs. muitos agentes, afinidades polarizadas vs. uniformes, layout aberto vs. corredor), gravar vídeo e anotar métricas. Inclui o **cenário complexo** da 10ª reunião. **Adicionar `MetricsLogger` a cada cena.** |
| 🚧 | Sociograma | Cena `Sociograma.unity` criada (merge da `main`). Falta reproduzir o sociograma do trabalho original (Musse & Thalmann) para os resultados do artigo. |
| 📊 | Estrutura do trabalho/apresentação final | 8ª reunião: Introdução → Trabalhos relacionados → Modelo (o que foi adicionado, parâmetros novos, resultados) → Métricas dos experimentos. |
| 📊 | Bateria de testes de variação | Caderno 14/05/2026 — *"dois grupos com muita afinidade e dois grupos com afinidades muito distantes, testar variação de comportamentos"*. |
| ✅ | Métricas, gráficos, `.xlsx`, mapas, HUD, inspetor, controle de tempo, seed | **Entregue** — uso na seção [Métricas e Relatórios](#métricas-e-relatórios) e [Controles](#controles-da-simulação); histórico no [CHANGELOG](CHANGELOG.md). |

### 🔬 Longo prazo — pesquisa e extensões

| Status | Item | Notas |
|:---:|---|---|
| 🔬 | Pontos de densidade e caminhos preferenciais | Caderno 01/04/2026 — *"Pontos de densidade, caminhos específicos (caminhos futuros)"*. Os **mapas** já saem de `tools/plot_trajectories.py` (densidade + trajetórias); falta a **análise** (identificar gargalos no museu a partir deles). |
| 🔬 | Aplicação a eventos culturais / museus | Caderno 01/04/2026 — objetivo aplicado. `SpawnAreas` e `Goals` como salas/corredores/saídas; evitar aglomerações e preservar liberdade de movimento. |
| 🔬 | Comparação quantitativa `ALLOW_GROUP_CHANGES` on/off | Validar empiricamente o impacto da dinâmica de grupo. Estado gravado no CSV (`groupChangesEnabled`) + HUD; `tools/compare_runs.py` já sobrepõe runs on×off. Falta rodar as duas baterias e escrever a análise. |
| 🔬 | Otimização O(N²) → O(N log N) | Loops de proximidade via grid espacial ou KD-tree (relacionado ao ponto de frame rate). |
| 🔬 | Spatial grid para queries agente-agente (`CurrentCell`) | **Adiado / alto risco** (investigado em 25/06/2026). O grid de auxinas já usa `CurrentCell ± 1` (célula ≈ 2 u). Aplicar nos agentes exige: índice célula→agentes reconstruído por frame, clamp de borda e guarda de `CurrentCell` null. ⚠️ Não é `± 1` fixo: a janela tem que ser `ceil(raio/tamCélula)` — `FindNearbyGroupMembers`/avoidance (~3–4 u) precisam **±2**; `GROUP_PROXIMITY_DISTANCE` (15 u) precisaria **±8**. Usar `± 1` **quebra silenciosamente** a formação/troca de grupos. Só compensa em multidões grandes; com poucos agentes fica mais lento. Se feito, validar com run seeded (métricas devem bater) e decisão do grupo. |
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
