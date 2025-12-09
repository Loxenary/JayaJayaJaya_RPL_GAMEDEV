using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class JournalPopup : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] float smoothSpeed = 5f;
    [SerializeField] float timeClose = 7f;


    CanvasGroup cg;

    public UnityEvent OnCloseJournal;
    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        Vector3 direction = canvas.worldCamera.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction*(-1), Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    public void OpenJournal()
    {
        cg.DOFade(1, 1.5f);

        DOTween.Sequence().SetDelay(timeClose).OnComplete(() => {
            CloseJournal();
            OnCloseJournal?.Invoke();
        });
    }
    public void CloseJournal()
    {
        cg.DOFade(0, .5f);

    }
}
