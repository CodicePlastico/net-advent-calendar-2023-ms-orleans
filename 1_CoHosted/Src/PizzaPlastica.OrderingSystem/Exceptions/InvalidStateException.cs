namespace PizzaPlastica.OrderingSystem.Exceptions;

public class InvalidStateException : Exception
{
    public InvalidStateException() : base() { }
    public InvalidStateException(string message) : base(message) {}
}
