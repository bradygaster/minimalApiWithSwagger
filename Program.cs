using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

const string OpenApiDocumentName = "v1";
const string MemcacheKey = "items";
const string Greeting = "Hello {0}!";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc(OpenApiDocumentName, new OpenApiInfo
{
    Title = "Minimal Swagger Test",
    Description = "Simple description",
    Version = "v1"
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger(setup => setup.SerializeAsV2 = true);

app.Services.GetService<IMemoryCache>().Set<List<TheApiModel>>(MemcacheKey, new List<TheApiModel>());

app.MapGet("/", ([FromServices] IMemoryCache memcache) =>
    memcache.Get<List<TheApiModel>>(MemcacheKey)
);

app.MapGet("/{target}", (string target, [FromServices] IMemoryCache memcache) =>
    memcache.Get<List<TheApiModel>>(MemcacheKey).FirstOrDefault(_ => _.Target == target)
);

app.MapPost("/create", (TheApiModel model, [FromServices] IMemoryCache memcache) =>
{
    var existing = memcache.Get<List<TheApiModel>>(MemcacheKey);
    existing.Add(model);
    memcache.Set<List<TheApiModel>>(MemcacheKey, existing);
    return new CreatedResult("/", string.Format(Greeting, model.Target));
});

app.MapGet("/version", () => typeof(Microsoft.AspNetCore.Http.RequestDelegateFactory)
    .Assembly.GetCustomAttributes(true)
        .OfType<System.Reflection.AssemblyInformationalVersionAttribute>().First().InformationalVersion
    );

app.UseSwaggerUI();

app.Run();

public record TheApiModel(string Target = "World");