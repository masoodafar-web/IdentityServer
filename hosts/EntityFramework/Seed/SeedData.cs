using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using IdentityServerHost.Data;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IdentityServerHost.Models;

namespace IdentityServerHost;

public class SeedData
{
    public static void EnsureSeedData(IServiceProvider serviceProvider)
    {
        try
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // using (var context = scope.ServiceProvider.GetService<PersistedGrantDbContext>())
                // {
                //     context.Database.Migrate();
                // }
                //
                // using (var context = scope.ServiceProvider.GetService<ConfigurationDbContext>())
                // {
                //     context.Database.Migrate();
                //     EnsureConfigurationSeedData(context);
                // }

                using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                    EnsureUserSeedData(context, userMgr,roleMgr);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }


    private static void EnsureConfigurationSeedData(ConfigurationDbContext context)
    {
        Console.WriteLine("Seeding database...");
        List<Client> clients = new();
        List<IdentityResource> identityResources = new();
        List<ApiResource> apiResources = new();
        List<ApiScope> apiScopes = new();
        List<IdentityProvider> identityProviders = new();
        using (StreamReader r = new StreamReader("data.json"))
        {
            string json = r.ReadToEnd();
            var x = JsonSerializer.Deserialize<dynamic>(json);
            var rd=x["Clients"] as List<Client>;
            // clients = JsonSerializer.Deserialize<List<Client>>(json);
            identityResources = JsonSerializer.Deserialize<List<IdentityResource>>(json);
            apiResources = JsonSerializer.Deserialize<List<ApiResource>>(json);
            apiScopes = JsonSerializer.Deserialize<List<ApiScope>>(json);
            identityProviders = JsonSerializer.Deserialize<List<IdentityProvider>>(json);
        }

        foreach (var client in clients)
        {
            if (!context.Clients.Any(a => a.ClientId == client.ClientId))
            {
                Console.WriteLine($"{client.ClientId} being populated");
                context.Clients.Add(client);

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine($"{client.ClientId} already populated");
            }
        }

        foreach (var identityResource in identityResources)
        {
            if (!context.IdentityResources.Any(a => a.Name == identityResource.Name))
            {
                Console.WriteLine($"{identityResource.Name} being populated");
                context.IdentityResources.Add(identityResource);

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine($"{identityResource.Name} already populated");
            }
        }

        foreach (var apiResource in apiResources)
        {
            if (!context.ApiResources.Any(a => a.Name == apiResource.Name))
            {
                Console.WriteLine($"{apiResource.Name} being populated");
                context.ApiResources.Add(apiResource);

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine($"{apiResource.Name} already populated");
            }
        }

        foreach (var apiScope in apiScopes)
        {
            if (!context.ApiScopes.Any(a => a.Name == apiScope.Name))
            {
                Console.WriteLine($"{apiScope.Name} being populated");
                context.ApiScopes.Add(apiScope);

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine($"{apiScope.Name} already populated");
            }
        }

        // foreach (var identityProvider in identityProviders)
        // {
        //     if (!context.IdentityProviders.Any(a=>a.Scheme==identityProvider.Scheme))
        //     {
        //         Console.WriteLine($"{identityProvider.Scheme} being populated");
        //         context.IdentityProviders.Add(identityProvider);
        //         // context.IdentityProviders.Add(new OidcProvider
        //         // {
        //         //     Scheme = "demoidsrv",
        //         //     DisplayName = "IdentityServer",
        //         //     Authority = "https://demo.duendesoftware.com",
        //         //     ClientId = "login",
        //         // }.ToEntity());
        //         // context.IdentityProviders.Add(new OidcProvider
        //         // {
        //         //     Scheme = "google",
        //         //     DisplayName = "Google",
        //         //     Authority = "https://accounts.google.com",
        //         //     ClientId = "998042782978-gkes3j509qj26omrh6orvrnu0klpflh6.apps.googleusercontent.com",
        //         //     Scope = "openid profile email",
        //         //     Properties =
        //         //     {
        //         //         { "foo", "bar" }
        //         //     }
        //         // }.ToEntity());
        //         context.SaveChanges();
        //     }
        //     else
        //     {
        //         Console.WriteLine($"{identityProvider.Scheme} already populated");
        //     } 
        // }        


        Console.WriteLine("Done seeding database.");
        Console.WriteLine();
    }


    private static void EnsureUserSeedData(ApplicationDbContext context, UserManager<ApplicationUser> userMgr,
        RoleManager<ApplicationRole> roleMgr)
    {
        List<ApplicationUser> applicationUsers = new();
        List<ApplicationRole> applicationRoles = new();
        using (StreamReader r = new StreamReader("data.json"))
        {
            string json = r.ReadToEnd();
           
            applicationUsers = JsonSerializer.Deserialize<List<ApplicationUser>>(json);
            applicationRoles = JsonSerializer.Deserialize<List<ApplicationRole>>(json);
        }

        foreach (var identityRole in applicationRoles)
        {
            var ExistRole = roleMgr.FindByNameAsync(identityRole.Name).Result;
            if (ExistRole == null)
            {
                var newRole = new ApplicationRole
                {
                    Name = identityRole.Name,
                    Id = identityRole.Id
                };
                var result = roleMgr.CreateAsync(newRole).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = roleMgr.AddClaimAsync(newRole, new Claim(JwtClaimTypes.Role, newRole.Name)).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                Console.WriteLine($"{newRole.Name} created");
            }
            else
            {
                Console.WriteLine($"{identityRole.Name} already exists");
            }
        }

        foreach (var user in applicationUsers)
        {
            var ExistUser = userMgr.FindByNameAsync(user.UserName).Result;
            if (ExistUser == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = user.UserName
                };
                if (user.UserRoles.Any())
                {
                    newUser.UserRoles = user.UserRoles;
                }

                var result = userMgr.CreateAsync(newUser, user.PasswordHash).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, newUser.FirstName + " " + newUser.LastName),
                    new Claim(JwtClaimTypes.GivenName, newUser.UserName),
                    new Claim(JwtClaimTypes.FamilyName, newUser.LastName),
                    new Claim(JwtClaimTypes.PhoneNumber, newUser.PhoneNumber),
                    // new Claim(JwtClaimTypes.Email, newUser.Email),
                    // new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    // new Claim(JwtClaimTypes.WebSite, "http://admin.com"),
                };
                if (user.UserRoles.Any())
                {
                    foreach (var userRole in user.UserRoles)
                    {
                        claims.Add(new Claim(JwtClaimTypes.Role, userRole.Role.Name));
                    }
                }

                result = userMgr.AddClaimsAsync(newUser, claims.ToArray()).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                Console.WriteLine($"{newUser.UserName} created");
            }
            else
            {
                Console.WriteLine($"{user.UserName} already exists");
            }
        }
    }
}