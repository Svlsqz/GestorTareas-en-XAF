﻿using System.Configuration;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win.ApplicationBuilder;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.EFCore;
using DevExpress.EntityFrameworkCore.Security;
using DevExpress.XtraEditors;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.ExpressApp.Design;

namespace GestorTareas.Win;

public class ApplicationBuilder : IDesignTimeApplicationFactory {
    public static WinApplication BuildApplication() {
        var builder = WinApplication.CreateBuilder();
        // Register custom services for Dependency Injection. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/404430/
        // builder.Services.AddScoped<CustomService>();
        // Register 3rd-party IoC containers (like Autofac, Dryloc, etc.)
        // builder.UseServiceProviderFactory(new DryIocServiceProviderFactory());
        // builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.UseApplication<GestorTareasWindowsFormsApplication>();
        builder.Modules
            .AddConditionalAppearance()
            .AddValidation(options => {
                options.AllowValidationDetailsAccess = false;
            })
            .Add<GestorTareas.Module.GestorTareasModule>()
            .Add<GestorTareasWinModule>();
        builder.ObjectSpaceProviders
            .AddEFCore(options => options.PreFetchReferenceProperties())
                .WithDbContext<GestorTareas.Module.BusinessObjects.GestorTareasEFCoreDbContext>((application, options) => {
                    options.UseMiddleTier(application.Security);
                    options.UseChangeTrackingProxies();
                    options.UseObjectSpaceLinkProxies();
                })
            .AddNonPersistent();
        builder.Security
            .UseMiddleTierMode(options => {
#if DEBUG
                options.WaitForMiddleTierServerReady();
#endif
                options.BaseAddress = new Uri("http://localhost:5000/");
                options.Events.OnHttpClientCreated = client => client.DefaultRequestHeaders.Add("Accept", "application/json");
                options.Events.OnCustomAuthenticate = (sender, security, args) => {
                    args.Handled = true;
                    HttpResponseMessage msg = args.HttpClient.PostAsJsonAsync("api/Authentication/Authenticate", (AuthenticationStandardLogonParameters)args.LogonParameters).GetAwaiter().GetResult();
                    string token = (string)msg.Content.ReadFromJsonAsync(typeof(string)).GetAwaiter().GetResult();
                    if(msg.StatusCode == HttpStatusCode.Unauthorized) {
                        XafExceptions.Authentication.ThrowAuthenticationFailedFromResponse(token);
                    }
                    msg.EnsureSuccessStatusCode();
                    args.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
                };
            })
            .AddPasswordAuthentication();
        builder.AddBuildStep(application => {
            application.DatabaseUpdateMode = DatabaseUpdateMode.Never;
        });
        var winApplication = builder.Build();
        return winApplication;
    }

    XafApplication IDesignTimeApplicationFactory.Create() {
        DevExpress.EntityFrameworkCore.Security.MiddleTier.ClientServer.MiddleTierClientSecurity.DesignModeUserType = typeof(GestorTareas.Module.BusinessObjects.ApplicationUser);
        DevExpress.EntityFrameworkCore.Security.MiddleTier.ClientServer.MiddleTierClientSecurity.DesignModeRoleType = typeof(DevExpress.Persistent.BaseImpl.EF.PermissionPolicy.PermissionPolicyRole);
        return BuildApplication();
    }
}
