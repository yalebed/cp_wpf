#define MyAppName "CarMixApp"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Lebed Polina"
#define MyAppURL "https://your-website.com"
#define MyAppExeName "RentalCarApplication.exe"
#define SourcePath ".\publish"

[Setup]
AppId={{0FCE23DB-F453-445A-8C7E-A99A05A6594E}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=.\installer
OutputBaseFilename=RentalCarSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
MinVersion=10.0
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Основные файлы приложения
Source: "{#SourcePath}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Скрипт установки БД
Source: "DBFiles\setup-database.ps1"; DestDir: "{app}\DBFiles"; Flags: ignoreversion

; Скрипт удаления БД
Source: "DBFiles\remove-database.ps1"; DestDir: "{app}\DBFiles"; Flags: ignoreversion

; Файл бекапа базы данных
Source: "DBFiles\dbfull.bak"; DestDir: "{app}\DBFiles"; Flags: ignoreversion

; Установщик SQL Server 2025 Express (содержит LocalDB)
Source: "redist\SQL2025-SSEI-Expr.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsLocalDBInstalled

; Установщик .NET 8 Desktop Runtime (версия 8.0.26)
Source: "redist\windowsdesktop-runtime-8.0.26-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet8Installed

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Установка SQL Server 2025 LocalDB через SSEI-Expr (если не установлен)
Filename: "{tmp}\SQL2025-SSEI-Expr.exe"; \
    Parameters: "/quiet /norestart /IACCEPTSQLSERVERLICENSETERMS=YES /ACTION=Install /FEATURES=LocalDB"; \
    Check: not IsLocalDBInstalled; \
    StatusMsg: "Установка SQL Server 2025 LocalDB..."; \
    Flags: runhidden

; Ожидание завершения установки и создание экземпляра LocalDB
Filename: "{cmd}"; \
    Parameters: "/C sqllocaldb create MSSQLLocalDB -s"; \
    Check: IsLocalDBInstalled and not IsLocalDBInstanceExists; \
    StatusMsg: "Настройка экземпляра LocalDB..."; \
    Flags: runhidden

; Установка .NET 8 Desktop Runtime (версия 8.0.26, если не установлен)
Filename: "{tmp}\windowsdesktop-runtime-8.0.26-win-x64.exe"; \
    Parameters: "/quiet /norestart"; \
    Check: not IsDotNet8Installed; \
    StatusMsg: "Установка .NET 8 Runtime..."; \
    Flags: runhidden

; Настройка базы данных
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\DBFiles\setup-database.ps1"" -AppPath ""{app}"""; \
    StatusMsg: "Настройка базы данных..."; \
    Flags: runhidden

; Запуск приложения
Filename: "{app}\{#MyAppExeName}"; \
    Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; \
    Flags: nowait postinstall skipifsilent

[UninstallRun]
; Удаление базы данных при деинсталляции
Filename: "powershell.exe"; \
    Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\DBFiles\remove-database.ps1"" -AppPath ""{app}"""; \
    RunOnceId: "RemoveDatabase"; \
    Flags: runhidden

[Code]
// Проверка установки .NET 8 Desktop Runtime
function IsDotNet8Installed: Boolean;
var
  Version: string;
begin
  Result := RegQueryStringValue(
    HKLM64, 
    'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App', 
    '8.0.0', 
    Version
  );
  if not Result then
  begin
    Result := RegQueryStringValue(
      HKLM32, 
      'SOFTWARE\dotnet\Setup\InstalledVersions\x86\sharedfx\Microsoft.WindowsDesktop.App', 
      '8.0.0', 
      Version
    );
  end;
end;

// Проверка установки SQL Server LocalDB (версия 17.x для SQL 2025)
function IsLocalDBInstalled: Boolean;
begin
  Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server\LocalDB\Installed Versions\17.0');
  if not Result then
  begin
    Result := RegKeyExists(HKLM64, 'SOFTWARE\Microsoft\Microsoft SQL Server\LocalDB\Installed Versions\17.0');
  end;
end;

// Проверка существования экземпляра MSSQLLocalDB
function IsLocalDBInstanceExists: Boolean;
var
  ResultCode: Integer;
  Output: string;
begin
  Result := False;
  if Exec('cmd', '/C sqllocaldb info MSSQLLocalDB', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then
      Result := True;
  end;
end;

// Обновление appsettings.json с правильной строкой подключения
procedure UpdateAppSettings();
var
  AppSettingsPath: string;
  FileContent: string;
begin
  AppSettingsPath := ExpandConstant('{app}\appsettings.json');
  
  ForceDirectories(ExpandConstant('{app}'));
  
  FileContent := 
    '{' + #13#10 +
    '  "ConnectionStrings": {' + #13#10 +
    '    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB; Database=RentalCarDB; Trusted_Connection=True; TrustServerCertificate=True;"' + #13#10 +
    '  }' + #13#10 +
    '}';
  
  SaveStringToFile(AppSettingsPath, FileContent, False);
end;

// Выполнение после установки
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    UpdateAppSettings();
  end;
end;