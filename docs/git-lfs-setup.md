# Git LFS Setup & Repo Size Fix

## Kenapa dokumen ini ada

`.git` project ini sempat membengkak sampai **~5.7 GB** karena file binary
besar (texture PNG/TIF, model FBX, audio, dll) di-commit langsung ke git
tanpa Git LFS. Git tidak bisa mem-delta-compress file binary dengan baik,
jadi setiap kali texture/model diedit sedikit saja, versi barunya tersimpan
utuh lagi di histori.

Jumlah **file** yang di-track git (~13.500) itu sendiri **normal** untuk
project Unity ini (tiap asset punya file `.meta` pasangan, ditambah banyak
asset pack besar seperti `Flooded_Grounds` dan `MonsterMutant 7`). Yang jadi
masalah adalah **ukuran** repo, bukan jumlah filenya.

## Yang sudah dipasang (aman, tidak mengubah histori lama)

1. [Git LFS](https://git-lfs.com/) — package terpisah dari git, perlu
   diinstall manual (lihat langkah instalasi di bawah).
2. `.gitattributes` di root project — mendaftarkan tipe file binary Unity
   supaya **file baru / yang diedit ke depannya** otomatis lewat LFS:
   png, tga, tif/tiff, psd, exr, fbx, wav, mp3, ogg, aiff/aif, mp4, mov,
   unitypackage, gif, jpg/jpeg, hdr, ttf, otf.

Perubahan ini **tidak mengecilkan** `.git` yang sudah 5.7 GB — itu cuma
menghentikan pembengkakan lebih lanjut. Untuk benar-benar mengecilkan,
lihat bagian "Migrasi histori lama" di bawah.

## Setup wajib untuk SETIAP anggota tim (sekali per komputer)

Setiap orang di tim harus melakukan ini setelah `git pull` perubahan
`.gitattributes` ini, kalau tidak, file besar akan ke-download/ke-upload
sebagai file biasa (rusak/korup) alih-alih lewat LFS.

```bash
# 1. Install Git LFS (sekali per komputer)
# Windows: download installer dari https://git-lfs.com/ , atau via winget/choco:
winget install GitHub.GitLFS
# atau: choco install git-lfs

# 2. Aktifkan LFS untuk git di komputer ini (sekali per komputer, bukan per repo)
git lfs install

# 3. Di dalam folder project ini, tarik perubahan .gitattributes
git pull

# 4. Pastikan file yang sudah ada di working directory ikut ter-convert
#    ke pointer LFS sesuai .gitattributes yang baru
git add --renormalize .
git status   # review dulu sebelum commit
git commit -m "chore: renormalize assets to git lfs"
```

Setelah ini, `git add` pada file `.png`/`.fbx`/dll baru akan otomatis
disimpan sebagai LFS object, bukan blob biasa.

## Migrasi histori lama (opsional, BELUM dijalankan — perlu koordinasi tim)

Ini yang benar-benar mengecilkan `.git` dari 5.7 GB, tapi **rewrite semua
commit hash** di seluruh histori. Konsekuensinya:

- Perlu `git push --force` ke semua branch yang di-migrasi.
- **Semua kolaborator wajib re-clone repo** (branch lokal lama mereka
  tidak akan bisa di-merge/rebase dengan histori baru).
- Branch remote lain (`dev`, `aan`, `feat/*`, dst) juga perlu
  dikoordinasikan — sebaiknya semua orang push/merge dulu pekerjaan
  mereka sebelum migrasi dijalankan.
- GitHub punya kuota Git LFS storage/bandwidth gratis terbatas per bulan;
  cek kebutuhan tim sebelum migrasi besar-besaran.

Langkah migrasi (dijalankan SEKALI oleh satu orang, saat tim sudah setuju):

```bash
# Pastikan tidak ada perubahan uncommitted, dan semua branch penting sudah di-push
git lfs migrate import --include="*.png,*.tga,*.tif,*.tiff,*.psd,*.exr,*.fbx,*.wav,*.mp3,*.ogg,*.aiff,*.aif,*.mp4,*.mov,*.unitypackage,*.gif,*.jpg,*.jpeg,*.hdr,*.ttf,*.otf" --everything

# Verifikasi ukuran .git sudah mengecil dan history masih benar
git log --oneline -10
du -sh .git

# Force-push semua branch yang ikut di-migrasi
git push --force --all
git push --force --tags
```

Setelah force-push, beri tahu semua kolaborator untuk:

```bash
# Clone ulang dari nol (paling aman), ATAU jika ingin pertahankan clone lama:
git fetch origin
git reset --hard origin/<branch-mereka>
```

## Status migrasi (2026-07-03)

Migrasi histori sudah **dijalankan dan di-push**:

- `main` — histori di-rewrite, force-pushed. ✅
- `version/v1.4` — pushed sebagai branch baru dengan histori ter-LFS-kan. ✅
- `dev` — **tertunda**, ditolak GitHub karena branch protection rule
  ("cannot force-push"). Perlu admin repo melonggarkan rule itu sementara
  di Settings → Branches, baru di-force-push ulang, lalu rule diaktifkan
  lagi.
- 30 branch remote lain (`feat/*`, `aan`, `bugfix-*`, dll) **belum**
  di-migrasi — disepakati tim sudah mati/merged, aman diabaikan. Kalau
  ternyata masih ada yang perlu dipakai, branch itu harus di-migrasi
  manual dulu sebelum di-merge ke `main`/`dev` baru (histori lama tidak
  kompatibel lagi dengan histori baru).

### WAJIB untuk semua anggota tim setelah ini

Karena histori `main` (dan nanti `dev`) di-rewrite total, **repo lokal siapa
pun yang sudah clone sebelumnya TIDAK BISA `git pull` biasa** — commit
hash-nya sudah beda semua. Cara teraman:

```bash
# Backup pekerjaan yang belum di-push dulu kalau ada, lalu:
cd ..
rm -rf lakshana   # atau nama folder repo masing-masing
git clone https://github.com/Loxenary/JayaJayaJaya_RPL_GAMEDEV.git lakshana
cd lakshana
git lfs install   # sekali per komputer kalau belum pernah
```

Kalau ada kerjaan di branch fitur lama yang based on `main`/`dev` lama dan
belum di-merge, branch itu harus di-`cherry-pick` manual ke atas histori
baru (bukan di-merge/rebase langsung, karena history-nya "unrelated").

## Referensi

- Git LFS: https://git-lfs.com/
- `git lfs migrate` docs: https://github.com/git-lfs/git-lfs/blob/main/docs/man/git-lfs-migrate.adoc
- Unity + Git LFS guide: https://docs.unity3d.com/Manual/Git-LFS-Support.html
