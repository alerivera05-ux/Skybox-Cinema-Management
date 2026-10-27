using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SkyBox.Web.Binding;

// HTML number inputs submit a dot decimal separator, independently of display culture.
public sealed class InvariantDecimalBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext context)
    {
        var value = context.ValueProvider.GetValue(context.ModelName);
        if (value == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, value);
        if (string.IsNullOrWhiteSpace(value.FirstValue))
            context.Result = ModelBindingResult.Success(null);
        else if (decimal.TryParse(value.FirstValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                     CultureInfo.InvariantCulture, out var price))
            context.Result = ModelBindingResult.Success(price);
        else
            context.ModelState.AddModelError(context.ModelName, "Ingresa un precio válido, por ejemplo 30.50.");
        return Task.CompletedTask;
    }
}
