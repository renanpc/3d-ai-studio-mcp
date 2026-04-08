param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,

    [Parameter(Mandatory=$true)]
    [int]$Port,

    [Parameter(Mandatory=$true)]
    [string]$RegisterClaudeCode,

    [Parameter(Mandatory=$true)]
    [string]$RegisterCodexMcp,

    [Parameter(Mandatory=$true)]
    [string]$InstallCodexSkill,

    [Parameter(Mandatory=$true)]
    [string]$CodexSkillName,

    [switch]$RunWorker
)

$ErrorActionPreference = "Continue"
$logFile = Join-Path $env:TEMP "3dai_register.log"
"Starting at $(Get-Date)" | Out-File $logFile -Append

if (-not $RunWorker) {
    try {
        $stdoutLog = Join-Path $env:TEMP "3dai_register_worker.stdout.log"
        $stderrLog = Join-Path $env:TEMP "3dai_register_worker.stderr.log"
        $escapedScriptPath = $PSCommandPath.Replace('"', '""')
        $escapedServerName = $ServerName.Replace('"', '""')
        $escapedRegisterClaudeCode = $RegisterClaudeCode.Replace('"', '""')
        $escapedRegisterCodexMcp = $RegisterCodexMcp.Replace('"', '""')
        $escapedInstallCodexSkill = $InstallCodexSkill.Replace('"', '""')
        $escapedCodexSkillName = $CodexSkillName.Replace('"', '""')
        $argumentList = "-NoLogo -NoProfile -ExecutionPolicy Bypass -File ""$escapedScriptPath"" -ServerName ""$escapedServerName"" -Port ""$Port"" -RegisterClaudeCode ""$escapedRegisterClaudeCode"" -RegisterCodexMcp ""$escapedRegisterCodexMcp"" -InstallCodexSkill ""$escapedInstallCodexSkill"" -CodexSkillName ""$escapedCodexSkillName"" -RunWorker"

        Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') `
            -ArgumentList $argumentList `
            -WindowStyle Hidden `
            -RedirectStandardOutput $stdoutLog `
            -RedirectStandardError $stderrLog | Out-Null

        "Detached worker launched at $(Get-Date)" | Out-File $logFile -Append
    } catch {
        "Failed to launch detached worker: $_" | Out-File $logFile -Append
    }

    exit 0
}

function Get-CommandAvailable {
    param([string]$Name)
    try {
        $null = Get-Command -Name $Name -ErrorAction SilentlyContinue
        return $true
    } catch { return $false }
}

function Get-CommandPath {
    param([string]$Name)
    try {
        $command = Get-Command -Name $Name -ErrorAction Stop
        return $command.Source
    } catch {
        return $null
    }
}

function Invoke-Safe {
    param([scriptblock]$SB, [string]$Description)
    try {
        $result = & $SB 2>&1
        "[$Description] OK: $result" | Out-File $logFile -Append
    } catch {
        "[$Description] ERROR: $_" | Out-File $logFile -Append
    }
}

function Wait-ForTcpPort {
    param(
        [int]$PortNumber,
        [int]$TimeoutSeconds = 60
    )

    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)

    while ([DateTime]::UtcNow -lt $deadline) {
        $client = $null
        try {
            $client = [System.Net.Sockets.TcpClient]::new()
            $async = $client.BeginConnect("127.0.0.1", $PortNumber, $null, $null)
            if ($async.AsyncWaitHandle.WaitOne(1000, $false) -and $client.Connected) {
                $client.EndConnect($async)
                "Port $PortNumber is accepting connections" | Out-File $logFile -Append
                return $true
            }
        } catch {
        } finally {
            if ($null -ne $client) {
                $client.Dispose()
            }
        }

        Start-Sleep -Seconds 1
    }

    "Timed out waiting for port $PortNumber" | Out-File $logFile -Append
    return $false
}

$installFolder = Split-Path -Parent $PSCommandPath
$serverUrl = "http://localhost:$Port"
$skillSrc = Join-Path $installFolder "SKILL.md"

"ServerUrl=$serverUrl InstallFolder=$installFolder" | Out-File $logFile -Append
"claude available: $(Get-CommandAvailable 'claude')" | Out-File $logFile -Append
"codex available: $(Get-CommandAvailable 'codex')" | Out-File $logFile -Append

$claudePath = Get-CommandPath 'claude'
$codexPath = Get-CommandPath 'codex'

"claude path: $claudePath" | Out-File $logFile -Append
"codex path: $codexPath" | Out-File $logFile -Append

if (-not (Wait-ForTcpPort -PortNumber $Port)) {
    "Skipping registration because the service endpoint did not become ready" | Out-File $logFile -Append
    exit 0
}

if ($RegisterClaudeCode -eq "1" -and $claudePath) {
    Invoke-Safe { & $claudePath mcp remove $ServerName 2>$null } "claude mcp remove"
    Invoke-Safe { & $claudePath mcp add --transport http --scope user $ServerName $serverUrl } "claude mcp add"
}

if ($RegisterCodexMcp -eq "1" -and $codexPath) {
    Invoke-Safe { & $codexPath mcp remove $ServerName 2>$null } "codex mcp remove"
    Invoke-Safe { & $codexPath mcp add $ServerName --url $serverUrl } "codex mcp add"
}

if ($InstallCodexSkill -eq "1") {
    $skillsRoot = Join-Path $env:USERPROFILE ".codex\skills"
    $skillDir = Join-Path $skillsRoot $CodexSkillName
    if (Test-Path $skillSrc) {
        Invoke-Safe { New-Item -ItemType Directory -Path $skillDir -Force | Out-Null } "mkdir skill dir"
        Invoke-Safe { Copy-Item $skillSrc (Join-Path $skillDir "SKILL.md") -Force } "copy skill"
    } else {
        "SKILL.md not found at $skillSrc" | Out-File $logFile -Append
    }
}

"Done at $(Get-Date)" | Out-File $logFile -Append
