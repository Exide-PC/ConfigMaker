using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConfigMaker.Utils.Converters
{
    public class NameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string name = (string)value;

            return !string.IsNullOrEmpty(name)?
                ValidationResult.ValidResult:
                new ValidationResult(false, Properties.Resources.CanNotBeEmpty_Error);
        }
    }
}
