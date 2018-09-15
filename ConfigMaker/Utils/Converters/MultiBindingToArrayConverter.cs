using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ConfigMaker.Utils.Converters
{
    public class MultiBindingToArrayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                object[] extendedValues = new object[values.Length + 1];

                for (int i = 0; i < values.Length; i++)
                    extendedValues[i] = values[i];

                int extraParameter = int.Parse(parameter.ToString());
                extendedValues[extendedValues.Length - 1] = extraParameter;

                return extendedValues;
            }
            else 
                return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
