// File: SceneGroup.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "SceneManagement/SceneGroup")]
public class SceneGroup : ScriptableObject
{
    [SerializeField] private SceneReference activeScene;
    [SerializeField] private List<SceneReference> additionalScenes;

    public SceneReference ActiveScene => activeScene;
    public List<SceneReference> AdditionalScenes => additionalScenes;
}