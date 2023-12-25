using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gsof.Xaml.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gsof.Xaml.Controls.Behaviours;

public class ScrollViewerWheelHorizontallyBehavior : Behavior<UIElement>
{
    public ModifierKeys? ModifierKey { get; set; }

    protected override void OnAttached()
    {
        base.OnAttached();
        this.AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        this.AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
    {
        var scrollViewer = ((UIElement)sender).ChildOfType<ScrollViewer>();

        if (scrollViewer == null)
        {
            return;
        }

        if (ModifierKey != null && Keyboard.Modifiers != ModifierKey)
        {
            return;
        }

        if (args.Delta < 0)
        {
            scrollViewer.LineRight();
        }
        else
        {
            scrollViewer.LineLeft();
        }

        args.Handled = true;
    }

    public static bool GetWheelScrollsHorizontally(DependencyObject obj)
    {
        return (bool)obj.GetValue(WheelScrollsHorizontallyProperty);
    }

    public static void SetWheelScrollsHorizontally(DependencyObject obj, bool value)
    {
        obj.SetValue(WheelScrollsHorizontallyProperty, value);
    }

    public static readonly DependencyProperty WheelScrollsHorizontallyProperty =
        DependencyProperty.RegisterAttached("WheelScrollsHorizontally", typeof(bool), typeof(ScrollViewerWheelHorizontallyBehavior), new PropertyMetadata(false,
            OnWheelScrollsHorizontallyCallback));

    private static void OnWheelScrollsHorizontallyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not bool value)
        {
            return;
        }

        if (value)
        {
            d.ApplyBehavior<ScrollViewerWheelHorizontallyBehavior>();
        }
        else
        {
            d.RemoveBehavior<ScrollViewerWheelHorizontallyBehavior>();
        }
    }
}