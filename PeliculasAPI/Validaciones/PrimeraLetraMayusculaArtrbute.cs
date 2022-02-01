using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Validaciones
{
    public class PrimeraLetraMayuscula : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var firstWord = value.ToString()[0].ToString();

            if (firstWord != firstWord.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayuscula");
            }

            return ValidationResult.Success;
        }
    }
}
