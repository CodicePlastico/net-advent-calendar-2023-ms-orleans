using Orleans.Runtime;
using PizzaPlastica.OrderingSystem.Abstractions;
using PizzaPlastica.OrderingSystem.Exceptions;
using System.Collections.ObjectModel;

namespace PizzaPlastica.OrderingSystem.Grains;

[GenerateSerializer]
public class TableOrderState
{
    [Id(0)]
    public bool IsOpen { get; set; }
    [Id(1)]
    public List<TableOrderItem> OrderItems { get; set; }
}

public class TableOrderGrain : Grain, ITableOrderGrain, IRemindable
{
    private IPersistentState<TableOrderState> TableOrder { get; }
    private IGrainReminder _reminder = null;

    public TableOrderGrain(
        [PersistentState(stateName: "table-order", storageName: "tableorderstorage")] IPersistentState<TableOrderState> state)
    {
        this.TableOrder = state;
    }

    public async Task OpenTableOrder()
    {
        if (TableOrder.State.IsOpen)
            throw new InvalidStateException("Table has already opened.");

        this.TableOrder.State.IsOpen = true;
        this.TableOrder.State.OrderItems = new List<TableOrderItem>();

        _reminder = await this.RegisterOrUpdateReminder("TableOrderExpired",
               TimeSpan.Zero,
               TimeSpan.FromHours(3));
        
        await TableOrder.WriteStateAsync();
    }

    public async Task CloseTableOrder()
    {
        if (!TableOrder.State.IsOpen)
            throw new InvalidStateException("Table has already closed.");

        TableOrder.State.IsOpen = false;
        TableOrder.State.OrderItems = new List<TableOrderItem>();

        if (_reminder is not null)
        {
            await this.UnregisterReminder(_reminder);
            _reminder = null;
        }

        await TableOrder.WriteStateAsync();
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        return reminderName switch
        {
            "TableOrderExpired" => CloseTableOrder(),
            _ => Task.CompletedTask
        };
    }

    public async Task<Guid> AddOrderItem(string name, double cost, int quantity)
    {
        if (!TableOrder.State.IsOpen)
            throw new InvalidStateException("Table should be opened.");

        var orderItemId = Guid.NewGuid();
        TableOrder.State.OrderItems.Add(new TableOrderItem 
        {
            Id = orderItemId,
            Name = name,
            Cost = cost,
            Quantity = quantity
        });

        await TableOrder.WriteStateAsync();

        return orderItemId;
    }

    public Task<TableOrderItem> GetOrderItemDetails(Guid orderItemId)
    {
        var orderItemDetails = TableOrder.State.OrderItems.Single(x => x.Id == orderItemId);
        return Task.FromResult(orderItemDetails);
    }

    public Task<ReadOnlyCollection<TableOrderItem>> GetOrderItems()
    {
        return Task.FromResult(TableOrder.State.OrderItems.AsReadOnly());
    }

    public Task RemoveOrderItem(Guid orderItemId)
    {
        var orderItem = TableOrder.State.OrderItems.Single(x => x.Id == orderItemId);
        TableOrder.State.OrderItems.Remove(orderItem);

        return TableOrder.WriteStateAsync();
    }


}
