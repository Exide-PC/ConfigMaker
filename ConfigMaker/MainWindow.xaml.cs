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
using ConfigMaker.Csgo.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Data;
using ConfigMaker.Csgo.Config.Entries;
using ConfigMaker.Utils.Converters;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using System.Windows.Documents;
using Res = ConfigMaker.Properties.Resources;
using ConfigMaker.Csgo.Config.Enums;
using ConfigMaker.Csgo.Config.Entries.interfaces;
using ConfigMaker.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ConfigMaker.Mvvm.ViewModels;
using ConfigMaker.Mvvm;

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

            Version a = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            //title.Text += $" {a.ToString()}";
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
