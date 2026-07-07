# Konvensi Messaging — Lakshana

Satu halaman. Kalau ragu event/komunikasi pakai apa, jawabannya di sini.
(Latar belakang: audit menemukan 4 mekanisme messaging paralel — lihat
`docs/game-audit-2026-07.md` Bagian 4.2.)

## Aturan

1. **Lintas-fitur → EventBus** (`Assets/Scripts/lib/EventBus/EventBus.cs`).
   Payload = struct bernama domain (`SanityChanged`, `FirstPuzzleEvent`,
   `StoryCompletedEvent`). Subscribe di `OnEnable`, unsubscribe di `OnDisable`
   — SELALU berpasangan.

2. **UnityEvent hanya untuk wiring designer di Inspector** (`onInteract`,
   `OnGhostSpawned`, dsb). Tidak pernah untuk komunikasi kode-ke-kode.

3. **Dalam satu fitur → referensi langsung / C# event instance.**
   Coupling internal fitur itu sehat; tidak semua hal butuh bus.

4. **`static event` DILARANG untuk kode baru.** Alasan: menjahit subscriber
   ke tipe konkret, bocor antar scene reload bila lupa unsubscribe, dan tidak
   terlihat di debug instrumentation EventBus. Yang sudah ada
   (`PlayerAttributes.onSanityUpdate` dkk.) dimigrasi bertahap ke EventBus
   saat file tersentuh (T-P2.2) — jangan big-bang.

5. **ServiceLocator hanya untuk service** (`ServiceBase<T>`). Resolve di
   `Awake`/`Initialize` dan simpan sebagai field — jangan resolve berulang
   inline di method.

## Jangan tertukar (B-17)

| Event | Makna | Publisher | Subscriber utama |
|---|---|---|---|
| `CollectibleManager.FirstPuzzleCollectedEvent` | Momen COLLECT puzzle pertama | CollectibleManager saja | SanityTimerSystem |
| `FirstPuzzleEvent` | Reaksi dunia (ghost spawn, lighting, endgame door) | CollectibleManager, InteractableFirstPuzzle, FirstPuzzleInvoker | GhostManager, GhostListener, LightingHandler, InteractableEndGameDoor, PosGuideAfterPuzzle |

## Async

- `async void` hanya boleh di Unity message (Awake/Start) dengan try/catch
  penuh membungkus body.
- Semua lainnya: `private async Task XAsync()` + entry point tipis
  `public void X() => XAsync().Forget(nameof(X));`
  (`Assets/Scripts/utils/TaskExtensions.cs`) — exception tercatat di
  BetterLogger, tidak hilang diam-diam.

## State & restart

- Setiap class dengan state sesi WAJIB `IRestartable` + register/unregister
  di OnEnable/OnDisable (pola `NarrativeSystem`).
- Setiap field `static` mutable baru harus menjawab di deskripsi PR:
  **"apa yang terjadi saat scene reload?"** (pelajaran B-02/B-08).
