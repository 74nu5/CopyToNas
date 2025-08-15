// <copyright file="Program.cs" company="CopyToNas">
// Copyright (c) CopyToNas. Tous droits réservés.
// </copyright>

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SftpCopyTool;
using SftpCopyTool.Web.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/web-sftp-copy-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHealthChecks();

// Enregistrement des services métier
builder.Services.AddScoped<SftpService>();
builder.Services.AddScoped<SftpExecutionService>();
builder.Services.AddScoped<IFileExplorerService, FileExplorerService>();
builder.Services.AddSingleton<IProgressReporter, ProgressReporter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Endpoint de health check
app.MapHealthChecks("/health");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
