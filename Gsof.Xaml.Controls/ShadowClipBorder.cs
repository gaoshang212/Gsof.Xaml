using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Gsof.Xaml.Controls;

public class ShadowClipBorder : Border
{
    public double ShadowWidth
    {
        get => (double)GetValue(ShadowWidthProperty);
        set => SetValue(ShadowWidthProperty, value);
    }
    public static readonly DependencyProperty ShadowWidthProperty =
        DependencyProperty.Register(nameof(ShadowWidth), typeof(double), typeof(ShadowClipBorder), new PropertyMetadata(0d,
            (d, e) =>
            {
                if (d is not ShadowClipBorder scb)
                {
                    return;
                }

                scb.OnShadowWidthChanged((double)e.NewValue, (double)e.OldValue);

            }));

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);

        this.Clip = CreateClipGeometry(size);

        return size;
    }

    protected virtual void OnShadowWidthChanged(double value, double oldValue)
    {
        this.Effect = new BlurEffect() { Radius = value };
    }

    protected virtual Geometry CreateClipGeometry(Size size)
    {
        CombinedGeometry combinedGeometry = new CombinedGeometry();

        combinedGeometry.GeometryCombineMode = GeometryCombineMode.Exclude;

        combinedGeometry.Geometry1 = new RectangleGeometry(new Rect(-ShadowWidth, -ShadowWidth, size.Width + ShadowWidth * 2, size.Height + ShadowWidth * 2));

        var rectGeometry = new RectangleGeometry(new Rect(0, 0, size.Width, size.Height), this.CornerRadius.TopLeft, this.CornerRadius.BottomRight);

        combinedGeometry.Geometry2 = rectGeometry;

        return combinedGeometry;
    }
}