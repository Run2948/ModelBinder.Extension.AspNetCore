using Microsoft.AspNetCore.Mvc;
using System;

namespace ModelBinder.Extension.AspNetCore.FromJsonBody
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class FromJsonBodyAttribute : ModelBinderAttribute
    {
        public string PropertyName { get; private set; }

        public FromJsonBodyAttribute(string propertyName = null) : base(typeof(FromJsonBodyModelBinder))
        {
            PropertyName = propertyName;
        }
    }
}