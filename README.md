# EventReservation

EventReservation, etkinlik ve koltuk rezervasyon senaryosu üzerine geliştirilmiş kapsamlı bir ASP.NET Core Web API projesidir.

---

## Özellikler

- Clean Architecture ile katmanlı proje yapısı
- CQRS + MediatR tabanlı use-case akışı
- Entity Framework Core ile SQL Server kullanımı
- JWT authentication ve refresh token desteği
- Role-based authorization
  - Admin
  - Organizer
  - Customer
- Venue, Seat, Event, EventSeat, Reservation ve Payment modülleri
- Rezervasyon oluşturma, ödeme yapma ve iptal etme akışları
- Hangfire ile süresi dolan rezervasyonların otomatik expire edilmesi
- SignalR ile gerçek zamanlı koltuk durumu güncellemeleri
- Rate limiting
- Health check endpointleri
- Global exception handling
- FluentValidation ile request validation
- Docker Compose ile local development altyapısı
- Unit test ve integration test altyapısı
- 130 başarılı test

---

## Kullanılan Teknolojiler

- ASP.NET Core Web API
- C#
- .NET 8
- Entity Framework Core
- SQL Server
- MediatR
- FluentValidation
- JWT Bearer Authentication
- Hangfire
- SignalR
- Docker
- Swagger
- xUnit
- Moq
- FluentAssertions
- WebApplicationFactory

---

## Proje Yapısı

```text
EventReservation
├── src
│   ├── EventReservation.API
│   ├── EventReservation.Application
│   ├── EventReservation.Domain
│   └── EventReservation.Infrastructure
├── tests
│   ├── EventReservation.UnitTests
│   └── EventReservation.IntegrationTests
├── docker-compose.yml
└── EventReservation.sln
```

---

## Katmanlar

### Domain

Domain katmanında entity'ler, enum'lar ve temel iş kuralları yer alır.

Başlıca entity'ler:

- User
- RefreshToken
- Venue
- Seat
- Event
- EventSeat
- Reservation
- ReservationSeat
- Payment

Koltuk ve rezervasyon durum geçişleri domain davranışları üzerinden yönetilir.

Örnek davranışlar:

```text
EventSeat.Lock()
EventSeat.Reserve()
EventSeat.Release()

Reservation.Confirm()
Reservation.Cancel()
Reservation.Expire()
```

---

### Application

Application katmanı use-case akışlarının bulunduğu katmandır. CQRS yapısı burada yer alır.

Bu katmanda:

- Command ve Query sınıfları
- Handler sınıfları
- DTO / Response modelleri
- Validation kuralları
- Custom exception sınıfları
- Repository abstraction'ları
- Unit of Work abstraction'ı
- Authentication abstraction'ları
- Realtime notifier abstraction'ı
- Background job abstraction'ları

bulunur.

Handler sınıfları doğrudan Entity Framework, SignalR veya Hangfire gibi teknolojilere bağımlı değildir. Bu bağımlılıklar abstraction üzerinden yönetilir.

---

### Infrastructure

Infrastructure katmanında dış bağımlılıkların implementasyonları bulunur.

Bu katmanda:

- AppDbContext
- Entity configurations
- Repository implementasyonları
- Unit of Work implementasyonu
- JWT token servisi
- Password hasher
- Hangfire reservation expiration job

yer alır.

---

### API

API katmanı dış dünyaya açılan katmandır.

Bu katmanda:

- Controller'lar
- Middleware'ler
- Authentication / Authorization ayarları
- SignalR Hub
- Health Check endpointleri
- Rate Limiting ayarları
- Swagger konfigürasyonu
- Dependency Injection ayarları

bulunur.

---

## Roller

Projede üç temel rol bulunmaktadır:

```text
Admin
Organizer
Customer
```

Rol bazlı yetkilendirme örnekleri:

```text
Venue oluşturma       -> Organizer, Admin
Event oluşturma       -> Organizer, Admin
Event publish etme    -> Organizer, Admin
Reservation oluşturma -> Customer
Payment yapma         -> Customer
Report görüntüleme    -> Organizer, Admin
```

---

## Rezervasyon Akışı

Koltuk durumları:

```text
Available -> Koltuk müsait
Locked    -> Rezervasyon oluşturuldu, ödeme bekleniyor
Reserved  -> Ödeme tamamlandı, koltuk kesin olarak rezerve edildi
```

Rezervasyon durumları:

```text
PendingPayment
Confirmed
Cancelled
Expired
```

Genel akış:

```text
1. Customer müsait koltukları seçer.
2. Reservation oluşturulur.
3. Seçilen EventSeat kayıtları Locked olur.
4. Kullanıcı ödeme yaparsa reservation Confirmed olur.
5. Koltuklar Reserved olur.
6. Kullanıcı iptal ederse veya süre dolarsa koltuklar Available olur.
```

---

## Hangfire

Projede Hangfire, ödeme süresi dolan rezervasyonları otomatik olarak expire etmek için kullanılmıştır.

Reservation oluşturulduğunda bir expiration job planlanır. Süre dolduğunda:

