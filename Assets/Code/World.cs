/// ---------------------------------------------
/// Contact: Henry Braun
/// Brief: Defines the world environment
/// Thanks to VHLab for original implementation
/// Date: November 2017
/// ---------------------------------------------
/// Group Dynamics extension:
/// Group affinity averages, centroids, pair
/// detection and membership switching.
/// Added: 2026 - Pratica em Pesquisa PUCRS
/// ---------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.AI;
using System.Collections;
using UnityEngine.AI;
using System.Linq;

namespace Biocrowds.Core
{
    public class World : MonoBehaviour
    {
        // ── CONFIGURAÇÃO DA SIMULAÇÃO ──────────────────────────────────────

        [Header("Simulation Configuration")]
        public SimulationConfiguration.MarkerSpawnMethod markerSpawnMethod;

        [SerializeField] private float SIMULATION_TIME_STEP   = 0.02f;
        [SerializeField] private float MAX_AGENTS             = 0;
        [SerializeField] private float AGENT_RADIUS           = 1.00f;
        [SerializeField] private float AUXIN_RADIUS           = 0.1f;
        [SerializeField] private float AUXIN_DENSITY          = 0.50f;
        [SerializeField] private float GOAL_DISTANCE_THRESHOLD = 1.0f;

        // ── DINÂMICA DE GRUPOS ─────────────────────────────────────────────

        [Header("Group Dynamics")]

        // Distância máxima entre os centróides de dois grupos
        // para que sejam considerados "próximos" e troquem agentes.
        [SerializeField] private float GROUP_DETECTION_RADIUS = 10f;

        // Diferença mínima de afinidade (entre grupo atual e grupo candidato)
        // para que o agente efetivamente troque de grupo.
        // Ex: 0.15 significa que o novo grupo precisa ser pelo menos 15% mais compatível.
        [SerializeField] private float AFFINITY_SWITCH_THRESHOLD = 0.15f;

        // Diferença máxima de afinidade para um agente SOZINHO entrar em um grupo.
        // "Afinidade maior" = diferença menor que este valor.
        // Ex: 0.2 significa que o agente entra se estiver dentro de 20% de afinidade do grupo.
        [SerializeField] private float LONE_AGENT_JOIN_THRESHOLD = 0.2f;

        // ── TERRENO ────────────────────────────────────────────────────────

        [Header("Terrain Setting")]
        public MeshFilter planeMeshFilter;

        [SerializeField] private Terrain _terrain;

        [SerializeField]
        private Vector2 _dimension = new Vector2(30.0f, 20.0f);
        public Vector2 Dimension { get { return _dimension; } }

        [SerializeField]
        private Vector2 _offset = new Vector2(0.0f, 0.0f);
        public Vector2 Offset { get { return _offset; } }

        [SerializeField] private int _maxAgents = 30;

        // ── PREFABS E CONTAINERS ───────────────────────────────────────────

        [SerializeField] private List<Agent> _agentPrefabList;
        [SerializeField] private Cell        _cellPrefab;
        [SerializeField] private Auxin       _auxinPrefab;

        [SerializeField] private List<Agent>  _agents = new List<Agent>();
        List<Cell>  _cells  = new List<Cell>();
        List<Auxin> _auxins = new List<Auxin>();

        public List<SpawnArea> spawnAreas;

        [SerializeField] private Transform _agentsContainer;
        private int _newAgentID = 0;

        public List<Cell>  Cells  { get { return _cells;  } }
        public List<Auxin> Auxins { get { return _auxins; } }

        [SerializeField] private MarkerSpawner _markerSpawner = null;

        // ── DICIONÁRIOS DE GRUPO ───────────────────────────────────────────

        // Média de afinidade de cada grupo  →  groupId : média
        private Dictionary<int, float>   _groupAffinityAverages = new Dictionary<int, float>();

        // Posição central (centróide) de cada grupo  →  groupId : Vector3
        private Dictionary<int, Vector3> _groupCentroids        = new Dictionary<int, Vector3>();

        // Pares de grupos cujos centróides estão dentro de GROUP_DETECTION_RADIUS
        // Formato: (groupId do grupo A, groupId do grupo B)
        private List<(int groupA, int groupB)> _approachingGroupPairs = new List<(int, int)>();

        // ── ESTADO ────────────────────────────────────────────────────────

        private bool _isReady;

        // ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _newAgentID = 0;

