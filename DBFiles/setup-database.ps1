param(
    [string]$AppPath
)

$dbFilesPath = Join-Path $AppPath "DBFiles"
if (-not (Test-Path $dbFilesPath)) {
    New-Item -ItemType Directory -Path $dbFilesPath -Force | Out-Null
}

$logFile = Join-Path $dbFilesPath "setup-log.txt"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

function Write-Log {
    param([string]$Message)
    $logMessage = "$timestamp - $Message"
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage -ErrorAction SilentlyContinue
}

Write-Log "=== Database setup started ==="
Write-Log "AppPath: $AppPath"

try {
    Write-Log "Loading SQL assemblies..."
    Add-Type -AssemblyName System.Data -ErrorAction Stop
    Write-Log "System.Data loaded successfully"
    
    $databaseName = "RentalCarDB"
    $backupPath = Join-Path $AppPath "DBFiles\db2.bak"
    Write-Log "Backup path: $backupPath"
    
    # Находим правильный путь для LocalDB файлов
    $localDbDataPath = "$env:LOCALAPPDATA\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB"
    
    # Если путь не существует, пробуем альтернативные
    if (-not (Test-Path $localDbDataPath)) {
        $localDbDataPath = "C:\Program Files\Microsoft SQL Server\MSSQL17.LOCALDB\MSSQL\DATA"
    }
    
    if (-not (Test-Path $localDbDataPath)) {
        $localDbDataPath = "$env:USERPROFILE\Documents"
    }
    
    Write-Log "LocalDB data path: $localDbDataPath"
    
    $dataFilePath = Join-Path $localDbDataPath "$databaseName.mdf"
    $logFilePath = Join-Path $localDbDataPath "$databaseName`_log.ldf"
    
    Write-Log "Data file will be restored to: $dataFilePath"
    Write-Log "Log file will be restored to: $logFilePath"
    
    $masterConnectionString = "Server=(localdb)\MSSQLLocalDB; Database=master; Integrated Security=True; TrustServerCertificate=True;"
    
    Write-Log "Testing connection to SQL Server..."
    $testConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
    $testConn.Open()
    $testConn.Close()
    Write-Log "Connected to SQL Server successfully"
    
    if (-not (Test-Path $backupPath)) {
        Write-Log "ERROR: Backup file not found at: $backupPath"
        throw "Backup file not found"
    }
    
    $fileSize = (Get-Item $backupPath).Length
    Write-Log "Backup found. Size: $([math]::Round($fileSize/1MB, 2)) MB"
    
    Write-Log "Checking if database '$databaseName' exists..."
    
    $checkConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
    $checkConn.Open()
    $checkCmd = $checkConn.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$databaseName'"
    $dbExists = [int]$checkCmd.ExecuteScalar()
    $checkConn.Close()
    
    if ($dbExists -gt 0) {
        Write-Log "Database exists. Dropping it to restore fresh copy..."
        
        $dropConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
        $dropConn.Open()
        $dropCmd = $dropConn.CreateCommand()
        $dropCmd.CommandText = "ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$databaseName];"
        $dropCmd.ExecuteNonQuery()
        $dropConn.Close()
        
        Write-Log "Existing database dropped successfully"
    }
    
    Write-Log "Restoring database from backup with proper file paths..."
    
    $restoreConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
    $restoreConn.Open()
    $restoreCmd = $restoreConn.CreateCommand()
    
    # Формируем команду восстановления с правильными путями
    $restoreCommand = @"
RESTORE DATABASE [$databaseName]
FROM DISK = N'$backupPath'
WITH 
    MOVE N'RentalCarDB' TO N'$dataFilePath',
    MOVE N'RentalCarDB_log' TO N'$logFilePath',
    REPLACE, RECOVERY
"@
    
    Write-Log "Executing restore command..."
    Write-Log $restoreCommand
    
    $restoreCmd.CommandText = $restoreCommand
    $restoreCmd.ExecuteNonQuery()
    $restoreConn.Close()
    
    Write-Log "SUCCESS: Database restored from backup"
    
    # Проверяем восстановление
    $verifyConn = New-Object System.Data.SqlClient.SqlConnection("Server=(localdb)\MSSQLLocalDB; Database=$databaseName; Integrated Security=True; TrustServerCertificate=True;")
    $verifyConn.Open()
    
    $tableCmd = $verifyConn.CreateCommand()
    $tableCmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
    $tableCount = [int]$tableCmd.ExecuteScalar()
    Write-Log "Verification: Database restored with $tableCount tables"
    
    # Показываем список таблиц
    $tableCmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"
    $reader = $tableCmd.ExecuteReader()
    Write-Log "Tables in database:"
    while ($reader.Read()) {
        Write-Log "  - $($reader['TABLE_NAME'])"
    }
    $reader.Close()
    
    $verifyConn.Close()
    Write-Log "=== Setup completed successfully ==="
    
} catch {
    Write-Log "ERROR: $_"
    Write-Log "=== Setup failed ==="
    exit 1
}