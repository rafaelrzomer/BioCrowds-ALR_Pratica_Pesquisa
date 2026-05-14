# ROADMAP — Replicação de Musse & Thalmann (1997) sobre o BioCrowds

> Documento de planejamento para a continuação da prática em pesquisa.
> Branch de trabalho: `dev-humberto` (fork pessoal) → `dev-Humberto` (grupo) → `main`.
> Referências: paper **Musse & Thalmann (1997)** + paper base **Bicho et al. (2012)** + **Caderno de Pesquisa** (Google Docs do grupo).

---

## 0. Mapa de origem dos requisitos

Cada item deste roadmap é justificado por uma fonte. Notação:

- **[P]** = paper Musse & Thalmann (1997) — *A model of human crowd behavior: Group inter-relationship and collision detection analysis*.
- **[B]** = paper Bicho et al. (2012) — *Simulating crowds based on a space colonization algorithm*.
- **[C]** = Caderno de Pesquisa do grupo (Google Docs).
- **[O]** = OCEAN paper no project knowledge (*Perception of Personality Traits in Crowds of Virtual Humans*).
- **[W]** = WebCrowds (Silva et al.).

---

## 1. Status atual da code base

> Esta seção descreve o que já existe **depois da refatoração documentada em `Commit.MD`**.
> O código está em `Assets/Code/{World,Agent,Cell,Auxin,SceneController}.cs` e `Assets/Scripts/SpawnArea.cs`.

| Componente | Arquivo:Método | Status |
|---|---|---|
| BioCrowds core (auxinas, células, captura, movimento) | `Agent.CalculateDirection`, `Agent.FindNearAuxins`, `World.CreateCells` | ✅ herdado de Bicho et al. [B] |
| `groupId` por agente e por `SpawnArea` | `Agent.groupId`, `SpawnArea.groupId` | ✅ |
| Afinidade coerente por grupo no spawn | `World.SpawnNewAgentInArea` + `SpawnArea.groupAffinityMean/Spread` | ✅ amostragem uniforme mean±spread (commit `206a98c`) |
| Seed determinístico (`Random.InitState`) | `World.simulationSeed` + `Awake()` | ✅ log do seed no console (commit `206a98c`) |
| Thresholds diferenciados (adesão > troca) | `LONE_AGENT_JOIN_THRESHOLD = 0.20`, `AFFINITY_SWITCH_THRESHOLD = 0.15` | ✅ tooltips explicativos (commit `206a98c`) |
| Dominância individual | `Agent.dominance` | 🚧 só usada para eleger líder local |
| Média de afinidade do grupo | `World.ComputeGroupData` | ✅ |
| Centróide do grupo | `World._groupCentroids` | ✅ |
| Detecção de pares de grupos próximos | `World.DetectApproachingGroupPairs` | ✅ |
| Troca de grupo por afinidade | `World.UpdateGroupMembership` | ✅ + corrigida para herdar rota |
| Agente sozinho entra em grupo | `World.UpdateLoneAgents` | ✅ + corrigida para herdar rota |
| Eleição de líder por `dominance` | `World.UpdateGroupLeaders` + `World._groupLeaders` | ✅ |
| Coesão real (membros seguem líder) | `Agent.CalculateDirection` (termo de coesão) | ✅ corrigido nesta sessão |
| Sincronização de rota ao trocar | `World.SyncAgentToGroup` | ✅ corrigido nesta sessão |
| Visualização: cor por grupo + líder destacado | `Agent.UpdateGroupColor` | ✅ |
| Seed fixa, métricas, gizmos de debug | — | 🔭 não existe |
| Dominância inter-grupo, splitting, emoção | — | 🔭 não existe |

Legenda: ✅ feito · 🚧 parcial · 🔭 ainda não implementado.

---

## 2. Perguntas de pesquisa (Caderno de Campo)

Do Caderno [C], primeira reunião com a orientadora (01/04/2026):

> 1. Como os agentes mudam de grupo em meio a outras pessoas?
> 2. Quando os agentes se aproximam, como eles trocam de grupo?
> 3. Qual a distância/diferença de afinidade necessária para disparar uma troca de grupo?

E o objetivo geral (08/04/2026):

> Replicar o artigo de 1997 com ferramentas mais modernas, parametrizar os agentes, avaliar dinâmicas de grupo no espaço, e usar como ferramenta para organização de eventos culturais (museu como cenário-teste).

