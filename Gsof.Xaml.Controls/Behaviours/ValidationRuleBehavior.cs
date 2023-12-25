using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Gsof.Xaml.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gsof.Xaml.Controls.Behaviours
{
    /// <summary>
    /// Button 点击时重新更新校验
    /// </summary>
    public class ValidationRuleBehavior : Behavior<ButtonBase>
    {
        public UIElement? Target
        {
            get => (UIElement?)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(ValidationRuleBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {

            this.AssociatedObject.Click += OnAssociatedObjectClick;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Click -= OnAssociatedObjectClick;
            base.OnDetaching();
        }

        private void OnAssociatedObjectClick(object sender, RoutedEventArgs e)
        {
            var bindings = GetAllBindings();

            foreach (var binding in bindings)
            {
                binding.UpdateSource();
            }
        }

        private List<BindingExpression> GetAllBindings()
        {
            if (this.Target == null)
            {
                throw new Exception("the target can not be null.");
            }

            var bindings = this.Target.LogicalChildrenOfType<UIElement>().SelectMany(i => i.GetBindingExpressions());

            var list = bindings as List<BindingExpression> ?? bindings.ToList();

            return list;
        }
    }
}