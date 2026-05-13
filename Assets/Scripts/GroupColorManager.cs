using UnityEngine;
using System.Collections.Generic;
using Biocrowds.Core;

/// <summary>
/// Gerencia as cores associadas a cada grupo de agentes
/// Coloque este script em um GameObject na cena
/// </summary>
public class GroupColorManager : MonoBehaviour
{
    [System.Serializable]
    public struct GroupColor
    {
        public int groupId;
        public Color color;
    }

    [Header("Configuração de Cores")]
    [SerializeField]
    private List<GroupColor> groupColors = new List<GroupColor>();

    [Header("Cor para agentes sem grupo (groupId = -1)")]
    [SerializeField]
    public Color noGroupColor = Color.gray;

    private Dictionary<int, Color> _colorMap = new Dictionary<int, Color>();
    private static GroupColorManager _instance;

    public static GroupColorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GroupColorManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // Construir mapa de cores
        _colorMap.Clear();
        _colorMap[-1] = noGroupColor; // Cor para agentes sem grupo
        foreach (GroupColor gc in groupColors)
        {
            _colorMap[gc.groupId] = gc.color;
        }
    }

    /// <summary>
    /// Obtém a cor associada a um grupo
    /// </summary>
    public Color GetGroupColor(int groupId)
    {
        if (groupId == -1)
        {
            return noGroupColor;
        }

        if (_colorMap.ContainsKey(groupId))
        {
            return _colorMap[groupId];
        }
        return Color.white;
    }

    /// <summary>
    /// Define a cor para um grupo
    /// </summary>
    public void SetGroupColor(int groupId, Color color)
    {
        _colorMap[groupId] = color;
    }

    /// <summary>
    /// Define a cor para agentes sem grupo
    /// </summary>
    public void SetNoGroupColor(Color color)
    {
        noGroupColor = color;
        _colorMap[-1] = color;
    }

    /// <summary>
    /// Obtém a cor atual para agentes sem grupo
    /// </summary>
    public Color GetNoGroupColor()
    {
        return noGroupColor;
    }
}