Este roadmap responde a essas perguntas em três fases.

---

## 3. Fase A — Validação experimental (pré-requisito de tudo)

> Sem reprodutibilidade e sem métrica, qualquer mudança feita nas Fases B/C é inverificável.
> A Fase A precisa estar 100% completa antes de prosseguir.

### A.1 Reprodutibilidade — seed fixa ✅ (commit `206a98c`)

**Por quê:** [C] espera resultados reproduzíveis para comparar runs com parâmetros diferentes. Hoje `Random.Range` usa o estado global da Unity, que muda a cada execução.

**Onde:**

- `Assets/Code/World.cs`:
  - Novo campo serializado `[SerializeField] private int simulationSeed = 42;`
  - Em `Awake()`, chamar `Random.InitState(simulationSeed);` **antes** de qualquer outra coisa.
  - Logar o seed usado: `Debug.Log($"[World] simulationSeed = {simulationSeed}");`
- `Assets/Scripts/SceneController.cs`:
  - Ao apertar `R` (reset), passar o seed atual para o novo `World` ou recarregar a cena com `SceneManager.LoadScene` para forçar `Awake` de novo.

**Critério de aceite:** dois runs com mesmo seed produzem trajetórias idênticas dentro de tolerância numérica de ponto flutuante.

---

### A.2 Afinidade coerente por grupo no spawn ✅ (commit `206a98c`)

**Por quê:** hoje `Agent.affinity = Random.Range(0f, 1f)` independente do grupo. A "média do grupo" calculada por `ComputeGroupData` se aproxima de **0.5 para qualquer grupo grande** (lei dos grandes números). Resultado: a troca por afinidade é dominada por ruído e o paper [P] não pode ser reproduzido. O paper assume que agentes do mesmo grupo nascem **socialmente similares**.

**Onde:**

- `Assets/Scripts/SpawnArea.cs`:
  ```csharp
  [Header("Group Affinity")]
  [Range(0f, 1f)] public float groupAffinityMean   = 0.5f;
  [Range(0f, 0.5f)] public float groupAffinitySpread = 0.1f;
  ```
- `Assets/Code/World.cs::SpawnNewAgentInArea`:
  ```csharp
  newAgent.affinity = Mathf.Clamp01(
      _area.groupAffinityMean + Random.Range(-_area.groupAffinitySpread, _area.groupAffinitySpread)
  );
  ```
- Documentar no Inspector: "agentes do mesmo grupo nascem com afinidade próxima da média, com pequena variação". Default: 0.5 ± 0.1.

**Critério de aceite:** desvio-padrão de affinity dentro de um grupo ≤ `groupAffinitySpread`. Em runs com 3 grupos de médias 0.2, 0.5, 0.8, troca de grupo deve acontecer **principalmente** entre grupos vizinhos (não saltar de 0.2 para 0.8).

---

### A.3 Diferenciação de thresholds ✅ (commit `206a98c`)

**Por quê:** [P] sugere que aderir a um grupo (lone agent → grupo) e sair de um grupo (troca) têm custos sociais diferentes. Hoje `AFFINITY_SWITCH_THRESHOLD` e `LONE_AGENT_JOIN_THRESHOLD` são iguais (0.15), o que não modela essa assimetria.

**Onde:**

- `Assets/Code/World.cs`:
  - Alterar defaults: `LONE_AGENT_JOIN_THRESHOLD = 0.20f` (mais permissivo) e `AFFINITY_SWITCH_THRESHOLD = 0.15f` (mais rigoroso).
  - Adicionar tooltips com `[Tooltip("...")]` explicando que adesão é mais fácil que troca.

**Critério de aceite:** lone agents formam grupos rapidamente; trocas entre grupos acontecem só quando a diferença é substancial. Verificável visualmente via cor.

---

### A.4 Logger CSV de métricas

**Por quê:** [C] menciona "métricas" como entregável e [W] usa Density Map / Trajectories Map / Simulation Time. Sem export, qualquer análise depende de inspecionar a cena manualmente.

**Métricas mínimas (uma linha por frame, ou a cada N frames):**

