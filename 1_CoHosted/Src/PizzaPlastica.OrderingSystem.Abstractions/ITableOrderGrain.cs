using System.Collections.ObjectModel;

namespace PizzaPlastica.OrderingSystem.Abstractions;
public interface ITableOrderGrain : IGrainWithGuidCompoundKey
{
    Task OpenTableOrder();
    Task CloseTableOrder();
    Task<Guid> AddOrderItem(string name, double cost, int quantity);
    Task RemoveOrderItem(Guid orderItemId);
    [Orleans.Concurrency.ReadOnly]
    Task<TableOrderItem> GetOrderItemDetails(Guid orderItemId);
    [Orleans.Concurrency.ReadOnly]
    Task<ReadOnlyCollection<TableOrderItem>> GetOrderItems();
}