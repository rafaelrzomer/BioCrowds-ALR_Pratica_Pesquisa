using System.Text;
using UnityEngine;
using Biocrowds.Core;

/// <summary>
/// HUD de métricas em runtime (OnGUI). Lê o snapshot público do <see cref="World"/>
/// atualizado a cada eval cycle e desenha um painel no canto da tela.
///
/// Não exige Canvas nem prefab: basta colocar este componente em qualquer GameObject
/// da cena. A referência ao World é resolvida via Inspector ou FindObjectOfType.
///
/// Tecla <c>M</c> liga/desliga o painel.
/// </summary>
public class MetricsHUD : MonoBehaviour
{
    [Header("HUD")]
    [Tooltip("Opcional. Deixe VAZIO: a HUD acha o World automaticamente (prioriza o da mesma cena). " +
             "Preencha só se quiser forçar um World específico.")]
    [SerializeField] private World _world;
    [SerializeField] private bool _visible = true;
    [SerializeField] private KeyCode _toggleKey = KeyCode.M;

    [Header("Layout")]
    [SerializeField] private int _fontSize = 14;
    [SerializeField] private Vector2 _padding = new Vector2(10f, 10f);
    [SerializeField] private float _width = 300f;
    [SerializeField] private bool _showPerGroup = true;

    private GUIStyle _style;
    private readonly StringBuilder _sb = new StringBuilder(512);

    private void Awake()
    {
        TryFindWorld();
    }

    private void Update()
    {
        // Re-tenta achar o World enquanto não houver referência:
        // cobre o caso de o World ser criado depois do Awake desta HUD.
        if (_world == null)
            TryFindWorld();

        if (Input.GetKeyDown(_toggleKey))
            _visible = !_visible;
    }

    // Auto-find do World: prioriza o da MESMA cena deste GameObject; senão, qualquer World ativo.
    private void TryFindWorld()
    {
        if (_world != null)
            return;

        World[] worlds = FindObjectsOfType<World>();
        if (worlds == null || worlds.Length == 0)
            return;

        // 1) preferir um World na mesma cena deste GameObject
        for (int i = 0; i < worlds.Length; i++)
        {
            if (worlds[i].gameObject.scene == gameObject.scene)
            {
                _world = worlds[i];
                return;
            }
        }

        // 2) fallback: primeiro World ativo encontrado
        _world = worlds[0];
    }

    private void OnGUI()
    {
        if (!_visible || _world == null)
            return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                richText = true,
                alignment = TextAnchor.UpperLeft,
                wordWrap = false
            };
            _style.normal.textColor = Color.white;
        }

        _sb.Length = 0;
        _sb.AppendLine("<b>BioCrowds — Métricas</b>");
        _sb.AppendLine($"t = {_world.MetricTime:F2}s");
        _sb.AppendLine($"Agentes: {_world.MetricNumAgents}");
        _sb.AppendLine($"Grupos: {_world.MetricNumGroups}   Solos: {_world.MetricNumSolo}");
        _sb.AppendLine($"Trocas: {_world.MetricTotalSwitches} (ciclo: {_world.MetricSwitchesInterval})");
        _sb.AppendLine($"Travados (jam): {_world.MetricNumStuck}");
        _sb.AppendLine($"Dinâmica de grupos: {(_world.GroupChangesAllowed ? "<color=#7CFC00>ON</color>" : "<color=#FF6B6B>OFF</color>")}");
        string seedTxt = _world.UsesSeed ? _world.RandomSeed.ToString() : "off";
        string maxTxt = _world.MaxAgents <= 0 ? "ilim." : _world.MaxAgents.ToString("0");
        _sb.AppendLine($"<color=#AAAAAA>seed: {seedTxt}   maxAg: {maxTxt}</color>");

        if (_showPerGroup)
        {
            var groups = _world.MetricGroups;
            if (groups != null && groups.Count > 0)
            {
                _sb.AppendLine();
                _sb.AppendLine("<b>Por grupo</b>  id n coes affMed±dp tGrupo");
                for (int i = 0; i < groups.Count; i++)
                {
                    var g = groups[i];
                    _sb.AppendLine($"G{g.groupId}: n={g.size} coes={g.cohesion:F2} aff={g.meanAffinity:F2}±{g.affinityStdDev:F2} t={g.meanTimeInGroup:F1}s");
                }
            }
        }

        // altura estimada por nº de linhas; +2 de folga
        int lines = 0;
        for (int i = 0; i < _sb.Length; i++)
            if (_sb[i] == '\n') lines++;
        float height = (lines + 2) * (_fontSize + 4) + _padding.y * 2f;

        var bg = new Rect(_padding.x, _padding.y, _width, height);
        GUI.Box(bg, GUIContent.none);
        var content = new Rect(bg.x + 8f, bg.y + 6f, _width - 16f, height - 12f);
        GUI.Label(content, _sb.ToString(), _style);
    }
}
