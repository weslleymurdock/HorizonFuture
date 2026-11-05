[CmdletBinding()]
param(
    [string]$ApiPort = "5268",
    [string]$ClientName = "ApiClient",
    [string]$ClientNamespace = "HF.Client"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path

$apiDir = Join-Path $repoRoot "src/HF.API"
$clientDir = Join-Path $repoRoot "src/HF.Client"
$openApiDir = Join-Path $clientDir "OpenApi"
$specFile = Join-Path $openApiDir "openapi.json"
$generatedDir = Join-Path $clientDir "Generated"
$openApiUrl = "http://127.0.0.1:$ApiPort/openapi/v1.json"

if (-not (Test-Path $openApiDir)) {
    New-Item -ItemType Directory -Path $openApiDir -Force | Out-Null
}
if (-not (Test-Path $generatedDir)) {
    New-Item -ItemType Directory -Path $generatedDir -Force | Out-Null
}

Write-Host "==> Restoring dotnet local tools (including Kiota)..."
dotnet tool restore --tool-manifest (Join-Path $repoRoot "dotnet-tools.json")

Write-Host "==> Starting HF.API on port $ApiPort to extract OpenAPI specification..."
$env:ASPNETCORE_ENVIRONMENT = "Development"
if (-not $env:ConnectionStrings__PostgresConnection) {
    $env:ConnectionStrings__PostgresConnection = "Host=localhost;Database=horizonfuture_dev;Username=postgres;Password=postgres"
}
if (-not $env:Jwt__Key) {
    $env:Jwt__Key = "HorizonFuture-Development-Secret-Key-Must-Be-At-Least-32-Bytes-Long-123456"
}

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "dotnet"
$psi.Arguments = "run --project `"$apiDir`" --no-launch-profile --urls http://127.0.0.1:$ApiPort"
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true

$process = [System.Diagnostics.Process]::Start($psi)

try {
    Write-Host "==> Waiting for OpenAPI endpoint at $openApiUrl..."
    $timeoutSeconds = 35
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $success = $false

    while ($stopwatch.Elapsed.TotalSeconds -lt $timeoutSeconds) {
        if ($process.HasExited) {
            throw "HF.API process terminated unexpectedly with exit code $($process.ExitCode) before OpenAPI could be retrieved."
        }
        try {
            $response = Invoke-WebRequest -Uri $openApiUrl -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
            if ($response.StatusCode -eq 200 -and $response.Content.Length -gt 0) {
                Set-Content -Path $specFile -Value $response.Content -NoNewline
                $success = $true
                break
            }
        }
        catch {
            Start-Sleep -Milliseconds 1000
        }
    }

    if (-not $success) {
        throw "Failed to retrieve OpenAPI specification from $openApiUrl within $timeoutSeconds seconds."
    }

    Write-Host "==> OpenAPI specification successfully saved to $specFile."
}
finally {
    if ($process -and -not $process.HasExited) {
        Write-Host "==> Stopping HF.API..."
        $process.Kill()
        $process.WaitForExit(5000)
    }
}

Write-Host "==> Running Kiota to generate C# client..."
dotnet kiota generate `
  --openapi "$specFile" `
  --language CSharp `
  --class-name "$ClientName" `
  --namespace-name "$ClientNamespace" `
  --output "$generatedDir" `
  --clean-output `
  --log-level Warning

Write-Host "==> Kiota client successfully generated in $generatedDir."
