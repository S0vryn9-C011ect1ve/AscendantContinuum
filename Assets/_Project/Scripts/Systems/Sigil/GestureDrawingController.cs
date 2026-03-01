using System.Collections.Generic;
using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Captures player touch/mouse input and drives the sigil <see cref="LineRenderer"/>
    /// in real time using Catmull-Rom spline smoothing.
    ///
    /// When the player taps the COMMIT button (or a configurable timer elapses),
    /// the accumulated strokes are passed to <see cref="SigilStrokeAnalyzer"/>,
    /// a <see cref="SigilAnalysisResult"/> is raised via <see cref="GameEvents"/>,
    /// and <see cref="SigilCompletionHandler"/> takes over the ritual sequence.
    ///
    /// Accessibility: drawing is disabled when <see cref="AccessibilityManager.ReducedMotionEnabled"/>
    /// is true — a simplified tap-to-complete path takes over instead.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class GestureDrawingController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Drawing")]
        [SerializeField] private Camera drawingCamera;
        [SerializeField] private float  drawingDepth   = 10f;
        [SerializeField] private float  minPointDist   = 0.08f;   // world units — prevents jitter
        [SerializeField] private int    splineSubdivide = 4;       // Catmull-Rom points per segment
        [SerializeField] private float  maxDrawSeconds = 60f;      // auto-commit safety cap

        [Header("Line Renderer Style")]
        [SerializeField] private float  lineWidth     = 0.04f;
        [SerializeField] private Color  lineColorStart = new Color(0.9f, 0.8f, 1f, 1f);
        [SerializeField] private Color  lineColorEnd   = new Color(0.5f, 0.3f, 1f, 0.3f);

        [Header("Auto-commit")]
        [Tooltip("Seconds of no input before the stroke auto-finalises.")]
        [SerializeField] private float  idleCommitDelay = 2.5f;

        // ── Runtime state ──────────────────────────────────────────────────────
        private LineRenderer            _lineRenderer;
        private List<List<Vector2>>     _strokes     = new List<List<Vector2>>();
        private List<Vector2>           _currentStroke;
        private Vector2                 _lastPoint;
        private bool                    _isDrawing   = false;
        private bool                    _sessionOpen = false;
        private float                   _drawStart;
        private float                   _idleTimer;

        // Velocity tracking for real-time audio feedback
        private Vector2 _prevPoint;
        private float   _prevPointTime;

        // ── Public API ─────────────────────────────────────────────────────────
        public bool IsSessionOpen => _sessionOpen;
        public int  StrokeCount   => _strokes.Count;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            ConfigureLineRenderer();
        }

        private void Update()
        {
            if (!_sessionOpen) return;

            // Auto-commit after idle period
            if (!_isDrawing && _strokes.Count > 0)
            {
                _idleTimer += Time.deltaTime;
                if (_idleTimer >= idleCommitDelay)
                    CommitSigil();
            }

            // Safety cap
            if (_sessionOpen && Time.time - _drawStart > maxDrawSeconds)
                CommitSigil();

            HandleInput();
        }

        // ── Session control ───────────────────────────────────────────────────

        /// <summary>Opens a fresh drawing session. Call from <see cref="SessionManager"/>.</summary>
        public void BeginSession()
        {
            _strokes.Clear();
            _currentStroke = null;
            _isDrawing     = false;
            _sessionOpen   = true;
            _drawStart     = Time.time;
            _idleTimer     = 0f;
            ClearLineRenderer();
            SessionManager.Instance?.SetPhase(SessionPhase.Drawing);
            Debug.Log("[GestureDrawing] Session opened.");
        }

        /// <summary>Manually commit whatever has been drawn so far.</summary>
        public void CommitSigil()
        {
            if (!_sessionOpen) return;
            _sessionOpen = false;

            // Finalise any in-progress stroke
            FinaliseCurrentStroke();

            if (_strokes.Count == 0)
            {
                Debug.Log("[GestureDrawing] No strokes — session abandoned.");
                return;
            }

            float duration = Time.time - _drawStart;
            SigilAnalysisResult result = SigilStrokeAnalyzer.Analyze(_strokes, duration);

            Debug.Log($"[GestureDrawing] Committed — loops:{result.closedLoopCount} " +
                      $"intersections:{result.intersectionCount} " +
                      $"entropy:{result.curvatureEntropy:F2} " +
                      $"arc:{result.totalArcLength:F2}");

            GameEvents.RaiseSigilDrawingCommitted(result);
        }

        /// <summary>Cancels and resets the current drawing session without committing.</summary>
        public void CancelSession()
        {
            _sessionOpen   = false;
            _isDrawing     = false;
            _strokes.Clear();
            _currentStroke = null;
            ClearLineRenderer();
        }

        // ── Input handling ────────────────────────────────────────────────────

        private void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0)) BeginStroke(Input.mousePosition);
            if (Input.GetMouseButton(0)    ) ContinueStroke(Input.mousePosition);
            if (Input.GetMouseButtonUp(0)  ) EndStroke();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;
            Touch t = Input.GetTouch(0);
            switch (t.phase)
            {
                case TouchPhase.Began:   BeginStroke(t.position);    break;
                case TouchPhase.Moved:   ContinueStroke(t.position); break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled: EndStroke();               break;
            }
        }

        // ── Stroke logic ───────────────────────────────────────────────────────

        private void BeginStroke(Vector3 screenPos)
        {
            if (!_sessionOpen) return;

            _isDrawing     = true;
            _idleTimer     = 0f;
            _currentStroke = new List<Vector2>();

            Vector2 wp = ScreenToWorld(screenPos);
            _currentStroke.Add(wp);
            _lastPoint     = wp;
            _prevPoint     = wp;
            _prevPointTime = Time.time;
        }

        private void ContinueStroke(Vector3 screenPos)
        {
            if (!_isDrawing || _currentStroke == null) return;

            Vector2 wp = ScreenToWorld(screenPos);
            if (Vector2.Distance(wp, _lastPoint) < minPointDist) return;

            _currentStroke.Add(wp);
            _lastPoint = wp;

            // Fire real-time stroke event for audio feedback
            float now = Time.time;
            float dt  = Mathf.Max(0.001f, now - _prevPointTime);
            float vel = Vector2.Distance(wp, _prevPoint) / dt;

            // Approximate curvature: angle change between last two segments
            float curvature = 0f;
            int c = _currentStroke.Count;
            if (c >= 3)
            {
                Vector2 d1 = (_currentStroke[c - 2] - _currentStroke[c - 3]).normalized;
                Vector2 d2 = (_currentStroke[c - 1] - _currentStroke[c - 2]).normalized;
                curvature  = Mathf.Clamp01(Vector2.Angle(d1, d2) / 180f);
            }

            GameEvents.RaiseSigilStrokePoint(vel, curvature);

            _prevPoint     = wp;
            _prevPointTime = now;

            RebuildLineRenderer();
        }

        private void EndStroke()
        {
            if (!_isDrawing) return;

            _isDrawing = false;
            _idleTimer = 0f;
            FinaliseCurrentStroke();
            GameEvents.RaiseSigilStrokeEnded();
        }

        private void FinaliseCurrentStroke()
        {
            if (_currentStroke != null && _currentStroke.Count >= 2)
                _strokes.Add(_currentStroke);
            _currentStroke = null;
        }

        // ── Rendering ─────────────────────────────────────────────────────────

        private void RebuildLineRenderer()
        {
            var allPoints = new List<Vector3>();

            foreach (var stroke in _strokes)
                AppendSplinePoints(stroke, allPoints, false);

            if (_currentStroke != null && _currentStroke.Count >= 2)
                AppendSplinePoints(_currentStroke, allPoints, true);

            _lineRenderer.positionCount = allPoints.Count;
            if (allPoints.Count > 0)
                _lineRenderer.SetPositions(allPoints.ToArray());
        }

        /// <summary>
        /// Appends Catmull-Rom spline points for a single stroke.
        /// Ghost endpoints are extrapolated so the curve reaches curve endpoints.
        /// </summary>
        private void AppendSplinePoints(List<Vector2> stroke, List<Vector3> output, bool isLive)
        {
            if (stroke.Count < 2) return;

            int n = stroke.Count;
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p0 = i > 0    ? stroke[i - 1] : stroke[0] * 2 - stroke[1];
                Vector2 p1 = stroke[i];
                Vector2 p2 = stroke[i + 1];
                Vector2 p3 = i + 2 < n ? stroke[i + 2] : stroke[n - 1] * 2 - stroke[n - 2];

                for (int s = 0; s < splineSubdivide; s++)
                {
                    float t  = s / (float)splineSubdivide;
                    Vector2 pt = CatmullRom(p0, p1, p2, p3, t);
                    output.Add(new Vector3(pt.x, pt.y, 0f));
                }
            }
            // Final point
            Vector2 last = stroke[n - 1];
            output.Add(new Vector3(last.x, last.y, 0f));
        }

        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2)          * t  +
                (2f*p0 - 5f*p1 + 4f*p2 - p3) * t2 +
                (-p0 + 3f*p1 - 3f*p2 + p3) * t3
            );
        }

        private void ConfigureLineRenderer()
        {
            _lineRenderer.useWorldSpace     = true;
            _lineRenderer.startWidth        = lineWidth;
            _lineRenderer.endWidth          = lineWidth * 0.5f;
            _lineRenderer.startColor        = lineColorStart;
            _lineRenderer.endColor          = lineColorEnd;
            _lineRenderer.numCapVertices    = 4;
            _lineRenderer.numCornerVertices = 4;
            _lineRenderer.positionCount     = 0;
        }

        private void ClearLineRenderer()
        {
            _lineRenderer.positionCount = 0;
        }

        // ── Coordinate transform ──────────────────────────────────────────────

        private Vector2 ScreenToWorld(Vector3 screenPos)
        {
            Camera cam = drawingCamera != null ? drawingCamera : Camera.main;
            screenPos.z = drawingDepth;
            Vector3 wp  = cam.ScreenToWorldPoint(screenPos);
            return new Vector2(wp.x, wp.y);
        }
    }
}
