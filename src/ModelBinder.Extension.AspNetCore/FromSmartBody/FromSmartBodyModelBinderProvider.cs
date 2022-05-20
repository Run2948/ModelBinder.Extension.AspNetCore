using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

#if NET5_0_OR_GREATER
using ComplexDataModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexObjectModelBinderProvider;
#else
using ComplexDataModelBinderProvider = Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexTypeModelBinderProvider;
#endif

namespace ModelBinder.Extension.AspNetCore.FromSmartBody
{
    public class FromSmartBodyModelBinderProvider : IModelBinderProvider
    {
        private readonly BodyModelBinderProvider _bodyModelBinderProvider;
        private readonly ComplexDataModelBinderProvider _complexDataModelBinderProvider;

        public FromSmartBodyModelBinderProvider(BodyModelBinderProvider bodyModelBinderProvider, ComplexDataModelBinderProvider complexDataModelBinderProvider)
        {
            _bodyModelBinderProvider = bodyModelBinderProvider;
            _complexDataModelBinderProvider = complexDataModelBinderProvider;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.BindingInfo.BindingSource != null && (context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Body) || context.BindingInfo.BindingSource.CanAcceptDataFrom(FromSmartBodyBindingSource.FromSmartBody)))
            {
                var bodyBinder = _bodyModelBinderProvider.GetBinder(context);
                var complexBinder = _complexDataModelBinderProvider.GetBinder(context);
                return new FromSmartBodyModelBinder(bodyBinder, complexBinder);
            }
            return null;
        }
    }
}
