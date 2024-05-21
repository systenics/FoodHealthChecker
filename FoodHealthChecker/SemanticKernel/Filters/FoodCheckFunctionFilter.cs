using Microsoft.SemanticKernel;

namespace FoodHealthChecker.SemanticKernel.Filters
{
    //TODO - Update to use filters plugin
    public class FoodCheckFunctionFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                context.Result = new FunctionResult(context.Function, "Function call failed");
            }
        }
    }
}
