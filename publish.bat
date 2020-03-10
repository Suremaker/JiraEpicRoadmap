rd /S /Q out
mkdir out
xcopy /E /Y template out
copy /Y JiraEpicRoadmapper\Server\config.json out\config.json
dotnet publish JiraEpicRoadmapper\Server\JiraEpicRoadmapper.Server.csproj -o out\server