/// <summary>
/// Rumah terpusat semua teks UI yang muncul ke pemain (T-P3.6).
///
/// Aturan: string UI BARU ditaruh di sini, bukan literal di kode — supaya
/// kelak lokalisasi tinggal mengganti satu file, bukan berburu literal.
/// Field [SerializeField] di komponen boleh memakai konstanta ini sebagai
/// nilai default; nilai di scene/prefab tetap menang bila designer mengubahnya.
/// </summary>
public static class GameStrings
{
    // Inventory
    public const string KeyNamePrefix = "Kunci #";

    // Umum
    public const string InteractPrompt = "Tekan [E] untuk berinteraksi";
}
