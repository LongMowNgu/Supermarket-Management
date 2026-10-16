param([string]$MSBuild = 'D:\VisualStudio2026\MSBuild\Current\Bin\MSBuild.exe',[switch]$SkipRender)
$ErrorActionPreference = 'Stop'
$taskRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$taskOutput = Join-Path $taskRoot '.codex\verification'
New-Item -ItemType Directory -Path $taskOutput -Force | Out-Null
$taskDatabase = 'QuanLyBanHangSupermarket_Verify_' + [guid]::NewGuid().ToString('N').Substring(0,12)
$taskSql = Join-Path $taskOutput 'verify-setup.sql'
$taskSource = [IO.File]::ReadAllText((Join-Path $taskRoot 'Database\01_Setup.sql')).Replace('QuanLyBanHangSupermarket',$taskDatabase)
[IO.File]::WriteAllText($taskSql,$taskSource,(New-Object Text.UTF8Encoding($true)))
$taskCreated = $false
try {
    & $MSBuild (Join-Path $taskRoot 'QuanLyBanHangSupermarket\QuanLyBanHangSupermarket.csproj') /t:Rebuild /p:Configuration=Release /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Application build failed.' }
    & $MSBuild (Join-Path $PSScriptRoot 'Verification.csproj') /t:Rebuild /p:Configuration=Release /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Verification build failed.' }
    & sqlcmd -S '.\SQLEXPRESS01' -E -C -f 65001 -b -i $taskSql
    if ($LASTEXITCODE -ne 0) { throw 'Test database setup failed.' }
    $taskCreated = $true
    $taskExecutable = Join-Path $PSScriptRoot 'bin\Supermarket.Verification.exe'
    $taskConfig = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'App.config')).Replace('QuanLyBanHangSupermarket_Verify_NOT_CREATED',$taskDatabase)
    [IO.File]::WriteAllText($taskExecutable + '.config',$taskConfig,(New-Object Text.UTF8Encoding($true)))
    $taskArguments = @($taskOutput)
    if ($SkipRender) { $taskArguments += '--skip-render' }
    & $taskExecutable @taskArguments | Tee-Object -FilePath (Join-Path $taskOutput 'verification-results.txt')
    if ($LASTEXITCODE -ne 0) { throw 'Verification failed. Inspect verification-results.txt.' }
    & sqlcmd -S '.\SQLEXPRESS01' -E -C -f 65001 -d master -b -Q "BACKUP DATABASE [$taskDatabase] TO DISK=N'$taskDatabase.bak' WITH COPY_ONLY,CHECKSUM; RESTORE VERIFYONLY FROM DISK=N'$taskDatabase.bak' WITH CHECKSUM; ALTER DATABASE [$taskDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [$taskDatabase] FROM DISK=N'$taskDatabase.bak' WITH REPLACE,CHECKSUM; ALTER DATABASE [$taskDatabase] SET MULTI_USER;"
    if ($LASTEXITCODE -ne 0) { throw 'SQL Server backup/restore verification failed.' }
} finally {
    if ($taskCreated -and $taskDatabase -match '^QuanLyBanHangSupermarket_Verify_[a-f0-9]{12}$') {
        & sqlcmd -S '.\SQLEXPRESS01' -E -C -d master -b -Q "ALTER DATABASE [$taskDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$taskDatabase];"
        if ($LASTEXITCODE -ne 0) { Write-Warning "Cleanup failed for test database $taskDatabase" }
    }
}
