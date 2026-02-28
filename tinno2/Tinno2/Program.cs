using System.Net.Http.Headers;
using Scalar.AspNetCore;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:4173",
                "https://localhost:4173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var receitaWsBaseUrl = builder.Configuration.GetValue<string>("ReceitaWS:BaseUrl")!;
var receitaWsToken = builder.Configuration.GetValue<string>("ReceitaWS:Token")!;

builder.Services.AddHttpClient<IReceitaFederalService, ReceitaFederalService>(client =>
{
    client.BaseAddress = new Uri(receitaWsBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", receitaWsToken);
});

builder.Services.AddHttpClient<ISimplesNacionalService, SimplesNacionalService>(client =>
{
    client.BaseAddress = new Uri(receitaWsBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", receitaWsToken);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
