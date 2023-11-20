using Microsoft.AspNetCore.Identity;
using Stilbaai_Tourism_Web_Portal.Context;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Microsoft.IdentityModel.Logging;
using Stilbaai_Tourism_Web_Portal.Support;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//To use MVC we have to explicitly declare we are using it. Doing so will prevent a System.InvalidOperationException.
builder.Services.AddAuth0WebAppAuthentication(options =>
{
   options.Domain = builder.Configuration["Auth0:Domain"];
   options.ClientId = builder.Configuration["Auth0:ClientId"];
});

// Configure the HTTP request pipeline.
builder.Services.ConfigureSameSiteNoneCookies();

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
app.UseCookiePolicy();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();