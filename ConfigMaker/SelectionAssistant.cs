using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigMaker
{
    public static class SelectionAssistant
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(Boolean),
            typeof(SelectionAssistant),
            new FrameworkPropertyMetadata(false));

        public static void SetIsSelected(UIElement element, Boolean value)
        {
            element.SetValue(IsSelectedProperty, value);
        }
        public static Boolean GetIsSelected(UIElement element)
        {
            return (Boolean)element.GetValue(IsSelectedProperty);
        }
    }
}
