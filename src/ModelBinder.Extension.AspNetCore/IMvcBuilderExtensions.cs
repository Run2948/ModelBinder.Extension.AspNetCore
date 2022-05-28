using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using ModelBinder.Extension.AspNetCore.FromSmartBody;

#if NET5_0_OR_GREATER
using ComplexDataModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexObjectModelBinderProvider;
#else
using ComplexDataModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexTypeModelBinderProvider;
#endif

namespace ModelBinder.Extension.AspNetCore
{
    public static class IMvcBuilderExtensions
    {
        public static IMvcBuilder AddSmartBody(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMvcOptions(options =>
            {
                options.ModelBinderProviders.InsertSmartBodyBinding();
            });
            return mvcBuilder;
        }

        public static IList<IModelBinderProvider> InsertSmartBodyBinding(this IList<IModelBinderProvider> providers)
        {
            var bodyProvider = providers.OfType<BodyModelBinderProvider>().Single();
            var complexDataProvider = providers.OfType<ComplexDataModelBinderProvider>().Single();
            providers.Insert(0, new FromSmartBodyModelBinderProvider(bodyProvider, complexDataProvider));
            return providers;
        }

    }
}
