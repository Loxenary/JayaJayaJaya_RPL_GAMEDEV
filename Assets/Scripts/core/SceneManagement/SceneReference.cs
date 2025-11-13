using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(menuName = "SceneManagement/SceneReference")]
public class SceneReference : ScriptableObject
{
    [SerializeField] private string sceneName;

    // The public property now just returns the saved string.
    public string SceneName => sceneName;
#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
#endif

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            // Set the value of the serialized string field.
            sceneName = sceneAsset.name;
        }
        else
        {
            sceneName = "";
        }
#endif
    }
}