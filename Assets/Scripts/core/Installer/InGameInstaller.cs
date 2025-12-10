using UnityEngine;
using Ambience;
using EnemyAI;

/// <summary>
/// Installer for the In-Game scene.
/// Initializes all systems and objects needed for gameplay.
/// </summary>
public class InGameInstaller : BaseInstaller
{


    protected override void Install()
    {
        Log("Installing In-Game scene dependencies...");

        StopMenuMusic();

        Log("In-Game scene installation complete!");
    }


    private void StopMenuMusic()
    {
        var audioManager = ServiceLocator.Get<AudioManager>();
        audioManager.StopMusic();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
