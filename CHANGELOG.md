# BioCrowds — Notas de Atualização

Mudanças por versão. Cada patch está mapeado para uma tag Git e para uma entrada do **Caderno de Pesquisa** (`context/Caderno de Pesquisa - prática dem pesquisa.md`).

Resumo enxuto no [README](README.md#releases-dev-log). Tags publicadas: <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa/releases>.

Convenções: `valor antigo ⇒ valor novo` para ajustes numéricos. 🆕 novo · 🔧 ajuste · 🐛 correção · ⚡ performance · 🎨 visual.

---

## Patch v0.10.0 — 23–25/06/2026 · _pendente de tag_

### Métricas
- 🆕 `MetricsHUD.cs` (OnGUI): HUD runtime no canto da tela. Tecla `M` liga/desliga. Sem Canvas/prefab — basta o componente num GameObject da cena.
- 🆕 Snapshot público em `World`: struct `GroupMetric` + `MetricGroups` (lista read-only) + properties `MetricTime / MetricNumAgents / MetricNumGroups / MetricNumSolo / MetricSwitchesInterval / MetricTotalSwitches`.
- 🔧 `World.RecordMetrics` monta o snapshot **sempre** (alimenta a HUD); escrita no CSV passa a ser condicionada a `LOG_METRICS`. HUD funciona mesmo com logging desligado.
- 🔧 `MetricsLogger`: saída dos CSVs `persistentDataPath/Metrics/ ⇒ raiz do projeto /Metrics/` (via `Directory.GetParent(Application.dataPath)`). No Editor cai dentro do repo; em build, pasta do executável.
- 🆕 `.gitignore`: `/[Mm]etrics/` — CSVs gerados não vão para o git.
- 🆕 Métrica **tempo médio em grupo**: `Agent.timeInGroup` (incrementado em `World.Update` quando `HasGroup`, zerado em `SwitchGroup` e ao formar grupo novo). Agregado por grupo em `RecordMetrics` → coluna `meanTimeInGroup` no `*_groups.csv` + linha por grupo na HUD.
- 🆕 Flag **`ALLOW_GROUP_CHANGES`** exposta (`World.GroupChangesAllowed`): coluna `groupChangesEnabled` (0/1) no `*_summary.csv` + estado ON/OFF na HUD.
- 🆕 `tools/plot_metrics.py` (pandas+matplotlib): gera PNGs + `dashboard.png` do run mais recente em `Metrics/<run>/plots/` (população, trocas, coesão/afinidade/tamanho/tempo por grupo). Tema consistente, step plots, banda ±desvio na afinidade, opções `--run`/`--dpi`.
- 🔧 `MetricsLogger`: **um diretório por run** (`Metrics/<prefix>_<timestamp>/` com `groups.csv`/`summary.csv`) — não mistura runs. Cópias `*_excel.csv` no formato pt-BR (`;` separador, `,` decimal) via flag `WRITE_EXCEL_COPY` → abrem no Excel pt-BR com duplo-clique, sem quebrar o pipeline pandas (que lê os `.csv` padrão).
- 🆕 `tools/build_xlsx.py` (pandas+xlsxwriter): gera `Metrics/<run>/relatorio.xlsx` — tabelas formatadas, **gráficos nativos do Excel** (linha, editáveis), **fórmulas** de KPI (MAX/AVERAGE), uma aba por métrica de grupo (pivot tempo×grupo), aba de dados crus e aba **Config**.
- 🆕 Métrica de **jam** (`numStuck`): conta agentes que não estão esperando mas estão com velocidade `< STUCK_SPEED_THRESHOLD` (≈parados) → coluna no summary, `World.MetricNumStuck`, linha "Travados" na HUD e no gráfico de população. Quantifica gridlock/densidade.
- 🆕 **`config.csv` por run**: `World.BuildConfigCsv()` grava seed, `MAX_AGENTS`, thresholds e demais parâmetros (`MetricsLogger.WriteRunConfig`); HUD exibe seed + maxAg. Rastreia de quais parâmetros cada run saiu.
- 🆕 `tools/compare_runs.py`: sobrepõe uma métrica do summary (ex.: `numStuck`, `numGroups`, `totalSwitches`) de várias runs num gráfico (`Metrics/comparisons/`) — base para comparar `ALLOW_GROUP_CHANGES` on×off, seeds, densidades.
- 🆕 `AgentInspectorHUD.cs` (OnGUI, tecla `I`): clique seleciona um agente (por proximidade na tela, sem Collider) e abre painel para ver/editar `affinity`, `dominance` (sliders), `groupId` (via `SwitchGroup`) e `isGroupLeader` (toggle transitório). Mostra `goalIndex`/`isWaiting`/nº de vizinhos/idade + toggle "câmera segue". Atende o item do Caderno "interface runtime para ditar grupos e comportamentos".
- 🆕 `TimeController.cs` (teclas `P` pausa, `[`/`]` velocidade, `\` 1×): `World.Update` virou wrapper que roda `StepSimulation()` em loop por acumulador; `World.SimSpeed` (0.25×–4×) e `World.SimPaused` controlam o avanço. Comportamento padrão (1×) inalterado.
- 🆕 Mapas de densidade e trajetórias: `MetricsLogger` grava `positions.csv` (time, agentId, x, z, groupId) por eval cycle (flag `LOG_POSITIONS`); `tools/plot_trajectories.py` (pandas+matplotlib) gera mapa de trajetórias (cor = grupo) e heatmap de densidade em `Metrics/<run>/plots/`. Atende Caderno (pontos de densidade/caminhos) e WebCrowds.
- 🔧 `tools/report.bat`: agora também roda `plot_trajectories.py`.
- 🔧 Gráficos: eixo de "Trocas" a partir de 0 (com nota quando não há trocas); "coesão" rotulada como **"Dispersão (menor = mais coeso)"** (coluna CSV segue `cohesion`).

### Duração de run, organização e gráficos focados (25/06/2026 · `2b749b5`, `63c3255`)
- 🆕 **`World.RUN_DURATION`** (Inspector, header *Run Control*): duração da run em **segundos de simulação** (0 = ilimitado). Ao atingir, `FinishRun()` congela a sim (`_runFinished`), fecha os CSVs (`EndSession`) e — se `AUTO_GENERATE_REPORTS` — dispara os scripts Python (gráficos + xlsx) via `System.Diagnostics.Process` (fire-and-forget, não trava a Unity; requer Python no PATH). Reset no `Awake` (tecla `R` recomeça).
- 🔧 **Organização da pasta da run**: `MetricsLogger` grava todos os CSVs num subdiretório **`csv/`**. A raiz da run fica só com `csv/`, `plots/` e `relatorio.xlsx`. Os 3 scripts Python leem de `csv/` com **fallback** para a raiz (runs antigas não quebram).
- 🔧 **Gráficos focados** (decisão de reunião): `plot_metrics.py` reduzido a **2 gráficos** — *(1) Grupos e solos × tempo* e *(2) Dispersão por grupo × tempo* — + `dashboard.png` 1×2. `build_xlsx.py`: aba **Resumo** plota grupos+solos; mantida aba **Dispersao**; removidas abas Afinidade/Tamanho/Tempo e o gráfico de Trocas.
- 🆕 Mapa de densidade **suavizado** opcional: `plot_trajectories.py --smooth` (histograma 2D + `imshow` interpolação gaussiana = gradiente contínuo, sem dependência nova) e **`--ask-smooth`** (pergunta interativa só quando a run é grande, ≥ `--big-threshold`, default 5000 amostras). `report.bat` chama com `--ask-smooth`.
- 🐛 `report.bat`: linha de echo final com `<...>` era lida pelo cmd como redirecionamento de I/O → erro "O sistema não pode encontrar o arquivo especificado" (cosmético, após gerar tudo). Trocado por `[...]`.

> ℹ️ Setup da HUD: adicionar o componente `MetricsHUD` a um GameObject da `Museu.unity` (campo `_world` via Inspector ou auto-find).

### Cenários e experimentos
- 🆕 **Cenário complexo** de demonstração montado (10ª reunião, 11/06/2026): evidencia a evolução de grupos por afinidade no espaço. Base dos experimentos do artigo.
- 🆕 **Cenários múltiplos** criados em `Assets/Scenes/CenasTeste/` (`Cena#6A`, `Cena#6B`, `Sociograma`) para experimentos com variação controlada de parâmetros (poucos vs. muitos agentes, afinidades polarizadas vs. uniformes, layout aberto vs. corredor).
- 🆕 **Sociograma**: cena `Sociograma.unity` (merge da `main`) para reproduzir o sociograma do trabalho original (Musse & Thalmann) nos resultados.
- 🆕 **Bateria de testes de variação** (Caderno 14/05/2026): dois grupos de alta afinidade vs. dois de afinidades muito distantes, testando a variação de comportamentos.

### Reprodutibilidade
- 🆕 Seed reproduzível: `World` expõe `USE_SEED` + `RANDOM_SEED`; `Awake` chama `Random.InitState(RANDOM_SEED)` antes de qualquer spawn. Fixa toda a população inicial (afinidade, dominância, posições de marcadores).

### Correções de movimento (pré-existentes, herdadas da `main`)
- 🐛 NRE no NavMesh: agentes spawnados durante a run tinham `NavmeshStep` chamado no mesmo frame, antes do `Start()` inicializar `_navMeshPath` → `NullReferenceException` em `NavMesh.CalculatePath`. Corrigido com lazy-init de `_navMeshPath` + guardas de goal null/vazio em `UpdateGoalPositionAndNavmesh`.
- 🐛 Agentes afundavam no chão: `transform.Translate` aplicava o componente Y do movimento (vetores de auxina/avoidance) sem clamp; em alta densidade virava feedback vertical. `CalculateVelocity` agora zera `_rotation.y` → movimento travado no plano XZ.
- 🐛 Flicker ("agentes pulando"): removido `Random.Range` **por-frame/por-auxina** em `GetF` (re-sorteava pesos todo frame → direção tremia; entrava normalizado, era ruído puro). O ruído de velocidade em `CalculateVelocity` virou `_speedNoise`, amostrado **uma vez** no `Start` (mantém variação entre agentes, sem tremor). Bônus: menos RNG por-frame = runs com seed mais determinísticas.

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
