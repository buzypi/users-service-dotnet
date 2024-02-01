using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Startup
{
    private readonly IMongoCollection<BsonDocument> _userCollection;

    public Startup()
    {
        var client = new MongoClient(Environment.GetEnvironmentVariable("DB_HOST"));
        var database = client.GetDatabase("usersdb");
        _userCollection = database.GetCollection<BsonDocument>("user");

        // Demo purpose: Initialize with two users if empty
        if (_userCollection.CountDocuments(new BsonDocument()) == 0)
        {
            _userCollection.InsertOne(new BsonDocument { { "userid", 1 }, { "name", "John" } });
            _userCollection.InsertOne(new BsonDocument { { "userid", 2 }, { "name", "George" } });
        }
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/ping", async context =>
            {
                await context.Response.WriteAsync("pong");
            });

            endpoints.MapGet("/users", async context =>
            {
                var usersList = _userCollection.Find(new BsonDocument()).ToList();
                await context.Response.WriteAsJsonAsync(usersList?.ConvertAll(BsonTypeMapper.MapToDotNetValue));
            });

            endpoints.MapGet("/user/{userid:int}", async context =>
            {
                // Extract the userid from route values
                var useridStr = context.Request.RouteValues["userid"]?.ToString();

                // Safely try to parse the userid string to an integer
                if (int.TryParse(useridStr, out int userid))
                {
                    var user = _userCollection.Find(Builders<BsonDocument>.Filter.Eq("userid", userid)).FirstOrDefault();
                    var userMapped = BsonTypeMapper.MapToDotNetValue(user);
                    if (userMapped != null)
                    {
                        await context.Response.WriteAsJsonAsync(userMapped);
                    }
                    else
                    {
                        // Handle the case where the user is not found
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsync("User not found");
                    }
                }
                else
                {
                    // Handle the case where userid is not a valid integer
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid user ID");
                }
            });
        });
    }
}

public class Program
{
    public static Task Main(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .Build().RunAsync();
}

