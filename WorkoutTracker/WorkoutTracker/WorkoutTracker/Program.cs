using Microsoft.AspNetCore.Components.Authorization;
using WorkoutTracker.Components;
using WorkoutTracker.Components.Authentication;
using WorkoutTracker.Components.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add login services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<SimpleAuthenticationStateProvider>(); // Register SimpleAuthenticationStateProvider

// Register the UserService (for getting info from Users DB Table( with the correct connection string from appsettings.json
builder.Services.AddSingleton<UserService>(sp =>
    new UserService(builder.Configuration.GetConnectionString("ConnectionWorkoutTrackerDB"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();