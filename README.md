# Marketing Manager — ASP.NET Core 10

Переработанный backend исходного `Marketing-Manager-safarov7` по структурному примеру `FootCamp`.
Сохранены продуктовый CRUD, Identity/JWT, роли `Admin`, `Manager`, `User`, восстановление пароля по почте,
фильтры/пагинация, PostgreSQL, кеширование и минутный подсчёт товаров.

## Структура

```text
MarketingManager-NET10/
├── Domain/
│   ├── Entities/Product.cs
│   ├── DTOs/Auth/...
│   ├── DTOs/Products/...
│   ├── Filters/ProductFilter.cs
│   ├── Pagination/ValidFilter.cs
│   └── ApiResponse/...
├── Infrastructure/
│   ├── Auth/{Interfaces,Services}/
│   ├── Email/{Interfaces,Services}/
│   ├── Products/{Interfaces,Repositories,Services}/
│   ├── RedisCache/
│   ├── Data/{Configurations,DataContext.cs}/
│   ├── Migrations/
│   ├── Seeds/
│   ├── BackgroundTasks/
│   └── DI/DependencyInjection.cs
├── WebApi/
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
└── MarketingManager.sln
```

## Требования

- **.NET SDK 10** (`dotnet --version`)
- **PostgreSQL** (например, локально на 5432)
- **Redis необязателен**: по умолчанию используется встроенный `IDistributedCache` в памяти.
- SMTP нужен **только** для отправки письма сброса пароля.

## Первый запуск на Mac (zsh)

1. Создайте базу `marketing_manager` в PostgreSQL (либо разрешите создание базы данному пользователю).
2. Выполните в терминале **свои** значения:

```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=marketing_manager;Username=postgres;Password=YOUR_LOCAL_PASSWORD'
export Jwt__Key="$(openssl rand -base64 48)"
export SeedAdmin__Email='admin@example.com'
export SeedAdmin__Password='A_Strong_Password_123'
```

3. Перейдите в корень проекта и выполните:

```bash
dotnet restore MarketingManager.sln
dotnet build MarketingManager.sln -c Release
dotnet run --project WebApi --launch-profile http
```

Swagger UI (в окружении Development): `http://localhost:5279/swagger`.

`Database:AutoMigrate=true` создаёт/обновляет таблицы из сохранённой EF Core-миграции
при запуске приложения, затем создаёт роли и **опционально** администратора.
В production лучше отключить `AutoMigrate` и выполнять миграции отдельным процессом.

## Настройки

Все секреты передавайте через environment variables, user-secrets или секрет-хранилище:

| Ключ | Назначение |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL, обязательный |
| `Jwt__Key` | случайная строка минимум 32 UTF-8 байта, обязательная |
| `Jwt__Issuer`, `Jwt__Audience` | настроены для локальной разработки |
| `SeedAdmin__Email`, `SeedAdmin__Password` | если нужны, указываются **оба**; никаких паролей по умолчанию |
| `Email__Username`, `Email__Password`, `Email__From` | SMTP-логин, app password, адрес отправителя |
| `Email__SmtpHost`, `Email__Port` | по умолчанию Gmail/587 |
| `Caching__UseRedis` | `true`, если настроен работающий Redis |
| `ConnectionStrings__Redis` | адрес Redis |
| `Database__AutoMigrate` | по умолчанию `true` |
| `BackgroundTasks__CountProductsEnabled` | по умолчанию `true` |

Для Gmail включите 2FA и создайте отдельный **App Password**.

## Endpoints

| HTTP | URL | Доступ |
| --- | --- | --- |
| POST | `/api/Auth/register` | публичный |
| POST | `/api/Auth/login` | публичный |
| POST | `/api/Auth/change-password` | авторизованный |
| POST | `/api/Auth/request-password-reset?email=...` | публичный |
| POST | `/api/Auth/password-reset` | публичный |
| GET | `/api/Product?PageNumber=1&PageSize=10&Name=abc&StartPrice=10&ToPrice=100` | Admin, Manager |
| GET | `/api/Product/{id}` | Admin, Manager |
| POST | `/api/Product` | Admin, Manager |
| PUT | `/api/Product/{id}` | Admin, Manager |
| DELETE | `/api/Product/{id}` | Admin, Manager |

Создание продукта: `{ "name": "Example", "price": 12.5 }`.
Обновление: `PUT /api/Product/1` с таким же JSON. Поле `createdAt` задаёт сервер и при обновлении не меняет.
Логин возвращает `{ "token": "..." }`. В Swagger используйте Bearer JWT.
Для смены пароля JSON: `{ "oldPassword": "...", "newPassword": "...", "confirmPassword": "..." }`.

## Важные отличия от исходника

- Результаты CRUD возвращают правильные HTTP-коды (200, 201, 400, 404).
- Товары фильтруются и постранично выбираются в PostgreSQL, а не загружаются целиком.
- Добавлены отдельные DTO, валидация и ограничения пагинации (до 100 записей).
- Пароли, JWT secret и почтовые реквизиты **не хранятся** в конфигурации проекта.
- Исправлен сброс пароля: Identity реально проверяет токен; срок токена 10 минут.
- Identity login учитывает блокировку после неудачных входов.
- Кеш: Redis по настройке; без Redis используется in-memory cache; сбой Redis не ломает CRUD.
- Вместо Hangfire (ранее был только ежеминутный лог счётчика) теперь `BackgroundService` с тем же поведением — без отдельной Hangfire БД.
- Оригинальная миграция EF Core сохранена, чтобы старые таблицы/данные оставались совместимы.

## Ограничения / проверка

В среде подготовки архива **нет установленного .NET SDK** и доступа к NuGet/PostgreSQL,
поэтому реальная сборка, старт с БД и интеграционные тесты **не выполнялись**.
Команды `scripts/verify.sh` предназначены для проверки на вашей машине.
Нет оснований обещать «0 ошибок с первого раза» без реальной сборки/запуска.

**Безопасность:** в загруженном оригинале были реальные строки, похожие на секреты
(PostgreSQL, SMTP app password и JWT key). Эти значения исключены из обновлённого архива;
их необходимо **сменить/отозвать**, если они ещё действуют.
