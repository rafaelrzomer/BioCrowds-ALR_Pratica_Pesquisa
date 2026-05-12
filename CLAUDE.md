# CLAUDE.md

> Arquivo de contexto para o Claude Code. **Leia este documento integralmente no início de cada sessão** antes de propor mudanças, executar comandos ou abrir pull requests.

---

## 1. Identidade do Projeto

**Nome interno:** BioCrowds — Dinâmicas de Grupo
**Tipo:** Projeto de pesquisa acadêmica em simulação de multidões (crowd simulation).
**Engine:** Unity **2020.3.33f1 (LTS)** — versão obrigatória, não atualizar.
**Linguagem:** C# (.NET compatível com Unity 2020.3).

### Contexto Acadêmico

- **Disciplina:** Prática em Pesquisa — PUCRS (2026).
- **Orientadora:** Prof.ª Dr.ª **Soraia Raupp Musse** (PUCRS — Virtual Humans Lab / VHLab).
- **Grupo de pesquisa:**
  - Alice L. de Franceschi
  - Lara Volpato
  - Rafael R. Zomer
  - Pedro Idalencio
  - **Humberto Gomes** (usuário deste CLAUDE.md)
- **Caderno de Pesquisa do grupo (Google Docs):**
  <https://docs.google.com/document/d/1OPJZ78hbnJP-g-mE7tHlA3FJBRlt1RACRJdrid2FK6M>

---

## 2. Objetivo do Projeto

**Replicar o modelo de Musse & Thalmann (1997)** — *"A model of human crowd behavior: Group inter-relationship and collision detection analysis"* — sobre **a base de código do BioCrowds (Bicho et al., 2012)** já implementada em Unity.

### Perguntas de pesquisa (do caderno de campo)

1. **Como os agentes mudam de grupo em meio a outras pessoas?**
2. **Quando os agentes se aproximam, como eles trocam de grupo?**
3. **Qual a distância/diferença de afinidade necessária para disparar uma troca de grupo?**

### Objetivos derivados

- Avaliar **dinâmicas de grupo no espaço** e como elas se manifestam emergentemente.
- Identificar **pontos de densidade** e **caminhos preferenciais** (linha futura de pesquisa).
- Aplicação prática: **auxiliar na organização de eventos culturais** (museu como cenário-teste), evitando cenários aglomerados e preservando a liberdade de movimento das pessoas.

### Cenário-teste

Ambiente de **museu** dentro da Unity, com múltiplas `SpawnAreas` (cada uma associada a um `groupId`) e `Goals` distribuídos. Agentes percorrem o espaço, encontram outros grupos, e devem decidir trocar de grupo ou não conforme afinidade.

---

## 3. Fundamentação Teórica (Papers do Project Knowledge)

Estes quatro papers estão no project knowledge e formam a base teórica. Sempre que precisar justificar uma decisão de design, referenciar o paper correspondente.

### 3.1 Musse & Thalmann (1997) — **Paper de referência principal**

> Musse, S. R. & Thalmann, D. (1997). *A model of human crowd behavior: Group inter-relationship and collision detection analysis.* Computer Animation and Simulation '97, Springer.

- Introduz a **noção hierárquica** crowd → groups → agents.
- Cada grupo tem propriedades coletivas; cada agente herda e perturba essas propriedades.
- Base para os conceitos de `groupId`, `affinity`, `dominance` e troca de grupo deste projeto.

### 3.2 Bicho et al. (2012) — **Algoritmo base implementado**

> Bicho, A. L., Rodrigues, R. A., Musse, S. R., Jung, C. R., Paravisi, M., & Magalhães, L. P. (2012). *Simulating crowds based on a space colonization algorithm.* Computers & Graphics, 36(2), 70–79.

- Modelo **BioCrowds**: espaço discreto populado por **marcadores (auxins)**; agentes competem por marcadores dentro do raio de percepção.
- Movimento livre de colisões **provado matematicamente**.
- Comportamentos emergentes: lane formation, bottleneck, arc formation, stopping effect.

