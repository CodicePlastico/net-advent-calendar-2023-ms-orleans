using PizzaPlastica.OrderingSystem.Abstractions;

namespace PizzaPlastica.OrderingSystem.Extensions;

public static class TableOrderExtensions
{
    public static GetTableItem ToResponse(this TableOrderItem resource)
        => new()
        {
            Id = resource.Id.ToString(),
            Name = resource.Name,
            Cost = resource.Cost,
            Quantity = resource.Quantity
        };
}
