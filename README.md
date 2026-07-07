# Lakshana

Game horor first-person: jelajahi mansion, kumpulkan petunjuk, jaga kewarasanmu,
dan kabur sebelum sanity habis.

## Prasyarat

- **Unity 6000.3.19f1** (install versi PERSIS ini via Unity Hub)
- **Git + Git LFS** — WAJIB `git lfs install` SEBELUM clone.
  Tanpa LFS, semua texture/model berupa pointer text dan game tampak rusak
  (checkered/pink). Panduan lengkap: [docs/git-lfs-setup.md](docs/git-lfs-setup.md)

## Setup

```bash
git lfs install
git clone https://github.com/Loxenary/JayaJayaJaya_RPL_GAMEDEV.git
# Buka folder di Unity Hub, tunggu import (15-30 menit pertama kali)
```

## Menjalankan

- Scene entry utama: `Assets/Scenes/Core/Bootstrap.unity` → Play.
- Menjalankan scene gameplay langsung juga bisa (SceneBootstrapper fallback
  akan menginisialisasi service secara lokal).

## Build

- File → Build Settings → Windows x86_64 → Build.
- Setelah CI aktif: unduh artifact `build-win64` dari tab GitHub Actions.

## Struktur

| Folder | Isi |
|---|---|
| `Assets/Scripts/core` | Service engine-level: scene, time, save, checkpoint, bootstrap, enemy AI |
| `Assets/Scripts/features` | Sistem gameplay: sanity, ghost, interactable, narrative, damage |
| `Assets/Scripts/ui` | HUD, main menu, settings |
| `Assets/Scripts/lib` | EventBus, ServiceLocator, BetterLogger |
| `Assets/Scripts/config` | ScriptableObject konfigurasi (player, enemy, journal, ambience) |
| `docs/` | Arsitektur, konvensi messaging, audit, checklist mitigasi |

## Kontribusi

- Branch: `feat/<deskripsi>` · `fix/<deskripsi>` — PR ke `dev`, wajib 1 review.
- Baca [docs/messaging.md](docs/messaging.md) (aturan event) dan template PR.
- **Definisi selesai: terverifikasi di PLAYER BUILD, bukan hanya editor.**
- Konvensi kode: namespace untuk file baru, `BetterLogger` bukan `Debug.Log`,
  tidak ada `static event` baru, `async void` hanya Unity message ber-try/catch
  (gunakan `Task.Forget()`), angka game-feel di serialized field/config.

## Dokumen penting

- [docs/game-audit-2026-07.md](docs/game-audit-2026-07.md) — audit menyeluruh + master plan
- [docs/game-mitigation-checklist.md](docs/game-mitigation-checklist.md) — checklist eksekusi P0–P3
- [docs/git-lfs-setup.md](docs/git-lfs-setup.md) — setup Git LFS
