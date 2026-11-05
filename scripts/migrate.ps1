param(
    [Parameter(Mandatory = $true, HelpMessage = "Migration name")]
    [Alias("n")]
    [string]$MigrationName,
    [Parameter(Mandatory = $true, HelpMessage = "Application or Service Migration")]
    [Alias("a")]
    [ValidateSet("a", "s")]
    [string]$MigrationType
)
Push-Location "${PSScriptRoot}\.."
dotnet tool restore
if ($MigrationType -eq "a") {
    dotnet ef migrations add $MigrationName --startup-project src/HF.Mobile/ --output-dir Infrastructure/Persistence/Migrations
}
else
{
    dotnet ef migrations add $MigrationName --startup-project src/HF.API/ --project ./src/HF.Infrastructure/ --output-dir Persistence/Migrations
}