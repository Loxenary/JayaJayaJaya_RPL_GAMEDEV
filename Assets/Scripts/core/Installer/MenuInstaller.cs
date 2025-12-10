using UnityEngine;

/// <summary>
/// Installer for the Main Menu scene.
/// Initializes UI and menu-specific systems.
/// </summary>
public class MenuInstaller : BaseInstaller
{
    [Header("Menu Music")]
    [SerializeField] private MusicIdentifier menuMusic = MusicIdentifier.MainMenuBGM;

    protected override void Install()
    {
        var audioManager = ServiceLocator.Get<AudioManager>();

        audioManager.StopMusic();
        audioManager.PlayMusic(menuMusic);
    }
}
