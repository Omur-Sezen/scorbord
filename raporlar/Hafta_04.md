# 4. Hafta Raporu: Otomatik Güncelleme Servisi

## Hedef
Maç skorlarının değişmesi veya puan durumlarının yenilenmesi esnasında insanın manuel API'yi tetiklemesine gerek kalmadan işi yazılımın tek başına yapması.

## Neler Yapıldı?
Uygulama yayındayken tamamen arka planda bağımsız bir şekilde çalışan ve belirli sürelerde veritabanını besleyen modern bir asenkron görev başlatıcı oluşturuldu.
* **BackgroundService (Worker) Mekanizması:** .NET kütüphanesinin çekirdek `BackgroundService` yapısından türeyen `ScoreUpdateBackgroundService.cs` hayata geçirildi.
* **Döngü ve Periyotlama:** Belirlenen süre aralıklarında (`Task.Delay` ile) durmaksızın uyanarak Sportmonks API'sine istek gönderip güncel canlı maç skorlarını (live scores) okuyan kod bloğu çalışır duruma getirildi.
* **Service Scoping:** Background worker'lar singleton çalıştıkları için Transient ve Scoped sınıflarla çakışmamaları amacıyla `IServiceScopeFactory` yapısı kullanılarak veritabanı context'lerinin iş bitince güvenle kapanması (memory leak olmaması) sağlandı.
* **Lig İçi Seeding:** 3. Haftada yazılan API kodları, döngünün içerisine oturtularak hem verisi eksik liglerin takımlarının indirilmesi hem de canlı puan durumu listesinin (`Standings`) daima güvende tutulması başarıldı.

## Sonuç ve Çıktı
Proje, Visual Studio veya sunucuda "Run" edildiği andan itibaren hiç yorulmadan canlı skorları ve puan tablolarını çeken tıkır tıkır işleyen dev bir motor halini aldı.

---
**Durum:** Tamamlandı (%100)
