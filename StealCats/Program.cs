using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StealTheCats.Common;
using StealTheCats.Entities;
using StealTheCats.Entities.Models;
using StealTheCats.Interfaces;
using StealTheCats.Repositories;
using StealTheCats.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextPool<ApplicationDBContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("connectionString");
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICatService, CatService>();
builder.Services.Configure<CatApiOptions>(builder.Configuration.GetSection("CatApi"));
builder.Services.AddHttpClient<ICatApiService, CatApiService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<CatApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddSingleton<CacheInvalidationToken>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program));

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
