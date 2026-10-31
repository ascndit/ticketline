# Ticketline

Учебный REST API заявок: регистрация, вход и список своих обращений.

## Стек

- .NET 9, ASP.NET Core
- Entity Framework Core и PostgreSQL
- JWT, пароль хранится как хеш
- Swagger
- xUnit
- Docker Compose

## Что умеет

- `POST /api/auth/register` и `POST /api/auth/login` выдают JWT
- `GET /api/tickets` возвращает только заявки текущего пользователя, с пагинацией
- `POST /api/tickets` создаёт заявку в статусе `Open`
- `PUT /api/tickets/{id}` меняет текст и статус
- закрытую заявку нельзя снова открыть
- чужую заявку автор не видит

## Запуск

Нужны Docker и Docker Compose.

```bash
docker compose up --build
```

Swagger: `http://localhost:8080/swagger`

Локальная строка подключения и ключ JWT в `appsettings.json` предназначены только для разработки. Перед любым публичным запуском их нужно заменить.

## Тесты

```bash
dotnet test
```
