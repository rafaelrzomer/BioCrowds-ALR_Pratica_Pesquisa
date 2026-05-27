using System.Collections.Generic;
using UnityEngine;

namespace Biocrowds.Core
{
    /// <summary>
    /// Representa um grupo de agentes com identificador, líder e lista compartilhada de objetivos.
    /// Marcado como [System.Serializable] para que a lista de grupos do GroupManager apareça no Inspector.
    /// </summary>
    [System.Serializable]
    public class Group
    {
        [SerializeField] private int _id;
        public int Id => _id;

        [SerializeField] private Agent _leader;
        public Agent Leader { get => _leader; set => _leader = value; }

        [SerializeField] private List<Agent> _agents = new List<Agent>();
        public List<Agent> Agents => _agents;

        // goals compartilhados pelo grupo. O líder dita a lista; followers seguem.
        [SerializeField] private List<GameObject> _goalsList = new List<GameObject>();
        public List<GameObject> GoalsList { get => _goalsList; set => _goalsList = value; }

        [SerializeField] private List<float> _goalsWaitList = new List<float>();
        public List<float> GoalsWaitList { get => _goalsWaitList; set => _goalsWaitList = value; }

        public Group(int id)
        {
            _id = id;
        }

        public int Count => _agents.Count;

        public void AddAgent(Agent a)
        {
            if (a == null || _agents.Contains(a)) return;
            _agents.Add(a);
        }

        public void RemoveAgent(Agent a)
        {
            if (a == null) return;
            _agents.Remove(a);
            if (_leader == a) _leader = null;
        }
    }
}