            if (spawnAreas == null || spawnAreas.Count == 0)
                spawnAreas = FindObjectsOfType<SpawnArea>().ToList();

            if (planeMeshFilter != null)
            {
                if (planeMeshFilter.name != "Plane")
                    Debug.LogWarning("PlaneMeshFilter Mesh isn't a Plane. " +
                        "The difference in scale may cause unintended behavior.");

                _dimension = new Vector2(
                    Mathf.Ceil(planeMeshFilter.transform.localScale.x * 10f),
                    Mathf.Ceil(planeMeshFilter.transform.localScale.z * 10f)
                );
                _dimension.x += _dimension.x % 2;
                _dimension.y += _dimension.y % 2;

                _offset = new Vector2(
                    Mathf.Round(planeMeshFilter.transform.position.x),
                    Mathf.Round(planeMeshFilter.transform.position.z)
                );
                _offset.x -= (_dimension.x / 2f);
                _offset.y -= (_dimension.y / 2f);

                planeMeshFilter.gameObject.SetActive(false);
            }
        }

        public void LoadWorld()
        {
            var markerSpawnerMethods = transform.GetComponentsInChildren<MarkerSpawner>();
            _markerSpawner = markerSpawnerMethods.First(p => p.spawnMethod == markerSpawnMethod);
            StartCoroutine(SetupWorld());
        }

        IEnumerator SetupWorld()
        {
            _terrain.terrainData.size = new Vector3(
                _dimension.x,
                _terrain.terrainData.size.y,
                _dimension.y
            );
            _terrain.transform.position = new Vector3(
                _offset.x,
                _terrain.transform.position.y,
                _offset.y
            );

            GameObjectUtility.SetStaticEditorFlags(
                _terrain.gameObject,
                StaticEditorFlags.NavigationStatic
            );

            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

            yield return StartCoroutine(CreateCells());
            yield return StartCoroutine(_markerSpawner.CreateMarkers(_cells, _auxins));

            Debug.Log(_auxins.Count / _cells.Count);

            yield return StartCoroutine(CreateAgents());
            yield return new WaitForSeconds(1.0f);

            _isReady = true;
            Debug.Break();
        }

        // ── CRIAÇÃO DE CÉLULAS ─────────────────────────────────────────────

        private IEnumerator CreateCells()
        {
            Transform cellPool  = new GameObject("Cells").transform;
            Vector3   _spawnPos = new Vector3();

            for (int i = 0; i < _dimension.x / 2; i++)
            {
                for (int j = 0; j < _dimension.y / 2; j++)
                {
                    _spawnPos.x = (1.0f + (i * 2.0f)) + _offset.x;
                    _spawnPos.z = (1.0f + (j * 2.0f)) + _offset.y;

                    Cell newCell = Instantiate(
                        _cellPrefab, _spawnPos,
                        Quaternion.Euler(90.0f, 0.0f, 0.0f),
                        cellPool
                    );

                    newCell.name = "Cell [" + i + "][" + j + "]";
                    newCell.X    = i;
                    newCell.Z    = j;
                    newCell.ShowMesh(SceneController.ShowCells);

                    _cells.Add(newCell);
                    yield return null;
                }
            }
        }

        // ── CRIAÇÃO DE AGENTES ─────────────────────────────────────────────

        private IEnumerator CreateAgents()
        {
            _agentsContainer = new GameObject("Agents").transform;

            foreach (SpawnArea _area in spawnAreas)
            {
                for (int i = 0; i < _area.initialNumberOfAgents; i++)
                {
                    if (MAX_AGENTS == 0 || _agents.Count < MAX_AGENTS)
                        SpawnNewAgentInArea(_area, true);
                    yield return null;
                }
            }
        }

        // ── UPDATE ─────────────────────────────────────────────────────────

        void Update()
        {
            if (!_isReady) return;

            // ── Spawn cíclico ──────────────────────────────────────────────
            foreach (SpawnArea _area in spawnAreas)
            {
                _area.UpdateSpawnCounter(SIMULATION_TIME_STEP);
                if (_area.CycleReady)
                {
                    for (int i = 0; i < _area.quantitySpawnedEachCycle; i++)
                        if (MAX_AGENTS == 0 || _agents.Count < MAX_AGENTS)
                            SpawnNewAgentInArea(_area, false);
                }
                _area.ResetCycleReady();
            }

            // ── Visual dos agentes ─────────────────────────────────────────
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].UpdateVisualAgent();

