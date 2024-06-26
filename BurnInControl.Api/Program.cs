using BurnInControl.Infrastructure;
using BurnInControl.Shared;
using FastEndpoints;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var connectionStr=builder.Configuration.GetConnectionString("DefaultConnection") ?? "mongodb://172.20.3.41:27017";
builder.Services.AddSettings(builder);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFastEndpoints(o=>o.IncludeAbstractValidators = true);
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionStr));
builder.Services.AddApiPersistence();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseFastEndpoints();
app.UseHttpsRedirection();
app.Run();

