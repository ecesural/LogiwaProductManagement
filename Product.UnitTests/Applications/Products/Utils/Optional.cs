using Product.Api.Application.Features.Products.Dtos;

namespace Product.UnitTests.Applications.Products.Utils;

public readonly struct OptionalField<T>
{
    public bool IsSet { get; }
    public T Value { get; }

    public OptionalField(T value)
    {
        Value = value;
        IsSet = true;
    }
    
    public static OptionalField<Guid?> NotSet() => default;

    public override string ToString() => IsSet ? Value?.ToString() ?? "null" : "[NotSet]";
}
