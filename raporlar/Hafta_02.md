# 2. Hafta Raporu: Veri Yönetimi (CRUD İşlemleri)

## Hedef
Veritabanına manuel veya otomatik müdahale edebilmek amacıyla gerekli uç noktaları (Endpoints) inşa edip test edilebilir hale getirmek.

## Neler Yapıldı?
Dış dünyadan gönderilecek HTTP isteklerini karşılamak için kontroller yapısı tasarlandı.
* **API Controller'lar:** Entity Framework ile hazırladığımız modeller için çeşitli REST API Controller dosyaları üretildi (`LeaguesController`, `TeamsController`, `MatchesController`, `StandingsController`). 
* **CRUD Fonksiyonları:** Takım ekleme, lig güncelleme ve silme, maçların listesini çekme gibi standart GET, POST, PUT, DELETE fonksiyonları kodlandı.
* **Servis Katmanı:** Veritabanı sorgularının controller'ları şişirmesini engellemek için kod mantığı yavaş yavaş servis sınıflarına aktarıldı.
* **Swagger Entegrasyonu:** Sistemin ve API'lerin kolayca test edilebilmesi için Swagger arayüzü kuruldu. Bu arayüz üzerinden herhangi bir Frontend kodu yazmadan takım eklenebiliyor, tüm tablolar sorgulanabiliyor konuma gelindi.

## Sonuç ve Çıktı
Frontend veya üçüncü parti bağımsız araçlardan (Postman, REST Client, vb.) gelen taleplere yanıt veren, kendi içinde veri ekleyip çıkarabilen tutarlı bir API (Backend) ayağa kaldırıldı.

---
**Durum:** Tamamlandı (%100)
