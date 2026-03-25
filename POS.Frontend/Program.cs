using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using POS.Frontend;

using POS.Frontend.Services.Products;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var backendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? "https://localhost:60763";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(backendUrl) });

builder.Services.AddScoped<IProductService, ProductService>();

await builder.Build().RunAsync();
