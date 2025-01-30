using System.Globalization;
using System.Windows.Controls;

namespace RecoilMacro.Converters
{
    public class DoubleValidationRule : ValidationRule
    {
        #region Static Fields

        public static Dictionary<string, bool> ErrorStates = new Dictionary<string, bool>();

        #endregion

        #region Static Properties

        public static bool HasAnyErrors => ErrorStates.Any(x => x.Value);

        #endregion

        #region Properties

        public string FieldName { get; set; }
        public bool AllowNegative { get; set; } = false;

        #endregion

        #region Methods

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = (value as string ?? "").Trim();
            if (string.IsNullOrEmpty(text))
            {
                SetErrorState(true);
                return new ValidationResult(false, "Enter number");
            }

            text = text.Replace('.', ',');

            var ru = CultureInfo.GetCultureInfo("ru-RU");
            if (double.TryParse(text, NumberStyles.Float, ru, out double d))
            {
                if (!AllowNegative && d < 0)
                {
                    SetErrorState(true);
                    return new ValidationResult(false, "The number cannot be negative");
                }
                SetErrorState(false);
                return ValidationResult.ValidResult;
            }

            SetErrorState(true);
            return new ValidationResult(false, "Invalid number format");
        }

        private void SetErrorState(bool isError)
        {
            if (!string.IsNullOrEmpty(FieldName))
            {
                ErrorStates[FieldName] = isError;
            }
        }

        #endregion
    }
}