| Coluna | Descrição |
|---|---|
| `t` | tempo de simulação (`Time.time` ou contador de frames × `SIMULATION_TIME_STEP`) |
| `agentId` | id único do agente |
| `groupId` | id atual do grupo |
| `posX`, `posZ` | posição |
| `affinity` | afinidade individual |
| `dominance` | dominância individual |
| `isLeader` | bool |
| `groupSize` | tamanho do grupo no frame |
| `groupAvgAffinity` | média de afinidade do grupo |

**Métricas agregadas (uma linha por evento):**

| Evento | Quando |
|---|---|
| `group_switch` | agente troca de grupo (em `SyncAgentToGroup`) |
| `lone_join`    | agente sozinho entra em grupo |
| `leader_change`| líder de um grupo muda |

**Onde:**

- Novo arquivo `Assets/Code/SimulationLogger.cs`:
  ```csharp
  namespace Biocrowds.Core
  {
      public class SimulationLogger : MonoBehaviour
      {
          [SerializeField] private bool   enableLogging   = true;
          [SerializeField] private int    framesBetweenSnapshots = 30;
          [SerializeField] private string filePrefix      = "biocrowds";

          private StreamWriter _snapshotWriter;
          private StreamWriter _eventWriter;
          // ...
      }
  }
  ```
- Arquivos gravados em `Application.persistentDataPath + "/Logs/{prefix}_{timestamp}_snapshots.csv"` e `..._events.csv`.
- `World` recebe referência ao `SimulationLogger` e chama `LogSnapshot()` / `LogEvent()` nos pontos certos.

**Critério de aceite:** após um run de 60 segundos, existe um par de CSVs em `persistentDataPath` que pode ser aberto em Python/R para gerar:
- Gráfico de tamanho de cada grupo ao longo do tempo.
- Histograma de número de trocas por agente.
- Tempo médio de permanência em um grupo.

---

### A.5 Cenário de validação mínimo

**Por quê:** sem um cenário simples e bem-definido, validar Fase B fica vago.

**Cenário:** "Encontro de dois grupos com afinidades diferentes em corredor".

- Cena nova: `Assets/Scenes/Validation_TwoGroups.unity`.
- Dois `SpawnArea` em extremos opostos de um corredor.
  - Grupo 0: `groupAffinityMean=0.3`, `spread=0.05`, 10 agentes, goal no lado oposto.
  - Grupo 1: `groupAffinityMean=0.7`, `spread=0.05`, 10 agentes, goal no lado oposto.
- Seed fixa: 42.
- Esperado pelo paper [P]: pouca troca (afinidades distantes). Grupos passam um pelo outro coesos.
- Contra-cenário (`Validation_TwoGroups_Mixed.unity`):
  - Grupo 0: mean=0.45, Grupo 1: mean=0.55, ambos spread=0.1.
  - Esperado: muitas trocas no momento do encontro.

**Critério de aceite:** os dois cenários produzem padrões claramente diferentes nos CSVs e na inspeção visual.

---

## 4. Fase B — Núcleo do paper Musse & Thalmann (1997)

> Toda a Fase B pressupõe Fase A pronta.
> Cada item referencia diretamente a Seção 3 do paper [P] ("Group inter-relationship").

### B.1 Matriz de inter-relação entre grupos

**[P]** define, para a multidão, uma estrutura `IS` (Inter-relationship Status) que descreve como cada par de grupos se relaciona (amigável, hostil, neutro).

**Onde:**

- `Assets/Code/World.cs`:
  ```csharp
  // Inter-relação par-a-par: chave = (gid_menor, gid_maior), valor ∈ [-1, 1]
  // -1 = hostil, 0 = neutro, +1 = aliado
  private Dictionary<(int, int), float> _groupRelations = new Dictionary<(int, int), float>();

  public float GetRelation(int gA, int gB);
  public void  SetRelation(int gA, int gB, float value);
  ```
- Inicialização: configurável via componente `GroupRelationsConfig` (`MonoBehaviour` na cena), ou derivada inicialmente da distância entre `groupAffinityMean` de cada `SpawnArea`.
- A relação modifica a probabilidade de troca:
  - Em `UpdateGroupMembership`, multiplicar `improvement` por `(1 + relation)` antes de comparar com o threshold. Grupos aliados absorvem mais fácil; grupos hostis dificultam troca.

**Critério de aceite:** com `relation = -1` entre dois grupos, trocas não acontecem mesmo com afinidade compatível. Com `relation = +1`, trocas acontecem com diferenças menores.

