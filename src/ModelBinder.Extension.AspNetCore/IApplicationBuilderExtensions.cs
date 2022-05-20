using Microsoft.AspNetCore.Builder;
using ModelBinder.Extension.AspNetCore.FromJsonBody;

namespace ModelBinder.Extension.AspNetCore
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFromJsonBody(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<FromJsonBodyMiddleware>();
        }
    }
}