**Equação central** (vetor de movimento do agente *i*):

```
m⃗ = Σ wₖ · (a⃗ₖ − x⃗)
```

Lê-se: "vetor m igual ao somatório, para cada marcador k, do peso wk multiplicado pelo vetor que vai da posição do agente x⃗ até a posição do marcador a⃗k".

O peso `wₖ` é função do ângulo entre o vetor (goal − agente) e (marcador − agente). Marcadores alinhados com o objetivo recebem peso maior.

### 3.3 Perception of Personality Traits in Crowds of Virtual Humans — Extensão OCEAN

> Paper no project knowledge: *Perception of Personality Traits in Crowds of Virtual Humans.*

- Adiciona traços de **personalidade OCEAN** (Openness, Conscientiousness, Extraversion, Agreeableness, Neuroticism).
- Foco em **Extraversion** como modificador do conforto espacial do agente.
- Equação modificada de Normal Life com fator de extroversão `E_i`:

```
w'ₖ,ᵢ = δᵢ · wₖ,ᵢ · Eᵢ + (1 − δᵢ) · (1 − Eᵢ)
```

Lê-se: "peso modificado w-linha do marcador k para o agente i é igual a delta-i vezes o peso original wk,i vezes o fator de extroversão Ei, mais um menos delta-i, vezes um menos Ei".

> ⚠️ Este projeto **ainda não implementa OCEAN**. O paper está no knowledge como referência para extensões futuras (a literatura considera personalidade um passo natural depois de grupos).

### 3.4 Silva et al. — WebCrowds

> *WebCrowds: An Authoring Tool for Crowd Simulation* (VHLab/PUCRS).

- Ferramenta web de autoria para construir cenários e simular multidões usando BioCrowds.
- Não é dependência direta deste projeto, mas mostra o **caminho de aplicação prática** que o VHLab segue.
- Métricas úteis para inspiração: Density Map, Trajectories Map, Simulation Time.

---

## 4. Estrutura dos Repositórios — **REGRA CRÍTICA**

Existem **dois repositórios** envolvidos. Confundi-los pode causar perda de trabalho.

### 4.1 Repositório do usuário (fork pessoal)

- **URL:** `https://github.com/HumbertoCG18/BioCrowds-GS`
- **Branch única de trabalho:** **`dev-humberto`** (minúsculo)
- É um **fork** do BioCrowds-GS original do VHLab.
- Todo o trabalho de Humberto acontece aqui primeiro.

### 4.2 Repositório do grupo (mantido pelo Rafael)

- **URL:** `https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa`
- **Branches relevantes:**
  - **`main`** — branch principal/estável do grupo. Recebe merges aprovados.
  - **`dev-Humberto`** (com **H maiúsculo**) — branch destino dos PRs vindos do fork de Humberto.

### 4.3 Fluxo de Contribuição

```
HumbertoCG18/BioCrowds-GS          rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa
       (fork pessoal)                          (repo do grupo)
       
  dev-humberto  ── PR ──►  dev-Humberto  ── merge ──►  main
   (trabalho)               (destino do PR)             (estável)
```

### 4.4 Configuração de Remotes Esperada

```bash
origin    →  https://github.com/HumbertoCG18/BioCrowds-GS.git
upstream  →  https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa.git
```

### 4.5 Comandos de Sincronização

```bash
# Estado atual
git branch --show-current        # deve ser sempre dev-humberto
git status

# Atualizar a branch local com o que veio do grupo
git fetch upstream
git merge upstream/dev-Humberto   # atenção ao H maiúsculo aqui!

# Publicar trabalho local
git push origin dev-humberto
```

---

## 5. Regras de Git — **OBRIGATÓRIAS**

