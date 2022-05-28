using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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

            if (bindingContext.ModelType.IsArray)
            {
                //获取传入的值
                var valueProviderResult = bindingContext.ValueProvider.GetValue(fieldName);
                // ?dadIds=[1,2,3]
                var value = valueProviderResult.FirstValue;

                //获取数组的类型
                string typeName = bindingContext.ModelType.FullName.Replace("[]", string.Empty);
                Type memberType = bindingContext.ModelType.Assembly.GetType(typeName);

                // long, int, short, float, double, char, byte, bool
                if (memberType.IsPrimitive || memberType == typeof(string) || memberType == typeof(decimal) || memberType == typeof(object) || memberType == typeof(DateTime) || memberType == typeof(Guid))
                {
                    //  ?dadIds=1&dadIds=2&dadIds=3
                    if (!value.StartsWith("[") && !value.EndsWith("]"))
                    {
                        value = $"[{string.Join(",", valueProviderResult.Values)}]";
                    }

                    switch (typeName)
                    {
                        case "System.Boolean":
                            var @bool = value.Trim('[', ']').Split(',').Select(str => Convert.ToBoolean(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@bool);
                            return;
                        case "System.Byte":
                            var base64 = Convert.FromBase64String(value);
                            bindingContext.Result = ModelBindingResult.Success(base64);
                            return;
                        case "System.Char":
                            var @char = value.Trim('[', ']').Split(',').Select(str => Convert.ToChar(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@char);
                            return;
                        case "System.DateTime":
                            var @datetime = value.Trim('[', ']').Split(',').Select(str => Convert.ToDateTime(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@datetime);
                            return;
                        case "System.Decimal":
                            var @decimal = value.Trim('[', ']').Split(',').Select(str => Convert.ToDecimal(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@decimal);
                            return;
                        case "System.Double":
                            var @double = value.Trim('[', ']').Split(',').Select(str => Convert.ToDouble(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@double);
                            return;
                        case "System.Int16":
                            var @short = value.Trim('[', ']').Split(',').Select(str => Convert.ToInt16(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@short);
                            return;
                        case "System.Int32":
                            var @int = value.Trim('[', ']').Split(',').Select(str => Convert.ToInt32(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@int);
                            return;
                        case "System.Int64":
                            var @long = value.Trim('[', ']').Split(',').Select(str => Convert.ToInt64(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@long);
                            return;
                        case "System.SByte":
                            var @sbyte = value.Trim('[', ']').Split(',').Select(str => Convert.ToSByte(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@sbyte);
                            return;
                        case "System.Single":
                            var @float = value.Trim('[', ']').Split(',').Select(str => Convert.ToSingle(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@float);
                            return;
                        case "System.String":
                            var @string = value.Trim('[', ']').Split(',').Select(str => Convert.ToString(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@string);
                            return;
                        case "System.UInt16":
                            var @ushort = value.Trim('[', ']').Split(',').Select(str => Convert.ToUInt16(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@ushort);
                            return;
                        case "System.UInt32":
                            var @uint = value.Trim('[', ']').Split(',').Select(str => Convert.ToUInt32(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@uint);
                            return;
                        case "System.UInt64":
                            var @ulong = value.Trim('[', ']').Split(',').Select(str => Convert.ToUInt64(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@ulong);
                            return;
                        case "System.Guid":
                            var @guid = value.Trim('[', ']').Split(',').Select(str => Guid.Parse(str)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@guid);
                            return;
                        default:
                            var @object = value.Trim('[', ']').Split(',').Select(str => Convert.ChangeType(str, memberType)).ToArray();
                            bindingContext.Result = ModelBindingResult.Success(@object);
                            return;
                    }
                }
            }

            if (bindingContext.ModelType.IsGenericType)
            {
                //获取传入的值
                var valueProviderResult = bindingContext.ValueProvider.GetValue(fieldName);
                // ?dadIds=[1,2,3]
                var value = valueProviderResult.FirstValue;

                //获取数组的类型
                string typeName = bindingContext.ModelType.FullName.Replace($"{bindingContext.ModelType.Namespace}.{bindingContext.ModelType.Name}[[", string.Empty).Replace($", {bindingContext.ModelType.Assembly.FullName}]]", string.Empty);
                Type memberType = bindingContext.ModelType.Assembly.GetType(typeName);

                // long, int, short, float, double, char, byte, bool
                if (memberType.IsPrimitive || memberType == typeof(string) || memberType == typeof(decimal) || memberType == typeof(object) || memberType == typeof(DateTime) || memberType == typeof(Guid))
                {
                    //  ?dadIds=1&dadIds=2&dadIds=3
                    if (!value.StartsWith("[") && !value.EndsWith("]"))
                    {
                        value = $"[{string.Join(",", valueProviderResult.Values)}]";
                    }

                    switch (typeName)
                    {
                        case "System.Boolean":
                            var @bool = value.Trim('[', ']').Split(',').Select(str => Convert.ToBoolean(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@bool);
                            return;
                        case "System.Byte":
                            var base64 = Convert.FromBase64String(value);
                            bindingContext.Result = ModelBindingResult.Success(base64);
                            return;
                        case "System.Char":
                            var @char = value.Trim('[', ']').Split(',').Select(str => Convert.ToChar(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@char);
                            return;
                        case "System.DateTime":
                            var @datetime = value.Trim('[', ']').Split(',').Select(str => Convert.ToDateTime(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@datetime);
                            return;
                        case "System.Decimal":
                            var @decimal = value.Trim('[', ']').Split(',').Select(str => Convert.ToDecimal(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@decimal);
                            return;
                        case "System.Double":
                            var @double = value.Trim('[', ']').Split(',').Select(str => Convert.ToDouble(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@double);
                            return;
                        case "System.Int16":
                            var @short = value.Trim('[', ']').Split(',').Select(str => Convert.ToInt16(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@short);
                            return;
                        case "System.Int32":
                            var @int = value.Trim('[', ']').Split(',').Select(str => Convert.ToInt32(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@int);
                            return;
                        case "System.Int64":
                            var @long = value.Trim('[', ']').Split(',').Select(str => Convert.ToInt64(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@long);
                            return;
                        case "System.SByte":
                            var @sbyte = value.Trim('[', ']').Split(',').Select(str => Convert.ToSByte(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@sbyte);
                            return;
                        case "System.Single":
                            var @float = value.Trim('[', ']').Split(',').Select(str => Convert.ToSingle(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@float);
                            return;
                        case "System.String":
                            var @string = value.Trim('[', ']').Split(',').Select(str => Convert.ToString(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@string);
                            return;
                        case "System.UInt16":
                            var @ushort = value.Trim('[', ']').Split(',').Select(str => Convert.ToUInt16(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@ushort);
                            return;
                        case "System.UInt32":
                            var @uint = value.Trim('[', ']').Split(',').Select(str => Convert.ToUInt32(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@uint);
                            return;
                        case "System.UInt64":
                            var @ulong = value.Trim('[', ']').Split(',').Select(str => Convert.ToUInt64(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@ulong);
                            return;
                        case "System.Guid":
                            var @guid = value.Trim('[', ']').Split(',').Select(str => Guid.Parse(str)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@guid);
                            return;
                        default:
                            var @object = value.Trim('[', ']').Split(',').Select(str => Convert.ChangeType(str, memberType)).ToList();
                            bindingContext.Result = ModelBindingResult.Success(@object);
                            return;
                    }
                }
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
