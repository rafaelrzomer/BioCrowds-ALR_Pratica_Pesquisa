/// ---------------------------------------------
/// Contact: Henry Braun
/// Brief: Defines an Agent
/// Thanks to VHLab for original implementation
/// Date: November 2017
/// ---------------------------------------------
/// Group Dynamics extension:
/// Affinity, GroupId, Cohesion and Leader logic
/// Added: 2026 - Pratica em Pesquisa PUCRS
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

        // ── MOVIMENTO ──────────────────────────────────────────────────────

        public float agentRadius;
        public Vector3 _velocity;

        [SerializeField]
        private float _maxSpeed = 1.5f;

        // ── OBJETIVOS ──────────────────────────────────────────────────────

        public GameObject Goal;
        public List<GameObject> goalsList;
        public List<float> goalsWaitList;
        public bool isWaiting = false;

        [SerializeField]
        private float waitCount = 0f;

        [SerializeField]
        private int goalIndex = 0;
        public bool removeWhenGoalReached;
        public float goalDistThreshold = 1.0f;

        // ── PARÂMETROS DE GRUPO ────────────────────────────────────────────

        // Dominância: quem tem maior valor se torna líder do grupo
        [Range(0f, 1f)]
        public float dominance = 1.0f;

        // Afinidade: agentes com valores próximos são mais compatíveis
        [Range(0f, 1f)]
        public float affinity = 0f;

        // Identificador do grupo. -1 = agente sem grupo
        public int groupId = -1;

        // Retorna true se o agente pertence a algum grupo
        public bool HasGroup => groupId >= 0;

        // Indica se este agente é o líder do seu grupo
        public bool isGroupLeader = false;

        // Força de atração do agente em direção ao centróide do grupo
        [Range(0f, 1f)]
        public float groupCohesionStrength = 0.3f;

        // ── AUXINAS E CÉLULAS ──────────────────────────────────────────────

        [SerializeField]
        private List<Auxin> _auxins = new List<Auxin>();
        public List<Auxin> Auxins
        {
            get { return _auxins; }
            set { _auxins = value; }
        }

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

        private int _totalX;
        private int _totalZ;

        private NavMeshPath _navMeshPath;
        public VisualAgent _visualAgent;

        private float _elapsedTime;
        public List<Vector3> _distAuxin;

        // ── PARAVISI'S MODEL ───────────────────────────────────────────────
        private bool _isDenW = false;
        private float _denW;
        private Vector3 _rotation;
        private Vector3 _goalPosition;
        private Vector3 _dirAgentGoal;

        public int auxinCount;

        // ── DEBUG VISUAL ───────────────────────────────────────────────────
        // Coloração por grupo para facilitar visualização durante testes.
        // Para desativar, mude SHOW_GROUP_COLORS para false.
        private const bool SHOW_GROUP_COLORS = true;
        private Renderer _cachedRenderer;

        // ──────────────────────────────────────────────────────────────────

        void Start()
        {
            _navMeshPath = new NavMeshPath();

            _visualAgent ??= GetComponentInChildren<VisualAgent>();

            _goalPosition = Goal.transform.position;
            _dirAgentGoal = _goalPosition - transform.position;

            _visualAgent?.Initialize(transform.position, this);

            _totalX = Mathf.FloorToInt(_world.Dimension.x / 2.0f) - 1;
            _totalZ = Mathf.FloorToInt(_world.Dimension.y / 2.0f);

            if (SHOW_GROUP_COLORS)
                _cachedRenderer = GetComponentInChildren<Renderer>();
        }

        // Chamado pelo World a cada frame para atualizar a cor do agente
        public void UpdateGroupColor()
        {
            if (!SHOW_GROUP_COLORS || _cachedRenderer == null) return;

            _cachedRenderer.material.color = groupId switch
            {
                -1 => Color.white,
                0 => Color.red,
                1 => Color.blue,
                2 => Color.green,
                3 => Color.yellow,
                _ => Color.magenta,
            };
        }

        // ── NAVMESH ────────────────────────────────────────────────────────

        public void NavmeshStep(float _timeStep)
        {
            ClearAgent();
            _elapsedTime += _timeStep;

            if (_elapsedTime > UPDATE_NAVMESH_INTERVAL)
                UpdateGoalPositionAndNavmesh();

            if (_navMeshPath != null && SceneController.ShowNavMeshCorners)
            {
                for (int i = 0; i < _navMeshPath.corners.Length - 1; i++)
                    Debug.DrawLine(_navMeshPath.corners[i], _navMeshPath.corners[i + 1], Color.red);
            }
        }

        private void UpdateGoalPositionAndNavmesh()
        {
            if (goalIndex >= goalsList.Count) return;

            _elapsedTime = 0.0f;

            bool foundPath = NavMesh.CalculatePath(
                transform.position,
                goalsList[goalIndex].transform.position,
                NavMesh.AllAreas,
                _navMeshPath
            );

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

        // ── LIMPEZA DE FRAME ───────────────────────────────────────────────

        void ClearAgent()
        {
            _denW = 0;
            _distAuxin.Clear();
            _isDenW = false;
            _rotation = new Vector3(0f, 0f, 0f);
            _dirAgentGoal = _goalPosition - transform.position;
        }

        // ── MOVIMENTO ──────────────────────────────────────────────────────

        public void MovementStep(float _timeStep)
        {
            if (_velocity.sqrMagnitude > 0.0f)
                transform.Translate(_velocity * _timeStep, Space.World);
        }

        public void WaitStep(float _timeStep)
        {
            if (goalIndex != goalsWaitList.Count - 1 && goalIndex + 1 > goalsWaitList.Count)
                return;

            if (isWaiting)
            {
                waitCount += _timeStep;
                if (waitCount >= goalsWaitList[goalIndex])
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

        // ── CÁLCULO DE DIREÇÃO (Paravisi) ──────────────────────────────────

        public void CalculateDirection()
        {
            for (int k = 0; k < _distAuxin.Count; k++)
            {
                float valorW = CalculaW(k);
                if (_denW < 0.0001f) valorW = 0.0f;
                _rotation += valorW * _distAuxin[k] * _maxSpeed;
            }
        }

        float CalculaW(int indiceRelacao)
        {
            float fVal = GetF(indiceRelacao);

            if (!_isDenW)
            {
                _denW = 0f;
                for (int k = 0; k < _distAuxin.Count; k++)
                    _denW += GetF(k);
                _isDenW = true;
            }

            return fVal / _denW;
        }

        float GetF(int pRelationIndex)
        {
            float Ymodule = Vector3.Distance(_distAuxin[pRelationIndex], Vector3.zero);
            float Xmodule = _dirAgentGoal.normalized.magnitude;
            float dot     = Vector3.Dot(_distAuxin[pRelationIndex], _dirAgentGoal.normalized);

            if (Ymodule < 0.00001f) return 0.0f;

            return (float)((1.0 / (1.0 + Ymodule)) * (1.0 + ((dot) / (Xmodule * Ymodule))));
        }

        public void CalculateVelocity()
        {
            float moduleM = Vector3.Distance(_rotation, Vector3.zero);
            float s       = moduleM * Mathf.PI;

            if (s > _maxSpeed) s = _maxSpeed;

            if (moduleM > 0.0001f)
                _velocity = s * (_rotation / moduleM);
            else
                _velocity = Vector3.zero;
        }

        // ── AUXINAS ────────────────────────────────────────────────────────

        public void FindNearAuxins()
        {
            _auxins.Clear();
            List<Auxin> cellAuxins = _currentCell.Auxins;

            for (int i = 0; i < cellAuxins.Count; i++)
            {
                float distanceSqr = (transform.position - cellAuxins[i].Position).sqrMagnitude;
                if (distanceSqr < cellAuxins[i].MinDistance && distanceSqr <= agentRadius * agentRadius)
                {
                    if (cellAuxins[i].IsTaken)
                        cellAuxins[i].Agent.Auxins.Remove(cellAuxins[i]);

                    cellAuxins[i].IsTaken     = true;
                    cellAuxins[i].Agent       = this;
                    cellAuxins[i].MinDistance = distanceSqr;
                    _auxins.Add(cellAuxins[i]);
                }
            }

            FindCell();
        }

        private void FindCell()
        {
            float distanceToCellSqr = (transform.position - _currentCell.transform.position).sqrMagnitude;

            if (_currentCell.X > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + _currentCell.Z + 0]);
            if (_currentCell.X > 0 && _currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + _currentCell.Z + 1]);
            if (_currentCell.X > 0 && _currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X - 1) * _totalZ + (_currentCell.Z - 1)]);
            if (_currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 0) * _totalZ + _currentCell.Z + 1]);
            if (_currentCell.X < _totalX && _currentCell.Z < _totalZ - 1)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + _currentCell.Z + 1]);
            if (_currentCell.X < _totalX)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + _currentCell.Z + 0]);
            if (_currentCell.X < _totalX && _currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 1) * _totalZ + (_currentCell.Z - 1)]);
            if (_currentCell.Z > 0)
                CheckAuxins(ref distanceToCellSqr, _world.Cells[(_currentCell.X + 0) * _totalZ + (_currentCell.Z - 1)]);
        }

        private void CheckAuxins(ref float pDistToCellSqr, Cell pCell)
        {
            List<Auxin> cellAuxins = pCell.Auxins;

            for (int c = 0; c < cellAuxins.Count; c++)
            {
                float distanceSqr = (transform.position - cellAuxins[c].Position).sqrMagnitude;
                if (distanceSqr < cellAuxins[c].MinDistance && distanceSqr <= agentRadius * agentRadius)
                {
                    if (cellAuxins[c].IsTaken)
                        cellAuxins[c].Agent.Auxins.Remove(cellAuxins[c]);

                    cellAuxins[c].IsTaken     = true;
                    cellAuxins[c].Agent       = this;
                    cellAuxins[c].MinDistance = distanceSqr;
                    _auxins.Add(cellAuxins[c]);
                }
            }

            float distanceToNeighbourCell = (transform.position - pCell.transform.position).sqrMagnitude;
            if (distanceToNeighbourCell < pDistToCellSqr)
            {
                pDistToCellSqr = distanceToNeighbourCell;
                _currentCell   = pCell;
            }
        }

        // ── OBJETIVOS ──────────────────────────────────────────────────────

        public bool IsAtCurrentGoal()
        {
            Vector2 agentPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 goalPos  = new Vector2(
                goalsList[goalIndex].transform.position.x,
                goalsList[goalIndex].transform.position.z
            );
            return Vector2.Distance(agentPos, goalPos) <= goalDistThreshold;
        }

        public bool IsAtFinalGoal()
        {
            Vector2 agentPos = new Vector2(transform.position.x, transform.position.z);
            Vector2 goalPos  = new Vector2(
                goalsList[goalsList.Count - 1].transform.position.x,
                goalsList[goalsList.Count - 1].transform.position.z
            );
            return Vector2.Distance(agentPos, goalPos) <= goalDistThreshold;
        }
    }
}