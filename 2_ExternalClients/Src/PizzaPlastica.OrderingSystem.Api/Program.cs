using Orleans.Configuration;
using PizzaPlastica.OrderingSystem.Abstractions;
using PizzaPlastica.OrderingSystem.Abstractions.Exceptions;
using PizzaPlastica.OrderingSystem.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient((hostContext, clientBuilder) =>
{
    if (builder.Environment.IsDevelopment())
    {
        // CLUSTER LOCALE
        clientBuilder.UseLocalhostClustering();
    }
    else
    {
        // CREAZIONE DEL CLUSTER PER AMBIENTI DI STAGING / PRODUZIONE
        clientBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "PizzaPlasticaCluster";
            options.ServiceId = "BestOrderingSystemEver";
        })
        .UseAdoNetClustering(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("SqlOrleans");
            options.Invariant = "System.Data.SqlClient";
        });
    }
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("restaurants/{restaurantId}/tables/{tableId}",
    async (
        IClusterClient client,
        Guid restaurantId,
        int tableId) =>
{
    try
    {
        var grainRef = client.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());
        await grainRef.OpenTableOrder();
        return Results.Ok();
    }
    catch (InvalidStateException exc)
    {
        return Results.BadRequest(exc.Message);
    }
})
.WithName("OpenTable")
.WithOpenApi();

app.MapDelete("restaurants/{restaurantId}/tables/{tableId}", async (IClusterClient client, Guid restaurantId, int tableId) =>
{
    try
    {
        var grainRef = client.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());
        await grainRef.CloseTableOrder();
        return Results.NoContent();
    }
    catch (InvalidStateException exc)
    {
        return Results.BadRequest(exc.Message);
    }
})
.WithName("CloseTable")
.WithOpenApi();

app.MapGet("restaurants/{restaurantId}/tables/{tableId}", async (IClusterClient client, Guid restaurantId, int tableId) =>
{
    var grainRef = client.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());
    var orderItems = await grainRef.GetOrderItems();
    return Results.Ok(orderItems.Select(x => x.ToResponse()).ToArray());
})
.WithName("GetTableStatus")
.WithOpenApi();

app.MapPost("restaurants/{restaurantId}/tables/{tableId}/orders", async (PostAddTableItem model, IClusterClient client, Guid restaurantId, int tableId) =>
{
    var grainRef = client.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());
    var orderItemId = await grainRef.AddOrderItem(model.Name, model.Cost, model.Quantity);
    return Results.Created($"restaurants/{restaurantId}/tables/{tableId}/orders/{orderItemId}", orderItemId);
})
.WithName("AddItemToTableOrder")
.WithOpenApi();

app.MapDelete("restaurants/{restaurantId}/tables/{tableId}/orders/{orderId}", async (IClusterClient client, Guid restaurantId, int tableId, Guid orderId) =>
{
    var grainRef = client.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());
    await grainRef.RemoveOrderItem(orderId);
    return Results.NoContent();
})
.WithName("RemoveItemToTableOrder")
.WithOpenApi();

app.MapGet("restaurants/{restaurantId}/tables/{tableId}/orders/{orderId}", async (IClusterClient client, Guid restaurantId, int tableId, Guid orderId) =>
{
    var grainRef = client.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());
    var orderItemDetails = await grainRef.GetOrderItemDetails(orderId);
    return Results.Ok(orderItemDetails);
})
.WithName("GetItemToTableOrder")
.WithOpenApi();

app.Run();


public class PostAddTableItem
{
    public string Name { get; set; }
    public double Cost { get; set; }
    public int Quantity { get; set; }
}

public class GetTableItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double Cost { get; set; }
    public int Quantity { get; set; }
}
