using UnityEngine;
using Biocrowds.Core;

/// <summary>
/// Inspetor de agente em runtime (OnGUI). Clique num agente no Play para selecioná-lo
/// e ver/editar seus atributos: groupId, affinity, dominance, isGroupLeader.
///
/// Não exige Canvas/prefab: solte este componente em qualquer GameObject da cena.
/// A referência ao World é resolvida via Inspector ou FindObjectOfType (mesma cena).
///
/// Seleção por proximidade na tela (não precisa de Collider no agente): no clique,
/// escolhe o agente cuja projeção de tela está mais perto do cursor.
///
/// Tecla `I` liga/desliga a ferramenta.
/// </summary>
public class AgentInspectorHUD : MonoBehaviour
{
    [Header("Inspetor")]
    [Tooltip("Opcional: deixe vazio para auto-find do World na mesma cena.")]
    [SerializeField] private World _world;
    [SerializeField] private bool _active = true;
    [SerializeField] private KeyCode _toggleKey = KeyCode.I;
    [Tooltip("Raio (px) de tolerância do clique para selecionar um agente.")]
    [SerializeField] private float _pickRadiusPixels = 40f;
    [Tooltip("Altura (m) somada à posição do agente para mirar a cabeça na seleção.")]
    [SerializeField] private float _aimHeight = 1.0f;

    [Tooltip("Offset da câmera ao seguir o agente selecionado (mundo).")]
    [SerializeField] private Vector3 _camOffset = new Vector3(0f, 14f, -10f);

    private Agent _selected;
    private bool _followCam = false;
    private Rect _windowRect = new Rect(20, 120, 300, 0);
    private GUIStyle _hintStyle;

    private void Awake()
    {
        if (_world == null)
            _world = FindObjectOfType<World>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            _active = !_active;
            if (!_active) _selected = null;
        }

        if (!_active) return;
        if (_world == null) _world = FindObjectOfType<World>();
        if (_world == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            // ignora clique sobre a janela do inspetor (coords GUI: origem topo-esquerda)
            if (_selected != null)
            {
                Vector2 guiPos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if (_windowRect.Contains(guiPos))
                    return;
            }
            SelectNearestAgent(Input.mousePosition);
        }
    }

    /// <summary>Seleciona o agente cuja projeção de tela está mais perto do cursor.</summary>
    private void SelectNearestAgent(Vector2 mouseScreen)
    {
        Camera cam = Camera.main;
        if (cam == null || _world.Agents == null) return;

        Agent best = null;
        float bestDistSqr = _pickRadiusPixels * _pickRadiusPixels;

        var agents = _world.Agents;
        for (int i = 0; i < agents.Count; i++)
        {
            Agent a = agents[i];
            if (a == null) continue;

            Vector3 sp = cam.WorldToScreenPoint(a.transform.position + Vector3.up * _aimHeight);
            if (sp.z <= 0f) continue; // atrás da câmera

            float dSqr = (new Vector2(sp.x, sp.y) - mouseScreen).sqrMagnitude;
            if (dSqr < bestDistSqr)
            {
                bestDistSqr = dSqr;
                best = a;
            }
        }

        _selected = best; // null se clicou no vazio -> deseleciona
    }

    private void OnGUI()
    {
        if (!_active) return;

        if (_hintStyle == null)
        {
            _hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, richText = true };
            _hintStyle.normal.textColor = Color.white;
        }

        if (_selected == null)
        {
            GUI.Label(new Rect(20, 120, 360, 24),
                "<b>Inspetor (tecla I):</b> clique num agente para editar.", _hintStyle);
            return;
        }

        _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, "Inspetor de Agente");
    }

    private void DrawWindow(int id)
    {
        Agent a = _selected;
        if (a == null) { GUILayout.Label("—"); GUI.DragWindow(); return; }

        GUILayout.Label($"<b>{a.name}</b>", _hintStyle);
        GUILayout.Label($"groupId: {a.groupId}   {(a.HasGroup ? "(em grupo)" : "(solo)")}   {(a.isGroupLeader ? "★ líder" : "")}");
        GUILayout.Label($"timeInGroup: {a.timeInGroup:F1}s   |v|: {a._velocity.magnitude:F2}");
        int goalN = a.goalsList != null ? a.goalsList.Count : 0;
        GUILayout.Label($"goal: {a.CurrentGoalIndex + 1}/{goalN}   {(a.isWaiting ? "esperando" : "andando")}");
        int nearby = a._nearbyGroupMembers != null ? a._nearbyGroupMembers.Count : 0;
        GUILayout.Label($"vizinhos do grupo: {nearby}   idade: {a.timeSinceSpawn:F1}s");

        GUILayout.Space(6);

        // affinity (editável)
        GUILayout.BeginHorizontal();
        GUILayout.Label("affinity", GUILayout.Width(70));
        a.affinity = GUILayout.HorizontalSlider(a.affinity, 0f, 1f);
        GUILayout.Label(a.affinity.ToString("F2"), GUILayout.Width(36));
        GUILayout.EndHorizontal();

        // dominance (editável)
        GUILayout.BeginHorizontal();
        GUILayout.Label("dominance", GUILayout.Width(70));
        a.dominance = GUILayout.HorizontalSlider(a.dominance, 0f, 1f);
        GUILayout.Label(a.dominance.ToString("F2"), GUILayout.Width(36));
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        // groupId (editável via SwitchGroup, que atualiza GroupManager + cor)
        GUILayout.BeginHorizontal();
        GUILayout.Label("grupo", GUILayout.Width(70));
        if (GUILayout.Button("− grupo")) ChangeGroup(a, a.groupId - 1);
        if (GUILayout.Button("+ grupo")) ChangeGroup(a, a.groupId + 1);
        if (GUILayout.Button("solo")) ChangeGroup(a, -1);
        GUILayout.EndHorizontal();

        // isGroupLeader (transitório — UpdateGroupLeaders pode reverter no próximo ciclo)
        bool newLeader = GUILayout.Toggle(a.isGroupLeader, " isGroupLeader (transitório)");
        if (newLeader != a.isGroupLeader)
        {
            a.isGroupLeader = newLeader;
            a.ApplyGroupColor();
        }

        GUILayout.Space(6);
        _followCam = GUILayout.Toggle(_followCam, " câmera segue o agente");
        if (GUILayout.Button("Fechar")) _selected = null;

        GUI.DragWindow();
    }

    private void LateUpdate()
    {
        if (!_followCam || _selected == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = _selected.transform.position + _camOffset;
        cam.transform.LookAt(_selected.transform.position);
    }

    private void ChangeGroup(Agent a, int newGroupId)
    {
        if (newGroupId < -1) newGroupId = -1;
        if (newGroupId == a.groupId) return;
        a.SwitchGroup(newGroupId); // trata GroupManager, cor, reset de liderança
    }
}
