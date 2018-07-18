using ConfigMaker.Csgo.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for ToggleWindow.xaml
    /// </summary>
    public partial class ToggleWindow : Window
    {
        public bool IsInteger { get; }
        public double From { get; }
        public double To { get; }
        public string GeneratedArg { get; private set; } = null;

        public ToggleWindow(bool isInteger, double from, double to)
        {
            this.IsInteger = isInteger;
            this.From = from;
            this.To = to;
            InitializeComponent();

            string fromStr = from == double.MinValue || from == int.MinValue ? "-∞": Executable.FormatNumber(from, isInteger);
            string toStr = to == double.MaxValue || to == int.MaxValue ? "+∞": Executable.FormatNumber(to, isInteger);
            upperText.Text += $" ({fromStr} .. {toStr})";
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = ((TextBox)sender).Text;

            string[] parts = text.Trim().Split(' ');

            if (this.IsInteger)
            {
                if (parts.All(p =>
                {
                    int temp;
                    return int.TryParse(p, out temp) && temp >= this.From && temp <= this.To;
                }))
                {
                    this.GeneratedArg = string.Join(" ", parts);
                }
                else
                    this.GeneratedArg = null;
            }
            else
            {
                if (parts.All(p =>
                {
                    double temp;
                    return Executable.TryParseDouble(p, out temp) && temp >= this.From && temp <= this.To;
                }))
                {
                    string[] formattedArgs = parts.Select(p =>
                    {
                        double value;
                        Executable.TryParseDouble(p, out value);
                        return Executable.FormatNumber(value, false);
                    }).ToArray();
                    this.GeneratedArg = string.Join(" ", formattedArgs);
                }
                else
                    this.GeneratedArg = null;
            }

            this.okButton.IsEnabled = this.GeneratedArg != null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = this.GeneratedArg != null;
        }
    }
}
