using System.Collections.Generic;
using UnityEngine;

namespace AscendantContinuum.VFX
{
    /// <summary>
    /// Generates sacred geometry meshes at runtime — no art assets required.
    ///
    /// Supported shapes:
    ///   • Flower of Life   (7 interlocked circles, 60-vert each)
    ///   • Metatron's Cube  (Flower of Life + 13-node star tetrahedra lines)
    ///   • Vesica Piscis    (two overlapping circles, emphasised lunula)
    ///   • Single Rose      (parametric rhodonea curve, n=5)
    ///   • Sri Yantra lite  (9 interlocked triangles, simplified for mobile)
    ///
    /// Rendered via LineRenderer — supports realm tint and sigil complexity scaling.
    /// </summary>
    public sealed class SacredGeometryRenderer : MonoBehaviour
    {
        public enum GeometryType
        {
            FlowerOfLife,
            MetatronsCube,
            VesicaPiscis,
            Rose,
            SriYantraLite
        }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Shape")]
        [SerializeField] private GeometryType geometryType = GeometryType.FlowerOfLife;
        [SerializeField] private float        radius       = 1f;
        [SerializeField] private int          segments     = 64;

        [Header("Appearance")]
        [SerializeField] private Color  lineColor    = new Color(0.7f, 0.55f, 1f, 0.85f);
        [SerializeField] private float  lineWidth    = 0.018f;
        [SerializeField] private bool   animateRotation = false;
        [SerializeField] private float  rotationSpeed   = 3f;

        // ── Runtime ────────────────────────────────────────────────────────────
        private readonly List<LineRenderer> _lines = new List<LineRenderer>();

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Start()
        {
            Build();
        }

