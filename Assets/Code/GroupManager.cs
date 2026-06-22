using System.Collections.Generic;
using UnityEngine;

namespace Biocrowds.Core
{
    /// <summary>
    /// Registro central de grupos. Mantém uma lista serializada (visível no Inspector)
    /// e um Dictionary interno para lookup O(1) por groupId.
    /// Não decide proximidade nem trocas — apenas armazena membros, líder e goals do grupo.
    /// </summary>
    public class GroupManager : MonoBehaviour
    {
        public static GroupManager Instance { get; private set; }

        // serializada para Inspector; espelha _groupsById
        [SerializeField] private List<Group> _groups = new List<Group>();
        [SerializeField] private int _maxGroups = 10;
        private Dictionary<int, Group> _groupsById = new Dictionary<int, Group>();

        public IReadOnlyList<Group> AllGroups => _groups;
        public int MaxGroups => _maxGroups;
        public bool CanCreateGroup() => _groups.Count < _maxGroups;

        public int? GetNextFreeGroupId()
        {
            if (!CanCreateGroup())
                return null;

            bool[] usedIds = new bool[_maxGroups];
            foreach (Group g in _groups)
            {
                if (g != null && g.Id >= 0 && g.Id < _maxGroups)
                    usedIds[g.Id] = true;
            }

            for (int id = 0; id < _maxGroups; id++)
            {
                if (!usedIds[id])
                    return id;
            }

            return null;
        }

        public void SetMaxGroups(int maxGroups)
        {
            _maxGroups = Mathf.Max(0, maxGroups);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            RebuildIndex();
        }

        private void RebuildIndex()
        {
            _groupsById.Clear();
            foreach (Group g in _groups)
            {
                if (g != null && !_groupsById.ContainsKey(g.Id))
                    _groupsById.Add(g.Id, g);
            }
        }

        public bool TryGetGroup(int id, out Group group)
        {
            return _groupsById.TryGetValue(id, out group);
        }

        public Group GetGroup(int id)
        {
            return _groupsById.ContainsKey(id) ? _groupsById[id] : null;
        }

        public Group GetOrCreate(int id)
        {
            if (id < 0) return null;
            if (_groupsById.ContainsKey(id)) return _groupsById[id];
            if (_groups.Count >= _maxGroups)
            {
                Debug.LogWarning($"[GroupManager] não é possível criar grupo {id}: limite de {_maxGroups} grupos alcançado.");
                return null;
            }
            Group g = new Group(id);
            _groupsById.Add(id, g);

            // Insert sorted by Id so Inspector "Element N" aligns with group Id.
            int insertAt = _groups.Count;
            for (int i = 0; i < _groups.Count; i++)
            {
                if (_groups[i] != null && _groups[i].Id > id) { insertAt = i; break; }
            }
            _groups.Insert(insertAt, g);
            return g;
        }

        public void AddAgent(int groupId, Agent agent)
        {
            if (agent == null || groupId < 0) return;
            Group g = GetOrCreate(groupId);
            if (g == null) return;
            g.AddAgent(agent);
        }

        public void RemoveAgent(int groupId, Agent agent)
        {
            if (agent == null || groupId < 0) return;
            if (!_groupsById.ContainsKey(groupId)) return;
            _groupsById[groupId].RemoveAgent(agent);
        }

        public void MoveAgent(int fromGroupId, int toGroupId, Agent agent)
        {
            RemoveAgent(fromGroupId, agent);
            AddAgent(toGroupId, agent);
        }

        public Agent GetLeader(int groupId)
        {
            Group g = GetGroup(groupId);
            return g != null ? g.Leader : null;
        }

        /// <summary>
        /// Remove grupos sem membros do registro (lista e índice). Chamar após as operações
        /// de grupo do eval cycle. Grupos podem ser recriados por GetOrCreate se voltarem a receber agentes.
        /// </summary>
        public void PruneEmptyGroups()
        {
            for (int i = _groups.Count - 1; i >= 0; i--)
            {
                Group g = _groups[i];
                if (g == null || g.Count == 0)
                {
                    if (g != null) _groupsById.Remove(g.Id);
                    _groups.RemoveAt(i);
                }
            }
        }

        public void SetLeader(int groupId, Agent leader)
        {
            Group g = GetOrCreate(groupId);
            if (g == null) return;
            g.Leader = leader;
        }

        public void SetGoals(int groupId, List<GameObject> goals, List<float> waits)
        {
            Group g = GetOrCreate(groupId);
            if (g == null) return;
            g.GoalsList = goals;
            g.GoalsWaitList = waits;
        }

        /// <summary>Loga grupos no console: id, leader, agentes.</summary>
        public void DumpToLog()
        {
            Debug.Log($"[GroupManager] {_groups.Count} grupos ativos");
            foreach (Group g in _groups)
            {
                if (g == null) continue;
                string leaderName = g.Leader != null ? g.Leader.name : "(sem líder)";
                Debug.Log($"  Grupo {g.Id} | líder: {leaderName} | {g.Count} agentes");
                foreach (Agent a in g.Agents)
                {
                    if (a == null) continue;
                    Debug.Log($"    - {a.name} (aff={a.affinity:F2}, dom={a.dominance:F2})");
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
                DumpToLog();
        }
    }
}
