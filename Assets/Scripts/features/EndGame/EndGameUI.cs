using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class EndGame : FadeShowHideProcedural, IRestartable
{
    public struct OpenEndGameUI
    {
        public string content;
    }

    [SerializeField] private TextMeshProUGUI contentText;


    [Header("Configuraiton")]
    [SerializeField] private float characterRevealSpeed = 0.1f;
    [SerializeField] private SfxClipData typingSfx;
    [SerializeField] private float characterRevealWaitTime = 1f;
    [SerializeField] private AudioSource audioSourcesForTypingSound;


    private TextAnimationHelper _textAnimationHelper;

    protected override void OnEnable()
    {
        RestartManager.Register(this);
        base.OnEnable();
        EventBus.Subscribe<OpenEndGameUI>(OnEndGameShow);
        _textAnimationHelper = new(contentText);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<OpenEndGameUI>(OnEndGameShow);
        RestartManager.Unregister(this);
    }

    private void OnEndGameShow(OpenEndGameUI evt)
    {
        AudioManager audioManager = ServiceLocator.Get<AudioManager>();

        StartCoroutine(_textAnimationHelper.RevealTextWithTypingSoundUnscaledTime(evt.content, characterRevealSpeed, typingSfx, audioSourcesForTypingSound, audioManager, 2));
        ShowUI();
    }

<<<<<<< Updated upstream
    public void ExitToMainMenu() => ExitToMainMenuAsync().Forget(nameof(ExitToMainMenu));

    private async Task ExitToMainMenuAsync()
=======
    // Name kept as ExitToMainMenu so existing serialized button bindings keep working,
    // but finishing a story now returns to the world selection map (FlowManager decides
    // and falls back to the main menu when no selection scene is registered).
    public async void ExitToMainMenu()
>>>>>>> Stashed changes
    {
        await ServiceLocator.Get<FlowManager>().ReturnToSelection();
        HideUI();
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
    }

    public void RestartGame() => RestartGameAsync().Forget(nameof(RestartGame));

    private async Task RestartGameAsync()
    {
        await ServiceLocator.Get<FlowManager>().RestartCurrentStory();
        HideUI();
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
    }

    public void Restart()
    {
        StopAllCoroutines();
    }
}