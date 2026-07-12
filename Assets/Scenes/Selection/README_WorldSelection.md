# World Selection ("Select a Story")

Lapisan meta-game baru: dunia 3D top-down berisi beberapa gedung. Pemain memilih story
dengan mengklik gedung → kamera zoom in → narasi intro (hanya first visit, bisa di-skip)
→ gameplay first person. Keluar lewat pintu sebelum pintu terkunci (`FirstPuzzleEvent`)
→ kembali ke peta dengan zoom out. Story terbuka berurutan; ending apa pun menandai
story selesai (mati tidak dihitung) dan endingnya dicatat di save.

## Arsitektur singkat

- `StoryDefinition` / `StoryDatabase` (SO, `Assets/Resources/Config/Story/`) — data per story:
  id, judul, deskripsi, paragraf intro, `SceneGroup` gameplay. Urutan list di database = urutan unlock.
- `FlowManager` (service) — jantung flow: `OpenSelection()`, `PlayStory()`, `ReturnToSelection()`,
  `RestartCurrentStory()`, progression (`IsStoryUnlocked`, `MarkIntroSeen`, `MarkCompleted`).
  Save: `StoryProgress.json` via `SaveLoadManager` (`StoryProgressSaveData`).
- `SceneService.LoadSceneGroup(group)` (overload public baru) — load gameplay group langsung
  dari data story, tanpa menambah `SceneEnum`. `SceneEnum.SELECTION` → `Selection_Group`.
- Scene `WorldSelection.unity` — `WorldOrbitCameraRig` (drag kiri: orbit, drag kanan/tengah: pan,
  scroll: zoom), `WorldSelectionController` (hover/klik gedung, koreografi zoom, replay intro),
  `StoryBuilding` per gedung, `StoryInfoPanel`, `SelectionInstaller` (musik `MusicEventType.SelectionMap`).
- `StoryIntroPanel` + `StoryIntroController` (di "In Game UI") — intro first-visit, klik = lanjut,
  Esc = skip. TIDAK memakai event `OpenDialogNarrtiveUI` (itu milik jurnal in-game).
- `StoryExitTrigger` (di "In Game", di luar ambang pintu) — keluar sebelum pintu terkunci →
  balik ke peta. Nonaktif otomatis saat `FirstPuzzleEvent`.
- `EndGameListener` sekarang juga publish `StoryCompletedEvent { endingId }` → dicatat FlowManager.

## Setup satu kali (WAJIB, di Unity Editor)

1. **Tools > World Selection > Build Blockout Scene** — mengisi `WorldSelection.unity`
   dengan blockout (ground, 3 gedung, kamera, UI). Aman dijalankan ulang (menimpa isi scene).
2. **Tools > World Selection > Setup Gameplay Scenes (Intro + Exit Trigger)** —
   menambah intro panel ke "In Game UI" dan `StoryExitTrigger` ke "In Game".
3. **Posisikan `StoryExitTrigger`** di scene "In Game": letakkan tepat DI LUAR ambang pintu
   keluar (jangan sampai menyentuh posisi spawn player), lalu **aktifkan GameObject-nya**
   (sengaja dibuat nonaktif oleh tool).

Opsional:
- Isi `endingTitle` pada record `EndGameConfig` (`Assets/Resources/Config/Narrative/EndGameConfig.asset`)
  supaya nama ending di peta enak dibaca (default: "Ending <jumlah collectible>").
- Mapping musik `SelectionMap` di asset `MusicEventData` supaya peta punya musik sendiri.
- Assign `typingSfx` pada `StoryIntroPanel` (di kedua scene) untuk bunyi ketikan narasi.

## Menambah story baru

1. Buat scene gameplay + scene UI-nya, lalu buat `SceneReference` + `SceneGroup` (menu
   `Create > Scene Management`). Tambahkan scene ke Build Settings.
2. Buat asset `Create > Config > Story > Story Definition` — isi id unik (JANGAN diubah
   setelah rilis; jadi kunci save), judul, deskripsi, paragraf intro, dan `SceneGroup` tadi.
3. Masukkan ke `StoryDatabase.asset` di posisi urutan unlock yang diinginkan.
4. Di `WorldSelection.unity`: buat/duplikat gedung, pasang `StoryBuilding`, assign story
   asset + `ZoomCameraAnchor` (pose kamera di depan pintu) + visual locked/completed.
5. Scene gameplay story baru butuh: `InteractableEndGameDoor` + `StoryExitTrigger` di pintu,
   `EndGameListener` di zona ending, installer scene, dan "In Game UI"-nya memuat
   `StoryIntroPanel` + `StoryIntroController` (atau pakai scene UI bersama).

## Checklist verifikasi

- [ ] Save baru: Menu → Play → peta; hanya gedung Story 1 bisa dimasuki; 2 placeholder gelap.
- [ ] Klik gedung 1 → zoom in → fade → intro narasi (klik lanjut, Esc skip) → gameplay.
- [ ] Keluar pintu sebelum ambil item → fade → peta + kamera zoom out dari gedung; masuk lagi → tanpa intro, keadaan fresh.
- [ ] Ambil item pertama → pintu terkunci; keluar tidak bisa lagi.
- [ ] Mati → "Coba Lagi" reload story / quit → peta; TIDAK tercatat selesai.
- [ ] Tamat → layar ending → kembali → peta: Story 1 "Selesai — Ending: X", Story 2 terbuka ("Segera hadir" karena placeholder).
- [ ] Restart aplikasi → progress bertahan (`StoryProgress.json` di persistentDataPath).
- [ ] Play langsung scene "In Game" dari editor tetap jalan (fallback Story 1).
