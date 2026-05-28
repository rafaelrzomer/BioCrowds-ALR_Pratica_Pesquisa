
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Biocrowds.Core;

public class VisualAgent : MonoBehaviour 
{
    private Animator anim;
    [SerializeField]
    public Queue<float> moveMem;
    public Queue<Vector3> dirMem;
    public float[] qview;
    private Vector3 currPosition;
    private Vector3 currMoveVect;
    private Vector3 prevMoveVect;
    [SerializeField]
    Vector3 avgDirSum = new Vector3();
    [SerializeField]
    Vector3 avgDir;
    [SerializeField]
    public List<Vector3> dirView;

    private Renderer[] _renderers;
    private Material[] _materials;

	// Update is called once per frame
	public void Step() 
    {
        prevMoveVect = currMoveVect;
        currMoveVect = currPosition - transform.parent.position;
        currMoveVect.y = 0f;
        //Debug.Log(currMoveVect.x + " " + currMoveVect.z);
        moveMem.Dequeue();
        dirMem.Dequeue();
        //moveMem.Enqueue(currMoveVect);
        moveMem.Enqueue(currMoveVect.magnitude);
        dirMem.Enqueue(currMoveVect.normalized);
        float speedSum = 0;
        //float angleDifSum = 0;

        avgDirSum = new Vector3();
        var prevV = moveMem.Peek();
        foreach(float v in moveMem){
            speedSum += v;
            //angleDifSum += Vector3.SignedAngle(prevV, v,Vector3.back);
            prevV = v;
        }
        foreach (Vector3 d in dirMem)
        {
            avgDirSum += d;
        }
        float presentAvgSpeed = (speedSum  / moveMem.Count) ;
        float estFutureSpeed = currMoveVect.magnitude;
        float AvgSpeed = (presentAvgSpeed + estFutureSpeed) / 2;
        avgDir = avgDirSum / dirMem.Count;
        //float presentAvgAngleDif = angleDifSum / moveMem.Count;
        //float estFutureAngDif = Vector3.SignedAngle(prevV, currMoveVect, Vector3.back);
        //float avgAngleDif = (presentAvgAngleDif + estFutureAngDif) / 2;
        float totalAngleDiff = Vector3.SignedAngle(currMoveVect, prevMoveVect, Vector3.up);
        //Debug.Log(totalAngleDiff);
        float angFact = totalAngleDiff / 90f;
        //anim.SetFloat("AngSpeed", angFact * 0.5f);// Mathf.Clamp(angDif/6f,-1f,1f));


        //transform.Rotate(new Vector3(0, totalAngleDiff * 0.05f, 0), Space.World);
        //transform.rotation = Quaternion.Euler(0, Mathf.Atan2(speed.x,speed.z)*180f,0);
        
        Vector3 targetDirection = -avgDir.normalized;
        //transform.rotation = Quaternion.LookRotation(targetDirection);
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 36f);
        }
        //transform.LookAt(transform.position - currMoveVect, Vector3.up);
        anim.SetFloat("Speed", Mathf.Clamp(presentAvgSpeed*32f, 0f, 0.9f));
        //anim.SetFloat("AngSpeed", presentAvgAngleDif/3f);
        anim.SetFloat("Motion_Time", anim.GetFloat("Motion_Time") + (0.02f * presentAvgSpeed * 32f));
        //transform.position = currPosition;
        currPosition = transform.parent.position;
        qview = moveMem.ToArray();
        dirView = dirMem.ToList();

    }

    public void Initialize(Vector3 pos, Agent p_agent)
    {
        //transform.Rotate(Vector3.right,-90) ;
        anim = GetComponent<Animator>();
        moveMem = new Queue<float>();
        dirMem = new Queue<Vector3>();
        currPosition = new Vector3(pos.x, pos.y, pos.z);
        transform.position = currPosition;
        transform.LookAt(p_agent.goalsList[0].transform.position);
        for (int i = 0; i < 15; i++)
        {
            moveMem.Enqueue(0);
        }
        for (int i = 0; i < 10; i++)
        {
            dirMem.Enqueue((pos - p_agent.goalsList[0].transform.position).normalized);
        }
        dirView = dirMem.ToList();

        // Cachear renderers e materiais
        CacheRenderersAndMaterials();

        // Aplicar cor do grupo (incluindo agentes sem grupo)
        if (GroupColorManager.Instance != null)
        {
            Color groupColor = GroupColorManager.Instance.GetGroupColor(p_agent.groupId);
            ApplyGroupColor(groupColor);
        }
    }

    /// <summary>
    /// Encontra e cacheia todos os Renderers do agente
    /// </summary>
    private void CacheRenderersAndMaterials()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _materials = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _materials[i] = new Material(_renderers[i].material);
            _renderers[i].material = _materials[i];
        }
    }

    /// <summary>
    /// Aplica a cor do grupo a todos os materiais do agente
    /// </summary>
    public void ApplyGroupColor(Color color)
    {
        ApplyGroupColor(color, false);
    }

    [Header("Destaque do Líder")]
    [Tooltip("Aplica brilho e escala maior ao corpo do líder.")]
    [SerializeField] private bool _highlightLeaderBody = true;
    [Range(0f, 1f)]
    [SerializeField] private float _leaderBrighten = 0.4f;   // lerp em direção ao branco quando é líder
    [SerializeField] private float _leaderScale = 1.25f;     // multiplicador de escala uniforme do líder
    private Vector3 _baseScale = Vector3.zero;    // cached original scale

    [Header("Marcador do Líder")]
    [Tooltip("Liga/desliga o marcador flutuante acima da cabeça do líder.")]
    [SerializeField] private bool _showLeaderMarker = true;
    [Tooltip("Prefab opcional do marcador. Se vazio, usa um octaedro (diamante) procedural.")]
    [SerializeField] private GameObject _leaderMarkerPrefab;
    [Tooltip("Tinge o marcador com a cor do grupo. Desligado mantém a cor original do prefab/material.")]
    [SerializeField] private bool _tintMarkerWithGroupColor = true;
    [SerializeField] private float _markerHeight = 2.2f;     // metros acima do pivô do agente
    [SerializeField] private float _markerSize = 0.25f;      // escala uniforme do marcador
    [SerializeField] private float _markerSpinSpeed = 120f;  // graus/segundo de rotação em Y (0 = parado)
    private GameObject _leaderMarker;
    private Material _leaderMarkerMat;          // material do marcador (tingido pela cor do grupo)
    private static Mesh _diamondMeshCache;      // mesh do octaedro procedural, compartilhado

    /// <summary>
    /// Aplica a cor do grupo destacando o líder do grupo (brilho + escala maior).
    /// </summary>
    public void ApplyGroupColor(Color color, bool isLeader)
    {
        if (_materials == null || _materials.Length == 0)
        {
            CacheRenderersAndMaterials();
        }

        Color finalColor = (isLeader && _highlightLeaderBody) ? Color.Lerp(color, Color.white, _leaderBrighten) : color;

        foreach (Material mat in _materials)
        {
            if (mat != null)
                mat.color = finalColor;
        }

        // cache the original (non-leader) scale once so toggling leader on/off is reversible
        if (_baseScale == Vector3.zero)
            _baseScale = transform.localScale;

        transform.localScale = (isLeader && _highlightLeaderBody) ? _baseScale * _leaderScale : _baseScale;

        UpdateLeaderMarker(isLeader, color);
    }

    /// <summary>
    /// Cria/destroi o marcador flutuante acima da cabeça do líder.
    /// Usa o prefab informado no Inspector; se vazio, gera um octaedro procedural.
    /// </summary>
    private void UpdateLeaderMarker(bool isLeader, Color color)
    {
        if (!isLeader || !_showLeaderMarker)
        {
            if (_leaderMarker != null) Destroy(_leaderMarker);
            _leaderMarker = null;
            _leaderMarkerMat = null;
            return;
        }

        if (_leaderMarker == null)
        {
            if (_leaderMarkerPrefab != null)
            {
                _leaderMarker = Instantiate(_leaderMarkerPrefab, transform);
                Renderer rend = _leaderMarker.GetComponentInChildren<Renderer>();
                if (rend != null)
                    _leaderMarkerMat = rend.material; // instância própria, segura para tingir
            }
            else
            {
                _leaderMarker = BuildProceduralMarker();
            }

            _leaderMarker.transform.localPosition = new Vector3(0f, _markerHeight, 0f);
            _leaderMarker.transform.localScale = Vector3.one * _markerSize;
        }

        // tinge o marcador com a cor exata do grupo (atualiza caso o grupo mude)
        if (_tintMarkerWithGroupColor && _leaderMarkerMat != null)
        {
            _leaderMarkerMat.color = color;
            if (_leaderMarkerMat.HasProperty("_EmissionColor"))
                _leaderMarkerMat.SetColor("_EmissionColor", color);
        }

        // animação simples: gira em Y
        _leaderMarker.transform.localRotation = Quaternion.Euler(0f, Time.time * _markerSpinSpeed, 0f);
    }

    /// <summary>Octaedro procedural usado quando nenhum prefab de marcador é informado.</summary>
    private GameObject BuildProceduralMarker()
    {
        GameObject marker = new GameObject("LeaderMarker");
        marker.transform.SetParent(transform, false);

        MeshFilter mf = marker.AddComponent<MeshFilter>();
        MeshRenderer mr = marker.AddComponent<MeshRenderer>();
        mf.sharedMesh = GetDiamondMesh();
        _leaderMarkerMat = new Material(Shader.Find("Standard"));
        _leaderMarkerMat.EnableKeyword("_EMISSION");
        mr.material = _leaderMarkerMat;
        return marker;
    }

    private static Mesh GetDiamondMesh()
    {
        if (_diamondMeshCache != null) return _diamondMeshCache;

        // Octaedro: 6 vértices, 8 faces triangulares
        Vector3[] verts = new Vector3[6]
        {
            new Vector3(0f,  1f,  0f), // 0 topo
            new Vector3(0f, -1f,  0f), // 1 base
            new Vector3( 1f, 0f,  0f), // 2 +X
            new Vector3(-1f, 0f,  0f), // 3 -X
            new Vector3(0f,  0f,  1f), // 4 +Z
            new Vector3(0f,  0f, -1f), // 5 -Z
        };
        int[] tris = new int[24]
        {
            0,2,4,  0,4,3,  0,3,5,  0,5,2,   // topo
            1,4,2,  1,3,4,  1,5,3,  1,2,5    // base (winding invertida)
        };

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        _diamondMeshCache = mesh;
        return mesh;
    }

    private void Update()
    {
        // mantém o marcador girando, mesmo entre chamadas de ApplyGroupColor
        if (_leaderMarker != null)
            _leaderMarker.transform.localRotation = Quaternion.Euler(0f, Time.time * _markerSpinSpeed, 0f);
    }



}

