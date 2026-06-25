using UnityEngine;
using Biocrowds.Core;

/// <summary>
/// Controle de tempo da simulação em runtime. A sim do BioCrowds avança em passos
/// fixos (1 passo/frame por padrão); este controlador ajusta quantos passos rodam
/// por frame via <see cref="World.SimSpeed"/> e congela via <see cref="World.SimPaused"/>.
///
/// Teclas:
///   P  — pausa / retoma
///   [  — mais devagar   ]  — mais rápido   (0.25× .. 4×)
///   \  — volta para 1× (normal)
///
/// Solte o componente em qualquer GameObject da cena (não precisa de referências).
/// </summary>
public class TimeController : MonoBehaviour
{
    [SerializeField] private KeyCode _pauseKey = KeyCode.P;
    [SerializeField] private KeyCode _slowerKey = KeyCode.LeftBracket;
    [SerializeField] private KeyCode _fasterKey = KeyCode.RightBracket;
    [SerializeField] private KeyCode _resetKey = KeyCode.Backslash;

    private static readonly float[] SPEEDS = { 0.25f, 0.5f, 1f, 2f, 4f };
    private int _speedIndex = 2; // 1×

    private GUIStyle _style;

    private void Update()
    {
        if (Input.GetKeyDown(_pauseKey))
            World.SimPaused = !World.SimPaused;

        if (Input.GetKeyDown(_slowerKey))
            SetSpeedIndex(_speedIndex - 1);

        if (Input.GetKeyDown(_fasterKey))
            SetSpeedIndex(_speedIndex + 1);

        if (Input.GetKeyDown(_resetKey))
            SetSpeedIndex(2);
    }

    private void SetSpeedIndex(int idx)
    {
        _speedIndex = Mathf.Clamp(idx, 0, SPEEDS.Length - 1);
        World.SimSpeed = SPEEDS[_speedIndex];
    }

    private void OnGUI()
    {
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                richText = true,
                alignment = TextAnchor.UpperRight
            };
            _style.normal.textColor = Color.white;
        }

        string txt = World.SimPaused
            ? "<color=#FFD166>⏸ PAUSADO</color>"
            : $"▶ {World.SimSpeed:0.##}×";
        txt += "   <color=#AAAAAA>[P] pausa  [ ] vel  [\\] 1×</color>";

        var rect = new Rect(Screen.width - 430f, 8f, 420f, 24f);
        GUI.Label(rect, txt, _style);
    }
}
