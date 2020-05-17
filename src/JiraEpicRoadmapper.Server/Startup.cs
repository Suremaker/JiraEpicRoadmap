using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using JiraEpicRoadmapper.Server.Clients;
using JiraEpicRoadmapper.Server.Mappers;
using JiraEpicRoadmapper.Server.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace JiraEpicRoadmapper.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JiraEpicRoadmapper API", Version = "v1" });
            });

            services.Configure<Config>(Configuration.GetSection("Config"));
            services.AddHttpClient(nameof(IJiraClient), ConfigureJiraClient);
            services.AddSingleton<IJiraClient, JiraClient>();
            services.AddSingleton<IEpicProvider, EpicProvider>();
            services.AddSingleton<IEpicMapper, EpicMapper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JiraEpicRoadmapper API V1");
            });


            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });


        }

        private static void ConfigureJiraClient(IServiceProvider sp, HttpClient client)
        {
            var cfg = sp.GetService<IOptions<Config>>().Value;
            client.BaseAddress = new Uri(cfg.JiraUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(cfg.AuthKey)));
            client.DefaultRequestHeaders.Add("ContentType", "application/json");
        }
    }
}
