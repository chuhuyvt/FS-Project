using PlcTagApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddScoped<IPlcTagService, PlcTagService>();
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

var app = builder.Build();

// Thêm các service ở đây, VÍ DỤ:

app.UseCors("AllowLocalhost");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
