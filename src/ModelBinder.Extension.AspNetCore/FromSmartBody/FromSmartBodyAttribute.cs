using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace ModelBinder.Extension.AspNetCore.FromSmartBody
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class FromSmartBodyAttribute : Attribute, IBindingSourceMetadata
    {
        public string PropertyName { get; private set; }

        public BindingSource BindingSource => FromSmartBodyBindingSource.FromSmartBody;

        public FromSmartBodyAttribute(string propertyName = null)
        {
            PropertyName = propertyName;
        }
    }
}
