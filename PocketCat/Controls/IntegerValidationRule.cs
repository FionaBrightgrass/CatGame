using System.Globalization;
using System.Windows.Controls;

namespace PocketCat.Controls
{
    public class IntegerValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (int.TryParse(value.ToString(), out int inputValue))
            {
                if (inputValue >= Min && inputValue <= Max)
                {
                    return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, $"Please enter an integer between {Min} and {Max}.");
        }
    }

}
