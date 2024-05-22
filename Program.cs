using FoodHealthChecker.Components;
using FoodHealthChecker.SemanticKernel.Plugins;
using Markdig;

namespace FoodHealthChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<MarkdownPipeline>(serviceProvider =>
            {
                return new MarkdownPipelineBuilder().UseSoftlineBreakAsHardlineBreak().Build();
            });
            builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
            builder.Services.AddSingleton<FoodCheckerPlugin>();
            builder.Services.AddTransient<FoodCheckerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
