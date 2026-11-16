$ErrorActionPreference = "Stop"

$localDbExe = (Get-Command "SqlLocalDB.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Source)
if (-not $localDbExe) {
    $candidates = @()
    foreach ($version in @("170", "160", "150", "140")) {
        $candidates += Join-Path $env:ProgramFiles "Microsoft SQL Server\$version\Tools\Binn\SqlLocalDB.exe"
        if (${env:ProgramFiles(x86)}) {
            $candidates += Join-Path ${env:ProgramFiles(x86)} "Microsoft SQL Server\$version\Tools\Binn\SqlLocalDB.exe"
        }
    }
    $localDbExe = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}

if (-not $localDbExe) {
    Write-Host "SQL Server Express LocalDB не найден." -ForegroundColor Red
    Write-Host "Установите компонент через Visual Studio Installer → Изменить → Отдельные компоненты → SQL Server Express LocalDB."
    exit 1
}

$instancesOutput = @(& $localDbExe info 2>&1) -join "`n"
if ($LASTEXITCODE -ne 0 -or $instancesOutput -match "(?i)unexpected error|error occurred|ошибка") {
    throw "LocalDB установлен, но не может прочитать настройки экземпляров. Восстановите компонент SQL Server Express LocalDB через Visual Studio Installer и запустите скрипт снова.`n$instancesOutput"
}
$instances = $instancesOutput -split "`r?`n" | ForEach-Object { $_.Trim() }

if ($instances -notcontains "MSSQLLocalDB") {
    $createOutput = @(& $localDbExe create MSSQLLocalDB -s 2>&1) -join "`n"
    if ($LASTEXITCODE -ne 0 -or $createOutput -match "(?i)unexpected error|error occurred|ошибка") { throw "Не удалось создать экземпляр MSSQLLocalDB.`n$createOutput" }
} else {
    & $localDbExe start MSSQLLocalDB 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        $details = @(& $localDbExe info MSSQLLocalDB 2>&1) -join "`n"
        if ($details -notmatch "(?i)Running|Запущ") { throw "Не удалось запустить MSSQLLocalDB.`n$details" }
    }
}

$instanceOutput = @(& $localDbExe info MSSQLLocalDB 2>&1) -join "`n"
if ($LASTEXITCODE -ne 0 -or $instanceOutput -match "(?i)unexpected error|error occurred|ошибка|not found|не найден") {
    throw "Экземпляр MSSQLLocalDB не готов. Восстановите компонент LocalDB и повторите запуск.`n$instanceOutput"
}

Write-Host "Экземпляр LocalDB MSSQLLocalDB готов." -ForegroundColor Green
Write-Host "Строка подключения: Server=(localdb)\MSSQLLocalDB;Database=ScenaStudioCoursework;Trusted_Connection=True;TrustServerCertificate=True;"
Write-Host "Откройте ScenaAvalonia.csproj в Visual Studio. База и таблицы создаются при первом обращении к разделу с данными."

