using FoodHealthChecker.Components;
using FoodHealthChecker.SemanticKernel.Plugins;
using Markdig;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.SignalR;

namespace FoodHealthChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();
            builder.Services.AddDataProtection()
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
           .AddHubOptions(options =>
                {
                    options.MaximumReceiveMessageSize = 32 * 1024 * 1024;
                    options.MaximumParallelInvocationsPerClient = 1;
                }); ;
               
            builder.Services.AddSingleton<MarkdownPipeline>(serviceProvider =>
            {
                return new MarkdownPipelineBuilder().UseSoftlineBreakAsHardlineBreak().Build();
            });
            builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
            builder.Services.AddSingleton<FoodCheckerPlugin>();
            builder.Services.AddTransient<FoodCheckerService>();

            var app = builder.Build();
            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            app.MapControllers();
            app.Run();
        }
    }
}
