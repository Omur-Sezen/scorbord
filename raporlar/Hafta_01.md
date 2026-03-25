# 1. Hafta Raporu: Proje Temeli ve Veritabanı Tasarımı

## Hedef
Projeye sağlam bir altyapı oluşturmak ve veri modellerini Entity Framework Core aracılığıyla veritabanına sorunsuz bir şekilde yansıtmak.

## Neler Yapıldı?
Bu hafta projenin ilk adımları atıldı ve sistem mimarisi kuruldu.
* **Proje Başlatma:** ASP.NET Core Web API kullanılarak tek katmanlı (Monolith) modern bir proje iskeleti oluşturuldu.
* **Veritabanı Altyapısı:** Veritabanı olarak SQL Server (LocalDB) tercih edildi. Çekirdek veri tiplerimizi tutmak için `League`, `Team`, `Match` ve `Player` C# modelleri (entity'ler) kodlandı.
* **Entity Framework Bağlantısı:** `SkorbordDbContext` sınıfı üzerinden tablolarımız tanımlandı. Yabancı anahtar (Foreign Key) kısıtlamaları (Örn: `LeagueId`, `TeamId` üzerinden) Fluent API aracılığıyla özenle kuruldu.
* **Migration İşlemleri:** `InitialCreate` vb. isimlerle `dotnet ef migrations` kullanılarak fiziksel olarak veritabanı dosyası proje üzerinde oluşturuldu. Model kodlarıyla SQL Server birbirine bağlandı.

## Sonuç ve Çıktı
Uygulama başarıyla SQL Server'a bağlanıp derlenebilir duruma gelmiştir. Önümüzdeki hafta kurulacak API entegrasyonu ve CRUD işlemleri için sağlam ve ilişkisel veri kuralları belirlenmiştir.

---
**Durum:** Tamamlandı (%100)
