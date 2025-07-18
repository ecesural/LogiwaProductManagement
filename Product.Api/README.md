🧠 Product Management API

Bu proje, .NET 9 kullanılarak geliştirilmiş DDD (Domain Driven Design) uyumlu bir Ürün Yönetim API'sidir.

🚀 Temel Özellikler

✅ .NET 9 ile yazılmış modern Web API

🧱 DDD (Domain Driven Design) ile katmanlı mimari

📤 Event publishing desteği (Domain Event yapısı)

🧩 CQRS ve MediatR entegrasyonu

🛡️ FluentValidation ile model doğrulama

🧾 Logging Middleware ile hata ve istek/yanıt loglama

🧪 Unit test desteği (xUnit + Moq)

🧰 Redis entegrasyonu (opsiyonel cache altyapısı)

📚 Swagger 

🐳 Docker desteği

🏗️ Proje Yapısı

├── src/

│   ├── Product.Presentation      → API Layer (Controllers, Middlewares, Extensions)

│   ├── Product.Persistence       → (DbContext, Repositories)

│   ├── Product.Application       → Application Layer (Commands, Queries, Handlers)

│   ├── Product.Domain            → Domain Layer (Entities, Events)

│   ├── Product.Infrastructure    → Infrastructure Layer (Events, Caching, Logging)

│   ├── docker-compose.yml           → Docker setup 

│   ├── README.md                    → Proje dokümantasyonu

├── tests/
  
└── Product.Tests             → UnitTests


⚙️ Kurulum Adımları
🔧 Gereksinimler

.NET 9 SDK

MSSQL Server (veya hangi veritabanı kullanılıyorsa)

Redis

Docker 

🌍 Swagger UI

https://localhost:5001/swagger/index.html

🧠 Event Sistemi

Domain içerisinde Domain Events kullanılarak bağımsız iş kuralları birbirinden ayrılmıştır.

Uygulama katmanında INotificationHandler<T> (MediatR) kullanılarak event’ler asenkron olarak işlenir.

Olası senaryolar: ProductCreatedEvent, ProductUpdatedEvent, ProductDeletedEvent, vb.

📓 Loglama

Özel ILoggerService<T> ile loglama soyutlandı.

ExceptionHandlingMiddleware ile tüm global hatalar loglanır.