            // ── DINÂMICA DE GRUPOS ─────────────────────────────────────────
            // Passo 1: calcula média de afinidade e centróide de cada grupo
            ComputeGroupData();
            // Passo 2: detecta pares de grupos cujos centróides estão próximos
            DetectApproachingGroupPairs();
            // Passos 3 e 4: compara afinidade e troca agentes se necessário
            UpdateGroupMembership();
            // Agentes sozinhos buscam grupo compatível independente de pares
            UpdateLoneAgents();
            // Atualiza cor de debug de cada agente
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].UpdateGroupColor();
            // ──────────────────────────────────────────────────────────────

            // ── Reset das auxinas ──────────────────────────────────────────
            for (int i = 0; i < _cells.Count; i++)
                for (int j = 0; j < _cells[i].Auxins.Count; j++)
                    _cells[i].Auxins[j].ResetAuxin();

            // ── Captura de auxinas por agente ──────────────────────────────
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].FindNearAuxins();

            for (int i = 0; i < _agents.Count; i++)
                _agents[i].auxinCount = _agents[i].Auxins.Count;

            // ── Loop principal de movimento ────────────────────────────────
            List<Agent> _agentsToRemove   = new List<Agent>();
            bool        _showAuxinVectors = SceneController.ShowAuxinVectors;

            for (int i = 0; i < _agents.Count; i++)
            {
                List<Auxin> agentAuxins = _agents[i].Auxins;

                for (int j = 0; j < agentAuxins.Count; j++)
                {
                    _agents[i]._distAuxin.Add(
                        agentAuxins[j].Position - _agents[i].transform.position
                    );

                    if (_showAuxinVectors)
                        Debug.DrawLine(agentAuxins[j].Position, _agents[i].transform.position, Color.green);
                }

                _agents[i].CalculateDirection();
                _agents[i].CalculateVelocity();

                if (!_agents[i].isWaiting)
                    _agents[i].MovementStep(SIMULATION_TIME_STEP);

                _agents[i].WaitStep(SIMULATION_TIME_STEP);

                if (_agents[i].removeWhenGoalReached && _agents[i].IsAtFinalGoal())
                    _agentsToRemove.Add(_agents[i]);
            }

            foreach (Agent a in _agentsToRemove)
            {
                _agents.Remove(a);
                Destroy(a.gameObject);
            }
            _agentsToRemove.Clear();

            // ── NavMesh step ───────────────────────────────────────────────
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].NavmeshStep(SIMULATION_TIME_STEP);
        }

        // ── DINÂMICA DE GRUPOS — MÉTODOS ───────────────────────────────────

        /// <summary>
        /// Percorre todos os agentes com grupo e calcula, em uma única passagem:
        ///   - A média de afinidade de cada grupo (_groupAffinityAverages)
        ///   - O centróide (posição central) de cada grupo (_groupCentroids)
        /// </summary>
        private void ComputeGroupData()
        {
            var affinityAccum = new Dictionary<int, float>();
            var positionAccum = new Dictionary<int, Vector3>();
            var memberCount   = new Dictionary<int, int>();

            foreach (Agent agent in _agents)
            {
                if (!agent.HasGroup) continue;

                int gid = agent.groupId;

                if (!memberCount.ContainsKey(gid))
                {
                    affinityAccum[gid] = 0f;
                    positionAccum[gid] = Vector3.zero;
                    memberCount[gid]   = 0;
                }

                affinityAccum[gid] += agent.affinity;
                positionAccum[gid] += agent.transform.position;
                memberCount[gid]   += 1;
            }

            _groupAffinityAverages.Clear();
            _groupCentroids.Clear();

            foreach (var kvp in memberCount)
            {
                int gid   = kvp.Key;
                int count = kvp.Value;

                _groupAffinityAverages[gid] = affinityAccum[gid] / count;
                _groupCentroids[gid]        = positionAccum[gid] / count;
            }
        }

        /// <summary>
        /// Compara os centróides de cada par único de grupos (A, B).
        /// Se a distância entre eles for menor ou igual a GROUP_DETECTION_RADIUS,
        /// o par é adicionado a _approachingGroupPairs.
        /// </summary>
        private void DetectApproachingGroupPairs()
        {
            _approachingGroupPairs.Clear();

            var groupIds = new List<int>(_groupCentroids.Keys);

            // Itera pares únicos: (0,1), (0,2), (1,2) — nunca (1,0) ou repetidos
            for (int i = 0; i < groupIds.Count; i++)
            {
                for (int j = i + 1; j < groupIds.Count; j++)
                {
                    int   groupA = groupIds[i];
                    int   groupB = groupIds[j];
                    float dist   = Vector3.Distance(
                        _groupCentroids[groupA],
                        _groupCentroids[groupB]
                    );

                    if (dist <= GROUP_DETECTION_RADIUS)
                        _approachingGroupPairs.Add((groupA, groupB));
                }
            }
        }

        /// <summary>
        /// Para cada par de grupos próximos detectado, verifica cada agente:
        ///
        /// Passo 3 — Compara a diferença de afinidade do agente com cada grupo:
        ///   diffAtual  = |agent.affinity - média do grupo atual|
        ///   diffNovo   = |agent.affinity - média do grupo candidato|
        ///   improvement = diffAtual - diffNovo
        ///
        /// Passo 4 — Se improvement >= AFFINITY_SWITCH_THRESHOLD, o agente troca.
        ///
        /// Agentes sem grupo (-1) entram no grupo mais compatível se a diferença
        /// de afinidade for menor que o threshold.
        /// </summary>
        private void UpdateGroupMembership()
        {
            foreach (var (groupA, groupB) in _approachingGroupPairs)
            {
                if (!_groupAffinityAverages.TryGetValue(groupA, out float avgA)) continue;
                if (!_groupAffinityAverages.TryGetValue(groupB, out float avgB)) continue;

                foreach (Agent agent in _agents)
                {
                    float diffWithA = Mathf.Abs(agent.affinity - avgA);
                    float diffWithB = Mathf.Abs(agent.affinity - avgB);

                    // Agente no grupo A → verifica se B é mais compatível
                    if (agent.groupId == groupA)
                    {
                        float improvement = diffWithA - diffWithB;
                        if (improvement >= AFFINITY_SWITCH_THRESHOLD)
                        {
                            Debug.Log($"[Grupo] {agent.name} | {groupA} → {groupB} " +
                                      $"| melhoria: {improvement:F2}");
                            agent.groupId      = groupB;
                            agent.isGroupLeader = false;
                        }
                    }
                    // Agente no grupo B → verifica se A é mais compatível
                    else if (agent.groupId == groupB)
                    {
                        float improvement = diffWithB - diffWithA;
                        if (improvement >= AFFINITY_SWITCH_THRESHOLD)
                        {
                            Debug.Log($"[Grupo] {agent.name} | {groupB} → {groupA} " +
                                      $"| melhoria: {improvement:F2}");
                            agent.groupId      = groupA;
                            agent.isGroupLeader = false;
                        }
                    }
                    // Agentes sozinhos são tratados pelo método UpdateLoneAgents()
                }
            }
        }

        /// <summary>
        /// Percorre todos os agentes SEM grupo e verifica se há algum grupo
        /// próximo o suficiente (dentro de GROUP_DETECTION_RADIUS) com afinidade
        /// compatível.
        ///
        /// Lógica:
        ///   1. Para cada agente sozinho, busca todos os grupos dentro do raio
        ///   2. Calcula a diferença de afinidade com cada grupo encontrado
        ///   3. Guarda o grupo com a MENOR diferença (mais compatível)
        ///   4. Se essa diferença ≤ LONE_AGENT_JOIN_THRESHOLD → agente entra no grupo
        ///      ("afinidade maior" = diferença menor = mais compatível)
        /// </summary>
        private void UpdateLoneAgents()
        {
            foreach (Agent agent in _agents)
            {
                // Ignora agentes que já têm grupo
                if (agent.HasGroup) continue;

                int   bestGroup = -1;
                float bestDiff  = float.MaxValue;

                // Percorre todos os grupos existentes
                foreach (var kvp in _groupCentroids)
                {
                    int   gid         = kvp.Key;
                    float distToGroup = Vector3.Distance(
                        agent.transform.position, kvp.Value
                    );

                    // Passo 2: só considera grupos dentro do raio de detecção
                    if (distToGroup > GROUP_DETECTION_RADIUS) continue;

                    // Passo 3: calcula diferença de afinidade com este grupo
                    if (!_groupAffinityAverages.TryGetValue(gid, out float avg)) continue;

                    float diff = Mathf.Abs(agent.affinity - avg);

                    // Guarda o mais compatível (menor diferença)
                    if (diff < bestDiff)
                    {
                        bestDiff  = diff;
                        bestGroup = gid;
                    }
                }

                // Passo 4: entra no grupo se a compatibilidade for alta o suficiente
                if (bestGroup != -1 && bestDiff <= LONE_AGENT_JOIN_THRESHOLD)
                {
                    Debug.Log($"[Sozinho] {agent.name} " +
                              $"(afinidade: {agent.affinity:F2}) " +
                              $"→ entrou no grupo {bestGroup} " +
                              $"(média do grupo: {_groupAffinityAverages[bestGroup]:F2}, " +
                              $"diferença: {bestDiff:F2})");

                    agent.groupId = bestGroup;
                }
            }
        }

        // ── SPAWN DE AGENTES ───────────────────────────────────────────────

        private void SpawnNewAgentInArea(SpawnArea _area, bool _isInitialSpawn)
        {
            Vector3 _pos = _area.GetRandomPoint();

            Agent newAgent = Instantiate(
                _agentPrefabList[Random.Range(0, _agentPrefabList.Count)],
                _pos,
                Quaternion.identity,
                _agentsContainer
            );

            newAgent.name             = "Agent [" + GetNewAgentID() + "]";
            newAgent.CurrentCell      = GetClosestCellToPoint(_pos);
            newAgent.agentRadius      = AGENT_RADIUS;
            newAgent.goalDistThreshold = GOAL_DISTANCE_THRESHOLD;
            newAgent.World            = this;

            // Atribui o grupo da área de spawn
            newAgent.groupId = _area.groupId;

            // Afinidade aleatória entre 0 e 1
            newAgent.affinity = Random.Range(0f, 1f);

            if (_isInitialSpawn)
            {
                newAgent.Goal                 = _area.initialAgentsGoalList[0];
                newAgent.goalsList            = _area.initialAgentsGoalList;
                newAgent.removeWhenGoalReached = _area.initialRemoveWhenGoalReached;
                newAgent.goalsWaitList        = _area.initialWaitList;
            }
            else
            {
                newAgent.Goal                 = _area.repeatingGoalList[0];
                newAgent.goalsList            = _area.repeatingGoalList;
                newAgent.removeWhenGoalReached = _area.repeatingRemoveWhenGoalReached;
                newAgent.goalsWaitList        = _area.repeatingWaitList;
            }

            _agents.Add(newAgent);
        }

        private void SpawnNewAgent(Vector3 _pos, bool _removeWhenGoalReached,
            List<GameObject> _goalList)
        {
            Agent newAgent = Instantiate(
                _agentPrefabList[Random.Range(0, _agentPrefabList.Count)],
                _pos,
                Quaternion.identity,
                _agentsContainer
            );

            newAgent.name                 = "Agent [" + GetNewAgentID() + "]";
            newAgent.CurrentCell          = GetClosestCellToPoint(_pos);
            newAgent.agentRadius          = AGENT_RADIUS;
            newAgent.Goal                 = _goalList[0];
            newAgent.goalsList            = _goalList;
            newAgent.removeWhenGoalReached = _removeWhenGoalReached;
            newAgent.World                = this;
            newAgent.affinity             = Random.Range(0f, 1f);

            _agents.Add(newAgent);
        }

        // ── UTILIDADES ────────────────────────────────────────────────────

        private Cell GetClosestCellToPoint(Vector3 point)
        {
            float _minDist  = Vector3.Distance(point, _cells[0].transform.position);
            int   _minIndex = 0;

            for (int i = 1; i < _cells.Count; i++)
            {
                if (Vector3.Distance(point, _cells[i].transform.position) < _minDist)
                {
                    _minDist  = Vector3.Distance(point, _cells[i].transform.position);
                    _minIndex = i;
                }
            }

            return _cells[_minIndex];
        }

        private int GetNewAgentID()
        {
            _newAgentID++;
            return _newAgentID - 1;
        }

        public void ShowAuxinMeshes(bool p_enable)
        {
            foreach (Auxin _a in Auxins) _a.ShowMesh(p_enable);
        }

        public void ShowCellMeshes(bool p_enable)
        {
            foreach (Cell _c in Cells) _c.ShowMesh(p_enable);
        }
    }
}