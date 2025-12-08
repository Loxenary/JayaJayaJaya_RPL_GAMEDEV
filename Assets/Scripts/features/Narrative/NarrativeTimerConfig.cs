using UnityEngine;

[CreateAssetMenu(menuName = "Config/Narrative/Narrative Timer Config")]
public class NarrativeTimerConfig : ScriptableObject
{
    [SerializeField] private TimerContentRecord[] timerContentRecords;

    public TimerContentRecord[] TimerContentRecords => timerContentRecords;
}