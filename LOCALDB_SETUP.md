# Подключение SQL Server LocalDB

Проект использует Entity Framework Core 10 и строку подключения из `appsettings.json`:

```text
Server=(localdb)\MSSQLLocalDB;Database=ScenaStudioCoursework;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=5;
```

## Установка LocalDB

1. Откройте **Visual Studio Installer** и нажмите **Изменить** у установленной Visual Studio.
2. Перейдите на вкладку **Отдельные компоненты**, найдите **SQL Server Express LocalDB** и установите этот компонент.
3. После завершения откройте PowerShell в папке проекта и запустите:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\setup-localdb.ps1
   ```

   Скрипт найдёт `SqlLocalDB.exe`, создаст экземпляр `MSSQLLocalDB`, если его ещё нет, и запустит его.

## Запуск приложения

Откройте `ScenaAvalonia.csproj` в Visual Studio и запустите приложение. При первом обращении к разделу «Отзывы», «Поддержка», «Личный кабинет» или «Администрирование» Entity Framework Core создаст базу `ScenaStudioCoursework` и необходимые таблицы. Данные хранятся в локальном экземпляре SQL Server под учётной записью Windows; отдельный пароль не нужен.

Клиентский каталог курсов и расписание остаются справочными данными в коде. В SQL Server сохраняются клиенты, заявки, отзывы и обращения. Избранное и корзина хранятся отдельно в JSON-файле профиля Windows.

## Проверка состояния

В терминале выполните:

```powershell
sqllocaldb info
sqllocaldb info MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

Если приложение сообщает, что не удалось найти экземпляр или файл LocalDB, установите компонент через Visual Studio Installer и повторите подготовку скриптом. Если используется другой экземпляр или обычный SQL Server, измените `ConnectionStrings:Studio` в `appsettings.json`.

`schema.sql` содержит идемпотентную схему для ручного создания базы. При стандартном запуске вручную выполнять его не нужно: таблицы создаёт Entity Framework Core.

