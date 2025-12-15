using UnityEngine;

public class EndGameListener : MonoBehaviour, IRestartable
{
    [SerializeField] private EndGameConfig endGameConfig;
#if UNITY_EDITOR
    [SerializeField, ReadOnly]
#endif
    private int _currentPuzzleCount = 0;
#if UNITY_EDITOR
    [SerializeField, ReadOnly]
    //private int puzzleCount => _currentPuzzleCount;
#endif
    private int puzzleCount => _currentPuzzleCount;

    private void OnEnable()
    {
        EventBus.Subscribe<InteractedPuzzleCount>(OnPuzzleCountChange);
        RestartManager.Register(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InteractedPuzzleCount>(OnPuzzleCountChange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var nearestEndGame = endGameConfig.GetNearestEndGame(_currentPuzzleCount);
            EventBus.Publish(new EndGame.OpenEndGameUI()
            {
                content = nearestEndGame.Area
            });
        }
    }

    private void OnPuzzleCountChange(InteractedPuzzleCount evt)
    {
        _currentPuzzleCount = evt.puzzleCount;
    }

    public void Restart()
    {
        _currentPuzzleCount = 0;
    }

}