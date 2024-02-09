using Bucketlist.APIEndpoints;
using Bucketlist.Database.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
// add auth scheme
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => 
    {
        builder.Configuration.Bind("EntraId", options);
        options.Events = new JwtBearerEvents();
    },
    options=>{
        builder.Configuration.Bind("EntraId", options);
    });

// swagger options & enable in UI auth. Dang but this is ugly looking. // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Bucketlist API", Version = "v1" });
    
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// allow endpoints access to the context 
builder.Services.AddDbContext<BucketlistContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Bucketlist"))
);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.RegisterBucketlist();
app.UseAuthentication();
// slacker redirect to swagger
app.MapGet("/", () => Results.Redirect("/swagger")).WithOpenApi();

app.Run();