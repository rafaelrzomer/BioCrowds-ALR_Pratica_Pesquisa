using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Biocrowds.Core
{
public class SpawnArea : MonoBehaviour
{
    private Collider      _collider;
    private MeshRenderer  _meshRenderer;

    // ── GRUPO ──────────────────────────────────────────────────────────────
    [Header("Spawn Area Group")]
    // Identificador do grupo que esta área vai spawnar.
    // -1 = agentes sem grupo (vão procurar um grupo por afinidade)
    public int groupId = -1;

    // Agentes do mesmo grupo nascem com afinidade próxima da média, com
    // pequena variação. Reproduz a hipótese do paper Musse & Thalmann (1997)
    // de que membros de um grupo são socialmente similares.
    [Header("Group Affinity")]
    [Tooltip("Média de afinidade dos agentes spawnados nesta área.")]
    [Range(0f, 1f)]   public float groupAffinityMean   = 0.5f;
    [Tooltip("Variação máxima em torno da média (uniform ±spread).")]
    [Range(0f, 0.5f)] public float groupAffinitySpread = 0.1f;

    // ── SPAWN INICIAL ──────────────────────────────────────────────────────
    [Header("Initial Spawner Settings")]
    public int              initialNumberOfAgents;
    public bool             initialRemoveWhenGoalReached;
    public List<GameObject> initialAgentsGoalList;
    public List<float>      initialWaitList;

    // ── SPAWN CÍCLICO ──────────────────────────────────────────────────────
    [Header("Repeating Spawner Settings")]
    [FormerlySerializedAs("cycleLenght")]
    public float            cycleLength = 1.0f;
    public int              quantitySpawnedEachCycle;
    public bool             repeatingRemoveWhenGoalReached;
    public List<GameObject> repeatingGoalList;
    public List<float>      repeatingWaitList;

    private float cycleCounter = 0.0f;
    private bool  cycleReady   = false;

    public bool CycleReady { get => cycleReady; }

    // ──────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();
        if (_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();

        cycleCounter = 0.0f;
        cycleReady   = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Debug.Log(GetRandomPoint());
    }

    public void UpdateSpawnCounter(float dt)
    {
        if (cycleLength == 0.0f || quantitySpawnedEachCycle == 0) return;

        cycleCounter += dt;
        if (cycleCounter >= cycleLength)
        {
            cycleCounter -= cycleLength;
            cycleReady    = true;
        }
    }

    public void ResetCycleReady()
    {
        cycleReady = false;
    }

    public Vector3 GetRandomPoint(float height = 0.0f)
    {
        Vector3 point = new Vector3(
            Random.Range(_collider.bounds.min.x, _collider.bounds.max.x),
            height,
            Random.Range(_collider.bounds.min.z, _collider.bounds.max.z)
        );
        return _collider.ClosestPoint(point);
    }

    public void ShowMesh(bool _show)
    {
        _meshRenderer.enabled = _show;
    }
}
}