```text
Reservation -> Expired
EventSeat   -> Available
```

durumuna çekilir.

Bu sayede kullanıcı ödeme yapmadan koltuğu süresiz olarak kilitli tutamaz.

---

## SignalR

SignalR, koltuk durumlarını gerçek zamanlı bildirmek için kullanılmıştır.

Hub endpoint:

```text
/hubs/event-seats
```

Client ilgili event grubuna katılır:

```text
JoinEventGroup(eventId)
```

Backend tarafında koltuk durumu değiştiğinde client'lara şu event gönderilir:

```text
eventSeatsChanged
```

SignalR bildirimi gönderilen durumlar:

```text
CreateReservation      -> Locked
PayReservation         -> Reserved
CancelReservation      -> Available
ReservationExpiration  -> Available
```

SignalR entegrasyonunda Application katmanı doğrudan SignalR'a bağımlı değildir. Bunun yerine `IRealtimeNotifier` abstraction'ı kullanılmıştır. Gerçek SignalR implementasyonu API katmanında yer alır.

---

## Health Checks

Projede servis sağlığını kontrol etmek için health check endpointleri bulunmaktadır.

```http
GET /health
GET /health/live
GET /health/ready
```

Açıklama:

```text
/health       -> Tüm health check kontrollerini çalıştırır
/health/live  -> API process ayakta mı kontrol eder
/health/ready -> API istek almaya hazır mı, database bağlantısı sağlıklı mı kontrol eder
```

---

## Rate Limiting

API endpointlerinde rate limiting uygulanmıştır. Böylece aynı client veya kullanıcı tarafından kısa sürede aşırı istek gönderilmesi sınırlandırılır.

---

## Global Exception Handling

Projede merkezi hata yönetimi için global exception middleware kullanılmıştır.

Örnek exception türleri:

```text
NotFoundException
BadRequestException
UnauthorizedException
ForbiddenAccessException
ValidationException
```

Bu sayede hata cevapları standart JSON formatında döner.

---

## Önemli Endpointler

### Auth

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me
```

### Venues

```http
POST   /api/venues
GET    /api/venues
GET    /api/venues/{id}
PUT    /api/venues/{id}
DELETE /api/venues/{id}
```

### Venue Seats

```http
POST /api/venues/{venueId}/seats/generate
GET  /api/venues/{venueId}/seats
```

### Events

```http
POST   /api/events
GET    /api/events
GET    /api/events/{id}
PUT    /api/events/{id}
DELETE /api/events/{id}
POST   /api/events/{id}/publish
POST   /api/events/{id}/cancel
POST   /api/events/{id}/complete
```

### Event Seats

```http
POST /api/events/{eventId}/seats/generate
GET  /api/events/{eventId}/seats
```

### Reservations

```http
POST /api/reservations
POST /api/reservations/{reservationId}/pay
POST /api/reservations/{reservationId}/cancel
GET  /api/reservations/my
GET  /api/reservations/{reservationId}
```

### Event Reports

```http
GET /api/events/{eventId}/reservations
GET /api/events/{eventId}/summary
```

---

## Testler

Projede unit test ve integration test altyapısı bulunmaktadır.

```text
130 tests passing
```

Test projeleri:

```text
tests/EventReservation.UnitTests
tests/EventReservation.IntegrationTests
```

Test edilen başlıca alanlar:

- Domain entity davranışları
- EventSeat status geçişleri
- Reservation oluşturma akışı
- Payment akışı
- Reservation cancel akışı
- Reservation expiration job
- Authentication endpointleri
- Venue endpointleri
- VenueSeat endpointleri
- Event endpointleri
- EventSeat endpointleri
- Reservation endpointleri
- Event report endpointleri
- Authorization senaryoları
- Hatalı request senaryoları

Tüm testleri çalıştırmak için:

```bash
dotnet test
```

---

## Docker ile Çalıştırma

Docker container'larını başlatmak için:

```bash
docker compose up -d
```

Container'ları durdurmak için:

```bash
docker compose down
```

---

## Kurulum

Projeyi klonlayın:

```bash
git clone https://github.com/fskalkan/EventReservation.git
cd EventReservation
```

Bağımlılıkları yükleyin:

```bash
dotnet restore
```

Docker container'larını başlatın:

```bash
docker compose up -d
```

Projeyi build edin:

```bash
dotnet build
```

Migration uygulayın:

```bash
dotnet ef database update --project src/EventReservation.Infrastructure --startup-project src/EventReservation.API
```

API projesini çalıştırın:

```bash
dotnet run --project src/EventReservation.API
```

Swagger arayüzüne tarayıcıdan erişebilirsiniz:

```text
https://localhost:{PORT}/swagger
```

---

## Örnek Reservation Request

```json
{
  "eventId": "event-guid",
  "eventSeatIds": [
    "event-seat-guid-1",
    "event-seat-guid-2"
  ]
}
```

---

## Örnek Payment Request

```json
{
  "amount": 1750,
  "method": 1
}
```

---

## Geliştirici

Ferhat Samet Kalkan