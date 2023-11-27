namespace PizzaPlastica.OrderingSystem.Abstractions;

[GenerateSerializer]
public class TableOrderItem
{
    [Id(0)]
    public Guid Id { get; set; }
    [Id(1)]
    public string Name { get; set; }
    [Id(2)]
    public double Cost { get; set; }
    [Id(3)]
    public int Quantity { get; set; }
}
