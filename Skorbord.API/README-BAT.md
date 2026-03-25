# SKORBORD - Hızlı Başlatma Kılavuzu

## 🚀 Bat Dosyaları

### 1. **QuickStart.bat** - En Hızlı Başlatma
- Uygulamayı direkt başlatır
- Hiçbir soru sormaz
- En basit kullanım

### 2. **Skorbord.bat** - Gelişmiş Menü
- 8 farklı seçenek sunar
- Database reset
- Build işlemleri
- Status kontrolü
- Otomatik link açma

### 3. **start.bat** - Otomatik Yeniden Başlatma
- Uygulama durduğunda otomatik yeniden başlar
- Sürekli çalışması için ideal

## 📋 Menü Seçenekleri (Skorbord.bat)

1. **Start Web Application** - Normal başlatma
2. **Start with Database Reset** - Veritabanını sıfırlayıp başlat
3. **Build Only** - Sadece derleme
4. **Clean and Build** - Temizle ve derle
5. **View Application Status** - Durum kontrolü
6. **Open Swagger** - API dokümantasyonunu aç
7. **Open Web Application** - Web arayüzünü aç
8. **Exit** - Çıkış

## 🔗 Linkler

- **Web Arayüzü**: http://localhost:5208
- **Swagger API**: http://localhost:5208/swagger
- **API Endpoints**: http://localhost:5208/swagger/index.html

## 💡 Kullanım İpuçları

### İlk Başlatma İçin:
```
QuickStart.bat
```

### Geliştirme İçin:
```
Skorbord.bat
```

### Sürekli Çalışma İçin:
```
start.bat
```

## ⚙️ Sistem Gereksinimleri

- .NET SDK 6.0 veya üzeri
- Windows işletim sistemi
- İnternet bağlantısı (API için)

## 🐛 Sorun Giderme

### "dotnet komutu bulunamadı"
- .NET SDK'yı yükleyin: https://dotnet.microsoft.com/download

### "Project file not found"
- Doğru dizinde olduğunuzdan emin olun
- `Skorbord.API.csproj` dosyası olmalı

### "Port 5208 kullanımda"
- Komut satırında `netstat -ano | findstr :5208` çalıştırın
- Gerekli process'i kapatın

---

**Hazır! Artık her seferinde `dotnet run` yazmanıza gerek yok!** 🎉
