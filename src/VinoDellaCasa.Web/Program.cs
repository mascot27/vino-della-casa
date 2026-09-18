using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VinoDellaCasa.Application.Abstractions;
using VinoDellaCasa.Application.Services;
using VinoDellaCasa.Infrastructure.Persistence;
using VinoDellaCasa.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Browser path: IndexedDB via JS interop (see wwwroot/js/cellarStore.js + Docs/architecture.md).
// CI / unit tests register InMemoryCellarStore instead — no OAuth/auth.
builder.Services.AddScoped<ICellarStore, IndexedDbCellarStore>();
builder.Services.AddScoped<CellarService>();

await builder.Build().RunAsync();
