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

using ConfigMaker.Mvvm.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ConfigMaker.Mvvm.Models.VirtualKeyboardModel;

namespace ConfigMaker
{
    /// <summary>
    /// Interaction logic for VirtualKeyboard.xaml
    /// </summary>
    public partial class VirtualKeyboard : UserControl
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

            // Как только будет задан нужный контекст - повесим соответствующий обработчик,
            // который будет редактировать цвета кнопок клавиатуры
            this.DataContextChanged += (_, newContextArg) =>
            {
                if (newContextArg.NewValue is VirtualKeyboardViewModel viewModel)
                {
                    viewModel.PropertyChanged += (__, arg) =>
                    {
                        if (arg.PropertyName is nameof(VirtualKeyboardViewModel.KeyStates))
                            this.ColorizeKeyboard();
                    };
                }
            };
        }

        void ColorizeKeyboard()
        {
            // сбрасываем цвета перед обновлением
            foreach (Button key in this.allButtons)
            {
                key.ClearValue(ButtonBase.BackgroundProperty);
                key.ClearValue(ButtonBase.ForegroundProperty);
            }

            // Получим информацию о новом состоянии клавиш
            Dictionary<string, KeyState> keyMap = ((VirtualKeyboardViewModel)this.DataContext).KeyStates;

            // Также вытащим необходимые для работы кисти
            SolidColorBrush keyInSequenceBackground = (SolidColorBrush)this.FindResource("SecondaryAccentBrush");
            SolidColorBrush keyInSequenceForeground = (SolidColorBrush)this.FindResource("SecondaryAccentForegroundBrush");

            SolidColorBrush firstKeyBackground = (SolidColorBrush)this.FindResource("PrimaryHueMidBrush");
            SolidColorBrush firstKeyForeground = (SolidColorBrush)this.FindResource("PrimaryHueMidForegroundBrush");

            SolidColorBrush secondKeyBackground = (SolidColorBrush)this.FindResource("PrimaryHueDarkBrush");
            SolidColorBrush secondKeyForeground = (SolidColorBrush)this.FindResource("PrimaryHueDarkForegroundBrush");

            foreach (KeyValuePair<string, KeyState> pair in keyMap)
            {
                string key = pair.Key;
                Button targetButton = this.GetButtonByName(key);

                switch(pair.Value)
                {
                    case KeyState.FirstInSequence:
                        {
                            targetButton.Background = firstKeyBackground;
                            targetButton.Foreground = firstKeyForeground;
                            break;
                        };
                    case KeyState.SecondInSequence:
                        {
                            targetButton.Background = secondKeyBackground;
                            targetButton.Foreground = secondKeyForeground;
                            break;
                        }
                    case KeyState.InCurrentSequence:
                        {
                            targetButton.Background = keyInSequenceBackground;
                            targetButton.Foreground = keyInSequenceForeground;
                            break;
                        }
                }
            }
        }

        // для быстродействия сохраним все кнопки отдельно
        private IEnumerable<Button> allButtons;

        public Button GetButtonByName(string key)
        {
            key = key.ToLower();
            return allButtons.FirstOrDefault(b => ((string)b.Tag).ToLower() == key);
        }
    }
}
