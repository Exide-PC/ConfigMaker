/*
MIT License

Copyright (c) 2018 Exide-PC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
