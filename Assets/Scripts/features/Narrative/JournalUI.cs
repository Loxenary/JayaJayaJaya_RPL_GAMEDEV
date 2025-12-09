using TMPro;
using UnityEngine;

public class JournalUI : FadeShowHideProcedural
{
    public struct OpenJournalUI
    {
        public string content;
    }

    [SerializeField] private TextMeshProUGUI contentTextJournal;

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus.Subscribe<OpenJournalUI>(OnOpenJournalUI);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus.Unsubscribe<OpenJournalUI>(OnOpenJournalUI);
    }

    private void OnOpenJournalUI(OpenJournalUI evt)
    {
        contentTextJournal.text = evt.content;
        ShowUI();
    }
}