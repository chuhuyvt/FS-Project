using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PlcTagApi.Core.Interfaces;  // ← ADD THIS
using PlcTagApi.Infrastructure.Implementations;
using PlcTagApi.Infrastructure.Validators;

var builder = WebApplication.CreateBuilder(args);

// Register validators
builder.Services.AddSingleton<IPlcConnectionValidator, PlcConnectionValidator>();

// Register connection pool (singleton - shared across requests)
builder.Services.AddSingleton<IPlcConnectionPool, PLCConnectionPool>();

// Register tag services (transient - new instance per request)
builder.Services.AddTransient<IPLCTagReader, PlcTagReader>();
builder.Services.AddTransient<IPLCTagMonitor, PlcTagMonitor>();
builder.Services.AddTransient<IPlcTagService, PlcTagService>();  
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PLC Tag Monitoring API",
        Version = "v1",
        Description = "API để đọc và theo dõi tags từ nhiều PLC"
    });
});

var app = builder.Build();

// Thêm các service ở đây, VÍ DỤ:
app.UseCors("AllowLocalhost");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PLC API v1"));
}

// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
