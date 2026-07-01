# ⚙️ Yago Code - Expense Tracker Backend API

Bu proje, **Expense Tracker** uygulamasının backend mimarisini yöneten, C# ve .NET 8 ile geliştirilmiş güvenli, performanslı ve ölçeklenebilir bir RESTful API'dir.

## 🛠 Mimari ve Teknolojiler

- **Framework:** .NET 8
- **Mimari:** N-Tier Architecture (Katmanlı Mimari)
- **Veritabanı:** PostgreSQL (Entity Framework Core ile)
- **Kimlik Doğrulama:** JWT (JSON Web Token)
- **API Dokümantasyonu:** Swagger/OpenAPI

## ⚡ Temel Özellikler

- ✅ **Güvenli Kimlik Doğrulama:** JWT tabanlı güvenli giriş ve yetkilendirme.
- ✅ **N-Tier Yapı:** Business (İş), Data Access (Veri) ve API katmanları ile modüler kod tabanı.
- ✅ **Veri Yönetimi:** Harcamaların kategorize edilmesi ve detaylı raporlama işlemleri.
- ✅ **Performans:** Hızlı veri erişimi için optimize edilmiş SQL sorguları.

## 🚀 Başlatma (Setup)

1. **Gereksinimler:** .NET 8 SDK'nın yüklü olduğundan emin ol.
2. **Depoyu Klonla:**
   `git clone https://github.com/yagiztkn/ExpenseTracker.API.git`
3. **Veritabanı Ayarı:** `appsettings.json` dosyasındaki `ConnectionStrings` kısmına kendi PostgreSQL bağlantı bilgini gir.
4. **Çalıştır:**
   `dotnet run`
5. **API Dokümantasyonu:** Proje çalıştıktan sonra `https://localhost:PORT/swagger` adresinden API endpoint'lerini test edebilirsin.

## 🔗 Environment Variables
Projeyi çalıştırmadan önce aşağıdaki değişkenlerin tanımlı olduğundan emin ol:
- `JWT_SECRET_KEY`
- `DB_CONNECTION_STRING`

---
*Geliştirici: Yağız Tekin*
Linkedin: https://www.linkedin.com/in/yagiztekin-software/
