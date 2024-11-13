using System.Web;
using BlueSkyEmbed.Web;
using Microsoft.AspNetCore.HttpOverrides;
using RestSharp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/bluesky/sample", () => Results.Content(File.ReadAllText("sample.html"), "text/html"));

app.MapGet("/bluesky", async (string url) =>
{
    var client = new RestClient(new RestClientOptions("https://embed.bsky.app/oembed"));
    var request = new RestRequest("");
    request.AddQueryParameter("url", url);

    var response = await client.ExecuteAsync<Response>(request);

    if (!response.IsSuccessful || response.Data == null || string.IsNullOrEmpty(response.Data.Html))
        return Results.NotFound();
    
    return Results.Content(HttpUtility.HtmlDecode(response.Data.Html), "text/html");
});

app.Run();