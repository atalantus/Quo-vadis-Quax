using System;
using System.Diagnostics;
using Algorithm.Pathfinding;
using Algorithm.Quadtree;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Algorithm
{
    /// <inheritdoc />
    /// <summary>
    ///     General algorithm Manager
    /// </summary>
    public class AlgorithmManager : MonoBehaviour
    {
        #region Properties

        /// <summary>
        ///     The Singleton Instance
        /// </summary>
        public static AlgorithmManager Instance { get; private set; }

        /// <summary>
        ///     Finished algorithm delegate
        /// </summary>
        /// <param name="foundPath">Was a path found or not</param>
        /// <param name="quadcopterFlights">Amount of Quadcopter flights</param>
        /// <param name="algorithmTime">Algorithm execution time</param>
        /// <param name="executionTime">Total execution time</param>
        public delegate void FinishedAlgorithmEventHandler(bool foundPath, int quadcopterFlights,
            TimeSpan algorithmTime, TimeSpan executionTime);

        /// <summary>
        ///     Finished algorithm event
        /// </summary>
        public event FinishedAlgorithmEventHandler FinishedAlgorithm;

        /// <summary>
        ///     OptionsManager reference
        /// </summary>
        [SerializeField] private OptionsManager _optionsManager;

        /// <summary>
        ///     ContainerManager reference
        /// </summary>
        [SerializeField] private ContainerManager _containerManager;

        /// <summary>
        ///     Preparing algorithm message ID
        /// </summary>
        private const string PREPARING_ALGORITHM_MSG_ID = "preparing_algorithm";

        /// <summary>
        ///     Searching path message ID
        /// </summary>
        private const string SEARCHING_PATH_MSG_ID = "searching_path";

        /// <summary>
        ///     Found a path
        /// </summary>
        private bool _foundPath;

        /// <summary>
        ///     Counter for Quadcopter flights
        /// </summary>
        private int _quadcopterFlights;

        /// <summary>
        ///     General algorithm Stopwatch
        /// </summary>
        public Stopwatch Stopwatch { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Unity Awake
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
        }

        /// <summary>
        ///     Unity Start
        /// </summary>
        private void Start()
        {
            // Make sure to run as fast as possible
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 300;

            Stopwatch = new Stopwatch();

            /**
             * Subscribe to events
             */
            _optionsManager.StartedAlgorithm += (a, b) =>
            {
                _containerManager.CreateMessage("Preparing Quadcopter", PREPARING_ALGORITHM_MSG_ID, true);
            };

            PathfindingManager.Instance.StartedPathfinding += () =>
            {
                _containerManager.DestroyMessage(PREPARING_ALGORITHM_MSG_ID);
                _containerManager.CreateMessage("Finding Path", SEARCHING_PATH_MSG_ID, true);
                Stopwatch.Start();
            };

            PathfindingManager.Instance.FinishedPathfinding += (a, foundPath) =>
            {
                Stopwatch.Stop();
                _foundPath = foundPath;
            };

            QuadtreeManager.Instance.CreatedNode += a => { _quadcopterFlights++; };
        }

        /// <summary>
        ///     Start algorithm
        /// </summary>
        /// <param name="quaxPos"></param>
        /// <param name="cityPos"></param>
        public void StartAlgorithm(Vector2Int quaxPos, Vector2Int cityPos)
        {
            _foundPath = false;
            _quadcopterFlights = 0;
            Stopwatch.Reset();

            // Setup Quadtree and Pathfinding
            QuadtreeManager.Instance.SetupQuadtree();
            PathfindingManager.Instance.SetupPathfinding(quaxPos, cityPos);
        }

        /// <summary>
        ///     Finished algorithm
        /// </summary>
        public void FinishAlgorithm()
        {
            TimeSpan ts;

#if DEVELOPMENT_BUILD || UNITY_EDITOR

            // Get pure algorithm time
            var time = QuadtreeManager.Instance.QuadtreeTime +
                       QuadtreeManager.Instance.SpecialSearchTime +
                       PathfindingManager.Instance.TotalTimeUpdateGrid +
                       PathfindingManager.Instance.TotalTimePathfinding;

            ts = TimeSpan.FromMilliseconds(time);

#else
            ts = Stopwatch.Elapsed;

#endif

            _containerManager.DestroyMessage(SEARCHING_PATH_MSG_ID);
            _containerManager.CreateMessage(ts.Seconds + "s " + ts.Milliseconds + "ms", "algorithm_time", false, 5f);

            if (FinishedAlgorithm != null)
                FinishedAlgorithm.Invoke(_foundPath, _quadcopterFlights, ts, Stopwatch.Elapsed);

#if DEVELOPMENT_BUILD || UNITY_EDITOR

            var totalOffset = AlgorithmAnalyzer.Instance.QuadtreeOffsetTime +
                              AlgorithmAnalyzer.Instance.SpecialSearchOffsetTime +
                              AlgorithmAnalyzer.Instance.UpdateGridOffsetTime +
                              AlgorithmAnalyzer.Instance.PathfindingOffsetTime;

            Debug.Log("===========================");
            Debug.Log("----- ALGORITHM TIMES -----");
            Debug.Log("===========================");
            Debug.Log("Quadtree Searches: " + QuadtreeManager.Instance.QuadtreeTime + " ms");
            Debug.Log("Special Searches: " + QuadtreeManager.Instance.SpecialSearchTime + " ms");
            Debug.Log("Updating A* Grid: " + PathfindingManager.Instance.TotalTimeUpdateGrid + " ms");
            Debug.Log("Pathfinding: " + PathfindingManager.Instance.TotalTimePathfinding + " ms");
            Debug.Log("-------------------------");
            Debug.Log("WHOLE ALGORITHM: " + time + " ms");
            Debug.Log("WHOLE PROGRAM: " + Stopwatch.ElapsedMilliseconds + " ms");
            Debug.Log("-------------------------");
            Debug.Log("Quadtree Offset: " + AlgorithmAnalyzer.Instance.QuadtreeOffsetTime + " ms");
            Debug.Log("Special Searches Offset: " + AlgorithmAnalyzer.Instance.SpecialSearchOffsetTime + " ms");
            Debug.Log("Updating A* Grid Offset: " + AlgorithmAnalyzer.Instance.UpdateGridOffsetTime + " ms");
            Debug.Log("Pathfinding Offset: " + AlgorithmAnalyzer.Instance.PathfindingOffsetTime + " ms");
            Debug.Log("TOTAL OFFSET: " + totalOffset + " ms");

#endif
        }

        #endregion
    }
}