# 3. Hafta Raporu: Canlı Veri Entegrasyonu (API)

## Hedef
Dış dünyanın spor verilerini (gerçek maçları, takımları ve puan durumlarını) kendi veritabanımıza dahil edebilecek mekanizmaları kurmak.

## Neler Yapıldı?
Proje artık gerçek bilgilerle donatılmaya başlandı.
* **Sportmonks API Entegrasyonu:** Gerçek dünya spor verileri sağlaması için "Sportmonks V3 Football API" sağlayıcısı ile anlaşılarak gerekli token bilgisi `appsettings.json` aracılığıyla projeye bağlandı.
* **HttpClient Hizmeti:** Gelen JSON verilerini hızlı bir şekilde parse edebilmek amacıyla `ExternalApiService` yazıldı. Ligler, Takımlar, Fikstürler (Fixtures) ve Puan Durumunu (Standings) getiren 4 farklı dış istek fonksiyonu oluşturuldu.
* **Veri İşleme ve Kayıt:** `DataProcessingService.cs` adlı bir katman oluşturuldu. Bu katman sayesinde Dış API'den okunan JSON içindeki karmaşık array yapıları okunup, daha öncesinde modellediğimiz kendi `Team` veya `Match` sınıflarımıza map'lenip (dönüştürülüp) güvenli adımlarla SQL veritabanına yazıldı. Yabancı anahtar çakışmalarını (%100 eşleşme sağlaması için) denetleyen mantıklar kuruldu.

## Sonuç ve Çıktı
Artık üçüncü şahıs bir sistemde olan sayısız spor verisi ve ID'leri okumaya hazır, JSON çıktılarını SQL formuna temizce çeviren çalışan bir backend mimarimiz oldu.

---
**Durum:** Tamamlandı (%100)
