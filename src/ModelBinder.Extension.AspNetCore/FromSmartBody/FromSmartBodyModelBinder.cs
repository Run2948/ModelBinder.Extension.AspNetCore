using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelBinder.Extension.AspNetCore.FromSmartBody
{
    public class FromSmartBodyModelBinder : IModelBinder
    {
        private const string RequestSmartBody_Key = "RequestSmartBody";

        private readonly IModelBinder _bodyBinder;
        private readonly IModelBinder _complexBinder;

        public static readonly IDictionary<string, FromSmartBodyAttribute> fromJsonBodyAttrCache = new ConcurrentDictionary<string, FromSmartBodyAttribute>();

        public FromSmartBodyModelBinder(IModelBinder bodyBinder, IModelBinder complexBinder)
        {
            _bodyBinder = bodyBinder;
            _complexBinder = complexBinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;

            string text = request.HttpContext.Items[RequestSmartBody_Key]?.ToString();
            if (text == null)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                _ = await request.Body.ReadAsync(buffer, 0, buffer.Length);
                text = Encoding.UTF8.GetString(buffer);

                request.HttpContext.Items[RequestSmartBody_Key] = text;
            }

            request.Body.Position = 0;

            string fieldName = bindingContext.FieldName;
            FromSmartBodyAttribute fromSmartBodyAttr = GetFromSmartBodyAttr(bindingContext, fieldName);
            if (!string.IsNullOrWhiteSpace(fromSmartBodyAttr.PropertyName))
                fieldName = fromSmartBodyAttr.PropertyName;

            if (bindingContext.ModelType.IsPrimitive || bindingContext.ModelType == typeof(string) || bindingContext.ModelType.IsEnum || bindingContext.ModelType == typeof(DateTime) || bindingContext.ModelType == typeof(Guid) || (bindingContext.ModelType.IsGenericType && bindingContext.ModelType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                // var paremeter = bindingContext.ModelMetadata.ParameterName;
                var value = "";
                if (request.Query.ContainsKey(fieldName))
                {
                    value = request.Query[fieldName] + "";
                }
                else if (request.ContentType != null && request.ContentType.StartsWith("application/json"))
                {
                    try
                    {
                        value = JObject.Parse(text)[fieldName] + "";
                    }
                    catch
                    {
                        value = text.Trim('"');
                    }
                }
                else if (request.HasFormContentType)
                {
                    value = request.Form[fieldName] + "";
                }

                if (value.TryConvertTo(bindingContext.ModelType, out var result))
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                }
                return;
            }

            if (request.HasFormContentType)
            {
                if (bindingContext.ModelType.IsClass)
                {
                    await DefaultBindModel(bindingContext);
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(request.Form[fieldName].ToString().ConvertTo(bindingContext.ModelType));
                }
                return;
            }

            try
            {
                bindingContext.Result = ModelBindingResult.Success(JsonConvert.DeserializeObject(text, bindingContext.ModelType) ?? request.Query[fieldName!].ToString().ConvertTo(bindingContext.ModelType));
            }
            catch
            {
                await DefaultBindModel(bindingContext);
            }
        }

        private async Task DefaultBindModel(ModelBindingContext bindingContext)
        {
            await _bodyBinder.BindModelAsync(bindingContext);

            if (bindingContext.Result.IsModelSet)
            {
                return;
            }

            bindingContext.ModelState.Clear();
            await _complexBinder.BindModelAsync(bindingContext);
        }

        private static FromSmartBodyAttribute GetFromSmartBodyAttr(ModelBindingContext bindingContext, string fieldName)
        {
            var actionDesc = bindingContext.ActionContext.ActionDescriptor;
            string actionId = actionDesc.Id;
            string cacheKey = $"{actionId}:{fieldName}";

            //fetch from cache to improve performance
            if (!fromJsonBodyAttrCache.TryGetValue(cacheKey, out FromSmartBodyAttribute fromJsonBodyAttr))
            {
                var ctrlActionDesc = bindingContext.ActionContext.ActionDescriptor as ControllerActionDescriptor;
                var fieldParameter = ctrlActionDesc.MethodInfo.GetParameters().Single(p => p.Name == fieldName);
                fromJsonBodyAttr = fieldParameter.GetCustomAttributes(typeof(FromSmartBodyAttribute), false).Single() as FromSmartBodyAttribute;
                fromJsonBodyAttrCache[cacheKey] = fromJsonBodyAttr;
            }
            return fromJsonBodyAttr;
        }
    }
}
