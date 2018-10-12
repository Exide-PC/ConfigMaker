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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ConfigMaker.Mvvm.ViewModels;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel MainVM => this.DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Using code behind to avoid using external library reference
            this.MainVM.SaveAppCommand.Execute(null);
        }

        /// <summary>
        /// Method used to calculate MaxHeight of action category items control dynamically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Take panel and find it's itemscontrol child
            StackPanel panel = (StackPanel)sender;
            ItemsControl itemsControl = panel.Children.OfType<ItemsControl>().First();

            // If itemscontrol hosts ActionViewModels then we take height of main stackpanel,
            // compare it with current MaxActionHeight property and replace if new one is bigger
            if (itemsControl.HasItems && itemsControl.Items[0] is ActionViewModel)
            {
                if (panel.ActualHeight > this.MainVM.MaxActionHeight)
                    this.MainVM.MaxActionHeight = panel.ActualHeight;
            }
        }
    }
}
