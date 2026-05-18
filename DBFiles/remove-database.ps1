param(
    [string]$AppPath
)

$logFile = Join-Path $AppPath "DBFiles\uninstall-log.txt"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

function Write-Log {
    param([string]$Message)
    $logMessage = "$timestamp - $Message"
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage -ErrorAction SilentlyContinue
}

Write-Log "=== Database removal started ==="
Write-Log "AppPath: $AppPath"

try {
    Add-Type -AssemblyName System.Data -ErrorAction Stop
    
    $databaseName = "RentalCarDB"
    $masterConnectionString = "Server=(localdb)\MSSQLLocalDB; Database=master; Integrated Security=True; TrustServerCertificate=True;"
    
    Write-Log "Testing connection to SQL Server..."
    $testConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
    $testConn.Open()
    $testConn.Close()
    Write-Log "Connected to SQL Server successfully"
    
    # Проверяем, существует ли база данных
    $checkConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
    $checkConn.Open()
    $checkCmd = $checkConn.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$databaseName'"
    $dbExists = [int]$checkCmd.ExecuteScalar()
    $checkConn.Close()
    
    if ($dbExists -gt 0) {
        Write-Log "Database '$databaseName' exists. Removing..."
        
        $dropConn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
        $dropConn.Open()
        $dropCmd = $dropConn.CreateCommand()
        $dropCmd.CommandText = @"
ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [$databaseName];
"@
        $dropCmd.ExecuteNonQuery()
        $dropConn.Close()
        
        Write-Log "SUCCESS: Database '$databaseName' has been removed"
        
        # Удаляем физические файлы базы данных, если они остались
        $dataFiles = @(
            "$env:LOCALAPPDATA\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\$databaseName.mdf",
            "$env:LOCALAPPDATA\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\$databaseName`_log.ldf",
            "C:\Program Files\Microsoft SQL Server\MSSQL17.LOCALDB\MSSQL\DATA\$databaseName.mdf",
            "C:\Program Files\Microsoft SQL Server\MSSQL17.LOCALDB\MSSQL\DATA\$databaseName`_log.ldf"
        )
        
        foreach ($file in $dataFiles) {
            if (Test-Path $file) {
                try {
                    Remove-Item $file -Force -ErrorAction SilentlyContinue
                    Write-Log "Removed file: $file"
                } catch {
                    Write-Log "Could not remove file: $file"
                }
            }
        }
        
    } else {
        Write-Log "Database '$databaseName' does not exist. Nothing to remove."
    }
    
    Write-Log "=== Database removal completed successfully ==="
    
} catch {
    Write-Log "ERROR: $_"
    Write-Log "=== Database removal failed ==="
}