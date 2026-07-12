using Ambience;

/// <summary>
/// Installer for the world selection scene.
/// Stops whatever music was playing and requests the selection-map track.
/// </summary>
public class SelectionInstaller : BaseInstaller
{
    protected override void Install()
    {
        Log("Installing World Selection scene dependencies...");

        var audioManager = ServiceLocator.Get<AudioManager>();
        audioManager?.StopMusic();

        EventBus.Publish(new MusicEventRequest(MusicEventType.SelectionMap));

        Log("World Selection scene installation complete!");
    }
}
