using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ModelBinder.Extension.AspNetCore.FromJsonBody
{
    public sealed class FromJsonBodyMiddleware
    {
        public const string RequestJsonObject_Key = "RequestJsonObject";

        private readonly RequestDelegate _next;
        private ILogger<FromJsonBodyMiddleware> _logger;

        public FromJsonBodyMiddleware(RequestDelegate next, ILogger<FromJsonBodyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            //for request that has large body, EnableBuffering will reduce performance,
            //and generally the body of "application/json" is not too large,
            //so only EnableBuffering on contenttype="application/json"
            var request = context.Request;
            if (request.ContentType == null || !request.ContentType.StartsWith("application/json") || "GET".Equals(request.Method, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
            var contentType = new ContentType(request.ContentType);
            var charset = contentType.CharSet;
            Encoding encoding;
            if (string.IsNullOrWhiteSpace(charset))
            {
                encoding = Encoding.UTF8;
            }
            else
            {
                encoding = Encoding.GetEncoding(charset);
            }

            context.Request.EnableBuffering();//Ensure the HttpRequest.Body can be read multipletimes
            int contentLen = 255;
            if (context.Request.ContentLength != null)
            {
                contentLen = (int)context.Request.ContentLength;
            }
            Stream body = context.Request.Body;
            string bodyText;
            if (contentLen <= 0)
            {
                bodyText = "";
            }
            else
            {
                using StreamReader reader = new StreamReader(body, encoding, true, contentLen, true);
                //parse json into JsonObject in advance,
                //to reduce multiple times of parseing in FromJsonBodyBinder.BindModelAsync
                bodyText = await reader.ReadToEndAsync();
            }
            //no request body
            if (string.IsNullOrWhiteSpace(bodyText))
            {
                await _next(context);
                return;
            }
            //not invalid json
            if (!(bodyText.StartsWith("{") && bodyText.EndsWith("}")))
            {
                await _next(context);
                return;
            }

            try
            {
                var document = JObject.Parse(bodyText);
                body.Position = 0;
                var jsonRoot = JsonConvert.SerializeObject(document);
                context.Items[RequestJsonObject_Key] = jsonRoot;
                await _next(context);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, "Parsing json failed:" + bodyText);
                //invalid json format
                await _next(context);
                return;
            }
        }
    }
}
