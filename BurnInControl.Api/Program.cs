using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using FastEndpoints;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.AddApiSettings();
var connectionStr=builder.Configuration.GetConnectionString("DefaultConnection") ?? "mongodb://172.20.3.41:27017";
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFastEndpoints();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionStr));
builder.Services.AddApiPersistence();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseFastEndpoints();
app.Run();