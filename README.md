# homebot

like homarr but in your favorite chat application

supported chat apps:
- discord

supported functionality:
- jellyfin
    - see how many movies/series/episodes there are currently added
    - see basic system info
- prometheus
    - see current value of an expression
    - graph an expression
    - see number of unhealthytargets
    - see firing alerts
    - see basic prometheus system info
    - defined predefined queries for quickly graphing important things with auto completion in discord
    - restrict users from graphing anything else other than predefined queries by default

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