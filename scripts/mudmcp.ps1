[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Install', 'Start', 'Stop', 'Uninstall')]
    [string] $Action,
    [string] $InstallRoot,
    [string] $MudBlazorVersion = '9.0.0',
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
$UpstreamUrl = 'https://github.com/mcbodge/MudMCP.git'
$UpstreamTag = 'v1.1.0'
$ProjectName = 'horizonfuture-mudmcp'
$ComposeFileName = 'docker-compose.horizon.yml'

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    if ([string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
        throw 'LOCALAPPDATA is not set. Pass -InstallRoot with a writable directory.'
    }
    $InstallRoot = Join-Path $env:LOCALAPPDATA 'HorizonFuture\MudMCP'
}
$InstallRoot = [System.IO.Path]::GetFullPath($InstallRoot)

function Invoke-Checked {
    param([string] $Executable, [string[]] $Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) { throw "$Executable failed with exit code $LASTEXITCODE." }
}

function Assert-DockerAvailable {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw 'Docker CLI is required. Install Docker Desktop or Docker Engine with the Compose plugin.'
    }
    Invoke-Checked 'docker' @('info')
    Invoke-Checked 'docker' @('compose', 'version')
}

function Get-ComposeArguments {
    $composePath = Join-Path $InstallRoot $ComposeFileName
    if (-not (Test-Path -LiteralPath $composePath -PathType Leaf)) {
        throw "MudMCP is not installed at '$InstallRoot'. Run scripts/mudmcp-install.ps1 first."
    }
    return @('compose', '--project-name', $ProjectName, '--project-directory', $InstallRoot, '-f', $composePath)
}

function Write-ComposeFile {
    $compose = @'
services:
  mudblazor-mcp:
    build:
      context: .
      dockerfile: Dockerfile
    image: horizonfuture-mudmcp:v1.1.0
    container_name: horizonfuture-mudmcp
    ports:
      - "127.0.0.1:8000:8080"
    volumes:
      - mudmcp-data:/app/data
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      MUDBLAZOR_VERSION: "${MUDBLAZOR_VERSION:-9.0.0}"
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 600s
    restart: unless-stopped
volumes:
  mudmcp-data:
    name: horizonfuture-mudmcp-data
'@
    Set-Content -LiteralPath (Join-Path $InstallRoot $ComposeFileName) -Value $compose -Encoding utf8
}

function Ensure-MudMcpInstalled {
    if (-not (Test-Path -LiteralPath $InstallRoot -PathType Container)) {
        if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Git is required for the first install.' }
        New-Item -ItemType Directory -Path (Split-Path -Parent $InstallRoot) -Force | Out-Null
        Invoke-Checked 'git' @('clone', '--branch', $UpstreamTag, '--depth', '1', $UpstreamUrl, $InstallRoot)
    }
    if (-not (Test-Path -LiteralPath (Join-Path $InstallRoot 'Dockerfile') -PathType Leaf)) {
        throw "The install directory '$InstallRoot' does not contain the pinned MudMCP Docker build context."
    }
    Write-ComposeFile
}

function Invoke-MudMcpCompose {
    param([string[]] $ComposeCommand)
    $priorVersion = $env:MUDBLAZOR_VERSION
    try {
        $env:MUDBLAZOR_VERSION = $MudBlazorVersion
        Invoke-Checked 'docker' ((Get-ComposeArguments) + $ComposeCommand)
    }
    finally {
        if ($null -eq $priorVersion) { Remove-Item Env:\MUDBLAZOR_VERSION -ErrorAction SilentlyContinue }
        else { $env:MUDBLAZOR_VERSION = $priorVersion }
    }
}

switch ($Action) {
    'Install' {
        Assert-DockerAvailable
        Ensure-MudMcpInstalled
        Invoke-MudMcpCompose @('build')
        Write-Host "MudMCP $UpstreamTag image built."
    }
    'Start' {
        Assert-DockerAvailable
        Ensure-MudMcpInstalled
        Invoke-MudMcpCompose @('up', '--detach', '--build')
        Write-Host 'MCP endpoint: http://localhost:8000/mcp'
        Write-Host 'The first index build may take several minutes.'
    }
    'Stop' {
        Assert-DockerAvailable
        Invoke-MudMcpCompose @('down')
        Write-Host 'MudMCP stopped; its cache volume was preserved.'
    }
    'Uninstall' {
        Assert-DockerAvailable
        $composeArgs = Get-ComposeArguments
        if (-not $Force) {
            $answer = Read-Host 'This removes the container, image, cloned source, and cached index. Type REMOVE to continue'
            if ($answer -cne 'REMOVE') { Write-Host 'Uninstall cancelled.'; return }
        }
        Invoke-Checked 'docker' ($composeArgs + @('down', '--volumes', '--remove-orphans', '--rmi', 'local'))
        Remove-Item -LiteralPath $InstallRoot -Recurse -Force
        Write-Host 'MudMCP and cached data removed.'
    }
}
