using Microsoft.SemanticKernel;

namespace FoodHealthChecker.SemanticKernel.Filters
{
    //TODO - Update to use filters plugin
    public class FoodCheckPromptRenderFilter : IPromptRenderFilter
    {
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            await next(context);
        }
    }
}
