
echo "가장 최근 작성된 데이터베이스 마이그레이션을 제거합니다."
cd SampleService.Data
dotnet ef migrations remove --startup-project ../SampleService.Database.Manager/SampleService.Database.Manager.csproj --context DataContext --project ../SampleService.Data.SqlServer/SampleService.Data.SqlServer.csproj --json --verbose
cd ..