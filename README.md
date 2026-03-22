# RusAuth.Authorization.Example

`RusAuth.Authorization.Example` — публичный эталонный пример для внешней интеграции с RusAuth по REST.

## Назначение

Используйте это решение, если ваша система должна обращаться к RusAuth только за подтверждением звонком.

Важно:

- внешний клиент работает только с публичным REST API RusAuth
- внешний клиент авторизуется токеном компании в заголовке `api-key`
- после подтверждения звонка RusAuth не выдаёт локальную сессию вашего приложения: это остаётся ответственностью вашей системы

## Что показывает пример

- вызов `CallToConfirm`
- получение номера RusAuth и `TransactionId`
- приём webhook с подтверждением
- ручную проверку через `CheckConfirmation`
- локальный журнал подтверждений

## Структура

- `RusAuth.Authorization.Example`
  - Blazor Web App Server
  - демонстрирует внешний сценарий подтверждения звонком
- `RusAuth.Authorization.Example.Tests`
  - проверяет фасадную логику примера без живого RusAuth

## Локальная разработка

Пример использует обычные `PackageReference` на:

- `RusAuth.Authorization`
- `RusAuth.Authorization.Contracts`

Для локальной разработки с ещё не опубликованными пакетами сначала упакуйте их из репозитория RusAuth:

```powershell
powershell -File D:\Developer\SERVER\RusAuth\RusAuth\scripts\pack-rusauth-authorization-packages.ps1
```

Затем восстановите, соберите и протестируйте пример, явно указав локальный source:

```powershell
dotnet restore D:\Developer\SERVER\RusAuth\Example\RusAuth.Authorization.Example.slnx `
  --source D:\Developer\SERVER\RusAuth\RusAuth\artifacts\packages `
  --source https://api.nuget.org/v3/index.json
dotnet build D:\Developer\SERVER\RusAuth\Example\RusAuth.Authorization.Example.slnx --nologo
dotnet test D:\Developer\SERVER\RusAuth\Example\RusAuth.Authorization.Example.slnx --nologo
```

После публикации пакетов в ваш NuGet feed достаточно обычного `dotnet restore`, без локальных путей и без специальных MSBuild-флагов.

## Конфигурация

В `appsettings.json` или `appsettings.Development.json` настройте секции `RusAuth` и `Example`:

```json
{
  "RusAuth": {
    "BaseUrl": "http://localhost:7005/",
    "Token": "ВАШ_ТОКЕН_КОМПАНИИ",
    "TimeOut": 15
  },
  "Example": {
    "CallbackBearerToken": "ВАШ_ЛОКАЛЬНЫЙ_CALLBACK_BEARER"
  }
}
```

Где:

- `RusAuth:BaseUrl` указывает на публичный REST API RusAuth
- `RusAuth:Token` — токен компании, которым RusAuth распознаёт интегратора
- `Example:CallbackBearerToken` — ваш собственный Bearer-токен для защиты callback endpoint-а

## Что должен сделать внешний клиент

1. Вызвать `CallToConfirm`.
2. Сохранить `TransactionId` и номер клиента у себя.
3. Дождаться webhook или выполнить ручную проверку через `CheckConfirmation`.
4. После статуса `Success` применить свои правила входа, подтверждения операции или подтверждения действия.

## Что пример не делает намеренно

- не навязывает модель вашей собственной SSO/авторизации

Это учебный и документационный пример, а не готовое production-приложение.
