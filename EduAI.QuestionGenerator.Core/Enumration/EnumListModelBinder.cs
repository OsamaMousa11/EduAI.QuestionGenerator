using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduAI.QuestionGenerator.Api.ModelBinders
{
    public class EnumListModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
                return Task.CompletedTask;

            var values = valueProviderResult.Values;
            if (values.Count == 0)
                return Task.CompletedTask;

            var elementType = bindingContext.ModelType.GetGenericArguments()[0];
            var result = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            var addMethod = result.GetType().GetMethod("Add");

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                try
                {
                   
                    if (int.TryParse(value, out var intValue))
                    {
                        var enumValue = Enum.ToObject(elementType, intValue);
                        addMethod?.Invoke(result, new[] { enumValue });
                    }
        
                    else if (Enum.TryParse(elementType, value, true, out var enumValueByName))
                    {
                        addMethod?.Invoke(result, new[] { enumValueByName });
                    }
                }
                catch
                {
                    throw new FormatException($"Value '{value}' is not valid for enum type '{elementType.Name}'.");
                }
            }

            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }
    }
}