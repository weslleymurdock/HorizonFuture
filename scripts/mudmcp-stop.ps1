[CmdletBinding()]
param([string] $InstallRoot, [string] $MudBlazorVersion = '9.0.0')
& (Join-Path $PSScriptRoot 'mudmcp.ps1') -Action Stop -InstallRoot $InstallRoot -MudBlazorVersion $MudBlazorVersion