---

### B.2 Dominância em nível de grupo

**[P]** usa dominância para resolver conflitos entre grupos: em encontro, o grupo mais dominante absorve membros do menos dominante (ou impõe sua rota). Hoje `dominance` só elege líder local.

**Onde:**

- Adicionar em `World.ComputeGroupData`: cálculo de `_groupDominanceAverages` (mesmo padrão de `_groupAffinityAverages`).
- Novo método `World.ResolveDominanceConflicts()`:
  - Para cada par em `_approachingGroupPairs`:
    - Se `|domA - domB| > DOMINANCE_GAP_THRESHOLD`:
      - Membros do grupo menos dominante **com baixa afinidade ao próprio grupo** são candidatos a absorção pelo grupo mais dominante.
      - Aplicar `SyncAgentToGroup(agent, dominantGid)`.

**Critério de aceite:** dois grupos com afinidades parecidas, mas dominâncias muito diferentes, terminam fundidos (o mais dominante absorve o outro).

---

### B.3 Status emocional do grupo

**[P]** introduz `emotional status` como variável do grupo que modula comportamento (calmo, agitado, em pânico). Mesma ideia pode ser estendida com OCEAN [O] mais tarde.

**Onde:**

- `World.cs`:
  ```csharp
  // Emoção por grupo: 0 = calmo, 1 = agitado
  private Dictionary<int, float> _groupEmotion = new Dictionary<int, float>();
  ```
- Default: 0. Pode ser elevado por eventos (gatilho a definir; por enquanto, manipulável no Inspector via `GroupEmotionTrigger` MonoBehaviour de teste).
- Efeito mecânico no `Agent.CalculateDirection`:
  - Multiplicador de velocidade: `_maxSpeed_efetivo = _maxSpeed * (1 + emotion * 0.5f)` (cap).
  - Multiplicador de coesão inverso: `cohesion_efetivo = groupCohesionStrength * (1 - emotion * 0.5f)` (grupos em pânico dispersam).

**Critério de aceite:** disparar emoção=1 em um grupo durante a simulação produz aumento visível de velocidade e queda de coesão.

---

### B.4 Splitting de grupos

**[P]** descreve grupos dividindo quando ficam grandes demais ou quando subgrupos divergem em objetivo.

**Critério de divisão (heurístico, derivado do paper):**

- Tamanho do grupo > `MAX_GROUP_SIZE` (ex.: 12), **ou**
- Variância de affinity dentro do grupo > `GROUP_VARIANCE_THRESHOLD`.

**Algoritmo:**

1. Em `World.Update`, após `UpdateGroupMembership`, rodar `EvaluateGroupSplitting()`.
2. Para cada grupo que satisfaz o critério, particionar membros em dois subgrupos via mediana de affinity.
3. O subgrupo "novo" recebe um `groupId` livre (próximo inteiro não usado). Líder é reeleito no próximo frame.

**Onde:**

- `World.cs::EvaluateGroupSplitting()`
- `World.cs::GetNextFreeGroupId()` (utilitário)

**Critério de aceite:** grupo com 20 agentes e afinidades variadas se divide em 2-3 subgrupos coesos em poucos segundos.

---

### B.5 Goals em nível de grupo (concluir Bug B)

**Hoje:** rota é compartilhada via referência ao `goalsList` do líder no momento da troca (corrigido nesta sessão). Mas se o líder atual mudar de `goalIndex`, **só ele avança** — membros mantêm o índice antigo.

**Próximo passo:**

- Mover o estado `goalIndex` para o **grupo**, não o agente:
  ```csharp
  // World.cs
  private Dictionary<int, int> _groupGoalIndex = new Dictionary<int, int>();
  ```
- `Agent.GetGoalIndex()` deve consultar `_world` se `HasGroup`, e voltar ao campo local só se for lone.
- Avanço do índice é decidido pelo **líder** (quando líder chega no goal, o grupo todo avança).

**Critério de aceite:** todos os membros de um grupo seguem o mesmo `goalIndex` em todo frame, sincronizadamente. Trocar de grupo no meio da rota não causa "voltar atrás".

---

## 5. Fase C — Suporte experimental e visualização

> Esta fase é o que torna o trabalho **apresentável** e **comparável** com o paper original.

### C.1 Gizmos de debug no Editor

