using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BootstrapConfig", menuName = "Config/BoostrapConfig")]

public class BootstrapConfig : ScriptableObject
{

    [Header("Game Configuration")]

    [Tooltip("A list of scene that will be persistent througout the game")]
    [SerializeField] private List<SceneReference> persistanceSceneReferences;

    [Tooltip("The first scene to load")]
    [SerializeField] private SceneEnum initialScene;

    public List<SceneReference> PersistanceSceneReferences => persistanceSceneReferences;

    public SceneEnum InitialScene => initialScene;
}