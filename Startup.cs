using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace RequestInfo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async context =>
            {
                var requestString = context.Request.GetDetails();
                Console.WriteLine(requestString);

                context.Response.ContentType = "application/json";

                byte[] data = System.Text.Encoding.UTF8.GetBytes(requestString);
                await context.Response.Body.WriteAsync(data, 0, data.Length);
            });
        }
    }

    public static class HttpRequestExtensions
    {
        public static string GetDetails(this HttpRequest request)
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString.Value}";
            var headers = new StringBuilder();
            foreach (var header in request.Headers)
            {
                headers.AppendLine($"{header.Key}: {header.Value}");
            }

            var body = "";
            if (request.Body.CanSeek)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(request.Body))
                {
                    body = sr.ReadToEnd();
                }
            }

            return JsonSerializer.Serialize(new { request.Protocol, request.Method, BaseUrl = baseUrl, Headers = headers, Body = body });
        }
    }
}