**Por quê:** [C] fala em "inspeção visual e métricas". Hoje a única inspeção visual é cor + escala. Insuficiente para depurar parâmetros como `GROUP_DETECTION_RADIUS`.

**Onde:** `World.OnDrawGizmos`:

- Círculo (cor por grupo) ao redor de cada centróide com raio `GROUP_DETECTION_RADIUS`.
- Linha entre centróides de grupos em `_approachingGroupPairs`.
- Label sobre cada agente: `gid={groupId} aff={affinity:F2} dom={dominance:F2}`.
- Setinha do membro até o líder do seu grupo (visualiza a força de coesão).

**Critério de aceite:** abrir a cena no Editor com a simulação rodando permite entender, a olho, por que uma troca aconteceu (ou não).

---

### C.2 Cenários reproduzíveis do paper

Reproduzir os experimentos clássicos do paper [P] e de Bicho [B]:

| Cena | Descrição | Métrica esperada |
|---|---|---|
| `Experiments/TwoGroupsCorridor.unity` | Dois grupos atravessando corredor | Lane formation [B] |
| `Experiments/Bottleneck.unity`        | Um grupo passando por porta estreita | Stopping effect / arc formation [B] |
| `Experiments/MeetingHostile.unity`    | Dois grupos com `relation = -1` se encontrando | Zero trocas, possível repulsão [P] |
| `Experiments/MeetingFriendly.unity`   | Dois grupos com `relation = +1` se encontrando | Muitas trocas, possível fusão [P] |
| `Experiments/MuseumHall.unity`        | Cenário-teste real do projeto: museu | Pontos de densidade emergentes [C] |

Cada cena tem `simulationSeed` fixo e parâmetros documentados em comentário no `World` ou em um asset `ScriptableObject`.

---

### C.3 Pequena interface in-game (Caderno)

**[C], 01/04/2026:** "Opção: Pequena interface para ditar grupos e comportamentos (como colocar todas as informações do editor no jogo?)"

**Escopo mínimo (sem virar projeto de UI):**

- Painel uGUI no canto da tela mostrando:
  - Lista de grupos com cor, tamanho, média de affinity, média de dominance, emoção atual.
  - Contador de trocas total e por par.
  - Botão "Aumentar emoção do grupo X" e "Reset".
- Tecla `T` para abrir/fechar o painel.

**Onde:** `Assets/Scripts/UI/SimulationHUD.cs` + prefab `Canvas_HUD.prefab`.

**Critério de aceite:** rodar a simulação e poder, ao vivo, ver as métricas e disparar emoção sem precisar do Inspector.

---

### C.4 Modelos visuais (Mixamo)

**[C], 07/05/2026:** menção a `<https://www.mixamo.com/#/>` para modelos 3D.

**Escopo:**

- Substituir prefabs cilindro/cápsula por modelos humanos de Mixamo em `Assets/Prefabs/Agents/`.
- Animação básica: `Walking`, `Idle`.
- `VisualAgent` controla `Animator` baseado em `_velocity.magnitude`.
- Mantém a coloração por grupo via material override.

**Risco:** modelos com muitos polígonos podem derrubar performance com 100+ agentes. Validar com profiler antes de adotar.

---

### C.5 Análise pós-simulação

**Por quê:** o CSV da Fase A.4 não vale nada sem scripts de análise.

**Onde:** novo diretório `Tools/Analysis/` na raiz do repo (fora de `Assets/` para não ser importado pela Unity):

- `Tools/Analysis/parse_logs.py` — lê os CSVs e gera:
  - Gráfico de tamanho de grupo × tempo (matplotlib).
  - Heatmap de densidade (binning espacial de posições).
  - Distribuição de trocas por agente.
- `Tools/Analysis/requirements.txt`.
- `Tools/Analysis/README.md` com instrução de uso.

**Critério de aceite:** `python parse_logs.py path/to/snapshots.csv` gera um PDF/PNG com 3 figuras prontas para a redação do paper.

---

## 6. Fase D — Extensões futuras (fora do escopo da prática)

> Já listadas em `CLAUDE.md` como 🔭 futuro. Mantidas aqui para referência.

- Personalidade OCEAN [O] (começando por Extraversion).
- Caminhos preferenciais (path preference) — pergunta de pesquisa adicional do Caderno [C].
- Métricas de WebCrowds [W] — Density Map e Trajectories Map nativos.
- Multi-objetivo: agentes com lista dinâmica de goals e prioridade.

