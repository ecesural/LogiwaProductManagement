namespace Product.Api.Application.Features.Products.Utils;
public class OptionalField<T>
{
    public bool IsSet { get; set; }
    public T? Value { get; set; }

    public static implicit operator OptionalField<T>(T? value)
    {
        return new OptionalField<T>
        {
            IsSet = true,
            Value = value
        };
    }
}
