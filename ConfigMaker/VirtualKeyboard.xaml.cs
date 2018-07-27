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

using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for VirtualKeyboard.xaml
    /// </summary>
    public partial class VirtualKeyboard : UserControl, IEnumerable<Button>
    {
        public VirtualKeyboard()
        {
            InitializeComponent();

            this.allButtons = this.grid.Children.OfType<Button>() // возьмем кнопки из основой сетки
                .Concat(mouseGrid.Children.OfType<Button>()) // объединим с кнопками в сетке с клавишами мыши
                .Where(b => b.Tag != null); // и выберем те, для которых задан тег

            // зададим всплывающий тултип для всех клавиш
            foreach (Button b in this.allButtons)
                b.ToolTip = b.Tag as string;
        }

        [Flags]
        public enum SpecialKey
        {
            Ctrl = 0x0001,
            Shift = 0x0010,
            Alt = 0x0011
        }

        // для быстродействия сохраним все кнопки отдельно
        private IEnumerable<Button> allButtons;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SpecialKey flags = 0;

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                flags |= SpecialKey.Ctrl;
            if (Keyboard.IsKeyDown(Key.LeftShift))
                flags |= SpecialKey.Shift;
            if (Keyboard.IsKeyDown(Key.LeftAlt))
                flags |= SpecialKey.Alt;

            this.OnKeyboardKeyDown?.Invoke(this, new KeyboardClickRoutedEvtArgs(e, flags));
        }

        public Button GetButtonByName(string key)
        {
            key = key.ToLower();
            return allButtons.FirstOrDefault(b => ((string)b.Tag).ToLower() == key);
        }

        public IEnumerator<Button> GetEnumerator() => this.allButtons.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public event EventHandler<KeyboardClickRoutedEvtArgs> OnKeyboardKeyDown;

        public class KeyboardClickRoutedEvtArgs: RoutedEventArgs
        {
            public string Key { get; }
            public SpecialKey SpecialKeyFlags { get; }

            public KeyboardClickRoutedEvtArgs(RoutedEventArgs innerArgs, SpecialKey flags)
            {
                this.RoutedEvent = innerArgs.RoutedEvent;
                this.Handled = false;
                this.Source = innerArgs.Source;
                this.SpecialKeyFlags = flags;

                this.Key = (string)((Button)this.Source).Tag;
            }
        }

        
    }
}
