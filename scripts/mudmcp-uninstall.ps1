[CmdletBinding()]
param([string] $InstallRoot, [string] $MudBlazorVersion = '9.0.0', [switch] $Force)
& (Join-Path $PSScriptRoot 'mudmcp.ps1') -Action Uninstall -InstallRoot $InstallRoot -MudBlazorVersion $MudBlazorVersion -Force:$Force
