# homebot

like homarr but in your favorite chat application

supported chat apps:
- discord*

supported integrations:
- qbittorrent*
- home assistant*
- sonarr*
- radarr*
- prometheus*
- loki*

\* planned

## dev

windows
```ps1
winget install -e --id Microsoft.DotNet.SDK.10
winget install -e --id Git.Git
winget install -e --id GitHub.cli

gh auth login
gh repo clone andreiverse/homebot

Copy-Item set_env.ps1.example set_env.ps1
# set values inside set_env.ps1

.\run.ps1
```