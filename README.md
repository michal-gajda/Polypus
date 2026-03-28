# Polypus

```powershell
git init
dotnet new gitignore
dotnet new sln --name Polypus
```

## WebApi

```powershell
dotnet new webapi --framework net10.0 --no-https --use-program-main --output src/WebApi --use-controllers --name Polypus.WebApi
dotnet sln add src/WebApi
```

## WebUI

```powershell
dotnet new blazorwasm --framework net10.0 --no-https --use-program-main --output src/WebUI --name Polypus.WebUI
dotnet sln add src/WebUI
dotnet add src/WebApi reference src/WebUI
```


```powershell
[Environment]::SetEnvironmentVariable("VAULT_SECRET_ID", "hvs.A0nNeSZPQLDiqVqEgucKq0jn", "Machine")
```
