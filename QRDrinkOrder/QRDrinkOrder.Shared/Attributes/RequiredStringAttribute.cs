using System.ComponentModel.DataAnnotations;

namespace QRDrinkOrder.Shared.Attributes;

public class RequiredStringAttribute : RequiredAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
        
        return base.IsValid(value);
    }
}
