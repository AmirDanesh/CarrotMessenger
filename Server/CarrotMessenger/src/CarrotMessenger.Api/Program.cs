using CarrotMessenger.Api;
using CarrotMessenger.Application;
using CarrotMessenger.Application.Contacts.Queries.SearchContacts;
using CarrotMessenger.Identity;
using CarrotMessenger.Infrastructure.Common.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, true)
    .AddJsonFile($"appsettings.{environment}.json", true, true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddOptions();
builder.Services.AddEndpointsApiExplorer();
builder.AddApplicationDbConnection(typeof(Program).Assembly.FullName!);
builder.AddApplicationIdentity();

builder.Services.AddApplicationServices();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddHostedService<ListenerBackgroundService>();
builder.Services.AddMediatR(options => { options.RegisterServicesFromAssembly(typeof(SearchContactsQuery).Assembly); });

var app = builder.Build();
app.MapRoutes();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseApplicationIdentity();


app.Run();