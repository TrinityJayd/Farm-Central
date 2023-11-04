using FirebaseAdmin;
using Microsoft.EntityFrameworkCore;
using PROG7311_P2.Models;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<Progp2Context>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));

//create deafualt firebase instance
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("prog7311-f7f97-firebase-adminsdk-k5k9q-ecac9ca3ef.json")
});

builder.Services.AddSingleton(FirebaseApp.DefaultInstance);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
