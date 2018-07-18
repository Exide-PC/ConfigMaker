using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ConfigMaker.Converters
{
    [ValueConversion(typeof(Button), typeof(FontWeight))]
    class TagToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new Exception("Тег равен null, но при этом есть кнопка");

            FrameworkElement tag = (FrameworkElement)value;
            FrameworkElement param = (FrameworkElement)parameter;

            return tag == param ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
