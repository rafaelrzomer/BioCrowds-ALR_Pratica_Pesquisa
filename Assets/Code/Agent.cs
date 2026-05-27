/// ---------------------------------------------
/// Contact: Henry Braun
/// Brief: Defines an Agent
/// Thanks to VHLab for original implementation
/// Date: November 2017 
/// ---------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Biocrowds.Core
{
    public class Agent : MonoBehaviour
    {
        private const float UPDATE_NAVMESH_INTERVAL = 1.0f;

        //agent radius
        public float agentRadius;
        //agent speed
        public Vector3 _velocity;
        //max speed
        [SerializeField]
        private float _maxSpeed = 1.5f;

        //dominance parameter
        [Range(0f, 1f)]
        public float dominance = 1.0f;

        // affinity parameter: agents with closer values are more compatible
        [Range(0f, 1f)]
        public float affinity = 0f;

        // group membership: -1 means no group
        public int groupId = -1;
        public bool HasGroup => groupId >= 0;

        // time since spawn: used to prevent immediate group switching after creation.
        // Starts at 0; World increments it each step and the grace period in World checks
        // timeSinceSpawn < GROUP_SWITCH_GRACE_PERIOD. The previous default of 2f bypassed
        // the grace period entirely for newly spawned agents.
        public float timeSinceSpawn = 0f;

        // group cohesion strength
        [Range(0f, 1f)]
        public float groupCohesionStrength = 0.3f;

        // is this agent the leader of its group?
        public bool isGroupLeader = false;

        [SerializeField] private float leaderSyncRadius = 6f; // multiplicador do agentRadius

        //goal
        public GameObject Goal;
        // Multiple goals
        public List<GameObject> goalsList;
        public List<float> goalsWaitList;
        public bool isWaiting = false;
        [SerializeField]
        private float waitCount = 0f;

        [SerializeField]
        private int goalIndex = 0;
        public int CurrentGoalIndex => goalIndex; // public getter to access current goal index
        public bool removeWhenGoalReached;

        public float goalDistThreshold = 30.0f;

        //list with all auxins in his personal space
        [SerializeField]
        private List<Auxin> _auxins = new List<Auxin>();
        public List<Auxin> Auxins
        {
            get { return _auxins; }
            set { _auxins = value; }
        }

        //agent cell
        [SerializeField]
        private Cell _currentCell;
        public Cell CurrentCell
        {
            get { return _currentCell; }
            set { _currentCell = value; }
        }

        private World _world;
        public World World
        {
            get { return _world; }
            set { _world = value; }
        }

        // get the average affinity of the agent's group
        public float GroupAverageAffinity
        {
            get
            {
                if (!HasGroup || _world == null)
                    return affinity; // return individual affinity if not in a group
                
                if (_world.GroupAffinityAverages.ContainsKey(groupId))
                    return _world.GroupAffinityAverages[groupId];
                
                return 0f;
            }
        }

        private int _totalX;
        private int _totalZ;

        private NavMeshPath _navMeshPath;

        public VisualAgent _visualAgent;

        //time elapsed (to calculate path just between an interval of time)
        private float _elapsedTime;
        //auxins distance vector from agent
        public List<Vector3> _distAuxin;

        // group cohesion: list of nearby group members
        public List<Agent> _nearbyGroupMembers = new List<Agent>();

        /*-----------Paravisis' model-----------*/
        private bool _isDenW = false; //  avoid recalculation
        private float _denW;    //  avoid recalculation
        private Vector3 _rotation; //orientation vector (movement)
        private Vector3 _goalPosition; //goal position
        private Vector3 _dirAgentGoal; //diff between goal and agent
        private Vector3 _effectiveGoalDir; //goal direction blended with leader direction (group cohesion via weight modulation)

        public int auxinCount;

        void Start()
        {
            _navMeshPath = new NavMeshPath();
            if (_visualAgent == null) _visualAgent = GetComponentInChildren<VisualAgent>();

            // dominance/affinity are assigned by the spawner (World.SpawnNewAgentInArea)
            // before Start() runs on the next frame. Do NOT randomize here or those values
            // get clobbered (e.g. SpawnArea.affinityMin/Max would be ignored).

            _goalPosition = Goal.transform.position;
            _dirAgentGoal = _goalPosition - transform.position;
            if (_visualAgent != null) _visualAgent.Initialize(transform.position, this);
            //cache world info
            _totalX = Mathf.FloorToInt(_world.Dimension.x / 2.0f) - 1;
            _totalZ = Mathf.FloorToInt(_world.Dimension.y / 2.0f);
        }

        public void NavmeshStep(float _timeStep)
        {
            //clear agent´s information
            ClearAgent();
            
            // Update the way to the goal every second.
            _elapsedTime += _timeStep;

            if (_elapsedTime > UPDATE_NAVMESH_INTERVAL)
            {
                UpdateGoalPositionAndNavmesh();
            }

            //draw line to goal
            if (_navMeshPath != null && SceneController.ShowNavMeshCorners)
            {
                for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
                    Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.red);
            }
        }

        /*void Update()
        {
            //clear agent´s information
            ClearAgent();

            // Update the way to the goal every second.
            _elapsedTime += 0.02f;

            if (_elapsedTime > UPDATE_NAVMESH_INTERVAL)
            {
                UpdateGoalPositionAndNavmesh();
            }

            //draw line to goal
            for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
                Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.red);
        }*/

        private void UpdateGoalPositionAndNavmesh()
        {
            if (goalIndex >= goalsList.Count)
                return;

            _elapsedTime = 0.0f;

            //calculate agent path
            //bool foundPath = NavMesh.CalculatePath(transform.position, Goal.transform.position, NavMesh.AllAreas, _navMeshPath);
            bool foundPath = NavMesh.CalculatePath(transform.position, goalsList[goalIndex].transform.position,
                NavMesh.AllAreas, _navMeshPath);
            //update its goal if path is found
            if (foundPath)
            {
                _goalPosition = new Vector3(_navMeshPath.corners[1].x, 0f, _navMeshPath.corners[1].z);
                _dirAgentGoal = _goalPosition - transform.position;
            }
            else
            {
                _goalPosition = goalsList[goalIndex].transform.position;
                _dirAgentGoal = _goalPosition - transform.position;
            }
        }

        public void UpdateVisualAgent()
        {
            if (_visualAgent != null) _visualAgent.Step();
        }

        //clear agent´s informations
        void ClearAgent()
        {
            //re-set inicial values
            _denW = 0;
            _distAuxin.Clear();
            _nearbyGroupMembers.Clear();
            _isDenW = false;
            _rotation = new Vector3(0f, 0f, 0f);
            _dirAgentGoal = _goalPosition - transform.position;
        }

        //walk
        public void MovementStep(float _timeStep)
        {
            if (_velocity.sqrMagnitude > 0.0f)
                transform.Translate(_velocity * _timeStep, Space.World);
        }

        public void WaitStep(float _timeStep)
        {
            if (HasGroup && !isGroupLeader && _world != null)
            {
                Agent leader = _world.GetGroupLeader(groupId);

                if (leader != null)
                {
                    float distToLeaderSqr = (transform.position - leader.transform.position).sqrMagnitude;
                    float syncRadius = agentRadius * leaderSyncRadius;

                    if (distToLeaderSqr <= syncRadius * syncRadius)
                    {
                        // perto do líder — sincroniza goal
                        if (leader.CurrentGoalIndex != goalIndex)
                        {
                            goalIndex = Mathf.Clamp(leader.CurrentGoalIndex, 0, goalsList.Count - 1);
                            Goal      = goalsList[goalIndex];
                            isWaiting = leader.isWaiting;
                            waitCount = 0f;
                            UpdateGoalPositionAndNavmesh();
                        }
                    }
                    else
                    {
                        // longe do líder — ignora goal próprio e vai em direção ao líder
                        _goalPosition = leader.transform.position;
                        _dirAgentGoal = _goalPosition - transform.position;
                    }

                    return;
                }

                // líder morreu — age independente até novo líder ser eleito
            }

            // líder, solo, ou follower sem líder: gerencia goals próprios
            if (goalIndex != goalsWaitList.Count - 1 && goalIndex + 1 > goalsWaitList.Count)
                return;

            if (isWaiting)
            {
                waitCount += _timeStep;
                float mult = (_world != null) ? _world.WaitTimeMultiplier : 1.0f;
                if (waitCount >= goalsWaitList[goalIndex] * mult)
                {
                    isWaiting = false;
                    goalIndex++;
                    UpdateGoalPositionAndNavmesh();
                }
            }
            else if (IsAtCurrentGoal() && goalIndex < goalsList.Count - 1)
            {
                if (goalsWaitList[goalIndex] >= 0.1f)
                {
                    waitCount = 0.0f;
                    isWaiting = true;
                }
                else
                {
                    waitCount = 0.0f;
                    goalIndex++;
                    UpdateGoalPositionAndNavmesh();
                }
            }
        }
    

        //The calculation formula starts here
        //the ideia is to find m=SUM[k=1 to n](Wk*Dk)
        //where k iterates between 1 and n (number of auxins), Dk is the vector to the k auxin and Wk is the weight of k auxin
        //the weight (Wk) is based on the degree resulting between the goal vector and the auxin vector (Dk), and the
        //distance of the auxin from the agent
        public void CalculateDirection()
        {
            // build the effective goal direction. for non-leader followers, blend the real goal direction
            // with the direction toward the group leader. cohesion then modulates auxin weights through
            // GetF (Bicho et al. formula), instead of adding an external force. This preserves the
            // collision-free guarantee of BioCrowds because movement is still a weighted sum of vectors
            // toward auxins the agent owns.
            Vector3 goalDir = _dirAgentGoal.sqrMagnitude > 0.0001f ? _dirAgentGoal.normalized : Vector3.zero;
            _effectiveGoalDir = goalDir;

            if (HasGroup && !isGroupLeader && _nearbyGroupMembers.Count > 0)
            {
                Agent leader = null;
                foreach (Agent groupMember in _nearbyGroupMembers)
                {
                    if (groupMember.isGroupLeader)
                    {
                        leader = groupMember;
                        break;
                    }
                }

                if (leader != null)
                {
                    Vector3 toLeader = leader.transform.position - transform.position;
                    float distSqr = toLeader.sqrMagnitude;
                    float minDist = agentRadius * 1.5f;
                    if (distSqr > minDist * minDist && goalDir.sqrMagnitude > 0.0001f)
                    {
                        Vector3 leaderDir = toLeader / Mathf.Sqrt(distSqr);

                        // scale cohesion by 1/sqrt(groupSize): larger groups create less per-agent
                        // pull toward the leader, preventing density jams where many followers fight
                        // for the same auxins along the leader's path.
                        int groupSize = _world != null ? _world.GetGroupSize(groupId) : 1;
                        float sizeScale = groupSize > 1 ? 1f / Mathf.Sqrt(groupSize) : 1f;
                        float effectiveCohesion = groupCohesionStrength * sizeScale;

                        Vector3 blended = (1f - effectiveCohesion) * goalDir + effectiveCohesion * leaderDir;
                        if (blended.sqrMagnitude > 0.0001f)
                            _effectiveGoalDir = blended.normalized;
                    }
                }
            }

            //for each agent´s auxin
            for (int k = 0; k < _distAuxin.Count; k++)
            {
                //calculate W
                float valorW = CalculaW(k);
                if (_denW < 0.0001f)
                    valorW = 0.0f;

                //sum the resulting vector * weight (Wk*Dk)
                _rotation += valorW * _distAuxin[k] * _maxSpeed;
            }
            
            // Local avoidance: repel from nearby agents to break queue formation
            if (_world != null)
            {
                Vector3 repulsionForce = Vector3.zero;
                int repelCount = 0;
                
                float avoidanceRadius = agentRadius * 3.0f;
                float avoidanceRadiusSqr = avoidanceRadius * avoidanceRadius;
                
                foreach (Agent other in _world.Agents)
                {
                    if (other == this) continue;
                    
                    Vector3 toOther = other.transform.position - transform.position;
                    float distSqr = toOther.sqrMagnitude;
                    
                    if (distSqr < avoidanceRadiusSqr && distSqr > 0.0001f)
                    {
                        float dist = Mathf.Sqrt(distSqr);
                        Vector3 repel = -toOther / dist;
                        float strength = 1.0f - (dist / avoidanceRadius);
                        strength = strength * strength;
                        repulsionForce += repel * strength;
                        repelCount++;
                    }
                }
                
                if (repelCount > 0)
                {
                    float dominanceFactor = 0.5f + (dominance * 0.5f);
                    repulsionForce *= (dominanceFactor / repelCount);
                    float repulsionWeight = 0.2f;
                    _rotation = (1.0f - repulsionWeight) * _rotation.normalized + repulsionWeight * repulsionForce;
                }
            }
        }


        //calculate W
        float CalculaW(int indiceRelacao)
        {
            //calculate F (F is part of weight formula)
            float fVal = GetF(indiceRelacao);

            if (!_isDenW)
            {
                _denW = 0f;

                //for each agent´s auxin
                for (int k = 0; k < _distAuxin.Count; k++)
                {
                    //calculate F for this k index, and sum up
                    _denW += GetF(k);
                }
                _isDenW = true;
            }

            return fVal / _denW;
        }

        //calculate F (F is part of weight formula)
        float GetF(int pRelationIndex)
        {
            //distance between auxin´s distance and origin
            float Ymodule = Vector3.Distance(_distAuxin[pRelationIndex], Vector3.zero);
            //distance between effective goal vector and origin (already normalized in CalculateDirection)
            float Xmodule = _effectiveGoalDir.magnitude;

            float dot = Vector3.Dot(_distAuxin[pRelationIndex], _effectiveGoalDir);

            if (Ymodule < 0.00001f)
                return 0.0f;

            // if no effective direction (e.g. agent already at goal), fall back to pure distance weighting
            if (Xmodule < 0.00001f)
                return (float)(1.0 / (1.0 + Ymodule));

            //return the formula, defined in thesis
            float baseF = (float)((1.0 / (1.0 + Ymodule)) * (1.0 + ((dot) / (Xmodule * Ymodule))));
            
            // Apply personality modulation:
            // - High dominance: more aggressive, prefers closer auxins (higher weights)
            // - Low affinity: more exploratory, adds randomness to escape follow patterns
            float personalityMod = 1.0f;
            
            // Dominance increases weight on nearby auxins (encourages closer paths)
            personalityMod *= (1.0f + dominance * 0.5f);
            
            // Low affinity adds random variation to avoid deterministic following
            if (affinity < 0.5f)
            {
                float randomFactor = 0.8f + Random.Range(0f, 0.4f) * (1.0f - affinity);
                personalityMod *= randomFactor;
            }
            
            return baseF * personalityMod;
        }

        //calculate speed vector    
        public void CalculateVelocity()
        {
            //distance between movement vector and origin
            float moduleM = Vector3.Distance(_rotation, Vector3.zero);

            //multiply for PI
            float s = moduleM * Mathf.PI;
            
            // Apply personality to speed:
            // - High dominance: moves faster (more aggressive)
            // - Low affinity: variable speed (more unpredictable)
            float speedModifier = 0.7f + (dominance * 0.3f); // dominance from 0.7x to 1.0x
            speedModifier *= (0.9f + Random.Range(0f, 0.2f) * (1.0f - affinity)); // affinity adds stability

            //if it is bigger than maxSpeed, use maxSpeed instead
            if (s > _maxSpeed * speedModifier)
                s = _maxSpeed * speedModifier;

            //Debug.Log("vetor M: " + m + " -- modulo M: " + s);
            if (moduleM > 0.0001f)
            {
                //calculate speed vector
                _velocity = s * (_rotation / moduleM);
            }
            else
            {
                //else, go idle
                _velocity = Vector3.zero;
            }
        }

        //find all auxins near him (Voronoi Diagram)
        //call this method from game controller, to make it sequential for each agent
        public void FindNearAuxins()
        {
            //clear them all, for obvious reasons
            _auxins.Clear();

            //get all auxins on my cell
            List<Auxin> cellAuxins = _currentCell.Auxins;

            //iterate all cell auxins to check distance between auxins and agent
            for (int i = 0; i < cellAuxins.Count; i++)
            {
                //see if the distance between this agent and this auxin is smaller than the actual value, and inside agent radius
                float distanceSqr = (transform.position - cellAuxins[i].Position).sqrMagnitude;
                if (distanceSqr < cellAuxins[i].MinDistance && distanceSqr <= agentRadius * agentRadius)
                {
                    //take the auxin!
                    //if this auxin already was taken, need to remove it from the agent who had it
                    if (cellAuxins[i].IsTaken)
                        cellAuxins[i].Agent.Auxins.Remove(cellAuxins[i]);

                    //auxin is taken
                    cellAuxins[i].IsTaken = true;

                    //auxin has agent
                    cellAuxins[i].Agent = this;
                    //update min distance
                    cellAuxins[i].MinDistance = distanceSqr;
                    //update my auxins
                    _auxins.Add(cellAuxins[i]);
                }
            }

            FindCell();
        }

        private void FindCell()
        {
            //distance from agent to cell, to define agent new cell
            float distanceToCellSqr = (transform.position - _currentCell.transform.position).sqrMagnitude; //Vector3.Distance(transform.position, _currentCell.transform.position);

            //cap the limits
            //[ ][ ][ ]
            //[ ][X][ ]
            //[ ][ ][ ]
            if (_currentCell.X > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z + 0)]);

            if (_currentCell.X > 0 && _currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z + 1)]);

            if (_currentCell.X > 0 && _currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z - 1)]);

            if (_currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 0) * _totalZ + (_currentCell.Z + 1)]);

            if (_currentCell.X < _totalX && _currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z + 1)]);

            if (_currentCell.X < _totalX)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z + 0)]);

            if (_currentCell.X < _totalX && _currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z - 1)]);

            if (_currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 0) * _totalZ + (_currentCell.Z - 1)]);

        }

        private void CheckAuxins(ref float pDistToCellSqr, Cell pCell)
        {
            //get all auxins on neighbourcell
            List<Auxin> cellAuxins = pCell.Auxins;

            //iterate all cell auxins to check distance between auxins and agent
            for (int c = 0; c < cellAuxins.Count; c++)
            {
                //see if the distance between this agent and this auxin is smaller than the actual value, and smaller than agent radius
                float distanceSqr = (transform.position - cellAuxins[c].Position).sqrMagnitude;
                if (distanceSqr < cellAuxins[c].MinDistance && distanceSqr <= agentRadius * agentRadius)
                {
                    //take the auxin
                    //if this auxin already was taken, need to remove it from the agent who had it
                    if (cellAuxins[c].IsTaken)
                        cellAuxins[c].Agent.Auxins.Remove(cellAuxins[c]);

                    //auxin is taken
                    cellAuxins[c].IsTaken = true;
                    //auxin has agent
                    cellAuxins[c].Agent = this;
                    //update min distance
                    cellAuxins[c].MinDistance = distanceSqr;
                    //update my auxins
                    _auxins.Add(cellAuxins[c]);
                }
            }

            //see distance to this cell
            float distanceToNeighbourCell = (transform.position - pCell.transform.position).sqrMagnitude; 
            if (distanceToNeighbourCell < pDistToCellSqr)
            {
                pDistToCellSqr = distanceToNeighbourCell;
                _currentCell = pCell;
            }
        }
        public bool IsAtCurrentGoal()
        {
            //Debug.Log(name + " : " + Vector3.Distance(transform.position, _goalPosition));
            Vector2 agentPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 goalPos = new Vector2(goalsList[goalIndex].transform.position.x,
                goalsList[goalIndex].transform.position.z);
            return (Vector2.Distance(agentPos, goalPos) <= goalDistThreshold);
        }

        public bool IsAtFinalGoal()
        {
            //Debug.Log(name + " : " + Vector3.Distance(transform.position, goalsList[goalsList.Count - 1].transform.position));
            Vector2 agentPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 goalPos = new Vector2(goalsList[goalsList.Count - 1].transform.position.x,
                goalsList[goalsList.Count - 1].transform.position.z);
            return (Vector2.Distance(agentPos, goalPos) <= goalDistThreshold);
        }

        //find nearby agents from the same group
        public void FindNearbyGroupMembers(List<Agent> allAgents)
        {
            if (!HasGroup) return;

            _nearbyGroupMembers.Clear();

            foreach (Agent otherAgent in allAgents)
            {
                if (otherAgent == this) continue;
                if (otherAgent.groupId != groupId) continue;

                float distanceSqr = (transform.position - otherAgent.transform.position).sqrMagnitude;
                float detectionRadius = otherAgent.isGroupLeader ? agentRadius * agentRadius * 16f : agentRadius * agentRadius * 9f; // increased for better group spacing

                if (distanceSqr <= detectionRadius)
                {
                    _nearbyGroupMembers.Add(otherAgent);
                }
            }
        }

        /// <summary>
        /// Switch this agent to a different group
        /// </summary>
        public void SwitchGroup(int newGroupId)
        {
            if (newGroupId == groupId)
                return; // already in the target group

            int oldGroupId = groupId;
            groupId = newGroupId;
            isGroupLeader = false; // reset leader status when switching groups
            _nearbyGroupMembers.Clear(); // clear nearby members list

            // Notifica GroupManager (se presente).
            if (GroupManager.Instance != null)
                GroupManager.Instance.MoveAgent(oldGroupId, newGroupId, this);

            // Get the leader of the new group and copy their goals
            if (_world != null)
            {
                Agent newGroupLeader = _world.GetGroupLeader(newGroupId);
                if (newGroupLeader != null && newGroupLeader.goalsList != null && newGroupLeader.goalsList.Count > 0)
                {
                    // Copy the leader's goal list
                    goalsList = new List<GameObject>(newGroupLeader.goalsList);
                    
                    // Copy the leader's wait list if available
                    if (newGroupLeader.goalsWaitList != null && newGroupLeader.goalsWaitList.Count > 0)
                        goalsWaitList = new List<float>(newGroupLeader.goalsWaitList);
                    
                    // Start from where the leader is in the goal list (follow the leader's progress)
                    goalIndex = Mathf.Clamp(newGroupLeader.CurrentGoalIndex, 0, goalsList.Count - 1);
                    Goal      = goalsList[goalIndex];
                    isWaiting = newGroupLeader.isWaiting; // sincroniza estado de espera
                    waitCount = 0f;
                    
                    // Update navigation mesh for the new goal
                    UpdateGoalPositionAndNavmesh();
                }
            }
            
            // Aplicar cor do novo grupo
            ApplyGroupColor();
        }

        /// <summary>
        /// Aplica a cor correspondente ao grupo do agente, destacando o líder visualmente.
        /// </summary>
        public void ApplyGroupColor()
        {
            if (_visualAgent == null) return;

            if (GroupColorManager.Instance != null)
            {
                Color groupColor = GroupColorManager.Instance.GetGroupColor(groupId);
                _visualAgent.ApplyGroupColor(groupColor, isGroupLeader);
            }
        }
    }
}