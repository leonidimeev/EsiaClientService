using EsiaClientService.Infrastructure;
using EsiaClientService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EsiaOptions>(builder.Configuration.GetSection("Esia"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

builder.Services.AddHttpClient();
builder.Services.Configure<CryptoServiceOptions>(builder.Configuration.GetSection("CryptoServiceOptions"));

builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IEsiaService, EsiaService>();

app.Run();
