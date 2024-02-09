using Bucketlist.Database.Context;
using Bucketlist.Database.Entities;
using Bucketlist.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using Microsoft.IdentityModel.Tokens;

namespace Bucketlist.APIEndpoints;

public static class BucketlistEndpoints
{    
    public static void RegisterBucketlist(this WebApplication app)
    {
        var blist = app.MapGroup("/api/v1/bucketlist")
            .WithTags("Bucketlist")
            // .RequireAuthorization(); <-- Try enabling this and see what happens ;)
            ;

        blist.MapGet("/items", async (HttpContext httpContext, BucketlistContext context) =>
        {
            // sloppy dump of the jwt token
            if (!httpContext.Request.Headers["Authorization"].IsNullOrEmpty() )
            {
                var jwt = httpContext.Request.Headers["Authorization"].ToString().Split(" ")[1]; // Assuming "Bearer " prefix
                Console.WriteLine(jwt);
            }

            return await context.BucketlistItems.ToListAsync();
        });

        blist.MapGet("/itemsAuth",
        [RequiredScopeOrAppPermission(
            RequiredScopesConfigurationKey = "AzureAD:Scopes:Write",
            RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Write"
        )]
        async (HttpContext httpContext, BucketlistContext context) =>
        {
            // sloppy, repetitive, dump of the jwt token
            if (!httpContext.Request.Headers["Authorization"].IsNullOrEmpty())
            {
                var jwt = httpContext.Request.Headers["Authorization"].ToString().Split(" ")[1]; // Assuming "Bearer " prefix
                Console.WriteLine(jwt);
            }
            return await context.BucketlistItems.ToListAsync();
        });

        blist.MapGet("/items/{id}", async (BucketlistContext context, Guid id) =>
        {
            return await context.BucketlistItems.FindAsync(id);
        });

        blist.MapPost("/items", async (BucketlistContext context, BucketListItemRequest item) =>
        {
            var newItem = new BucketListItem
            {
                Id = Guid.NewGuid(),
                Title = item.Title,
                IsComplete = false
            };
            context.BucketlistItems.Add(newItem);
            await context.SaveChangesAsync();
            return Results.Created($"/api/v1/bucketlist/items/{newItem.Id}", item);
        });

        blist.MapPut("/items/{id}", async (BucketlistContext context, Guid id, BucketListItem item) =>
        {
            if (id != item.Id)
            {
                return Results.BadRequest();
            }

            // context.Entry(item).State = EntityState.Modified; // Not needed but something to consider if you haven't seen it before

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await context.BucketlistItems.AnyAsync(e => e.Id == id))
                {
                    return Results.NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Results.NoContent();
        });

        blist.MapDelete("/items/{id}", async (BucketlistContext context, Guid id) =>
        {
            var item = await context.BucketlistItems.FindAsync(id);
            if (item == null)
            {
                return Results.NotFound();
            }

            context.BucketlistItems.Remove(item);
            await context.SaveChangesAsync();

            return Results.NoContent();
        });

    }
}