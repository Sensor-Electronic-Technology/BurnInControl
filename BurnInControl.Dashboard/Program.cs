using Radzen;
using BurnInControl.Dashboard.Components;
using BurnInControl.Dashboard.Data;
using BurnInControl.Dashboard.Services;
using BurnInControl.Infrastructure.FirmwareModel;
using BurnInControl.Shared.AppSettings;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.Configure<GitHubApiOptions>(builder.Configuration.GetSection(nameof(GitHubApiOptions)));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
//var token=builder.Configuration["GithubApi:Token"];

var gitHubApiOptions = builder.Configuration.GetSection("GitHubApiOptions").Get<GitHubApiOptions>();
Console.WriteLine($"Token {gitHubApiOptions.Token}");
builder.Configuration.GetSection("GitHubApiOptions").Bind(gitHubApiOptions);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient();
builder.Services.AddTransient<FirmwareReleaseService>();
builder.Services.AddTransient<FirmwareDataService>();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();