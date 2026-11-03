# MCP scripts

The workspace configures only MudMCP, exposed to Antigravity at `http://localhost:8000/mcp`. MudMCP upstream provides a Dockerfile and Compose setup; this repository pins source release `v1.1.0` and builds a local image. No prebuilt upstream image was verified. The generated Compose configuration binds to loopback and persists the component index in a named volume. First startup may take several minutes to clone/index MudBlazor. MudMCP is an independent community project.

## Requirements

Windows, PowerShell, Git, Docker Desktop/Engine, and Docker Compose plugin. MudBlazor documentation version should match the package actually resolved by the client. `HF.Web` currently floats `9.*`, so check the resolved version and pass it explicitly.

## Commands

Run from repository root:

```powershell
./scripts/mudmcp-install.ps1 -MudBlazorVersion 9.0.0
./scripts/mudmcp-start.ps1 -MudBlazorVersion 9.0.0
./scripts/mudmcp-stop.ps1
./scripts/mudmcp-uninstall.ps1
```

Install clones upstream tag `v1.1.0` to `%LOCALAPPDATA%\HorizonFuture\MudMCP` and builds the Docker image. Start builds if needed and launches the service. Stop preserves the container cache volume. Uninstall requires typing `REMOVE`, then deletes the container, local image, clone, and cache volume; pass `-Force` only to intentionally skip confirmation. Use `-InstallRoot <path>` to select another install location.

Check `http://localhost:8000/health` after startup. Antigravity reads `.agents/mcp_config.json`; reload MCP connections after installation or configuration changes. Never submit student data, credentials, or private source text to MudMCP.
