using PizzaPlastica.OrderingSystem.Abstractions;
using PizzaPlastica.OrderingSystem.Tests.Fixtures;

namespace PizzaPlastica.OrderingSystem.Tests;
public class TableOrderGrainTests
{
    private ClusterFixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new ClusterFixture();
    }

    [Test]
    public async Task Open_a_table_order_and_add_an_item()
    {
        // ARRANGE
        var restaurantId = Guid.NewGuid();
        var tableId = 3;
        var tableOrderGrain = fixture.Cluster.GrainFactory.GetGrain<ITableOrderGrain>(restaurantId, tableId.ToString());

        // ACT 
        await tableOrderGrain.OpenTableOrder();
        var orderItemId = await tableOrderGrain.AddOrderItem("Pizza Hawaii", 6.5, 1);
        var orderItems = await tableOrderGrain.GetOrderItems();

        // ASSERT
        Assert.That(orderItems.Count, Is.EqualTo(1));
        Assert.That(orderItems[0].Id, Is.EqualTo(orderItemId));
        Assert.That(orderItems[0].Name, Is.EqualTo("Pizza Hawaii"));
        Assert.That(orderItems[0].Cost, Is.EqualTo(6.5));
        Assert.That(orderItems[0].Quantity, Is.EqualTo(1));
    }
}
