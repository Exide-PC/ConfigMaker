using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ConfigMaker.Converters
{
    [ValueConversion(typeof(Brush), typeof(Brush))]
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush sourceBrush = (SolidColorBrush)value;
            Color sourceColor = sourceBrush.Color;

            if (parameter == null)
                return sourceBrush;

            int r = sourceColor.R;
            int g = sourceColor.G;
            int b = sourceColor.B;

            bool isLighter = (string)parameter == "Lighter";
            byte delta = 100;

            r += isLighter ? delta : -delta;
            g += isLighter ? delta : -delta;
            b += isLighter ? delta : -delta;

            Color newColor = new Color();
            newColor.A = sourceColor.A;
            newColor.R = CoerceValue(r);
            newColor.G = CoerceValue(g);
            newColor.B = CoerceValue(b);

            return new SolidColorBrush(newColor);
        }

        byte CoerceValue(int value)
        {
            if (value < byte.MinValue) return byte.MinValue;
            if (value > byte.MaxValue) return byte.MaxValue;
            return (byte)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
