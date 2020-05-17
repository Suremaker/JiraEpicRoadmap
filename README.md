# Jira Epic Roadmapper

**Website for displaying and managing development roadmap based on Jira epics**

Pipeline: [![Build status](https://ci.appveyor.com/api/projects/status/gxvk7u8k1qrkw0g9?svg=true)](https://ci.appveyor.com/project/Suremaker/jiraepicroadmap)

### Features

* Displays the roadmap view of all epics grouped by projects
* Displays epic cards with status and progress bar showing done, in progress and not started tickets
* Displays epic details with ability to change `Start date` and `Due date` of the epic, when card is selected
* Allows filtering by closed epics, unplanned epics as well as hiding epic details or today indicator on the screen
* Permalinks

### Technologies

* ASP.NET Core Web API for hosting the website
* Blazor UI interface

## Configuration

Edit `src\JiraEpicRoadmapper.Server\appsettings.json`:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Config": {
    "JiraUri": "[YOUR JIRA BASE URI]",
    "AuthKey": "[YOUR AUTHENTICATION KEY TO JIRA]",
    "EpicQueryFilter": "[ADDITIONAL FILTERS FOR EPICS]"
  }
}
```

### Publishing and deployment

Simply run `publish_linux.bat` or `dotnet publish src\JiraEpicRoadmapper.Server\JiraEpicRoadmapper.Server.csproj -c Release -r linux-x64 --self-contained -o publish`

Then deploy on favourite location.