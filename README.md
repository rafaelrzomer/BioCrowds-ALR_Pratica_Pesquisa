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

### Curto prazo — itens pendentes do caderno

- ⬜ **Marcador visual do líder** (07/05/2026 — *"Adicionar marcador no líder (MUDAR)"*). `isGroupLeader` hoje é só lógico. Opções: halo, escala, ícone acima da cabeça ou tom de cor mais claro via `VisualAgent.ApplyGroupColor`.
- ⬜ **Analisar `timeSinceSpawn`** (14/05/2026, 7ª reunião). Verificar se o grace period está se comportando como esperado e se o valor `2f` inicial em `Agent` faz sentido.
- ⬜ **Ajustar problema de frame rate** (14/05/2026, 7ª reunião). Profiling do Update; os loops de grupo são O(N²).
- ⬜ **Revisão de `GROUP_PROXIMITY_DISTANCE`.** Comentário "increased from 2.0f" inconsistente com valor 1.0 — alinhar com o grupo.

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