using Microsoft.EntityFrameworkCore;
using POS.Core.Features.Category;
using POS.Core.Features.Products;
using POS.Core.Features.Merchants;
using POS.Core.Features.Branch;
using POS.data.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));  
// Add services to the container.
builder.Services.AddScoped<IProductsServices, ProductsServices>();
builder.Services.AddScoped<ICategoryServices, CategoryService>();
builder.Services.AddScoped<IMerchantsServices, MerchantsServices>();
builder.Services.AddScoped<IBranchServices, BranchServices>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
