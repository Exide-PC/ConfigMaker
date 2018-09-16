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
    public class CsgoPathValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string path = (string)value;

            return Directory.Exists(Path.Combine(path, @"csgo\cfg"))?
                ValidationResult.ValidResult :
                new ValidationResult(false, Properties.Resources.NotCsgoPath_Error);
        }
    }
}
