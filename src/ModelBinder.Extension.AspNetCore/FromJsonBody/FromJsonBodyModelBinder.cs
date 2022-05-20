using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBinder.Extension.AspNetCore.FromJsonBody
{
    public class FromJsonBodyModelBinder : IModelBinder
    {
        public static readonly IDictionary<string, FromJsonBodyAttribute> fromJsonBodyAttrCache = new ConcurrentDictionary<string, FromJsonBodyAttribute>();

        /// <summary>
        /// BindModelAsync will be invoked on every parameter that is marked as [FromJsonBody]
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;
            if(request.ContentType == null || !request.ContentType.StartsWith("application/json"))
                throw new ApplicationException("ContentType of request should be application/json");

            var key = FromJsonBodyMiddleware.RequestJsonObject_Key;
            var itemValue = bindingContext.ActionContext.HttpContext.Items[key]?.ToString();
            if (itemValue == null)
                throw new ApplicationException("'RequestJsonObject' not found in HttpContext.Items,please app.UseMiddleware<FromJsonBodyMiddleware>() or app.UseFromJsonBody() first");

            JObject jsonObj = JObject.Parse(itemValue);
            string fieldName = bindingContext.FieldName;

            FromJsonBodyAttribute fromJsonBodyAttr = GetFromJsonBodyAttr(bindingContext, fieldName);
            //if the propertyName of FromJsonBodyAttribute is not null, use that value 
            // as the fieldName instanceof the parameter name
            //for example: [FromJsonBody("i2")] int i1
            if (!string.IsNullOrWhiteSpace(fromJsonBodyAttr.PropertyName))
                fieldName = fromJsonBodyAttr.PropertyName;

            //if property found
            //bindingContext.FieldName is the name of binded parameter
            var jsonValue = jsonObj.SelectToken(fieldName, false);
            if (jsonValue != null)
            {
                //conver to the type of jsonValue to  type of parameter
                //bindingContext.ModelType:parameter type
                var targetValue = jsonValue.ChangeType(bindingContext.ModelType);
                bindingContext.Result = ModelBindingResult.Success(targetValue);
            }
            else//if property not found
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }

        private static FromJsonBodyAttribute GetFromJsonBodyAttr(ModelBindingContext bindingContext, string fieldName)
        {
            var actionDesc = bindingContext.ActionContext.ActionDescriptor;
            string actionId = actionDesc.Id;
            string cacheKey = $"{actionId}:{fieldName}";

            //fetch from cache to improve performance
            if (!fromJsonBodyAttrCache.TryGetValue(cacheKey, out FromJsonBodyAttribute fromJsonBodyAttr))
            {
                var ctrlActionDesc = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
                var fieldParameter = ctrlActionDesc.MethodInfo.GetParameters().Single(p => p.Name == fieldName);
                fromJsonBodyAttr = fieldParameter.GetCustomAttributes(typeof(FromJsonBodyAttribute), false).Single() as FromJsonBodyAttribute;
                fromJsonBodyAttrCache[cacheKey] = fromJsonBodyAttr;
            }
            return fromJsonBodyAttr;
        }
    }
}
