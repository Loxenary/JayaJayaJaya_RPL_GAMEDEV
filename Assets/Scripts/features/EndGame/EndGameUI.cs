using TMPro;
using UnityEngine;

public class EndGame : FadeShowHideProcedural
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

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe<OpenEndGameUI>(OnEndGameShow);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<OpenEndGameUI>(OnEndGameShow);
    }

    private void OnEndGameShow(OpenEndGameUI evt)
    {
        AudioManager audioManager = ServiceLocator.Get<AudioManager>();

        StartCoroutine(TextAnimationHelper.RevealTextWithTypingSound(contentText, evt.content, characterRevealSpeed, typingSfx, audioSourcesForTypingSound, audioManager, 2));
        ShowUI();
    }

    public async void ExitToMainMenu()
    {
        RestartManager.Restart();
        await ServiceLocator.Get<SceneService>().LoadScene(SceneEnum.MAIN_MENU);
        HideUI();
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
    }

    public async void RestartGame()
    {
        RestartManager.Restart();
        await ServiceLocator.Get<SceneService>().LoadScene(SceneEnum.IN_GAME);
        HideUI();
        ServiceLocator.Get<TimeService>().RequestResumeWhileClearingQueue();
    }
}