        private void Update()
        {
            if (animateRotation)
                transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void SetColor(Color color)
        {
            lineColor = color;
            foreach (var lr in _lines)
            {
                if (lr == null) continue;
                lr.startColor = color;
                lr.endColor   = new Color(color.r, color.g, color.b, color.a * 0.4f);
            }
        }

        public void SetGeometry(GeometryType type)
        {
            geometryType = type;
            ClearLines();
            Build();
        }

        // ── Build ──────────────────────────────────────────────────────────────

        private void Build()
        {
            switch (geometryType)
            {
                case GeometryType.FlowerOfLife:   BuildFlowerOfLife();  break;
                case GeometryType.MetatronsCube:  BuildMetatrons();     break;
                case GeometryType.VesicaPiscis:   BuildVesica();        break;
                case GeometryType.Rose:           BuildRose(5);         break;
                case GeometryType.SriYantraLite:  BuildSriYantraLite(); break;
            }
        }

        // ── Flower of Life ────────────────────────────────────────────────────

        private void BuildFlowerOfLife()
        {
            // Centre circle
            AddCircle(Vector2.zero, radius);

            // 6 surrounding circles at distance = radius
            for (int i = 0; i < 6; i++)
            {
                float a    = i * Mathf.PI / 3f;
                Vector2 c  = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                AddCircle(c, radius);
            }

            // Outer ring: 6 circles at distance = 2*radius from outer ring midpoints
            for (int i = 0; i < 6; i++)
            {
                float a   = (i + 0.5f) * Mathf.PI / 3f;
                Vector2 c = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius * Mathf.Sqrt(3f);
                AddCircle(c, radius);
            }
        }

        // ── Metatron's Cube ───────────────────────────────────────────────────

        private void BuildMetatrons()
        {
            BuildFlowerOfLife();

            // 13 nodes: centre + 6 inner + 6 outer
            var nodes = new List<Vector2> { Vector2.zero };
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                nodes.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius);
            }
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                nodes.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius * 2f);
            }

            // Connect every node to every other (star tetrahedra approximation)
            for (int a = 0; a < nodes.Count; a++)
            {
                for (int b = a + 1; b < nodes.Count; b++)
                {
                    // Only draw lines between inner set and outer set for less clutter
                    if (a <= 6 && b > 6)
                        AddLine(nodes[a], nodes[b]);
                }
            }
        }

        // ── Vesica Piscis ─────────────────────────────────────────────────────

        private void BuildVesica()
        {
            float offset = radius * 0.5f;
            AddCircle(new Vector2(-offset, 0f), radius);
            AddCircle(new Vector2( offset, 0f), radius);

            // Emphasise the lunula (intersection almond)
            var lunulaPoints = new List<Vector3>();
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float a = Mathf.Lerp(-Mathf.PI / 3f, Mathf.PI / 3f, t);
                lunulaPoints.Add(new Vector3(Mathf.Cos(a) * radius - offset,
                                              Mathf.Sin(a) * radius, 0f));
            }
            for (int i = segments; i >= 0; i--)
            {
                float t = i / (float)segments;
                float a = Mathf.Lerp(-Mathf.PI / 3f, Mathf.PI / 3f, t);
                lunulaPoints.Add(new Vector3(-(Mathf.Cos(a) * radius - offset),
                                               Mathf.Sin(a) * radius, 0f));
            }
            var lr = CreateLineRenderer();
            lr.loop = true;
            lr.positionCount = lunulaPoints.Count;
            lr.SetPositions(lunulaPoints.ToArray());
        }

        // ── Rose (rhodonea) ───────────────────────────────────────────────────

        private void BuildRose(int petals)
        {
            var pts = new Vector3[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t  = i / (float)segments * Mathf.PI * 2f;
                float r  = radius * Mathf.Abs(Mathf.Cos(petals * t * 0.5f));
                pts[i]   = new Vector3(r * Mathf.Cos(t), r * Mathf.Sin(t), 0f);
            }
            var lr = CreateLineRenderer();
            lr.loop = false;
            lr.positionCount = pts.Length;
            lr.SetPositions(pts);
        }

        // ── Sri Yantra lite ───────────────────────────────────────────────────

        private void BuildSriYantraLite()
        {
            // 5 downward-pointing triangles + 4 upward-pointing triangles
            float[] scales = { 1f, 0.82f, 0.65f, 0.5f, 0.36f };
            for (int i = 0; i < scales.Length; i++)
            {
                // Downward triangle
                float r = radius * scales[i];
                AddTriangle(Vector2.zero, r, Mathf.PI * 0.5f + Mathf.PI, true);
            }
            float[] upScales = { 0.91f, 0.73f, 0.57f, 0.43f };
            for (int i = 0; i < upScales.Length; i++)
            {
                float r = radius * upScales[i];
                AddTriangle(Vector2.zero, r, Mathf.PI * 0.5f, true);
            }

            // Outer square (bhupura)
            AddSquare(radius * 1.15f);
            // Outer circle
            AddCircle(Vector2.zero, radius * 1.08f);
        }

        // ── Primitive helpers ─────────────────────────────────────────────────

        private void AddCircle(Vector2 centre, float r)
        {
            var lr = CreateLineRenderer();
            lr.loop = true;
            lr.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(centre.x + Mathf.Cos(a) * r,
                                               centre.y + Mathf.Sin(a) * r, 0f));
            }
        }

        private void AddLine(Vector2 a, Vector2 b)
        {
            var lr            = CreateLineRenderer();
            lr.loop           = false;
            lr.positionCount  = 2;
            lr.SetPosition(0, new Vector3(a.x, a.y, 0f));
            lr.SetPosition(1, new Vector3(b.x, b.y, 0f));
        }

        private void AddTriangle(Vector2 centre, float r, float startAngle, bool loop)
        {
            var lr = CreateLineRenderer();
            lr.loop = loop;
            lr.positionCount = 3;
            for (int i = 0; i < 3; i++)
            {
                float a = startAngle + i * Mathf.PI * 2f / 3f;
                lr.SetPosition(i, new Vector3(centre.x + Mathf.Cos(a) * r,
                                               centre.y + Mathf.Sin(a) * r, 0f));
            }
        }

        private void AddSquare(float halfSize)
        {
            var lr = CreateLineRenderer();
            lr.loop = true;
            lr.positionCount = 4;
            lr.SetPosition(0, new Vector3(-halfSize, -halfSize, 0f));
            lr.SetPosition(1, new Vector3( halfSize, -halfSize, 0f));
            lr.SetPosition(2, new Vector3( halfSize,  halfSize, 0f));
            lr.SetPosition(3, new Vector3(-halfSize,  halfSize, 0f));
        }

        private LineRenderer CreateLineRenderer()
        {
            var go         = new GameObject("GeoLine");
            go.transform.SetParent(transform, false);
            var lr         = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth  = lineWidth;
            lr.endWidth    = lineWidth * 0.5f;
            lr.startColor  = lineColor;
            lr.endColor    = new Color(lineColor.r, lineColor.g, lineColor.b, lineColor.a * 0.4f);
            _lines.Add(lr);
            return lr;
        }

        private void ClearLines()
        {
            foreach (var lr in _lines)
                if (lr != null) Destroy(lr.gameObject);
            _lines.Clear();
        }
    }
}
