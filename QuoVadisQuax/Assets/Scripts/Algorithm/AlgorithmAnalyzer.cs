using UnityEngine;

public class AlgorithmAnalyzer : MonoBehaviour
{
    #region Methods

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR

        optionsManager.StartedAlgorithm += (a, b) =>
        {
            UpdateGridOffsetTime = 0;
            PathfindingOffsetTime = 0;
            QuadtreeOffsetTime = 0;
            SpecialSearchOffsetTime = 0;
        };

#endif
    }

    #endregion

    #region Properties

    [SerializeField] private OptionsManager optionsManager;

    /// <summary>
    ///     The Singleton Instance
    /// </summary>
    public static AlgorithmAnalyzer Instance { get; private set; }

#if DEVELOPMENT_BUILD || UNITY_EDITOR

    public long LastAlgorithmStepTime { get; set; }

    public long UpdateGridOffsetTime { get; set; }

    public long PathfindingOffsetTime { get; set; }

    public long QuadtreeOffsetTime { get; set; }

    public long SpecialSearchOffsetTime { get; set; }

#endif

    #endregion
}