/// <summary>
/// Event "dunia berubah setelah puzzle pertama": memicu spawn ghost
/// (GhostManager/GhostListener), perubahan lighting (LightingHandler),
/// buka pintu endgame (InteractableEndGameDoor), dan reposisi guide.
///
/// JANGAN TERTUKAR dengan CollectibleManager.FirstPuzzleCollectedEvent (B-17):
/// - FirstPuzzleCollectedEvent = momen COLLECT puzzle pertama; hanya
///   di-publish CollectibleManager; didengar SanityTimerSystem.
/// - FirstPuzzleEvent (ini)     = trigger reaksi dunia; di-publish
///   CollectibleManager, InteractableFirstPuzzle, dan FirstPuzzleInvoker.
/// Subscriber baru: pilih berdasarkan MAKNA di atas, bukan nama.
/// </summary>
public struct FirstPuzzleEvent
{

}
