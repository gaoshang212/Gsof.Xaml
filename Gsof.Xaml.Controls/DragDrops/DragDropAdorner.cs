using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

// ReSharper disable once CheckNamespace
namespace Gsof.Xaml.Controls;

public class DragDropAdorner : Adorner
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private readonly FrameworkElement? _draggedElement;

    public DragDropAdorner(UIElement element)
        : base(element)
    {
        IsHitTestVisible = false;
        _draggedElement = element as FrameworkElement;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (_draggedElement == null)
        {
            return;
        }

        var screenPos = new Win32.Point();

        if (Win32.GetCursorPos(ref screenPos))
        {
            Point pos = this.PointFromScreen(new Point(screenPos.X, screenPos.Y));
            Rect rect = new Rect(pos.X, pos.Y, _draggedElement.ActualWidth, _draggedElement.ActualHeight);
            drawingContext.PushOpacity(1.0);
            drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 0), rect);
            drawingContext.DrawRectangle(new VisualBrush(_draggedElement), new Pen(Brushes.Transparent, 0), rect);
        }
    }

    public static class Win32
    {
        public struct Point { public int X; public int Y; }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref Point point);
    }
}