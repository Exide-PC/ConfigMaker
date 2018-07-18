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
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            this.defaultStyle = (Style)this.Resources[typeof(Button)];

            //foreach (UIElement element in this.grid.Children.OfType<Button>())
            //{
            //    ((CustomButton)element).Click += CustomButton_Click;
            //}
        }

        Style defaultStyle;

        [Flags]
        public enum SpecialKey
        {
            Ctrl = 0x0001,
            Shift = 0x0010,
            Alt = 0x0011
        }

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
        
        public Style AdaptButtonStyle(Style style)
        {
            // Создадим новый стиль, базирующийся на исходном
            Style adapted = new Style(typeof(Button), style);

            // И дополним его сеттерами каноничного стиля
            foreach (var setter in this.defaultStyle.Setters)
                adapted.Setters.Add(setter);

            return adapted;
        }

        public Button[] GetEnabledButtons()
        {
            return this.grid.Children.Cast<Button>().Where(b => b.IsEnabled).ToArray();
        }

        public Button GetButtonByName(string key)
        {
            key = key.ToLower();

            return this.grid.Children.OfType<Button>().FirstOrDefault(b => ((string)b.Tag).ToLower() == key);
        }

        public void ResetButtons()
        {
            foreach (Button b in this.grid.Children.OfType<Button>())
                b.Style = this.defaultStyle;
            //foreach (Button b in this.grid.Children.OfType<Button>())
            //    b.BaseColor = (SolidColorBrush)this.Resources["DefaultKeyBackground"];
        }

        public void ResetButton(string key)
        {
            Button selectedButton = this.GetButtonByName(key);
            selectedButton.Style = this.defaultStyle;
        }

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
