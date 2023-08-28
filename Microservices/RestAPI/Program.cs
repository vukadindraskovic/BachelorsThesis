using InfluxDB.Client;

// InfluxDB
var influxdbAddress = "http://influxdb:8086";
var influxdbToken = "jeDCcUAE5JDCVRpAKs97o00XXQikkuOxmITZ9XrOz2soYKBD_wOZ6IfRZidjm78QGdC6LJfaTKBFcNvA7pmsrw==".ToCharArray();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var influxDbClient = InfluxDBClientFactory.Create(influxdbAddress, influxdbToken);
builder.Services.AddSingleton(x => influxDbClient);

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseCors("CorsPolicy");

app.MapControllers();

app.Run();
