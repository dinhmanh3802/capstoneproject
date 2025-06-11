using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Utility
{
    public class DateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);

            if (valueProviderResult != ValueProviderResult.None && !string.IsNullOrEmpty(valueProviderResult.FirstValue))
            {
                var dateAsString = valueProviderResult.FirstValue;

                if (DateTime.TryParseExact(dateAsString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    bindingContext.Result = ModelBindingResult.Success(date);
                    return Task.CompletedTask;
                }
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.FieldName, "Invalid date format");
            return Task.CompletedTask;
        }
    }
}
