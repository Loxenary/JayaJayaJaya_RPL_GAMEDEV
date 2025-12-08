using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "BootstrapConfig", menuName = "Config/BoostrapConfig")]
public class BootstrapConfig : ScriptableObject
{

    [Header("Game Configuration")]

    [Tooltip("A list of scene that will be persistent througout the game")]
    [SerializeField] private List<SceneReference> persistanceSceneReferences;

    [Tooltip("The first scene to load")]
    [SerializeField] private SceneEnum initialScene;

    [Header("Testing Configuration (Editor Only)")]
    [Tooltip("Enable this to load a test scene instead of the default initial scene")]
    [SerializeField] private bool isTestingEnabled = false;

#if UNITY_EDITOR
    [Tooltip("The test scene to load when testing is enabled. Drag scene asset here.")]
    [SerializeField] private SceneAsset testSceneAsset;
#endif

    public List<SceneReference> PersistanceSceneReferences => persistanceSceneReferences;

    public SceneEnum InitialScene => initialScene;

    public bool IsTestingEnabled => isTestingEnabled;

#if UNITY_EDITOR
    /// <summary>
    /// Gets the scene path from the assigned SceneAsset (Editor only)
    /// </summary>
    public string TestScenePath => testSceneAsset != null ? AssetDatabase.GetAssetPath(testSceneAsset) : null;

    /// <summary>
    /// Gets the scene name from the assigned SceneAsset (Editor only)
    /// </summary>
    public string TestSceneName => testSceneAsset != null ? testSceneAsset.name : null;
#endif
}
