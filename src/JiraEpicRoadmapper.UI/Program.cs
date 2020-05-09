using System;
using System.Net.Http;
using System.Threading.Tasks;
using JiraEpicRoadmapper.UI.Repositories;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JiraEpicRoadmapper.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddTransient<IEpicsRepository, EpicsRepository>();

            await builder.Build().RunAsync();
        }
    }
}
