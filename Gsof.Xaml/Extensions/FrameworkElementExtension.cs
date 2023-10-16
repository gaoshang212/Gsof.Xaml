using System.Threading.Tasks;
using System.Windows;

namespace Gsof.Xaml.Extensions;

public static class FrameworkElementExtension
{
    public static bool IsIntoView(this FrameworkElement element, FrameworkElement container, bool allShow = false)
    {
        var bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
        var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
        return !allShow ? rect.Contains(bounds) : rect.Contains(bounds.TopLeft) && rect.Contains(bounds.BottomRight);
    }

    public static Task WaitLoadAsync(this FrameworkElement element)
    {
        if (element.IsLoaded)
        {
#if NET46_OR_GREATER
            return Task.CompletedTask;
#else
            return Task.FromResult(true);
#endif

        }

        var tsc = new TaskCompletionSource<bool>();

        SetTaskCompletionSource(element, tsc);
        element.Loaded += OnElementLoaded;

        return tsc.Task;
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        var fe = sender as FrameworkElement;
        if (fe == null)
        {
            return;
        }

        fe.Loaded -= OnElementLoaded;

        var tsc = GetTaskCompletionSource(fe);
        if (tsc == null)
        {
            return;
        }
        SetTaskCompletionSource(fe, null);
        tsc.SetResult(true);
    }


    public static TaskCompletionSource<bool>? GetTaskCompletionSource(DependencyObject obj)
    {
        return (TaskCompletionSource<bool>?)obj.GetValue(TaskCompletionSourceProperty);
    }
    public static void SetTaskCompletionSource(DependencyObject obj, TaskCompletionSource<bool>? value)
    {
        obj.SetValue(TaskCompletionSourceProperty, value);
    }
    public static readonly DependencyProperty TaskCompletionSourceProperty =
        DependencyProperty.RegisterAttached("TaskCompletionSource", typeof(TaskCompletionSource<bool>), typeof(FrameworkElementExtension), new PropertyMetadata(null));


}