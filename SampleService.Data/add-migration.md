## Add Migration

```
dotnet ef migrations add InitDB --startup-project ../SampleService.Database.Manager/SampleService.Database.Manager.csproj --context DataContext --project ../SampleService.Data.SqlServer/SampleService.Data.SqlServer.csproj --json --verbose
```

## Update 

```
dotnet ef database update --startup-project ../SampleService.Database.Manager/SampleService.Database.Manager.csproj --context DataContext --project ../SampleService.Data.SqlServer/SampleService.Data.SqlServer.csproj --verbose
```