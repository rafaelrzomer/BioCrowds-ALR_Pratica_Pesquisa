/// ---------------------------------------------
/// Contact: Henry Braun
/// Brief: Defines the world environment
/// Thanks to VHLab for original implementation
/// Date: November 2017 
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
        [Header("Simulation Configuration")]
        public SimulationConfiguration.MarkerSpawnMethod markerSpawnMethod;

        [SerializeField] private float SIMULATION_TIME_STEP = 0.02f;

        [SerializeField] private float MAX_AGENTS = 0;
        //agent radius
        [SerializeField] private float AGENT_RADIUS = 1.00f;

        //radius for auxin collide
        [SerializeField] private float AUXIN_RADIUS = 0.1f;

        //density
        [SerializeField] private float AUXIN_DENSITY = 0.50f;

        [SerializeField] private float GOAL_DISTANCE_THRESHOLD = 1.0f;

        // group interaction settings
        [SerializeField] private bool ALLOW_GROUP_CHANGES = true; // master switch for all group changes
        [SerializeField] private float GROUP_PROXIMITY_DISTANCE = 15.0f; // detection radius for group interactions
        [SerializeField] private float GROUP_SWITCH_GRACE_PERIOD = 0.1f; // time after spawn before group changes are allowed
        [Range(0f, 1f)]
        [SerializeField] private float AFFINITY_SWITCH_THRESHOLD = 0.6f; // higher threshold allows more group switching


        [Header("Terrain Setting")]
        public MeshFilter planeMeshFilter;

        [SerializeField]
        private Terrain _terrain;

        [SerializeField]
        private Vector2 _dimension = new Vector2(30.0f, 20.0f);
        public Vector2 Dimension
        {
            get { return _dimension; }
        }

        [SerializeField]
        private Vector2 _offset = new Vector2(0.0f, 0.0f);
        public Vector2 Offset
        {
            get { return _offset; }
        }
        // legacy cap; kept because it is serialized in existing scenes (Museu, Experiments, Test).
        // The active cap is MAX_AGENTS above. Field is intentionally unread.
#pragma warning disable 0414
        [SerializeField] private int _maxAgents = 30;
