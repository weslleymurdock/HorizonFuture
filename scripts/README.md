# MCP scripts

The workspace configures only MudMCP, exposed to Antigravity at `http://localhost:8000/mcp`. The upstream has a Dockerfile and Compose setup; this repo pins source release `v1.1.0` and builds a local image (no verified prebuilt image). The Compose configuration binds to loopback and persists the component index in a named volume. First startup may take several minutes to clone/index MudBlazor. MudMCP is an independent community project.

## Requirements

- Windows: PowerShell, Git, Docker Desktop/Engine, and Docker Compose plugin.
- Debian: Bash, Git, Docker CLI/Engine access, and Docker Compose plugin.
- Match the MudBlazor version served by MudMCP to the package version actually resolved by the client. `HF.Web` currently floats `9.*`.

## PowerShell

```powershell
./scripts/mudmcp-install.ps1 -MudBlazorVersion 9.0.0
./scripts/mudmcp-start.ps1 -MudBlazorVersion 9.0.0
./scripts/mudmcp-stop.ps1
./scripts/mudmcp-uninstall.ps1
```

## Bash (Debian)

```bash
./scripts/mudmcp-install.sh --mudblazor-version 9.0.0
./scripts/mudmcp-start.sh --mudblazor-version 9.0.0
./scripts/mudmcp-stop.sh
./scripts/mudmcp-uninstall.sh
```

Bash scripts use `$XDG_DATA_HOME/HorizonFuture/MudMCP` or `~/.local/share/HorizonFuture/MudMCP`. Override with `MUDMCP_INSTALL_ROOT=/path` or `--install-root /path`. Set `MUDMCP_MUDBLAZOR_VERSION` or pass `--mudblazor-version <version>`. Both install paths pin MudMCP tag `v1.1.0`. Stop preserves the cache volume. Uninstall requires typing `REMOVE` and removes the container, locally built image, clone, and cache; use `--force` (Bash) or `-Force` (PowerShell) only to skip that confirmation intentionally.

Check `http://localhost:8000/health` after startup. Antigravity reads `.agents/mcp_config.json`; reload MCP connections after installing or changing config. Do not submit student data, credentials, or private source to MudMCP.
