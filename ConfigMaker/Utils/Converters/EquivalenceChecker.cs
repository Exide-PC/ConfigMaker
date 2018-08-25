using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConfigMaker.Utils.Converters
{
    [ValueConversion(typeof(int), typeof(bool))]
    class EquivalenceChecker : IValueConverter
    {
        object storedValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.storedValue = value;
            return (int.Parse(value.ToString()) == int.Parse(parameter.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return storedValue;
        }
    }
}
