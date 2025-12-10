using UnityEngine;

[CreateAssetMenu(menuName = "Narrative/Guide", fileName = "GuideData")]
public class GuideData : ScriptableObject
{
    [TextArea(3, 10)]
    [SerializeField] private string content;

    public string Content => content;

}