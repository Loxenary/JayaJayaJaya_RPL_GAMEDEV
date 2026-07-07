## Apa & kenapa
<!-- 2-3 kalimat: apa yang berubah dan kenapa -->

## Cara menguji manual
<!-- Langkah reproduksi verifikasi, scene mana, expected result -->

## Checklist
- [ ] Build Win64 lokal sukses (atau CI hijau)
- [ ] Diverifikasi di PLAYER BUILD (untuk perubahan gameplay — bukan hanya editor)
- [ ] Restart-soak dijalankan (bila menyentuh state/scene/restart — lihat docs/game-mitigation-checklist.md)
- [ ] Tidak menambah `Debug.Log` langsung (pakai `BetterLogger`)
- [ ] Tidak menambah `static event` baru (lintas-fitur = EventBus, lihat docs/messaging.md)
- [ ] Angka game-feel (kecepatan/drain/durasi) di serialized field/config, bukan konstanta

## Scene/prefab terdampak
<!-- Daftar file .unity/.prefab yang berubah — reviewer harus tahu YAML mana yang berubah -->
