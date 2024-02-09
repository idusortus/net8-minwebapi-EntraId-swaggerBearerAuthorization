# EfCore
- add packages (Storage Type + .Tools)  EX:
  - Microsoft.EntityFrameworkCore.Sqlite
  - Microsoft.EntityFrameworkCore.Tools
- add connection string
  - ex: "Context": "Data Source = ./Database/MyDatabase.db"
- add services.Dbcontext
- add models/entities
- add dbcontext
- @dotnet cli: dotnet ef migrations add InitialCreate
- @dotnet cli: dotnet ef database update

# .Net8 Minimal WebApi
- add endpoints
```csharp
// XXXXEndpoints.cs
public static class XXXX {
    public static void RegisterXXXX(this WebApplication app){
        var groupName = app.MapGroup("/api/v1/GROUP");

        groupName.MapPost( ... );
        //
        // ... 
        //
    }    
}
// in program.cs
app.RegisterXXXX();
```

- Slacker root redirect:
```csharp
app.MapGet("/", () => Results.Redirect("/swagger"));
```

# Add Entra Authentication
- [API-M Reference](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-protect-backend-with-aad)


- [First Approach](https://learn.microsoft.com/en-us/entra/external-id/customers/tutorial-protect-web-api-dotnet-core-build-app)

- [Second Approach](https://learn.microsoft.com/en-us/entra/external-id/customers/tutorial-protect-web-api-dotnet-core-build-app-2)

## Well, nothing explicit so here goes

- + Microsoft.Identity.Web
- Modify appsettings.json with a registered [Entra application](https://entra.microsoft.com/#view/)
  - Most of this is available from the above link > 'Overview'
  - Scopes are available from the application registration > 'Expose an API'
```json
{
  "EntraId": {
    "Instance": "https://Enter_the_Tenant_Subdomain_Here.ciamlogin.com/", 
    "TenantId": "Enter_the_Tenant_Id_Here",
    "ClientId": "Enter_the_Application_Id_Here",

    "Scopes": {
      "Read": ["ToDoList.Read", "ToDoList.ReadWrite"],
      "Write": ["ToDoList.ReadWrite"]
    },

    "AppPermissions": {
      "Read": ["ToDoList.Read.All", "ToDoList.ReadWrite.All"],
      "Write": ["ToDoList.ReadWrite.All"]
    }
  },
  "Logging": {...},
  "AllowedHosts": "*"
}
```
- add Authentication Scheme
```csharp
// program.cs
...
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId"));
    // .EnableTokenAcquisitionToCallDownstreamApi()
    // .AddInMemoryTokenCaches();
...
app.UseAuthentication();
```
- Get a token
```bash
https://login.microsoftonline.com/<tenant-id>/oauth2/v2.0/authorize?client_id=<client-id>
&response_type=code
&redirect_uri=https://jwt.ms/
&response_mode=query
&scope=2ff814a6-3304-4ab8-85cb-cd0e6f879c1d%2F.default
&state=<state>
```





- Someone elses sample (for b2c)
```bash
https://login.microsoftonline.com/TENANTID/oauth2/v2.0/authorize?client_id=CLIENTID&response_type=id_token&redirect_uri=https%3A%2F%2Fjwt.ms&scope=openid%20profile%20email&response_mode=fragment&state=12345&nonce=678910
```


## Shifting to Adding Swagger support for Bearer Token
- [Old But Possibly Relevant](https://github.com/dotnet/AspNetCore.Docs/blob/main/aspnetcore/security/authentication/identity-api-authorization.md)
- [Swagger Auth Cheat](https://www.josephguadagno.net/2022/06/03/enabling-user-authentication-in-swagger-using-microsoft-identity)

- Populate SwaggerDoc, AddSecuirityDefinition, AddSecurityRequirement

- To see the token in the console:
```csharp
var jwt = HttpContext.Request.Headers["Authorization"].ToString().Split(" ")[1]; // Assuming "Bearer " prefix
Console.WriteLine(jwt);
```

- So... The token in this instance doesn't authenticate, but the application requires a valid JWT, which requires an authenticated user to obtain.
- Applying .RequireAuthentication() causes swagger to take a shit.


### Folow-Up Questions
- Whats the difference between these two method chains (multiple statements combined together in a specific sequence)?

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => 
    {
        builder.Configuration.Bind("EntraId", options);
        options.Events = new JwtBearerEvents();
        // {
        //     OnTokenValidated = context =>
        //     {
        //         var token = context.SecurityToken;
        //         Console.WriteLine(token);
        //         return Task.CompletedTask;
        //     }
        // };
    },
    options=>{
        builder.Configuration.Bind("EntraId", options);
    });

...

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId")); 
```  

> Top one works, bottom one does not.