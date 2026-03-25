# SKORBORD API Kullanım Kılavuzu

## 🚀 Swagger API Arayüzü Amacı

Swagger API arayüzü (`http://localhost:5208/swagger`) şu amaçlar için kullanılır:

### 📊 **Veri Yönetimi**
- **Lig Ekleme**: Yeni ligler veritabanına eklenir
- **Takım Ekleme**: Takım bilgileri kaydedilir  
- **Maç Senkronizasyonu**: External API'den maç verileri çekilir
- **Puan Durumu**: Otomatik hesaplanan sıralamalar

### 🔄 **Veri Senkronizasyonu Endpoints**

#### 1. **Ligleri Senkronize Et**
```
POST /api/externaldata/sync/leagues
```
- Tüm ligleri API-Sports'tan çeker
- Veritabanına kaydeder

#### 2. **Takımları Senkronize Et**
```
POST /api/externaldata/sync/teams/{leagueId}?season=2023
```
- Belirtilen ligin takımlarını çeker
- Kadro bilgilerini kaydeder

#### 3. **Maçları Senkronize Et**
```
POST /api/externaldata/sync/matches/{leagueId}?season=2023
```
- Maç programını ve sonuçlarını çeker
- Skorları günceller

#### 4. **Tam Senkronizasyon**
```
POST /api/externaldata/sync/full/{leagueId}?season=2023
```
- Takımları ve maçları birlikte çeker

#### 5. **Canlı Skorlar**
```
POST /api/externaldata/sync/live-scores
```
- Anlık olarak devam eden maçların skorlarını günceller

## 🌐 **Web Arayüzü Entegrasyonu**

API'den çekilen veriler web arayüzünde şu şekilde gösterilir:

### 📈 **Ana Sayfa (`/`)**
- Tüm liglerin listesi
- Lig bazında puan durumu ve maç linkleri

### 🏆 **Puan Durumu (`/Home/Standings/{leagueId}`)**
- Otomatik hesaplanan sıralama tablosu
- Galibiyet, beraberlik, mağlubiyet istatistikleri
- Gol farkları ve puanlar
- 30 saniyede bir otomatik yenileme

### ⚽ **Maç Listesi (`/Home/Matches/{leagueId}`)**
- Tüm maçların programı ve sonuçları
- Canlı maçlar için anlık güncelleme
- Maç detayları ve skorlar

## 🔧 **Önerilen Kullanım Akışı**

### 1. **İlk Kurulum**
```bash
# 1. Ligleri çek
POST /api/externaldata/sync/leagues

# 2. Premier Lig takımlarını çek (ID: 39)
POST /api/externaldata/sync/teams/39?season=2023

# 3. Premier Lig maçlarını çek
POST /api/externaldata/sync/matches/39?season=2023
```

### 2. **Otomatik Güncelleme**
- BackgroundService her 5 dakikada bir çalışır
- Canlı skorları otomatik günceller
- Yeni maçları veritabanına ekler

### 3. **Manuel Güncelleme**
- İhtiyaç anında Swagger'dan senkronizasyon yapılabilir
- Belirli ligler için güncelleme mümkün

## 📱 **Web Arayüzü Özellikleri**

### 🎨 **Responsive Tasarım**
- Mobil, tablet ve masaüstü uyumlu
- Tailwind CSS ile modern görünüm

### ⚡ **Canlı Güncelleme**
- Puan durumu ve maç sayfaları 30 saniyede bir yenilenir
- Sayfa yenilemeden skor güncellemeleri

### 🔄 **Otomatik Veri Akışı**
- BackgroundService → Veritabanı → Web Arayüzü
- Sürekli güncel skorlar ve puan durumu

## 🎯 **Avantajları**

✅ **Gerçek Zamanlı** - Canlı skor takibi
✅ **Otomatik** - Manuel müdahale gerekmez
✅ **Kapsamlı** - Çoklu lig desteği
✅ **Modern** - Responsive ve kullanıcı dostu
✅ **Esnek** - Manuel ve otomatik senkronizasyon

---

**Not**: API anahtarınız eklendiği için artık veri çekme işlemleri çalışacaktır!
