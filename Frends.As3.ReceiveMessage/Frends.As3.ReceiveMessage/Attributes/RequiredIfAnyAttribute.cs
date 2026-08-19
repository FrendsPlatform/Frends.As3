using System;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveMessage.Attributes;

/// <summary>
/// Validates that a property is required when any of the specified boolean properties is true.
/// If the property value is null, empty, or white space only, validation fails.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class RequiredIfAnyAttribute(params string[] dependentProperties) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        foreach (var dependentProperty in dependentProperties)
        {
            var field = validationContext.ObjectType.GetProperty(dependentProperty);
            if (field == null)
                return new ValidationResult($"Unknown property: {dependentProperty}");

            var dependentValue = field.GetValue(validationContext.ObjectInstance);
            if (dependentValue is true)
            {
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
            }
        }

        return ValidationResult.Success;
    }
}
