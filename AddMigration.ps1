
# 첫번째 입력값을 마이그레이션 이름으로 사용합니다
$migrationName = $args[0]
# 빈 문자열을 '_' 문자로 치환합니다
$migrationName = $migrationName -replace " ", "_"
echo "마이그레이션을 추가합니다. 이름: $migrationName"
cd SampleService.Data
dotnet ef migrations add "$migrationName" --startup-project ../SampleService.Database.Manager/SampleService.Database.Manager.csproj --context DataContext --project ../SampleService.Data.SqlServer/SampleService.Data.SqlServer.csproj --json --verbose
cd ..