using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Journal Config")]
public class JournalDatabaseConfig : ScriptableObject
{
    [Serializable]
    public class AreaString
    {
        [TextArea(3, 10)]
        public string Area;
    }

    [SerializeField] private AreaString[] journals;

    public string[] Journals => journals.Select(x => x.Area).ToArray();
}