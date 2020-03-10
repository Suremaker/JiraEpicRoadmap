rd /S /Q out
mkdir out
xcopy /E /Y bin out
dotnet publish JiraEpicRoadmapper\Server\JiraEpicRoadmapper.Server.csproj -o out\server