1. **Sempre operar na branch `dev-humberto`** (do fork pessoal). Verificar com `git branch --show-current` antes de qualquer alteração.
2. **Nunca** fazer commit/push diretamente em `main`, `master`, `develop` ou `dev-Humberto` (grupo).
3. **Nunca** fazer rebase ou force-push em branches compartilhadas.
4. **Antes de qualquer `git commit`:** mostrar o `git diff` ou `git status` e pedir confirmação explícita ao usuário.
5. **Pull Requests** vão sempre **de** `HumbertoCG18/BioCrowds-GS:dev-humberto` **para** `rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa:dev-Humberto`.
6. **Mensagens de commit** seguem [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat:` — nova funcionalidade
   - `fix:` — correção de bug
   - `docs:` — documentação
   - `refactor:` — refatoração sem mudança de comportamento
   - `chore:` — manutenção (gitignore, etc.)

---

## 6. Estrutura do Código

```
BioCrowds-GS/
├── Assets/
│   ├── Code/                              # Núcleo da simulação (namespace Biocrowds.Core)
│   │   ├── World.cs                       # Orquestrador da simulação:
│   │   │                                  # • Spawn de agentes/células/auxins
│   │   │                                  # • ComputeGroupData() — média de afinidade + centróide por grupo
│   │   │                                  # • DetectApproachingGroupPairs() — pares de grupos próximos
│   │   │                                  # • UpdateGroupMembership() — troca de grupo por afinidade
│   │   │                                  # • UpdateLoneAgents() — agentes sozinhos entram em grupos
│   │   │                                  # • UpdateGroupLeaders() — elege líder por dominance
│   │   ├── Agent.cs                       # Lógica do agente:
│   │   │                                  # • Captura de auxins, NavMesh, movimento
│   │   │                                  # • Campos: affinity, dominance, groupId, isGroupLeader
│   │   │                                  # • UpdateGroupColor() — feedback visual
│   │   ├── Cell.cs                        # Célula do grid espacial (acelera busca de auxins)
│   │   ├── Auxin.cs                       # Marcador competido pelos agentes
│   │   ├── SpawnArea.cs                   # Área de spawn (tem groupId, goals, wait times)
│   │   └── SceneController.cs             # Controle de cena (tecla 1 carrega mundo, R reinicia)
│   ├── Scripts/
│   │   └── MarkerSpawn/
│   │       ├── MarkerSpawner.cs           # Classe base abstrata
│   │       ├── RegularGridMarkerSpawner.cs# Marcadores em grade regular
│   │       └── DartThrowingMarkerSpawner.cs # Marcadores via dart-throwing (Poisson-disk)
│   ├── Prefabs/
│   │   └── Agents/                        # Prefabs visuais (man1, woman1, etc.)
│   └── Scenes/                            # Cena do museu fica aqui
├── ProjectSettings/
│   └── ProjectVersion.txt                 # m_EditorVersion: 2020.3.33f1
└── Packages/
    └── packages-lock.json
```

**Namespace principal:** `Biocrowds.Core`

---

## 7. Glossário do Domínio

| Termo no código | Equivalente teórico | O que faz |
|---|---|---|
| `Auxin` | *marker* (Bicho et al.) | Ponto discreto no chão; recurso de espaço disputado. |
| `Cell` | célula do grid | Acelera busca espacial por auxins próximos. |
| `Agent` | agente / pedestre | Entidade móvel competindo por auxins em direção a um goal. |
| `Goal` | objetivo | `GameObject` alvo; agentes podem ter lista sequencial com tempos de espera. |
| `SpawnArea` | área geradora | Cria agentes com um `groupId` específico e lista de goals. |
| `affinity` (∈ [0, 1]) | afinidade social | Compatibilidade entre agentes; quanto mais próximos os valores, mais compatíveis. |
| `dominance` (∈ [0, 1]) | dominância | Determina liderança; maior valor → `isGroupLeader = true`. |
| `groupId` (int) | identificador de grupo | `-1` significa agente sozinho (sem grupo). |
| `groupCohesionStrength` (∈ [0, 1]) | coesão | Quanto o agente tenta seguir o centróide do grupo. |
| `isGroupLeader` | líder eleito | Marcação visual: cor mais clara + escala 1.4×. |
| `GROUP_DETECTION_RADIUS` | raio de detecção | Distância máxima entre centróides para considerar grupos "próximos". |
| `AFFINITY_SWITCH_THRESHOLD` | limiar de troca | Melhoria mínima de afinidade para um agente trocar de grupo. |
| `LONE_AGENT_JOIN_THRESHOLD` | limiar de adesão | Diferença máxima de afinidade para um agente sozinho entrar em um grupo. |

---

## 8. Status de Implementação

### ✅ Concluído (já mergeado ou em PR)

- Campos de grupo em `Agent.cs`: `affinity`, `groupId`, `HasGroup`, `isGroupLeader`, `groupCohesionStrength`, `dominance`
- `SpawnArea.cs`: campo `groupId`
- `World.cs`:
  - `ComputeGroupData()` — média de afinidade + centróide por frame
  - `DetectApproachingGroupPairs()` — detecção de grupos vizinhos
  - `UpdateGroupMembership()` — troca de grupo por afinidade
  - `UpdateLoneAgents()` — agentes sozinhos entram em grupos compatíveis
  - `UpdateGroupLeaders()` — eleição de líder por `dominance`
- Feedback visual: cor por grupo + líderes diferenciados (cor clara, escala 1.4×)

### 🚧 Em andamento / próximos passos

- Métricas quantitativas: contagem de trocas de grupo, tempo médio em grupo, distância média entre membros.
- Mapas de densidade e mapas de trajetória (inspirados em WebCrowds).
- Cenários de teste reproduzíveis com seeds fixas.

### 🔭 Futuro (escopo aberto)

- Integração de personalidade OCEAN (Knob et al., 2018) — começando por Extraversion.
- Caminhos preferenciais (path preference) — pergunta de pesquisa do caderno.
- Exportação de logs para análise posterior em Python/R.

---

## 9. Convenções de Código

Seguir o padrão já existente no projeto:

- **Constantes/configs serializadas:** `UPPER_SNAKE_CASE` → `AGENT_RADIUS`, `AFFINITY_SWITCH_THRESHOLD`.
- **Campos privados:** `_camelCase` com underscore → `_agents`, `_groupAffinityAverages`.
- **Propriedades públicas:** `PascalCase` → `Dimension`, `GroupAffinityAverages`.
- **Métodos:** `PascalCase` → `SpawnNewAgent`, `UpdateGroupLeaders`.
- **Atributos Unity comuns:**
  - `[SerializeField]` para expor campos privados no Inspector.
  - `[Header("...")]` para agrupar campos relacionados.
  - `[Range(0f, 1f)]` para floats normalizados (`affinity`, `dominance`).
- **Comentários:** inglês curto no estilo já presente (`// agent radius`).
- **Compatibilidade C# (Unity 2020.3 = C# 8):**
  - ❌ `Dictionary.GetValueOrDefault()` — não existe na versão usada.
  - ✅ Usar: `dict.ContainsKey(k) ? dict[k] : fallback`
- **Cuidado com duplicações:** o erro `CS0102` (definição duplicada) já apareceu em `World.cs`. Antes de adicionar qualquer campo novo, fazer `grep -n "NOME" Assets/Code/World.cs`.

---

## 10. Controles da Simulação (Runtime)

- **Tecla `1`** — carrega/inicia o mundo (chama `World.LoadWorld()` via `SceneController`).
- **Tecla `R`** — reinicia a simulação.
- A **Game View** precisa ter foco do teclado para as teclas funcionarem.

---

## 11. Como Trabalhar com o Usuário (Humberto)

**Idioma de resposta:** Português (PT-BR). Termos técnicos do código permanecem em inglês.

**Perfil do usuário:**
- Estudante de Ciências da Computação na PUCRS, 23 anos.
- Em formação para *gameplay programmer*.
- Possui **TDAH, Dislexia e Discalculia**.
- Inglês intermediário-avançado.

**Diretrizes obrigatórias de comunicação:**

1. **Estrutura passo a passo numerada.** Quebrar tarefas grandes em etapas curtas.
2. **Clareza acima de brevidade.** Linguagem acadêmica formal, mas objetiva. Sem jargão desnecessário.
3. **Conectar sempre teoria com prática.** Quando aparecer um conceito (Voronoi, weighted sum, dart-throwing, centróide), dar uma analogia ou exemplo aplicado dentro do BioCrowds.
4. **Símbolos matemáticos e lógicos sempre lidos por extenso.** Exemplos:
   - `∀x ∈ S` → "para todo x pertencente ao conjunto S"
   - `Σ` → "somatório"
   - `≤` → "menor ou igual a"
   - `∈` → "pertence a"
   - `→` → "implica" ou "mapeia para"
   - `δ` → "delta"
   - `&&` → "E lógico"
   - `??` → "operador de coalescência nula"
5. **Erros de compilação C#:** identificar a linha exata, explicar a causa, **depois** propor a correção. Exemplo: `CS0102` = definição duplicada de membro na mesma classe.
6. **Antes de mudanças grandes:** apresentar o plano em texto, perguntar se pode prosseguir, **só então** editar arquivos.

---

## 12. Fluxo Padrão para Tarefas

Ao receber um pedido:

1. **Confirmar branch:** `git branch --show-current` deve retornar `dev-humberto`.
2. **Ler os arquivos relevantes** com `view`/`grep` antes de propor mudanças.
3. **Explicar o plano** em 3 a 6 passos numerados, em português claro.
4. **Implementar** seguindo Seções 7 e 9.
5. **Verificar duplicações** (lição do `CS0102`) e referências quebradas.
6. **Resumir** o que foi alterado, em qual arquivo e por quê.
7. **Não fazer commit automático** — perguntar antes (`git add` / `git commit`).
8. **PR:** se o usuário pedir abertura de PR, lembrar que destino é `rafaelrzomer/...:dev-Humberto` (com H maiúsculo).

---

## 13. O Que NÃO Fazer

- ❌ Atualizar a versão do Unity (fica em **2020.3.33f1**).
- ❌ Modificar arquivos `.meta` manualmente.
- ❌ Deletar `Library/`, `Temp/`, `obj/`, `Logs/` (já ignorados no `.gitignore`).
- ❌ Commitar `.sln`, `.csproj`, `.user` (gerados automaticamente).
- ❌ Trocar de branch sem avisar.
- ❌ Fazer rebase ou force-push em branches compartilhadas.
- ❌ Refatorar arquitetura sem confirmação explícita — o projeto segue o desenho original do VHLab.
- ❌ Usar `Dictionary.GetValueOrDefault()` (não existe no C# da Unity 2020.3).
- ❌ Assumir que um conceito matemático é trivial — sempre ler símbolos por extenso.
- ❌ Tratar `dev-humberto` (fork) e `dev-Humberto` (grupo) como a mesma branch — **são diferentes**.

---

## 14. Links Úteis

| Recurso | URL |
|---|---|
| Unity Hub | <https://unity.com/pt/download> |
| Unity 2020.3.33f1 (arquivo) | <https://unity.com/releases/editor/archive> |
| Repositório original BioCrowds (VHLab) | <https://github.com/Virtual-Humans-Lab/BioCrowds-GS> |
| Fork pessoal (Humberto) | <https://github.com/HumbertoCG18/BioCrowds-GS> |
| Repositório do grupo (Rafael) | <https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa> |
| Caderno de Pesquisa (Google Docs) | <https://docs.google.com/document/d/1OPJZ78hbnJP-g-mE7tHlA3FJBRlt1RACRJdrid2FK6M> |
| Paper Bicho et al. (2012) | <https://www.sciencedirect.com/science/article/pii/S0097849311001713> |
| DBLP — Soraia R. Musse | <https://dblp.org/pid/92/5311.html> |
| VHLab (Virtual Humans Lab — PUCRS) | <https://www.inf.pucrs.br/vhlab/> |

---

*Última atualização: maio de 2026.*