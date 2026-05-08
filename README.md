# 🧍 BioCrowds — Simulação de Multidões com Dinâmicas de Grupo

> Fork do projeto [BioCrowds-GS](https://github.com/Virtual-Humans-Lab/BioCrowds-GS) desenvolvido para a disciplina de **Prática em Pesquisa — PUCRS 2026**.  
> Grupo: Alice L. de Franceschi, Lara Volpato, Rafael R. Zomer, Pedro Idalencio e Humberto Gomes.

---

## 📋 Sobre o Projeto

Este projeto replica e expande o modelo de simulação de multidões descrito no artigo:

> **Musse, S. R. & Thalmann, D. (1997)**. *A model of human crowd behavior: Group inter-relationship and collision detection analysis.* In Computer Animation and Simulation '97. Springer, pp. 39–51.

O objetivo é simular como **agentes virtuais formam, mantêm e trocam de grupos** com base em parâmetros de **dominância** e **afinidade**, dentro de um ambiente de museu.

### O que foi implementado neste fork

- `dominance` — parâmetro por agente (0.0 a 1.0), define quem lidera o grupo
- `affinity` — parâmetro por agente (0.0 a 1.0), compatibilidade entre agentes
- `groupId` — identificação de grupo por SpawnArea
- **Coesão de grupo** — agentes do mesmo grupo tendem a andar juntos
- **Líder de grupo** — agente com maior dominância assume a liderança
- **Média de afinidade por grupo** — calculada a cada frame
- **Troca de grupo por afinidade** — agentes migram para grupos mais compatíveis
- **Agentes sozinhos** (`groupId = -1`) buscam um grupo compatível

---

## ⚙️ Pré-requisitos

### 1. Unity Hub

Baixe e instale o **Unity Hub** antes de qualquer coisa:

🔗 https://unity.com/pt/download

O Unity Hub é o gerenciador de versões da Unity. É por ele que você instala a versão correta do editor.

---

### 2. Versão do Unity Editor — OBRIGATÓRIO

Este projeto **só funciona** com a versão:

```
Unity 2020.3.33f1 (LTS)
```

> ⚠️ **Atenção:** Usar uma versão diferente pode causar erros de compatibilidade com o NavMesh, shaders e scripts do projeto.

**Como instalar a versão correta:**

1. Abra o **Unity Hub**
2. Clique em **Installs** (Instalações) no menu lateral
3. Clique em **Install Editor**
4. Procure por `2020.3.33` ou clique em **Archive** e acesse:  
   🔗 https://unity.com/releases/editor/archive  
   Vá até **Unity 2020.x** → encontre a `2020.3.33f1` → clique em **Unity Hub**
5. Na tela de módulos, marque:
   - ✅ **Windows Build Support** (ou Mac, dependendo do seu sistema)
   - ✅ **Documentation** (recomendado)
6. Clique em **Install** e aguarde o download

---

### 3. Git

Para clonar o repositório, você precisa do Git instalado:

🔗 https://git-scm.com/downloads

Verifique se está instalado rodando no terminal:
```bash
git --version
```

---

### 4. Visual Studio (recomendado para editar scripts)

O projeto está configurado para usar **Visual Studio com suporte a Unity**:

🔗 https://visualstudio.microsoft.com/

Durante a instalação, selecione a carga de trabalho:
- ✅ **Desenvolvimento de jogos com Unity** (Game development with Unity)

Alternativamente, você pode usar o **Visual Studio Code** com a extensão C# instalada.

---

## 📥 Instalação do Projeto

### Passo 1 — Clonar o repositório

Abra o terminal e execute:

```bash
git clone https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa.git
```

Para trabalhar na branch de desenvolvimento:

```bash
cd BioCrowds-ALR_Pratica_Pesquisa
git checkout dev-humberto
```

---

### Passo 2 — Abrir no Unity Hub

1. Abra o **Unity Hub**
2. Clique em **Open** (ou **Add project from disk**)
3. Navegue até a pasta onde você clonou o projeto
4. Selecione a **pasta raiz** do projeto (onde está o `Assets/`)
5. Clique em **Open**

> Se o Unity Hub detectar que a versão do editor não bate, ele vai perguntar qual versão usar. Selecione a `2020.3.33f1`.

---

### Passo 3 — Aguardar a importação

Na primeira abertura, a Unity vai:
- Importar todos os assets
- Compilar os scripts
- Construir o NavMesh (malha de navegação)

Isso pode levar **alguns minutos**. Não feche o editor durante esse processo.

---

## ▶️ Como Rodar a Simulação

### Passo 1 — Abrir a cena

Na aba **Project** (parte inferior do editor):

```
Assets → Scenes → [nome da cena do museu]
```

Dê duplo clique na cena para abri-la.

---

### Passo 2 — Configurar os SpawnAreas

Na **Hierarchy** (lista de objetos da cena), localize os objetos `SpawnArea`.

Para cada SpawnArea, no **Inspector** (painel direito), configure:

| SpawnArea | Group Id | Descrição |
|-----------|----------|-----------|
| SpawnArea_A | `0` | Grupo vermelho |
| SpawnArea_B | `1` | Grupo azul |
| SpawnArea_C | `-1` | Agentes sem grupo (buscam um) |

> O campo `Group Id` está na seção `[Header("Spawn Area Group")]` no Inspector.

---

### Passo 3 — Dar Play

1. Pressione o botão ▶️ **Play** (ou `Ctrl+P`)
2. Com o jogo rodando, pressione a tecla **`1`** para carregar o mundo

> A tecla `1` inicializa o `World`, constrói o NavMesh em tempo de execução e spawna os agentes.  
> A tecla `R` reinicia a cena do zero.

---

### Passo 4 — Observar no Console

Abra a aba **Console** (`Window → General → Console`).

Você verá mensagens como:
```
Agente [3] trocou do grupo 0 para o grupo 1
Agente [7] (sem grupo) entrou no grupo 1
```

---

## 🧪 Como Testar a Troca de Grupo

### Verificar visualmente

Adicione coloração por grupo no método de update de `Agent.cs` (código já está disponível na branch `dev-humberto`):

| Cor | Grupo |
|-----|-------|
| 🔴 Vermelho | Grupo 0 |
| 🔵 Azul | Grupo 1 |
| 🟢 Verde | Grupo 2 |
| ⚪ Branco | Sem grupo |

### Ajustar parâmetros em tempo real

Com o jogo rodando, selecione qualquer `Agent` na Hierarchy e altere no Inspector:

| Parâmetro | Efeito | Valor sugerido |
|-----------|--------|----------------|
| `groupSwitchRadius` | Raio que o agente "enxerga" outros grupos | `5.0` |
| `affinityThreshold` | Margem mínima para justificar a troca | `0.1` |
| `groupCohesionStrength` | Força de atração dentro do grupo | `0.3` |

---

## 📁 Estrutura de Pastas Relevante

```
Assets/
├── Code/
│   ├── Agent.cs          ← Parâmetros de dominância, afinidade e grupo
│   ├── World.cs          ← Lógica de cálculo de média e troca de grupo
│   ├── Auxin.cs          ← Marcadores de espaço (não modificar)
│   └── Cell.cs           ← Células do grid (não modificar)
├── Scripts/
│   ├── SpawnArea.cs      ← Define groupId por área de spawn
│   ├── SceneController.cs ← Controla teclas (1 = iniciar, R = reiniciar)
│   └── MarkerSpawn/      ← Métodos de geração de marcadores
└── Scenes/
    └── [Cena do museu]   ← Cena principal de simulação
```

---

## 🐛 Solução de Problemas

**A Unity não abre o projeto / pede para atualizar:**  
→ Certifique-se de que a versão `2020.3.33f1` está instalada no Unity Hub. Não aceite a atualização automática.

**Erro de compilação ao abrir:**  
→ Aguarde a Unity terminar de importar todos os assets. Se persistir, vá em `Assets → Reimport All`.

**Pressionar `1` não faz nada:**  
→ Clique uma vez na **Game View** para garantir que o foco do teclado está na janela do jogo.

**Agentes não aparecem:**  
→ Verifique se os `SpawnArea` têm `initialNumberOfAgents` maior que `0` e se a lista `initialAgentsGoalList` tem pelo menos um Goal configurado.

**Agentes não trocam de grupo:**  
→ Cheque se `ComputeGroupAffinityAverages()` e `UpdateGroupMembership()` estão sendo chamados no `Update()` de `World.cs`. Aumente o `groupSwitchRadius` se os agentes não se cruzam no mapa.

**Erro `GetValueOrDefault` não existe:**  
→ Substitua por:
```csharp
(_groupAffinityAverages.ContainsKey(agent.groupId)
    ? _groupAffinityAverages[agent.groupId]
    : agent.affinity)
```

---

## 🔗 Links Úteis

| Recurso | Link |
|---------|------|
| Unity Hub (download) | https://unity.com/pt/download |
| Unity 2020.3 Archive | https://unity.com/releases/editor/archive |
| Repositório original BioCrowds | https://github.com/Virtual-Humans-Lab/BioCrowds-GS |
| Paper BioCrowds (Bicho et al.) | https://www.sciencedirect.com/science/article/pii/S0097849311001713 |
| DBLP — Soraia R. Musse | https://dblp.org/pid/92/5311.html |
| Caderno de Pesquisa do grupo | https://docs.google.com/document/d/1OPJZ78hbnJP-g-mE7tHlA3FJBRlt1RACRJdrid2FK6M |

---

## 👥 Grupo de Pesquisa

Disciplina de **Prática em Pesquisa — PUCRS 2026**

- Alice L. de Franceschi  
- Lara Volpato  
- Rafael R. Zomer  
- Pedro Idalencio  
- Humberto Gomes  

Orientadora: **Soraia R. Musse**