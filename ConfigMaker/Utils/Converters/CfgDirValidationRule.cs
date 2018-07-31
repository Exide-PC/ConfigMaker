using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ConfigMaker.Utils.Converters
{
    public class CfgDirValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string path = (string)value;

            return System.IO.Directory.Exists(path) && path.EndsWith(@"csgo\cfg")?
                ValidationResult.ValidResult :
                new ValidationResult(false, Properties.Resources.NotCsgoCfgDir_Error);
        }
    }
}