---

## 7. Métricas de sucesso por fase

> Para cada fase, definir uma pergunta verificável **antes** de declarar concluída.

### Fase A
- [ ] Rodar `Validation_TwoGroups` duas vezes com mesmo seed produz CSV idêntico em pelo menos as primeiras 100 linhas?
- [ ] Em `Validation_TwoGroups`, número de trocas é < 5%? Em `Validation_TwoGroups_Mixed`, é > 30%?
- [ ] `parse_logs.py` consegue plotar tamanho de grupo × tempo?

### Fase B
- [ ] Grupos com `relation = -1` têm zero trocas mesmo em proximidade?
- [ ] Grupo dominante absorve grupo subordinado em encontros?
- [ ] Aumentar emoção dispersa visivelmente o grupo?
- [ ] Grupo de 20 agentes diversos se divide em 2-3 subgrupos coesos?
- [ ] Todos os membros de um grupo avançam para o próximo goal **simultaneamente** com o líder?

### Fase C
- [ ] Gizmos do `GROUP_DETECTION_RADIUS` aparecem corretos no Editor?
- [ ] Todas as 5 cenas-experimento rodam sem erro?
- [ ] HUD mostra métricas ao vivo e permite manipular emoção?
- [ ] Mixamo: 100 agentes mantém ≥ 30 FPS em hardware de desenvolvimento?

---

## 8. Ordem de execução sugerida (cronograma)

> Estimativa em sessões de trabalho de ~2h. Ajustar conforme andamento.

| # | Tarefa | Fase | Sessões | Risco |
|---|---|---|---|---|
| 1  | A.1 Seed fixa ✅ | A | 0.5 | baixo |
| 2  | A.2 Affinity coerente ✅ | A | 1   | baixo |
| 3  | A.3 Thresholds diferenciados ✅ | A | 0.3 | baixo |
| 4  | A.4 Logger CSV | A | 2   | médio |
| 5  | A.5 Cenário de validação | A | 1   | baixo |
| 6  | B.5 Goal em nível de grupo | B | 1.5 | médio |
| 7  | B.2 Dominância inter-grupo | B | 2   | médio |
| 8  | B.1 Matriz de relação | B | 2   | médio |
| 9  | B.3 Status emocional | B | 1.5 | baixo |
| 10 | B.4 Splitting | B | 2.5 | alto  |
| 11 | C.1 Gizmos | C | 1   | baixo |
| 12 | C.2 Cenários experimentais | C | 2   | médio |
| 13 | C.5 Scripts de análise | C | 1.5 | baixo |
| 14 | C.3 HUD | C | 2   | médio |
| 15 | C.4 Mixamo (opcional) | C | 1   | médio |

Total estimado: ~21 sessões. Realisticamente, ~30-35 considerando ajustes, bugs e validação.

---

## 9. Regras de conduta para cada PR desta sequência

Reaplicar as regras já definidas em `CLAUDE.md`:

1. Toda sessão começa com `git branch --show-current` → deve ser `dev-humberto`.
2. Antes de implementar, ler o arquivo e descrever o plano em 3-6 passos.
3. Cada PR resolve **um item** do roadmap. Não misturar fases.
4. Cada item gera um `Commit.MD` ou parágrafo equivalente na descrição do PR.
5. PR vai de `HumbertoCG18/BioCrowds-GS:dev-humberto` para `rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa:dev-Humberto` (atenção ao H maiúsculo).
6. Antes de mergear, rodar pelo menos uma das cenas de validação.

---

## 10. Referências cruzadas

- **Paper Musse & Thalmann (1997)** — `<https://link.springer.com/chapter/10.1007/978-3-7091-6874-5_3>`.
- **Paper Bicho et al. (2012)** — `<https://www.sciencedirect.com/science/article/pii/S0097849311001713>`.
- **Caderno de Pesquisa** — `<https://docs.google.com/document/d/1OPJZ78hbnJP-g-mE7tHlA3FJBRlt1RACRJdrid2FK6M>`.
- **CLAUDE.md** — instruções vivas do projeto.
- **Commit.MD** — log da refatoração que precedeu este roadmap.
- **VHLab — Virtual Humans Lab (PUCRS)** — `<https://www.inf.pucrs.br/vhlab/>`.

---
