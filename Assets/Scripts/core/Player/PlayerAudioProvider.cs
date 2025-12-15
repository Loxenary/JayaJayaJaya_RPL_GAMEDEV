using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAudioProvider : MonoBehaviour
{
    [Header("Audio SFX Data")]
    [SerializeField] SfxClipData walk;
    [SerializeField] SfxClipData run;


    bool isPlay;
    PlayerInputHandler input;
    AudioSource audioSource;
    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        audioSource = GetComponent<AudioSource>();

        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = true;
    }
    private void OnEnable()
    {
        input.OnWalkPerformed += OnWalk;
        input.OnRunPerformed += OnRun;
        input.OnStopMovePerformed += OnStop;
    }

    private void OnStop()
    {
        audioSource.Stop();
        isPlay = false;
    }

    private void OnDisable()
    {
        input.OnWalkPerformed -= OnWalk;
        input.OnRunPerformed -= OnRun;
        input.OnStopMovePerformed -= OnStop;

    }
    private void OnRun()
    {
        if (isPlay) return;
        isPlay = true;
        audioSource.resource = run.Clip;
        audioSource.Play();
    }

    private void OnWalk()
    {
        if (isPlay) return;
        isPlay = true;
        audioSource.resource = walk.Clip;
        audioSource.Play();

    }
}
