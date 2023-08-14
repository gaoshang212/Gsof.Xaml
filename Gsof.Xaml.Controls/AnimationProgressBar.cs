using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Gsof.Xaml.Controls;

public class AnimationProgressBar : ProgressBar
{
    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(double), typeof(AnimationProgressBar), new PropertyMetadata(500d));

    public double Value2
    {
        get => (double)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }
    public static readonly DependencyProperty Value2Property =
        DependencyProperty.Register(nameof(Value2), typeof(double), typeof(AnimationProgressBar), new PropertyMetadata(0d, OnValue2ChangedCallback, ConstrainToRange));


    private void DoAnimation(double start, double end)
    {
        var doubleAnimation = new DoubleAnimation(start, end, new Duration(TimeSpan.FromMilliseconds(AnimationDuration)));

        this.BeginAnimation(ValueProperty, doubleAnimation, HandoffBehavior.SnapshotAndReplace);
    }

    internal static void OnValue2ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AnimationProgressBar apb)
        {
            return;
        }

        apb.DoAnimation(apb.Value, (double)e.NewValue);
    }

    internal static object ConstrainToRange(DependencyObject d, object value)
    {
        var rangeBase = (RangeBase)d;
        var minimum = rangeBase.Minimum;
        var num = (double)value;
        if (num < minimum)
        {
            return minimum;
        }

        var maximum = rangeBase.Maximum;
        return num > maximum ? maximum : value;
    }
}