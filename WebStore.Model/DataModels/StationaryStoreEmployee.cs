using System.ComponentModel.DataAnnotations;

namespace WebStore.Model.DataModels;

public class StationaryStoreEmployee : User, IValidatableObject
{
    public int StationaryStoreId { get; set; }
    public StationaryStore StationaryStore { get; set; } = null!;
    public string Position { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            yield return new ValidationResult("Imię pracownika jest wymagane.", [nameof(FirstName)]);
        if (string.IsNullOrWhiteSpace(LastName))
            yield return new ValidationResult("Nazwisko pracownika jest wymagane.", [nameof(LastName)]);
        if (string.IsNullOrWhiteSpace(Position))
            yield return new ValidationResult("Stanowisko pracownika jest wymagane.", [nameof(Position)]);
    }
}
