namespace ProductStorage.Application.Exceptions;

public class ProductNotFoundException(Guid id) : Exception($"Product '{id}' was not found.")
{
    public Guid Id { get; } = id;
}