# HotelAutomatization (Backend API)

Бэкенд системы автоматизации гостиницы на ASP.NET Core + PostgreSQL.

## Что делает API
- Авторизация администраторов и владельца по JWT.
- Управление номерами, гостями, проживанием, бронированиями, сменами.
- Дашборды владельца (загрузка, доходы/расходы, финансовая сводка).
- Генерация документов (сервисный слой и история генераций).

## Быстрый запуск (5-10 минут)

### 1) Требования
- .NET SDK 10
- Docker Desktop
- (однократно) EF CLI: `dotnet tool install --global dotnet-ef`

### 2) Поднять PostgreSQL
Из корня `HotelAutomatization`:

```powershell
docker compose up -d
```

БД будет доступна по `localhost:5432`:
- DB: `hoteldb`
- User: `admin`
- Password: `admin`

### 3) Применить миграции
Из корня `HotelAutomatization`:

```powershell
dotnet ef database update --project Hotel.Infrastructure --startup-project Hotel.API
```

### 4) Запустить API

```powershell
dotnet run --project Hotel.API --urls http://127.0.0.1:5023
```

Проверка:
- Swagger: [http://127.0.0.1:5023/swagger/index.html](http://127.0.0.1:5023/swagger/index.html)
- OpenAPI JSON: [http://127.0.0.1:5023/openapi/v1.json](http://127.0.0.1:5023/openapi/v1.json)

### 5) Тестовые учетные записи
При включенном сидере создаются пользователи:
- `admin` / `admin123`
- `owner` / `owner123`
- `admin2` / `admin2123`

## Важно про сидер (Dev Data)
В `Hotel.API/appsettings.Development.json` включено:

```json
"Seeding": {
  "Enabled": true
}
```

Это удобно для демо/ручного тестирования, но сидер может пересоздавать тестовые данные при запуске.
Если хотите сохранять введенные вручную данные между перезапусками, выключите сидер:

```json
"Seeding": {
  "Enabled": false
}
```

## Полезные команды
Из корня `HotelAutomatization`:

```powershell
# Сборка
dotnet build Hotel.API\Hotel.API.csproj

# Тесты
dotnet test Hotel.Tests\Hotel.Tests.csproj

# Остановить БД
docker compose down

# Остановить БД и удалить volume с данными
docker compose down -v
```

## Текущая архитектура
- `Hotel.API` — контроллеры, middleware, конфигурация приложения.
- `Hotel.Application` — DTO, интерфейсы, контракты.
- `Hotel.Domain` — доменные сущности и константы.
- `Hotel.Infrastructure` — EF Core, сервисы, сидер, интеграции.
- `Hotel.Tests` — автотесты.

## План по Docker (финальный этап)
Сейчас Docker используется для PostgreSQL.
В конце разработки планируется полный containerized setup:
- `backend` (ASP.NET API)
- `frontend` (Vite build + web server)
- `postgres`
- единый `docker-compose` для запуска всей системы одной командой.
