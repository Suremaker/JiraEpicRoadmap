rd /S /Q out
mkdir out
xcopy /E /Y template out
copy /Y JiraEpicRoadmapper\Server\config.json out\config.json
dotnet publish src\JiraEpicRoadmapper.Server\JiraEpicRoadmapper.Server.csproj -c Release -r win-x64 --self-contained -o out\server