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

    #endregion

    #region Properties

    /// <summary>
    ///     The Singleton Instance
    /// </summary>
    public static AlgorithmAnalyzer Instance { get; private set; }

    public long LastAlgorithmStepTime { get; set; }

    #endregion
}