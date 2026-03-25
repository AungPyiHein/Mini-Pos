using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using POS.Frontend;

using POS.Frontend.Services.Products;
using POS.Frontend.Services.Categories;
using POS.Frontend.Services.Sales;

using POS.Frontend.Services.Merchants;
using POS.Frontend.Services.Inventory;
using POS.Frontend.Services.Users;
using POS.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var backendUrl = builder.Configuration.GetValue<string>("BackendUrl") ?? "https://localhost:60763";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(backendUrl) });

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ThemeService>();

await builder.Build().RunAsync();
