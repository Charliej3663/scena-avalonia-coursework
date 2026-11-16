# Театральная студия «Сцена»

Настольное приложение на C# и Avalonia UI. Данные заявок, клиентов и обращений хранятся в Microsoft SQL Server через Entity Framework Core.

## Запуск в Visual Studio

1. Установите Visual Studio с поддержкой .NET 10 SDK и workload **Разработка классических приложений .NET**.
2. В Visual Studio Installer откройте **Изменить → Отдельные компоненты** и установите **SQL Server Express LocalDB**. Для восстановления Avalonia и EF Core нужен доступ к NuGet.
3. В PowerShell из папки проекта выполните `powershell -ExecutionPolicy Bypass -File .\setup-localdb.ps1`. Скрипт проверит наличие LocalDB, создаст экземпляр `MSSQLLocalDB`, если он отсутствует, и запустит его.
4. Откройте `ScenaAvalonia.csproj` в Visual Studio и запустите проект. При первом обращении к разделу с данными EF Core создаст базу `ScenaStudioCoursework` и таблицы.

Из командной строки из папки проекта: `dotnet run`.

Готовая Windows x64 версия — отдельный самодостаточный архив `ScenaAvalonia-Windows-x64-LocalDB.zip`; устанавливать .NET Runtime отдельно не нужно. Перед запуском приложения установите **SQL Server Express LocalDB** и выполните `setup-localdb.ps1`. Для разработки в Visual Studio откройте исходный проект.

Строка в `appsettings.json` подключается к `(localdb)\MSSQLLocalDB` и создаёт базу `ScenaStudioCoursework`. Настройка, запуск и устранение неполадок описаны в [LOCALDB_SETUP.md](LOCALDB_SETUP.md). Для обычного SQL Server задайте, например:

```json
{
  "ConnectionStrings": {
    "Studio": "Server=localhost;Database=ScenaStudioCoursework;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Гостевой каталог и служебный вход администратора разделены. Доступ и журнал запусков описаны в [ADMIN_PANEL.md](ADMIN_PANEL.md).

## Функции

- премиальный интерфейс в палитре старинного театрального салона: изумруд, винный, слоновая кость и латунное золото;
- каталог 12 курсов, полнотекстовый поиск по названию, описанию, педагогу и направлению, фильтр и сортировка;
- сохранение избранного, контактов и корзины между запусками приложения;
- расписание групп и добавление выбранных занятий в корзину;
- карточка курса с описанием, педагогом, расписанием и выбором группы;
- промокоды `SCENA10` (скидка 10%) и `DEBUT` (скидка 5%);
- создание заявки с выбором способа оплаты в студии; личный кабинет показывает только заявки, найденные по номеру телефона;
- отзывы со звёздной оценкой, сохранением в SQL Server;
- обращения в поддержку и ответы администратора;
- административный экран для просмотра заявок и изменения их статуса.

Для курсовой откройте `ScenaAvalonia.csproj` в Visual Studio. Репозиторий проекта: [github.com/Charliej3663/scena-avalonia-coursework](https://github.com/Charliej3663/scena-avalonia-coursework).

## Git и GitHub

Исходники этого приложения размещены в GitHub-репозитории проекта. Для своей копии используйте `git clone https://github.com/Charliej3663/scena-avalonia-coursework.git`. Не добавляйте в репозиторий секреты подключения.

Дополнительные материалы для пояснительной записки и защиты находятся в `COURSEWORK_GUIDE.md`.

