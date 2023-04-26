using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace Gsof.Xaml.Behaviours
{
    public class WindowMinimizedBehavior : Behavior<Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnButtonClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnButtonClick;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(AssociatedObject);

            if (window == null)
            {
                return;
            }

            window.WindowState = WindowState.Minimized;
        }
    }
}