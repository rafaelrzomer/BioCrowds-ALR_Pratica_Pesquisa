**Caderno de Pesquisa**   
**Grupo:** Alice L. de Franceschi, Lara Volpato, Rafael R. Zomer, Pedro Idalencio e Humberto Gomes

**25/03/2026**  
**\-1º reunião com orientador** 

- Primeira reunião com orientador   
- Explicação do tópico “Crowd Simulation”  
- Repositório de BioCrowds no GitHub  
- Versão do programa: Unity 2020.3.33  
- Elementos mais usados para modificações na Unity: Goals, Spawners, Obstacles  
- Tarefas: ler paper do BioCrowds, baixar projeto do GitHub e abrir na Unity  
    
  UnityHub: [https://docs.unity.com/en-us/hub](https://docs.unity.com/en-us/hub)  
  GitHub com a versão do BioCrowds: [https://github.com/Virtual-Humans-Lab/BioCrowds-GS](https://github.com/Virtual-Humans-Lab/BioCrowds-GS)  
  Slides: [https://docs.google.com/presentation/d/17dE-8gBJ\_fDdPoHz2mrTeCv3c4QlOGusgURTCmSu0Mg/edit?usp=sharing](https://docs.google.com/presentation/d/17dE-8gBJ_fDdPoHz2mrTeCv3c4QlOGusgURTCmSu0Mg/edit?usp=sharing)  
  Paper do BioCrowds: [https://www.sciencedirect.com/science/article/pii/S0097849311001713?via%3Dihub](https://www.sciencedirect.com/science/article/pii/S0097849311001713?via%3Dihub)  
  Outros relacionados:  
  [https://ieeexplore.ieee.org/document/9961084/](https://ieeexplore.ieee.org/document/9961084/)  
  [https://ieeexplore.ieee.org/document/8636902](https://ieeexplore.ieee.org/document/8636902)  
  [https://ieeexplore.ieee.org/document/9637587](https://ieeexplore.ieee.org/document/9637587)


  
**01/04/2026**  
**\-2º reunião com orientador**

- Proposta do Trabalho : Replicar Paper Soraia 1997 \-   
  - Como mudam de grupo em meio a outras pessoas?  
  - Quando os agentes se aproximam, como eles mudam de grupo?  
  - Certa distância das afinidades para troca de grupo

     \-     Objetivo: Avaliar dinâmicas de grupo no espaço e como elas funcionam   
     \-     Pontos de densidade, caminhos específicos (caminhos futuros)  
     \-     Por que o trabalho? Como auxiliar  na organização de eventos culturais;  como trabalhar para evitar cenários aglomerados e permitir que pessoas tenham liberdade de movimento  
     \-       Inspeção visual e métricas  
     \-       Opção: Pequena interface para ditar grupos e comportamentos (como              colocar todas as informações do editor no jogo?)  
     \-      Tarefas: ler paper de 1997 e mexer na Unity, fazendo uma cópia de uma cena e testando alterações dos spawners, etc.

             Paper: pdf no grupo do WhatsApp  
	  DBLP (indexador de publicações):  
  [https://dblp.org/pid/92/5311.html](https://dblp.org/pid/92/5311.html)

**08/04/2026**  
**\-3º reunião com orientador**

Artigo Inicial:

- Resumos dos artigos de referência  
- Reprodução de cenário inicial, requisitos necessários  
- Resultado da reflexão, impacto na sociedade: simular como podemos influenciar pessoas, auxiliar com métricas em espaços, como museus (simular espaços culturais e educacionais)  
- Como redes de influência agem sobre pessoas  
- Parâmetros do paper de 1997  
- Descrição do objetivo do trabalho: replicar artigo de 97 com ferramentas mais modernas, parametrização dos agentes  
- O que precisamos ter, estudar e encontrar para gerar o objetivo, acesso a Unity, código-fonte do biocrowds.  
- Resultado da reflexão sobre impacto e conexão

**08/04/2026**  
**\-4º reunião com orientador**

- Checagem do artigo para a primeira entrega 

**08/04/2026**  
**\-5º reunião com orientador**  
\-   Projeto biocrowds, voltar para commit anterior (4 anos atrás)  \-\> fazer branch ou fork(cria projeto próprio) para trabalhar (usar essa versão anterior apenas para estudo do código  
\-  Fazer fork faz versão atual para usar no projeto

* [BioCrowds-GS](https://github.com/Virtual-Humans-Lab/BioCrowds-GS/tree/main) /[Assets](https://github.com/Virtual-Humans-Lab/BioCrowds-GS/tree/main/Assets)/Code/   
- *\[SerializeField\] private float SIMULATION\_TIME\_STEP \= 0.02f;*  
  atualização do mundo  
    
- *private const float UPDATE\_NAVMESH\_INTERVAL \= 1.0f;*

 Atualiza o caminho dele a cada 1 seg  
 Replaneja  a rota

- O que teremos que modificar com o tempo: goals (lista), quando um agente trocar de objetivo, vai ter que saber em que objetivo o grupo está (alterar o goal index dele para ficar igual ao do grupo)  
- visualAgente \-\> para fazer a animação  
- Quais parâmetros vamos colocar nos agentes? (na classe agents)  
- Ter maneira de, nos spawnpoints, dizer que os agentes pertencem a algum grupo ou estão sozinhos(mas eventualmente os que estão sozinhos têm que entrar em algum grupo  
- spawnArea tem que ter os parâmetros (le as infos e coloca no agente)  
- Líder \-\> pessoa de maior valor de dominância no grupo  
- Um spawn para cada grupo (mais simples)  
- 1\. ter grupos(saber os grupos de cada agente)


  
**21/04/2026**

- Criação do repositório com fork do BioCrows   
- Adição do cenário do museu no projeto  
- Link do repositório:  
  [https://github.com/rafaelrzomer/BioCrowds-ALR\_Pratica\_Pesquisa](https://github.com/rafaelrzomer/BioCrowds-ALR_Pratica_Pesquisa)  
  


**06/05/2026**  
Na classe agents, adição de parâmetros:  
//dominance parameter  
        \[Range(0f, 1f)\]  
        public float dominance \= 1.0f;  
 // affinity parameter: agents with closer values are more compatible  
        \[Range(0f, 1f)\]  
        public float affinity \= 0f;  
// assign random dominance at start  
            dominance \= Random.Range(0f, 1f);  
 		affinity \= Random.Range(0f, 1f);

\*\*Ideia: adicionar marcação para saber qual é o líder do grupo no momento FEITO

**GRUPOS (Todos agentes que nascem em cada spwanArea pertencem a um grupo):**

em agents  
// group membership: \-1 means no group  
        public int groupId \= \-1;  
        public bool HasGroup \=\> groupId \>= 0;

em spwanArea  
 \[Header("Spawn Area Group")\]  
    public int groupId \= \-1;

em World  
 newAgent.groupId \= \_area.groupId;

**Dinâmica das pessoas do mesmo grupos andarem juntos:**

em agents:  
\[Range(0f, 1f)\]  
        public float groupCohesionStrength \= 0.5f;

// group cohesion: list of nearby group members  
        public List\<Agent\> \_nearbyGroupMembers \= new List\<Agent\>();

 // add group cohesion force  
            if (HasGroup && \_nearbyGroupMembers.Count \> 0)  
            {  
                Vector3 groupCenter \= Vector3.zero;  
                foreach (Agent groupMember in \_nearbyGroupMembers)  
                {  
                    groupCenter \+= groupMember.transform.position;  
                }  
                groupCenter /= \_nearbyGroupMembers.Count;

                Vector3 cohesionDirection \= (groupCenter \- transform.position).normalized;  
                \_rotation \+= groupCohesionStrength \* cohesionDirection \* \_maxSpeed;  
            }

public void FindNearbyGroupMembers(List\<Agent\> allAgents)  
        {  
            if (\!HasGroup) return;

            \_nearbyGroupMembers.Clear();

            foreach (Agent otherAgent in allAgents)  
            {  
                if (otherAgent \== this) continue;  
                if (otherAgent.groupId \!= groupId) continue;

                float distanceSqr \= (transform.position \- otherAgent.transform.position).sqrMagnitude;  
                if (distanceSqr \<= agentRadius \* agentRadius \* 4f) // check within 2x agent radius  
                {  
                    \_nearbyGroupMembers.Add(otherAgent);  
                }  
            }  
        }

em world:  
//find nearby group members for each agent  
            for (int i \= 0; i \< \_agents.Count; i\++)  
                \_agents\[i\].FindNearbyGroupMembers(\_agents);

Ajuste de atração no grupo em agents:

 // add group cohesion force \- follow nearby group members  
            if (HasGroup && \_nearbyGroupMembers.Count \> 0)  
            {  
                // find the group member closest to the goal direction  
                Agent leader \= null;  
                float bestAlignment \= \-1f;

                foreach (Agent groupMember in \_nearbyGroupMembers)  
                {  
                    Vector3 toMember \= (groupMember.transform.position \- transform.position).normalized;  
                    float alignment \= Vector3.Dot(\_dirAgentGoal.normalized, toMember);

                    if (alignment \> bestAlignment)  
                    {  
                        bestAlignment \= alignment;  
                        leader \= groupMember;  
                    }  
                }

                if (leader \!= null)  
                {  
                    Vector3 followDirection \= (leader.transform.position \- transform.position).normalized;  
                    float distanceToLeader \= Vector3.Distance(transform.position, leader.transform.position);

                    // only follow if not too close (avoid overlapping)  
                    if (distanceToLeader \> agentRadius \* 0.8f)  
                    {  
                        \_rotation \+= groupCohesionStrength \* followDirection \* \_maxSpeed;  
                    }  
                }

Criando líder de grupo:

em agents:  
// is this agent the leader of its group?  
        public bool isGroupLeader \= false;

Em World.cs:  
UpdateGroupLeaders() \- método que identifica o agente com maior dominância em cada grupo como líder  
Chamada para UpdateGroupLeaders() a cada frame no Update()  
Cada agente do grupo segue o líder

**Fazer:**  
\-criar média geral de afinidade do grupo OK

\-troca de grupo pela afinidade/interesse: se o interesse do outro grupo for mais próximo do que o do meu grupo, troco OK

\-agentes sozinhos se juntam no grupo se afinidade for maior OK

**07/05/2026**  
**\-6º reunião com orientador**  
Site para modelos 3d de agentes:  
[https://www.mixamo.com/\#/](https://www.mixamo.com/#/)

- Agentes seguem o líder OK  
- Adicionar marcador no líder (MUDAR)  
- Adicionar cores para cada grupo (no material) \-\> classe de agente lista de cores (corGrupo1, corGrupo2…) \-\> (COLOCAR)


  
**08/05/2026**  
Afinidade dos grupos:  
em world  
 // group affinity averages: groupId \-\> average affinity  
        private Dictionary\<int, float\> \_groupAffinityAverages \= new Dictionary\<int, float\>();  
        public Dictionary\<int, float\> GroupAffinityAverages  
        {  
            get { return \_groupAffinityAverages; }  
        }

            // update group affinity averages  
            UpdateGroupAffinities();

 private void UpdateGroupAffinities()  
        {  
            // clear previous averages  
            \_groupAffinityAverages.Clear();

            // group agents by groupId  
            var groups \= new Dictionary\<int, List\<Agent\>\>();  
            foreach (Agent agent in \_agents)  
            {  
                if (agent.HasGroup)  
                {  
                    if (\!groups.ContainsKey(agent.groupId))  
                        groups\[agent.groupId\] \= new List\<Agent\>();  
                    groups\[agent.groupId\].Add(agent);  
                }  
            }

            // for each group, calculate the average affinity  
            foreach (var group in groups)  
            {  
                float totalAffinity \= 0f;  
                 
                foreach (Agent agent in group.Value)  
                {  
                    totalAffinity \+= agent.affinity;  
                }

                float averageAffinity \= group.Value.Count \> 0 ? totalAffinity / group.Value.Count : 0f;  
                \_groupAffinityAverages\[group.Key\] \= averageAffinity;  
            }  
        }  
    }

em agents  
// get the average affinity of the agent's group  
        public float GroupAverageAffinity  
        {  
            get  
            {  
                if (\!HasGroup || \_world \== null)  
                    return affinity; // return individual affinity if not in a group  
                 
                if (\_world.GroupAffinityAverages.ContainsKey(groupId))  
                    return \_world.GroupAffinityAverages\[groupId\];  
                 
                return 0f;  
            }  
        }

Troca de grupos  
1\. Detecta pares de grupos que se aproximam  
 2\. Para cada par, calcula se estão próximos o suficiente  
 3\. Compara a diferença entre a afinidade individual e cada grupo  
 4\. Se a diferença é ≥ AFFINITY\_SWITCH\_THRESHOLD, o agente troca de grupo

em world:

        /// \<summary\>  
        /// Switch this agent to a different group  
        /// \</summary\>  
        public void SwitchGroup(int newGroupId)  
        {  
            if (newGroupId \== groupId)  
                return; // already in the target group

            groupId \= newGroupId;  
            isGroupLeader \= false; // reset leader status when switching groups  
            \_nearbyGroupMembers.Clear(); // clear nearby members list  
        }  
    }

// group interaction settings  
        \[SerializeField\] private float GROUP\_PROXIMITY\_DISTANCE \= 10.0f;  
        \[Range(0f, 1f)\]  
        \[SerializeField\] private float AFFINITY\_SWITCH\_THRESHOLD \= 0.1f; // minimum difference to trigger group switch

**13/05/2026**  
Agentes sozinhos se juntam no grupo se afinidade for maior

private void EvaluateSoloAgentsJoiningGroups()  
        {  
            // find all solo agents (those without a group)  
            List\<Agent\> soloAgents \= new List\<Agent\>();  
            foreach (Agent agent in \_agents)  
            {  
                if (\!agent.HasGroup)  
                    soloAgents.Add(agent);  
            }

            if (soloAgents.Count \== 0)  
                return; // no solo agents to process

            // find all groups  
            var groups \= new Dictionary\<int, List\<Agent\>\>();  
            foreach (Agent agent in \_agents)  
            {  
                if (agent.HasGroup)  
                {  
                    if (\!groups.ContainsKey(agent.groupId))  
                        groups\[agent.groupId\] \= new List\<Agent\>();  
                    groups\[agent.groupId\].Add(agent);  
                }  
            }

            if (groups.Count \== 0)  
                return; // no groups available to join

            // for each solo agent, check if they should join any nearby group  
            foreach (Agent soloAgent in soloAgents)  
            {  
                foreach (var group in groups)  
                {  
                    // check if group is nearby  
                    List\<Agent\> groupAgents \= group.Value;  
                    float minDistance \= float.MaxValue;  
                    foreach (Agent groupAgent in groupAgents)  
                    {  
                        float distance \= Vector3.Distance(soloAgent.transform.position, groupAgent.transform.position);  
                        if (distance \< minDistance)  
                            minDistance \= distance;  
                    }

                    if (minDistance \<= GROUP\_PROXIMITY\_DISTANCE)  
                    {  
                        // group is nearby, check affinity compatibility  
                        float groupAffinity \= \_groupAffinityAverages.ContainsKey(group.Key) ? \_groupAffinityAverages\[group.Key\] : 0f;  
                        float affinityDifference \= Mathf.Abs(soloAgent.affinity \- groupAffinity);

                        // if affinity difference is small enough, join the group  
                        if (affinityDifference \<= AFFINITY\_SWITCH\_THRESHOLD)  
                        {  
                            soloAgent.SwitchGroup(group.Key);  
                            break; // solo agent joins the first compatible group found  
                        }  
                    }  
                }  
            }  
        }

Adicionando lógica para agentes solo se juntarem entre si quando próximos com afinidades semelhantes.  
 private int \_nextGroupId \= 0; // for creating new groups when solo agents meet

 // initialize next group id to avoid conflicts with existing group ids  
            foreach (SpawnArea area in spawnAreas)  
            {  
                if (area.groupId \>= 0 && area.groupId \>= \_nextGroupId)  
                    \_nextGroupId \= area.groupId \+ 1;  
            }

           // evaluate solo agents meeting each other and forming new groups  
            EvaluateSoloAgentsMeetings();

            // evaluate solo agents joining existing groups

 private void EvaluateSoloAgentsMeetings()  
        {  
            // find all solo agents (those without a group)  
            List\<Agent\> soloAgents \= new List\<Agent\>();  
            foreach (Agent agent in \_agents)  
            {  
                if (\!agent.HasGroup)  
                    soloAgents.Add(agent);  
            }

            if (soloAgents.Count \< 2)  
                return; // need at least 2 solo agents to form a pair

            // check all pairs of solo agents  
            for (int i \= 0; i \< soloAgents.Count; i\++)  
            {  
                for (int j \= i \+ 1; j \< soloAgents.Count; j\++)  
                {  
                    Agent agent1 \= soloAgents\[i\];  
                    Agent agent2 \= soloAgents\[j\];

                    // check if they are close  
                    float distance \= Vector3.Distance(agent1.transform.position, agent2.transform.position);  
                    if (distance \<= GROUP\_PROXIMITY\_DISTANCE)  
                    {  
                        // check if their affinities are similar  
                        float affinityDifference \= Mathf.Abs(agent1.affinity \- agent2.affinity);  
                        if (affinityDifference \<= AFFINITY\_SWITCH\_THRESHOLD)  
                        {  
                            // they should form a new group together  
                            int newGroupId \= \_nextGroupId\++;  
                            agent1.SwitchGroup(newGroupId);  
                            agent2.SwitchGroup(newGroupId);  
                        }  
                    }  
                }  
            }  
        }

Dar um tempo depois do spawn dos agentes até que eles possam trocar de grupo:  
\[SerializeField\] private float GROUP\_SWITCH\_GRACE\_PERIOD \= 1.0f; // time after spawn before group changes are allowed

// update agent age before group evaluation  
            for (int i \= 0; i \< \_agents.Count; i\++)  
                \_agents\[i\].timeSinceSpawn \+= SIMULATION\_TIME\_STEP;

if (agent.timeSinceSpawn \< GROUP\_SWITCH\_GRACE\_PERIOD)  
                return false;

**14/05/2026**  
**\-7º reunião com orientador**

- Analisar o timeSinceSpawn   
- Futuramente: criar mais testes \-\> dois grupos com muita afinidade e dois grupos com afinidades muito distantes,     testar variação de comportamentos  
- Ajustar Problema Frame Rate

**14/05/2026**  
Melhoria na dinâmica, coesão e visual dos grupos  
Correções e melhorias no comportamento, desempenho e visual dos grupos.

Principais correções:  
\- Redefinir Agent.timeSinceSpawn para 0f e defini-lo explicitamente no spawn para que o GROUP\_SWITCH\_GRACE\_PERIOD seja aplicado.

\- Impedir que agentes individuais sejam movidos para vários grupos em um único frame e evitar o reprocessamento de agentes que já mudaram de grupo.

\- Impedir trocas oscilantes recíprocas entre dois grupos, coletando intenções de migração para ambas as direções e aplicando apenas o bloco maior (em caso de empate, o desempate é feito aleatoriamente).

\- Aumentar GROUP\_PROXIMITY\_DISTANCE para 3.0 e corrigir a lógica de proximidade relacionada.

Alterações de comportamento:  
\- Os seguidores combinam sua direção de objetivo com a direção do líder (\_effectiveGoalDir) e GetF(...) usa essa direção efetiva para que a coesão module os pesos de auxina (preserva a propriedade de ausência de colisões do BioCrowds).  
\- A coesão é dimensionada por 1/sqrt(tamanhoDoGrupo) para reduzir a demanda por agente em grupos grandes e evitar congestionamentos.

Desempenho e robustez:  
\- Limitar a dinâmica de grupo com GROUP\_EVAL\_INTERVAL (executado a cada N passos) e contadores opcionais DEBUG\_LOG\_GROUP\_CHANGES.

\- Substituir muitas chamadas de Vector3.Distance por verificações de magnitude ao quadrado e adicionar saídas antecipadas para testes de proximidade.

\- Reutilizar coleções agrupadas (\_groupsScratch, \_agentListPool, \_soloScratch, listas de troca agrupadas) para reduzir a coleta de lixo e as alocações por quadro.

\- UpdateGroupLeaders agora se lembra dos líderes anteriores e atualiza os visuais apenas para agentes cujo estado de líder mudou.

\- Preservar \_maxAgents serializado e silenciar avisos CS0414.

Visualização:  
\- VisualAgent.ApplyGroupColor agora suporta um sinalizador isLeader; Os elementos principais estão mais brilhantes e dimensionados (interpolação linear para branco \+ escala de 1,25×).  
\- Campos não utilizados do VisualAgent foram removidos (atualizados e inicializados), assim como as atribuições relacionadas.

Diversos:  
\- Várias pequenas refatorações, comentários e diagnósticos; o arquivo README foi atualizado com um resumo das correções e suas justificativas.

**21/05/2026**  
Ajustando comportamento dos agentes para eles não andarem em fila e simularem um comportamento mais real de grupo  
Em agents:  
 // Local avoidance: repel from nearby agents to break queue formation  
            if (\_world \!= null)  
            {  
                Vector3 repulsionForce \= Vector3.zero;  
                int repelCount \= 0;  
                 
                float avoidanceRadius \= agentRadius \* 3.0f;  
                float avoidanceRadiusSqr \= avoidanceRadius \* avoidanceRadius;  
                 
                foreach (Agent other in \_world.Agents)  
                {  
                    if (other \== this) continue;  
                     
                    Vector3 toOther \= other.transform.position \- transform.position;  
                    float distSqr \= toOther.sqrMagnitude;  
                     
                    if (distSqr \< avoidanceRadiusSqr && distSqr \> 0.0001f)  
                    {  
                        float dist \= Mathf.Sqrt(distSqr);  
                        Vector3 repel \= \-toOther / dist;  
                        float strength \= 1.0f \- (dist / avoidanceRadius);  
                        strength \= strength \* strength;  
                        repulsionForce \+= repel \* strength;  
                        repelCount\++;  
                    }  
                }  
                 
                if (repelCount \> 0)  
                {  
                    float dominanceFactor \= 0.5f \+ (dominance \* 0.5f);  
                    repulsionForce \*= (dominanceFactor / repelCount);  
                    float repulsionWeight \= 0.2f;  
                    \_rotation \= (1.0f \- repulsionWeight) \* \_rotation.normalized \+ repulsionWeight \* repulsionForce;  
                }  
            }  
        }

 float baseF \= (float)((1.0 / (1.0 \+ Ymodule)) \* (1.0 \+ ((dot) / (Xmodule \* Ymodule))));  
             
            // Apply personality modulation:  
            // \- High dominance: more aggressive, prefers closer auxins (higher weights)  
            // \- Low affinity: more exploratory, adds randomness to escape follow patterns  
            float personalityMod \= 1.0f;  
             
            // Dominance increases weight on nearby auxins (encourages closer paths)  
            personalityMod \*= (1.0f \+ dominance \* 0.5f);  
             
            // Low affinity adds random variation to avoid deterministic following  
            if (affinity \< 0.5f)  
            {  
                float randomFactor \= 0.8f \+ Random.Range(0f, 0.4f) \* (1.0f \- affinity);  
                personalityMod \*= randomFactor;  
            }  
             
            return baseF \* personalityMod;  
   
// Apply personality to speed:  
            // \- High dominance: moves faster (more aggressive)  
            // \- Low affinity: variable speed (more unpredictable)  
            float speedModifier \= 0.7f \+ (dominance \* 0.3f); // dominance from 0.7x to 1.0x  
            speedModifier \*= (0.9f \+ Random.Range(0f, 0.2f) \* (1.0f \- affinity)); // affinity adds stability

Tempo de espera nos objetivos(agentes estão ficando muito tempo em cada um)  
Resolução: multiplicador de tempo de espera configurável em [World.cs](http://World.cs)

// wait time multiplier: increase to make agents wait longer at each goal  
        \[SerializeField\] private float WAIT\_TIME\_MULTIPLIER \= 1.0f;  
        public float WaitTimeMultiplier \=\> WAIT\_TIME\_MULTIPLIER;

Troca de grupos não estavam funcionando   
Ajuste:  
em world:  
 \[SerializeField\] private float GROUP\_PROXIMITY\_DISTANCE \= 15.0f; // detection radius for group interactions  
        \[SerializeField\] private float GROUP\_SWITCH\_GRACE\_PERIOD \= 0.1f; // time after spawn before group changes are allowed  
        \[Range(0f, 1f)\]  
        \[SerializeField\] private float AFFINITY\_SWITCH\_THRESHOLD \= 0.6f; // higher threshold allows more group switching

\*\*Arrumar: quando agentes trocam de grupo, devem se juntar ao grupo novo (agentes de mesma cor devem andar juntos)  
           \-Quando agente troca de grupo, ele tem que esquecer a lista de objetivos atual e pegar a lista de objetivos do grupo novo (objetivo do líder é o do grupo, gerenciador de grupos avisa os componentes do grupo para onde ir)

	\-Marcação do líder com um “diamante” na cabeça

**21/05/2026**  
**\-8º reunião com orientador**

- Testar com vários cenários (gravar vídeos de cada um, anotar métricas e fazer gráficos)

Para trabalho/apresentação final:

- Introdução  
-  trab relacionados  
-  O que é o nosso modelo (o que foi adicionado? \-\> parâmetros a mais e o resultado)  
- Métricas para os experimentos: coesão (distância média do grupo para o centro), troca de grupos, tamanho dos grupos ao longo da simulação.

\-Ter lista de agentes de cada grupo

[group.cs](http://group.cs)  
list\<agent\> agents  
agent leader  
int id

OU 

[groupManager.cs](http://groupManager.cs)  
list\<group\>

A FAZER AGORA:

* 1\. Arrumar: quando agentes trocam de grupo, devem se juntar ao grupo novo (agentes de mesma cor devem andar juntos)

           \-Quando agente troca de grupo, ele tem que esquecer a lista de objetivos atual e pegar a lista de objetivos do grupo novo (objetivo do líder é o do grupo, gerenciador de grupos avisa os componentes do grupo para onde ir)

* 2\. Marcação do líder com um “diamante” na cabeça  
* 3\. Ter lista de agentes de cada grupo  
* 4\. Fazer cenários

22/05/25

Resumo das mudanças para o commit:

* Corrigida a sincronização de goals entre agentes de um mesmo grupo.  
* Followers agora seguem continuamente o objetivo atual do líder, em vez de copiar apenas no `SwitchGroup`.  
* Implementado comportamento de coesão de grupo:  
  * Followers são atraídos em direção ao líder quando estão distantes.  
  * Só sincronizam o `goalIndex` ao se aproximarem do líder.  
* Adicionado fallback quando o líder morre:  
  * Followers passam a agir independentemente até um novo líder ser eleito.  
* Adicionado parâmetro `leaderSyncRadius` no `Agent`:  
  * Define a distância necessária para followers sincronizarem com o líder.  
  * Exposto no Inspector.  
* Adicionado controle de affinity por `SpawnArea`:  
  * Novos campos `affinityMin` e `affinityMax`.  
  * Agentes spawnados recebem affinity aleatória dentro do intervalo configurado.  
* Ajustado spawn de agentes no `World.cs` para aplicar affinity definida pelo spawner. //NAO FUNCIONOU, VERIFICAR.


 

	

     