#pragma warning restore 0414

        //agent prefab
        [SerializeField]
        private List<Agent> _agentPrefabList;

        [SerializeField]
        private Cell _cellPrefab;

        [SerializeField]
        private Auxin _auxinPrefab;


        [SerializeField]
        private List<Agent> _agents = new List<Agent>();
        List<Cell> _cells = new List<Cell>();
        List<Auxin> _auxins = new List<Auxin>();

        public List<SpawnArea> spawnAreas;

        [SerializeField]
        private Transform _agentsContainer;
        private int _newAgentID = 0;
        private int _nextGroupId = 0; // for creating new groups when solo agents meet

        // group dynamics do not need to run every simulation step; throttle them
        [SerializeField] private int GROUP_EVAL_INTERVAL = 5;
        private int _groupEvalCounter = 0;

        // diagnostic: when true, World logs a per-eval-cycle summary of group switches.
        // Toggle in the Inspector to inspect whether dynamics are actually firing.
        [SerializeField] private bool DEBUG_LOG_GROUP_CHANGES = true;
        private int _switchesThisCycle = 0;
        private int _newGroupsThisCycle = 0;
        private int _soloJoinsThisCycle = 0;

        // reusable scratch dictionaries / lists for group evaluation to avoid per-frame GC allocations
        private Dictionary<int, List<Agent>> _groupsScratch;
        private Stack<List<Agent>> _agentListPool;
        private List<Agent> _soloScratch;
        private HashSet<Agent> _previousLeaders;
        private List<Agent> _toSwitchToA;
        private List<Agent> _toSwitchToB;

        public List<Cell> Cells
        {
            get { return _cells; }
        }

        public List<Auxin> Auxins
        {
            get { return _auxins; }
        }

        public List<Agent> Agents
        {
            get { return _agents; }
        }

        [SerializeField]
        private MarkerSpawner _markerSpawner = null;

        // group affinity averages: groupId -> average affinity
        private Dictionary<int, float> _groupAffinityAverages = new Dictionary<int, float>();
        public Dictionary<int, float> GroupAffinityAverages
        {
            get { return _groupAffinityAverages; }
        }

        // group sizes: groupId -> number of members (updated each eval cycle)
        private Dictionary<int, int> _groupSizes = new Dictionary<int, int>();
        public int GetGroupSize(int gid)
        {
            int n;
            return _groupSizes.TryGetValue(gid, out n) ? n : 0;
        }

        /// <summary>
        /// Finds and returns the leader agent of the specified group
        /// </summary>
        public Agent GetGroupLeader(int groupId)
        {
            foreach (Agent agent in _agents)
            {
                if (agent.groupId == groupId && agent.isGroupLeader)
                    return agent;
            }
            return null;
        }

        //max auxins on the ground
        private bool _isReady;

        private void Awake()
        {
            _newAgentID = 0;
            _nextGroupId = 0;
            if (spawnAreas.Count == 0)
                spawnAreas = FindObjectsOfType<SpawnArea>().ToList();

            // initialize next group id to avoid conflicts with existing group ids
            foreach (SpawnArea area in spawnAreas)
            {
                if (area.groupId >= 0 && area.groupId >= _nextGroupId)
                    _nextGroupId = area.groupId + 1;
            }

            if (planeMeshFilter != null)
            {
                if (planeMeshFilter.name != "Plane")
                    Debug.LogWarning("PlaneMeshFilter Mesh isn't a Plane. " +
                        "The difference in scale may cause unintended behavior.");

                _dimension = new Vector2(Mathf.Ceil(planeMeshFilter.transform.localScale.x * 10f),
                    Mathf.Ceil(planeMeshFilter.transform.localScale.z * 10f));
                _dimension.x += _dimension.x % 2;
                _dimension.y += _dimension.y % 2;

                _offset = new Vector2(Mathf.Round(planeMeshFilter.transform.position.x),
                    Mathf.Round(planeMeshFilter.transform.position.z));
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

        // Use this for initialization
        IEnumerator SetupWorld()
        {
            //Application.runInBackground = true;

            //change terrain size according informed
            _terrain.terrainData.size = new Vector3(_dimension.x, _terrain.terrainData.size.y, _dimension.y);
            _terrain.transform.position = new Vector3(_offset.x, _terrain.transform.position.y, _offset.y);

            GameObjectUtility.SetStaticEditorFlags(_terrain.gameObject, StaticEditorFlags.NavigationStatic);

            //build the navmesh at runtime
            //NavMeshBuilder.BuildNavMesh();
            // UnityEditor.AI.NavMeshBuilder.BuildNavMesh();


            //create all cells based on dimension
            yield return StartCoroutine(CreateCells());

            yield return StartCoroutine(_markerSpawner.CreateMarkers(_cells, _auxins));
            Debug.Log(_auxins.Count/_cells.Count);

            //populate cells with auxins
            //yield return StartCoroutine(DartThrowing());

            //create our agents
            yield return StartCoroutine(CreateAgents());

            //wait a little bit to start moving
            yield return new WaitForSeconds(1.0f);
            _isReady = true;
            Debug.Break();
        }

        private IEnumerator CreateCells()
        {
            Transform cellPool = new GameObject("Cells").transform;
            Vector3 _spawnPos = new Vector3();

            for (int i = 0; i < _dimension.x / 2; i++) //i + agentRadius * 2
            {
                for (int j = 0; j < _dimension.y / 2; j++) // j + agentRadius * 2
                {
                    //instantiante a new cell
                    _spawnPos.x = (1.0f + (i * 2.0f)) + _offset.x;
                    _spawnPos.z = (1.0f + (j * 2.0f)) + _offset.y;

                    Cell newCell = Instantiate(_cellPrefab, _spawnPos, Quaternion.Euler(90.0f, 0.0f, 0.0f), cellPool);

                    //change its name
                    newCell.name = "Cell [" + i + "][" + j + "]";

                    //metadata for optimization
                    newCell.X = i;
                    newCell.Z = j;

                    newCell.ShowMesh(SceneController.ShowCells);

                    _cells.Add(newCell);

                    yield return null;
                }
            }
        }

        private IEnumerator DartThrowing()
        {
            //lets set the qntAuxins for each cell according the density estimation
            float densityToQnt = AUXIN_DENSITY;

            Transform auxinPool = new GameObject("Auxins").transform;

            densityToQnt *= 2f / (2.0f * AUXIN_RADIUS);
            densityToQnt *= 2f / (2.0f * AUXIN_RADIUS);

            
            int _maxAuxins = (int)Mathf.Floor(densityToQnt);

            //for each cell, we generate its auxins
            for (int c = 0; c < _cells.Count; c++)
            {
                //Dart throwing auxins
                //use this flag to break the loop if it is taking too long (maybe there is no more space)
                int flag = 0;
                for (int i = 0; i < _maxAuxins; i++)
                {
                    float x = Random.Range(_cells[c].transform.position.x - 0.99f, _cells[c].transform.position.x + 0.99f);
                    float z = Random.Range(_cells[c].transform.position.z - 0.99f, _cells[c].transform.position.z + 0.99f);

                    //see if there are auxins in this radius. if not, instantiante
                    List<Auxin> allAuxinsInCell = _cells[c].Auxins;
                    bool createAuxin = true;
                    for (int j = 0; j < allAuxinsInCell.Count; j++)
                    {
                        float distanceAASqr = (new Vector3(x, 0f, z) - allAuxinsInCell[j].Position).sqrMagnitude;

                        //if it is too near no need to add another
                        if (distanceAASqr < AUXIN_RADIUS * AUXIN_RADIUS)
                        {
                            createAuxin = false;
                            break;
                        }
                    }

                    //if i have found no auxin, i still need to check if is there obstacles on the way
                    if (createAuxin)
                    {
                        //sphere collider to try to find the obstacles
                        //NavMeshHit hit;
                        //createAuxin = NavMesh.Raycast(new Vector3(x, 2f, z), new Vector3(x, -2f, z), out hit, 1 << NavMesh.GetAreaFromName("Walkable")); //NavMesh.GetAreaFromName("Walkable")); // NavMesh.AllAreas);
                        //createAuxin = NavMesh.SamplePosition(new Vector3(x, 0.0f, z), out hit, 0.1f, 1 << NavMesh.GetAreaFromName("Walkable"));
                        //bool isBlocked = _obstacleCollider.bounds.Contains(new Vector3(x, 0.0f, z));
                        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(x, 0f, z), AUXIN_RADIUS + 0.1f, 1 << LayerMask.NameToLayer("Obstacle"));
                        createAuxin = (hitColliders.Length == 0);
                    }

                    //check if auxin can be created there
                    if (createAuxin)
                    {
                        Auxin newAuxin = Instantiate(_auxinPrefab, new Vector3(x, 0.0f, z), Quaternion.identity, auxinPool);

                        //change its name
                        newAuxin.name = "Auxin [" + c + "][" + i + "]";
                        //this auxin is from this cell
                        newAuxin.Cell = _cells[c];
                        //set position
                        newAuxin.Position = new Vector3(x, 0f, z);

                        newAuxin.ShowMesh(SceneController.ShowAuxins);

                        _auxins.Add(newAuxin);
                        //add this auxin to this cell
                        _cells[c].Auxins.Add(newAuxin);

                        //reset the flag
                        flag = 0;

                        //speed up the demonstration a little bit...
                        if (i % 200 == 0)
                            yield return null;
                    }
                    else
                    {
                        //else, try again
                        flag++;
                        i--;
                    }

                    //if flag is above qntAuxins (*2 to have some more), break;
                    if (flag > _maxAuxins * 2)
                    {
                        //reset the flag
                        flag = 0;
                        break;
                    }
                }
            }
        }

        private IEnumerator CreateAgents()
        {
            _agentsContainer = new GameObject("Agents").transform;
          
            //instantiate agents
            foreach (SpawnArea _area in spawnAreas)
            {
                for (int i = 0; i < _area.initialNumberOfAgents; i ++)
                {
                    if (MAX_AGENTS == 0 || _agents.Count < MAX_AGENTS)
                        SpawnNewAgentInArea(_area, true);
                    yield return null;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //TODO: Modificar de time-deltatime para fixed frame
            if (!_isReady)
                return;

            foreach (SpawnArea _area in spawnAreas)
            {
                _area.UpdateSpawnCounter(SIMULATION_TIME_STEP);
                if (_area.CycleReady)
                {
                    for (int i = 0; i < _area.quantitySpawnedEachCycle; i++)
                    {
                        if (MAX_AGENTS == 0 || _agents.Count < MAX_AGENTS)
                            SpawnNewAgentInArea(_area, false);
                    }
                }
                _area.ResetCycleReady();
            }

            // Update de Navmesh for each agent 
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].UpdateVisualAgent();

            //reset auxins
            for (int i = 0; i < _cells.Count; i++)
                for (int j = 0; j < _cells[i].Auxins.Count; j++)
                    _cells[i].Auxins[j].ResetAuxin();

            // update agent age before group evaluation
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].timeSinceSpawn += SIMULATION_TIME_STEP;

            //find nearest auxins for each agent
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].FindNearAuxins();

            //find nearby group members for each agent
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].FindNearbyGroupMembers(_agents);

            // throttle group dynamics: social decisions do not need 50 Hz
            _groupEvalCounter++;
            if (_groupEvalCounter >= GROUP_EVAL_INTERVAL)
            {
                _groupEvalCounter = 0;
                _switchesThisCycle = 0;
                _newGroupsThisCycle = 0;
                _soloJoinsThisCycle = 0;

                // update group leaders (agent with highest dominance in each group)
                UpdateGroupLeaders();

                // update group affinity averages
                UpdateGroupAffinities();

                // check for group proximity and potential group switches
                EvaluateGroupProximityAndSwitches();

                // evaluate solo agents meeting each other and forming new groups
                EvaluateSoloAgentsMeetings();

                // evaluate solo agents joining existing groups
                EvaluateSoloAgentsJoiningGroups();

                if (DEBUG_LOG_GROUP_CHANGES && (_switchesThisCycle > 0 || _newGroupsThisCycle > 0 || _soloJoinsThisCycle > 0))
                {
                    Debug.Log($"[Groups] cycle: swaps={_switchesThisCycle} newGroups={_newGroupsThisCycle} soloJoins={_soloJoinsThisCycle} groups={_groupSizes.Count} agents={_agents.Count}");
                }
            }

            for (int i = 0; i < _agents.Count; i++)
                _agents[i].auxinCount = _agents[i].Auxins.Count;
            /*
             * to find where the agent must move, we need to get the vectors from the agent to each auxin he has, and compare with 
             * the vector from agent to goal, generating a angle which must lie between 0 (best case) and 180 (worst case)
             * The calculation formula was taken from the Bicho´s master thesis and from Paravisi OSG implementation.
            */
            /*for each agent:
            1 - for each auxin near him, find the distance vector between it and the agent
            2 - calculate the movement vector 
            3 - calculate speed vector 
            4 - step
            */

            List<Agent> _agentsToRemove = new List<Agent>();
            bool _showAgentAuxingVector = SceneController.ShowAuxinVectors;
            //for (int i = 0; i < _maxAgents; i++)
            for (int i = 0; i < _agents.Count; i++)
            {
                //find the agent
                List<Auxin> agentAuxins = _agents[i].Auxins;

                //vector for each auxin
                for (int j = 0; j < agentAuxins.Count; j++)
                {
                    //add the distance vector between it and the agent
                    _agents[i]._distAuxin.Add(agentAuxins[j].Position - _agents[i].transform.position);

                    //just draw the lines to each auxin
                    if (_showAgentAuxingVector)
                        Debug.DrawLine(agentAuxins[j].Position, _agents[i].transform.position, Color.green);
                }

                //calculate the movement vector
                _agents[i].CalculateDirection();
                //calculate speed vector
                _agents[i].CalculateVelocity();
                //step
                if (!_agents[i].isWaiting)
                    _agents[i].MovementStep(SIMULATION_TIME_STEP);

                _agents[i].WaitStep(SIMULATION_TIME_STEP);
                //if (_agents[i].IsAtCurrentGoal() && !_agents[i].isWaiting)


                if (_agents[i].removeWhenGoalReached && _agents[i].IsAtFinalGoal())
                    _agentsToRemove.Add(_agents[i]);
            }

            foreach(Agent a in _agentsToRemove)
            {
                _agents.Remove(a);
                Destroy(a.gameObject);
            }
            _agentsToRemove.Clear();

            // Update de Navmesh for each agent 
            for (int i = 0; i < _agents.Count; i++)
                _agents[i].NavmeshStep(SIMULATION_TIME_STEP);

            
        }

        private Cell GetClosestCellToPoint (Vector3 point)
        {
            float _minDist = Vector3.Distance(point, _cells[0].transform.position);
            int _minIndex = 0;
            for (int i = 1; i < _cells.Count; i ++)
            {
                if (Vector3.Distance(point, _cells[i].transform.position) < _minDist)
                {
                    _minDist = Vector3.Distance(point, _cells[i].transform.position);
                    _minIndex = i;
                }
            }

            return _cells[_minIndex];
        }

        private void SpawnNewAgent(Vector3 _pos, bool _removeWhenGoalReached, 
            List<GameObject> _goalList)
        {
            Agent newAgent = Instantiate(_agentPrefabList[Random.Range(0, _agentPrefabList.Count)],
                _pos, Quaternion.identity, _agentsContainer);
            newAgent.name = "Agent [" + GetNewAgentID() + "]";  //name
            newAgent.CurrentCell = GetClosestCellToPoint(_pos);
            newAgent.agentRadius = AGENT_RADIUS;  //agent radius
            newAgent.Goal = _goalList[0];  //agent goal
            newAgent.goalsList = _goalList;
            newAgent.removeWhenGoalReached = _removeWhenGoalReached;
            newAgent.World = this;
            _agents.Add(newAgent);
        }

        private void SpawnNewAgentInArea(SpawnArea _area, bool _isInitialSpawn)
        {
            Vector3 _pos = _area.GetRandomPoint();
            Agent newAgent = Instantiate(_agentPrefabList[Random.Range(0, _agentPrefabList.Count)], 
                _pos, Quaternion.identity, _agentsContainer);
            newAgent.name = "Agent [" + GetNewAgentID() + "]";  //name
            newAgent.CurrentCell = GetClosestCellToPoint(_pos);
            newAgent.agentRadius = AGENT_RADIUS;  //agent radius
            newAgent.goalDistThreshold = GOAL_DISTANCE_THRESHOLD;
            if (_isInitialSpawn)
            {
                newAgent.Goal = _area.initialAgentsGoalList[0];  //agent goal
                newAgent.goalsList = _area.initialAgentsGoalList;
                newAgent.removeWhenGoalReached = _area.initialRemoveWhenGoalReached;
                newAgent.goalsWaitList = _area.initialWaitList;
            }
            else
            {
                newAgent.Goal = _area.repeatingGoalList[0];  //agent goal
                newAgent.goalsList = _area.repeatingGoalList;
                newAgent.removeWhenGoalReached = _area.repeatingRemoveWhenGoalReached;
                newAgent.goalsWaitList = _area.repeatingWaitList;
            }
            newAgent.groupId = _area.groupId;
            newAgent.timeSinceSpawn = 0f; // enforce grace period from spawn moment
            newAgent.World = this;
            _agents.Add(newAgent);
        }

        private int GetNewAgentID()
        {
            _newAgentID++;
            return _newAgentID - 1;
        }

        public void ShowAuxinMeshes (bool p_enable)
        {
            foreach (Auxin _a in Auxins)
                _a.ShowMesh(p_enable);
        }
        public void ShowCellMeshes(bool p_enable)
        {
            foreach (Cell _c in Cells)
                _c.ShowMesh(p_enable);
        }

        private void UpdateGroupLeaders()
        {
            // remember the previous leadership state so we only refresh visuals for agents whose state changed
            // (calling ApplyGroupColor on every agent every eval cycle is fine, but this is cheaper).
            // we use an instance HashSet of previous leaders, allocated once.
            if (_previousLeaders == null) _previousLeaders = new HashSet<Agent>();
            _previousLeaders.Clear();
            foreach (Agent agent in _agents)
            {
                if (agent.isGroupLeader)
                    _previousLeaders.Add(agent);
                agent.isGroupLeader = false;
            }

            // group agents by groupId (reuse pooled dictionary; see _groupsScratch)
            if (_groupsScratch == null) _groupsScratch = new Dictionary<int, List<Agent>>();
            ClearGroupsScratch();

            foreach (Agent agent in _agents)
            {
                if (agent.HasGroup)
                {
                    List<Agent> bucket;
                    if (!_groupsScratch.TryGetValue(agent.groupId, out bucket))
                    {
                        bucket = GetPooledAgentList();
                        _groupsScratch[agent.groupId] = bucket;
                    }
                    bucket.Add(agent);
                }
            }

            // for each group, find the agent with highest dominance
            foreach (var group in _groupsScratch)
            {
                Agent leader = null;
                float maxDominance = -1f;

                foreach (Agent agent in group.Value)
                {
                    if (agent.dominance > maxDominance)
                    {
                        maxDominance = agent.dominance;
                        leader = agent;
                    }
                }

                if (leader != null)
                    leader.isGroupLeader = true;
            }

            // refresh visuals only when leadership flag flipped
            foreach (Agent agent in _agents)
            {
                bool wasLeader = _previousLeaders.Contains(agent);
                if (wasLeader != agent.isGroupLeader)
                    agent.ApplyGroupColor();
            }
        }

        // ---- pooled list helpers ----
        private List<Agent> GetPooledAgentList()
        {
            if (_agentListPool == null) _agentListPool = new Stack<List<Agent>>();
            if (_agentListPool.Count > 0)
            {
                var list = _agentListPool.Pop();
                list.Clear();
                return list;
            }
            return new List<Agent>();
        }

        private void ReleaseAgentList(List<Agent> list)
        {
            if (list == null) return;
            list.Clear();
            if (_agentListPool == null) _agentListPool = new Stack<List<Agent>>();
            _agentListPool.Push(list);
        }

        private void ClearGroupsScratch()
        {
            if (_groupsScratch == null) return;
            foreach (var kv in _groupsScratch)
                ReleaseAgentList(kv.Value);
            _groupsScratch.Clear();
        }

        private void UpdateGroupAffinities()
        {
            // reuse the dictionary already built by UpdateGroupLeaders this eval cycle
            _groupAffinityAverages.Clear();
            _groupSizes.Clear();

            if (_groupsScratch == null)
                return;

            foreach (var group in _groupsScratch)
            {
                int count = group.Value.Count;
                float totalAffinity = 0f;
                foreach (Agent agent in group.Value)
                    totalAffinity += agent.affinity;

                _groupAffinityAverages[group.Key] = count > 0 ? totalAffinity / count : 0f;
                _groupSizes[group.Key] = count;
            }
        }

        private void EvaluateGroupProximityAndSwitches()
        {
            if (!ALLOW_GROUP_CHANGES)
                return;

            if (_groupsScratch == null || _groupsScratch.Count < 2)
                return;

            // snapshot the group lists into a flat array to iterate index-based without LINQ allocation
            int n = _groupsScratch.Count;
            var groupArr = new List<Agent>[n];
            int idx = 0;
            foreach (var kv in _groupsScratch)
                groupArr[idx++] = kv.Value;

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (AreGroupsNearby(groupArr[i], groupArr[j]))
                        EvaluateGroupSwaps(groupArr[i], groupArr[j]);
                }
            }
        }

        private bool AreGroupsNearby(List<Agent> group1, List<Agent> group2)
        {
            // squared thresholds to avoid sqrt in distance comparisons
            float proxSqr = GROUP_PROXIMITY_DISTANCE * GROUP_PROXIMITY_DISTANCE;
            float halfProxSqr = (GROUP_PROXIMITY_DISTANCE * 0.5f) * (GROUP_PROXIMITY_DISTANCE * 0.5f);

            bool anyWithinProx = false;
            int closePairs = 0;

            foreach (Agent agent1 in group1)
            {
                foreach (Agent agent2 in group2)
                {
                    float distSqr = (agent1.transform.position - agent2.transform.position).sqrMagnitude;

                    if (!anyWithinProx && distSqr <= proxSqr)
                        anyWithinProx = true;

                    if (distSqr <= halfProxSqr)
                    {
                        closePairs++;
                        if (anyWithinProx && closePairs >= 1)
                            return true; // early exit - reduced from 2 to 1
                    }
                }
            }

            return anyWithinProx && closePairs >= 2;
        }

        private void EvaluateGroupSwaps(List<Agent> group1, List<Agent> group2)
        {
            if (group1.Count == 0 || group2.Count == 0)
                return;

            int groupId1 = group1[0].groupId;
            int groupId2 = group2[0].groupId;

            float group1Affinity = _groupAffinityAverages.ContainsKey(groupId1) ? _groupAffinityAverages[groupId1] : 0f;
            float group2Affinity = _groupAffinityAverages.ContainsKey(groupId2) ? _groupAffinityAverages[groupId2] : 0f;

            // collect decisions in both directions first, then resolve to avoid reciprocal-swap oscillation
            if (_toSwitchToA == null) _toSwitchToA = new List<Agent>();
            if (_toSwitchToB == null) _toSwitchToB = new List<Agent>();
            _toSwitchToA.Clear(); // agents migrating to group2
            _toSwitchToB.Clear(); // agents migrating to group1

            foreach (Agent agent in group1)
            {
                if (ShouldAgentSwitchGroup(agent, group1Affinity, group2Affinity, groupId2))
                    _toSwitchToA.Add(agent);
            }

            foreach (Agent agent in group2)
            {
                if (ShouldAgentSwitchGroup(agent, group2Affinity, group1Affinity, groupId1))
                    _toSwitchToB.Add(agent);
            }

            // if both directions want to migrate, keep only the larger block; ties are broken
            // randomly to avoid systemic bias against lower-id groups (which always show up first
            // in the dictionary iteration and would otherwise always lose in ties).
            if (_toSwitchToA.Count > 0 && _toSwitchToB.Count > 0)
            {
                if (_toSwitchToA.Count > _toSwitchToB.Count)
                    _toSwitchToB.Clear();
                else if (_toSwitchToB.Count > _toSwitchToA.Count)
                    _toSwitchToA.Clear();
                else if (Random.value < 0.5f)
                    _toSwitchToB.Clear();
                else
                    _toSwitchToA.Clear();
            }

            for (int i = 0; i < _toSwitchToA.Count; i++)
            {
                _toSwitchToA[i].SwitchGroup(groupId2);
                _switchesThisCycle++;
            }
            for (int i = 0; i < _toSwitchToB.Count; i++)
            {
                _toSwitchToB[i].SwitchGroup(groupId1);
                _switchesThisCycle++;
            }
        }

        private bool ShouldAgentSwitchGroup(Agent agent, float currentGroupAffinity, float newGroupAffinity, int newGroupId)
        {
            // No grace period for existing group swaps - allow freely
            // calculate difference between agent's affinity and current group
            float currentDifference = Mathf.Abs(agent.affinity - currentGroupAffinity);
            
            // calculate difference between agent's affinity and new group
            float newDifference = Mathf.Abs(agent.affinity - newGroupAffinity);

            // switch if new group is BETTER (closer affinity) AND new group is compatible enough
            return newDifference < currentDifference && newDifference <= AFFINITY_SWITCH_THRESHOLD;
        }

        private void EvaluateSoloAgentsMeetings()
        {
            if (!ALLOW_GROUP_CHANGES)
                return;

            if (_soloScratch == null) _soloScratch = new List<Agent>();
            _soloScratch.Clear();
            foreach (Agent agent in _agents)
            {
                if (!agent.HasGroup)
                    _soloScratch.Add(agent);
            }
            List<Agent> soloAgents = _soloScratch;

            if (soloAgents.Count < 2)
                return; // need at least 2 solo agents to form a pair

            float proxSqr = GROUP_PROXIMITY_DISTANCE * GROUP_PROXIMITY_DISTANCE;

            // check all pairs of solo agents
            for (int i = 0; i < soloAgents.Count; i++)
            {
                Agent agent1 = soloAgents[i];
                if (agent1.HasGroup) continue; // already grouped earlier in this frame

                for (int j = i + 1; j < soloAgents.Count; j++)
                {
                    Agent agent2 = soloAgents[j];
                    if (agent2.HasGroup) continue;

                    if (agent1.timeSinceSpawn < GROUP_SWITCH_GRACE_PERIOD || agent2.timeSinceSpawn < GROUP_SWITCH_GRACE_PERIOD)
                        continue;

                    // squared distance to avoid sqrt
                    float distSqr = (agent1.transform.position - agent2.transform.position).sqrMagnitude;
                    if (distSqr <= proxSqr)
                    {
                        // check if their affinities are similar
                        float affinityDifference = Mathf.Abs(agent1.affinity - agent2.affinity);
                        if (affinityDifference <= AFFINITY_SWITCH_THRESHOLD)
                        {
                            // they should form a new group together
                            int newGroupId = _nextGroupId++;

                            // Elege líder ANTES de SwitchGroup — GetGroupLeader precisa encontrá-lo
                            Agent newLeader   = agent1.dominance >= agent2.dominance ? agent1 : agent2;
                            Agent newFollower = newLeader == agent1 ? agent2 : agent1;

                            newLeader.groupId       = newGroupId;
                            newLeader.isGroupLeader = true;
                            newLeader.ApplyGroupColor(); // registra a cor antes que o follower a busque

                            newFollower.SwitchGroup(newGroupId); // copia goals do líder corretamente
                            _newGroupsThisCycle++;
                    
                            break; // agent1 paired; move to next i
                        }
                    }
                }
            }
        }

        private void EvaluateSoloAgentsJoiningGroups()
        {
            if (!ALLOW_GROUP_CHANGES)
                return;

            // reuse the solo scratch populated by EvaluateSoloAgentsMeetings (still fresh: meetings
            // only switched a subset of these to a group; those skipped below via HasGroup).
            if (_soloScratch == null) _soloScratch = new List<Agent>();
            // re-filter in case meetings already moved some
            int kept = 0;
            for (int i = 0; i < _soloScratch.Count; i++)
            {
                if (!_soloScratch[i].HasGroup)
                {
                    _soloScratch[kept++] = _soloScratch[i];
                }
            }
            _soloScratch.RemoveRange(kept, _soloScratch.Count - kept);
            List<Agent> soloAgents = _soloScratch;

            if (soloAgents.Count == 0)
                return; // no solo agents to process

            // groups may have changed after EvaluateGroupSwaps/Meetings, rebuild from scratch using pool
            ClearGroupsScratch();
            foreach (Agent agent in _agents)
            {
                if (agent.HasGroup)
                {
                    List<Agent> bucket;
                    if (!_groupsScratch.TryGetValue(agent.groupId, out bucket))
                    {
                        bucket = GetPooledAgentList();
                        _groupsScratch[agent.groupId] = bucket;
                    }
                    bucket.Add(agent);
                }
            }
            var groups = _groupsScratch;

            if (groups.Count == 0)
                return; // no groups available to join

            float proxSqr = GROUP_PROXIMITY_DISTANCE * GROUP_PROXIMITY_DISTANCE;
            float halfProxSqr = (GROUP_PROXIMITY_DISTANCE * 0.5f) * (GROUP_PROXIMITY_DISTANCE * 0.5f);

            // for each solo agent, scan all candidate groups and pick the one with the smallest
            // affinity difference (within threshold). Avoids the prior "first match wins" bias
            // that always preferred the lowest-id group in the dictionary iteration order.
            foreach (Agent soloAgent in soloAgents)
            {
                // skip if agent already gained a group earlier this frame (via EvaluateSoloAgentsMeetings)
                if (soloAgent.HasGroup)
                    continue;

                if (soloAgent.timeSinceSpawn < GROUP_SWITCH_GRACE_PERIOD)
                    continue;

                int bestGroupId = -1;
                float bestDiff = float.MaxValue;

                foreach (var group in groups)
                {
                    // check if group is nearby - squared distances to avoid sqrt
                    List<Agent> groupAgents = group.Value;
                    bool anyWithinProx = false;
                    int closeAgents = 0;

                    foreach (Agent groupAgent in groupAgents)
                    {
                        float distSqr = (soloAgent.transform.position - groupAgent.transform.position).sqrMagnitude;
                        if (!anyWithinProx && distSqr <= proxSqr)
                            anyWithinProx = true;

                        if (distSqr <= halfProxSqr)
                            closeAgents++;
                    }

                    // solo agent can join group if:
                    // 1. At least one group agent is within proximity distance, AND
                    // 2. At least 2 group agents are within half proximity distance
                    if (anyWithinProx && closeAgents >= 2)
                    {
                        float groupAffinity = _groupAffinityAverages.ContainsKey(group.Key) ? _groupAffinityAverages[group.Key] : 0f;
                        float affinityDifference = Mathf.Abs(soloAgent.affinity - groupAffinity);

                        if (affinityDifference <= AFFINITY_SWITCH_THRESHOLD && affinityDifference < bestDiff)
                        {
                            bestDiff = affinityDifference;
                            bestGroupId = group.Key;
                        }
                    }
                }

                if (bestGroupId >= 0)
                {
                    soloAgent.SwitchGroup(bestGroupId);
                    _soloJoinsThisCycle++;
                }
            }
        }
    }
}