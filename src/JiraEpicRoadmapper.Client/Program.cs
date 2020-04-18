using System.Threading.Tasks;
using JiraEpicRoadmapper.Client.Model;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JiraEpicRoadmapper.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            builder.Services.AddScoped<ViewOptions>();

            await builder.Build().RunAsync();
        }
    }
}
