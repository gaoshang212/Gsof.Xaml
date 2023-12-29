using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Gsof.Extensions;
using Gsof.Xaml.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gsof.Xaml.Controls.Behaviours
{
    public class CheckAllBehavior : Behavior<CheckBox>
    {
        private bool _isRunable = true;
        private Panel? _panel;
        private int _count;

        public UIElement? Target
        {
            get => (UIElement?)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(CheckAllBehavior), new PropertyMetadata(null, OnTargetChanged));


        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Checked += OnAssociatedObjectChecked;
            this.AssociatedObject.Unchecked += OnAssociatedObjectUnchecked;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.Checked -= OnAssociatedObjectChecked;
            this.AssociatedObject.Unchecked -= OnAssociatedObjectUnchecked;
            this.RemoveTargetHandler(this.Target);
        }

        private void OnAssociatedObjectUnchecked(object sender, RoutedEventArgs e)
        {
            this.InvokeCheck(false);
        }

        private void OnAssociatedObjectChecked(object sender, RoutedEventArgs e)
        {
            if (this.Target == null)
            {
                this.AssociatedObject.IsChecked = false;
            }

            this.InvokeCheck(true);
        }

        public void InvokeCheck(bool check)
        {
            if (!this.Begin())
            {
                return;
            }
            try
            {
                if (this.Target == null)
                {
                    return;
                }

                var items = this.GetCheckBox();

                if (items == null)
                {
                    return;
                }

                if (!items.Any() && check)
                {
                    this.AssociatedObject.IsChecked = false;
                    return;
                }

                items.Apply(i => i.IsChecked = check);
            }
            finally
            {
                this.End();
            }
        }

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as CheckAllBehavior;
            if (behavior == null)
            {
                return;
            }

            behavior.OnTargetChanged(e.NewValue as UIElement, e.OldValue as UIElement);
        }

        private void OnTargetChanged(UIElement? element, UIElement? oldElement)
        {
            RemoveTargetHandler(oldElement);
            AddTargetHandler(element);
        }

        private void RemoveTargetHandler(UIElement? element)
        {
            if (element == null)
            {
                return;
            }

            element.RemoveHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnInTargetChecked));
            element.RemoveHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnInTargetUnchecked));

            if (_panel != null)
            {
                _panel.LayoutUpdated -= OnPanelLayoutUpdated;
                _panel = null;
            }
        }

        private async void AddTargetHandler(UIElement? element)
        {
            if (element == null)
            {
                return;
            }

            element.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnInTargetChecked), true);
            element.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(OnInTargetUnchecked), true);

            Panel? panel = null;
            if (element is Panel pan)
            {
                panel = pan;
            }
            else if (element is ItemsControl items)
            {
                await items.WaitLoadAsync();

                panel = typeof(ItemsControl).InvokeMember("ItemsHost",
                    BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance, null, element,
                    null) as Panel;
            }

            if (panel == null)
            {
                return;
            }

            _panel = panel;
            panel.LayoutUpdated += OnPanelLayoutUpdated;
        }

        private void OnPanelLayoutUpdated(object sender, EventArgs e)
        {
            var count = _panel?.Children.Count ?? 0;
            if (count == _count)
            {
                return;
            }

            _count = count;

            this.InvokeAssociatedCheckBox();
        }

        private void OnInTargetChecked(object sender, RoutedEventArgs e)
        {
            this.InvokeAssociatedCheckBox();

        }

        private void OnInTargetUnchecked(object sender, RoutedEventArgs e)
        {
            this.InvokeAssociatedCheckBox();
        }

        private void InvokeAssociatedCheckBox()
        {
            if (!this.Begin())
            {
                return;
            }
            try
            {
                var items = this.GetCheckBox();

                
                if (items == null)
                {
                    return;
                }

                bool? all = items.Any() && items.All(i => i.IsChecked == true);

                if (all != true)
                {
                    all = items.Any(i => i.IsChecked == true) ? null : false;
                }

                this.AssociatedObject.IsChecked = all;
            }
            finally
            {
                this.End();
            }
        }

        private bool Begin()
        {
            var result = this._isRunable;
            if (result)
            {
                this._isRunable = false;
            }

            return result;
        }

        private void End()
        {
            this._isRunable = true;
        }

        private List<CheckBox>? GetCheckBox()
        {
            var list = this.Target?.ChildrenOfType<CheckBox>().Where(i => i != this.AssociatedObject).ToList();

            return list;
        }

    }
}

