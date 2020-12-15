using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace layer7_http_proxy
{
    public class ReverseProxyMiddleware
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;
        public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext context)
        {
            using (var newRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), "https://www.daum.net"))// GetAbsoluteUri(context)))
            {
                if (context.Request.ContentLength > 0)
                    newRequest.Content = new StreamContent(context.Request.Body); 
                using (var responseMessage = await _httpClient.SendAsync(newRequest))
                {
                    var content = await responseMessage.Content.ReadAsByteArrayAsync();
                    await context.Response.Body.WriteAsync(content);
                }
            }

            await _nextMiddleware(context);
        }

        private static Uri GetAbsoluteUri(HttpContext context)
        {
            var request = context.Request;
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = request.Scheme;
            uriBuilder.Host = request.Host.Host;
            uriBuilder.Path = request.Path.ToString();
            uriBuilder.Query = request.QueryString.ToString();
            return uriBuilder.Uri;
        }
    }
}
