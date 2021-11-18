using OmpHandsOnUi;
using OmpHandsOnUi.Services;

namespace OMP.Connector.EdgeModule
{
	public static class Program
    {
        private static string _settingsJsonFileName;

        private static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSignalR();


            builder.Services.AddSingleton<ConfigurationService>();
            builder.Services.AddSingleton<MqttService>();
            builder.Services.AddSingleton<RequestService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.MapHub<HandsOnHub>("/handsOnHub");

            await app.RunAsync();
        }
    }
}