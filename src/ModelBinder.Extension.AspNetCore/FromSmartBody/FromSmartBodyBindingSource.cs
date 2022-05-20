using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ModelBinder.Extension.AspNetCore.FromSmartBody
{
    public class FromSmartBodyBindingSource : BindingSource
    {
        public static readonly BindingSource FromSmartBody = new FromSmartBodyBindingSource("FromSmartBody", "FromSmartBody", true, true);

        public FromSmartBodyBindingSource(string id, string displayName, bool isGreedy, bool isFromRequest) : base(id, displayName, isGreedy, isFromRequest)
        {
        }

        public override bool CanAcceptDataFrom(BindingSource bindingSource)
        {
            return bindingSource == Body || bindingSource == this;
        }
    }
}
