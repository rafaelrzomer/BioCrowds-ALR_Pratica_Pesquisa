/// ---------------------------------------------
/// GroupHUD: ring on the ground around each group's
/// centroid + 3D label with live info.
/// Added: 2026 - Pratica em Pesquisa PUCRS
/// ---------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Biocrowds.Core
{
    [RequireComponent(typeof(World))]
    public class GroupHUD : MonoBehaviour
    {
        [Header("Ring Settings")]
        [Tooltip("Raio do círculo desenhado em volta do centróide do grupo.")]
        [SerializeField] private float ringRadius   = 3.0f;
        [SerializeField] private int   ringSegments = 48;
        [SerializeField] private float ringYOffset  = 0.05f;
        [SerializeField] private float ringWidth    = 0.08f;

        [Header("Label Settings")]
        [SerializeField] private float labelYOffset    = 2.5f;
        [SerializeField] private int   labelFontSize   = 36;
        [SerializeField] private float labelCharSize   = 0.08f;

        private World _world;

        private class GroupVisual
        {
            public GameObject  root;
            public LineRenderer ring;
            public TextMesh    label;
        }

        private readonly Dictionary<int, GroupVisual> _visuals = new Dictionary<int, GroupVisual>();
        private Transform _hudContainer;

        private void Awake()
        {
            _world = GetComponent<World>();
            _hudContainer = new GameObject("GroupHUD").transform;
            _hudContainer.SetParent(transform, false);
        }

        private void LateUpdate()
        {
            if (_world == null) return;

            var centroids = _world.GroupCentroids;
            var averages  = _world.GroupAffinityAverages;
            if (centroids == null) return;

            // Marca todos como inativos antes de atualizar
            foreach (var kvp in _visuals)
                kvp.Value.root.SetActive(false);

            foreach (var kvp in centroids)
            {
                int     gid       = kvp.Key;
                Vector3 centroid  = kvp.Value;

                if (!_visuals.TryGetValue(gid, out GroupVisual gv))
                {
                    gv = CreateVisual(gid);
                    _visuals[gid] = gv;
                }

                gv.root.SetActive(true);

                Color groupColor = GetGroupColor(gid);
                gv.ring.startColor = groupColor;
                gv.ring.endColor   = groupColor;

                DrawCircle(gv.ring, centroid + Vector3.up * ringYOffset, ringRadius);

                int   size   = CountMembers(gid);
                float avgAff = averages != null && averages.TryGetValue(gid, out float a) ? a : 0f;
                Agent leader = _world.GetGroupLeader(gid);
                string leaderName = leader != null ? leader.name : "—";

                gv.label.transform.position = centroid + Vector3.up * labelYOffset;
                gv.label.color = groupColor;
                gv.label.text  = $"Group {gid}\n" +
                                 $"size: {size}\n" +
                                 $"affinity: {avgAff:F2}\n" +
                                 $"leader: {leaderName}";

                FaceCamera(gv.label.transform);
            }
        }

        private GroupVisual CreateVisual(int gid)
        {
            GameObject root = new GameObject($"GroupVisual_{gid}");
            root.transform.SetParent(_hudContainer, false);

            LineRenderer lr = root.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop          = true;
            lr.positionCount = ringSegments;
            lr.startWidth    = ringWidth;
            lr.endWidth      = ringWidth;
            lr.material      = new Material(Shader.Find("Sprites/Default"));
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows    = false;

            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(root.transform, false);

            TextMesh tm   = labelGO.AddComponent<TextMesh>();
            tm.fontSize    = labelFontSize;
            tm.characterSize = labelCharSize;
            tm.anchor      = TextAnchor.MiddleCenter;
            tm.alignment   = TextAlignment.Center;
            tm.fontStyle   = FontStyle.Bold;

            MeshRenderer mr = labelGO.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;

            return new GroupVisual { root = root, ring = lr, label = tm };
        }

        private void DrawCircle(LineRenderer lr, Vector3 center, float radius)
        {
            if (lr.positionCount != ringSegments)
                lr.positionCount = ringSegments;

            float step = 2f * Mathf.PI / ringSegments;
            for (int i = 0; i < ringSegments; i++)
            {
                float t = step * i;
                Vector3 p = new Vector3(
                    center.x + Mathf.Cos(t) * radius,
                    center.y,
                    center.z + Mathf.Sin(t) * radius
                );
                lr.SetPosition(i, p);
            }
        }

        private int CountMembers(int gid)
        {
            int count = 0;
            var agents = _world.Agents;
            if (agents == null) return 0;
            for (int i = 0; i < agents.Count; i++)
                if (agents[i] != null && agents[i].groupId == gid) count++;
            return count;
        }

        private static Color GetGroupColor(int gid)
        {
            switch (gid)
            {
                case -1: return Color.white;
                case  0: return Color.red;
                case  1: return Color.blue;
                case  2: return Color.green;
                case  3: return Color.yellow;
                default: return Color.magenta;
            }
        }

        private static void FaceCamera(Transform t)
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            t.rotation = Quaternion.LookRotation(t.position - cam.transform.position);
        }
    }
}
