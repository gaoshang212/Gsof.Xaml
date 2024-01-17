using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gsof.Xaml.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gsof.Xaml.Controls.Behaviours;

/// <summary>
/// 范围内存在没有通过 Validation 时， Target 对象 IsEnable 为 false. 需要绑定 ValidationRule 时设置 NotifyOnValidationError 为true
/// </summary>
public class ValidationRuleButtonEnableBehavior : Behavior<FrameworkElement>
{
    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        private set => SetValue(HasErrorPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HasErrorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasError), typeof(bool), typeof(ValidationRuleButtonEnableBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasErrorProperty = HasErrorPropertyKey.DependencyProperty;

    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(FrameworkElement),
            typeof(ValidationRuleButtonEnableBehavior), new PropertyMetadata(null));

    public bool UseLogicalTree
    {
        get => (bool)GetValue(UseLogicalTreeProperty);
        set => SetValue(UseLogicalTreeProperty, value);
    }

    public static readonly DependencyProperty UseLogicalTreeProperty =
        DependencyProperty.Register(nameof(UseLogicalTree), typeof(bool), typeof(ValidationRuleButtonEnableBehavior),
            new PropertyMetadata(true));


    protected override void OnAttached()
    {
        this.AssociatedObject.Loaded += OnAssociatedObjectLoaded;
        this.AssociatedObject.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        this.AssociatedObject.RemoveHandler(Validation.ErrorEvent, new RoutedEventHandler(OnErrorEvent));
        base.OnDetaching();
    }

    private void OnErrorEvent(object sender, RoutedEventArgs e)
    {
        this.AssociatedObject.Dispatcher.BeginInvoke(UpdateButtonEnable,
            System.Windows.Threading.DispatcherPriority.Send);
    }

    private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
    {
        UpdateButtonEnable();
    }

    private void UpdateButtonEnable()
    {
        if (this.Target == null)
        {
            return;
        }

        var elements = this.UseLogicalTree
            ? this.AssociatedObject.LogicalChildrenOfType<UIElement>()
            : this.AssociatedObject.ChildrenOfType<UIElement>();

        var bindings = elements.SelectMany(i => i.GetBindingExpressions());

        HasError = bindings.Any(i => i.HasError || !i.ValidateWithoutUpdate());
        this.Target.IsEnabled = !HasError;
    }
}