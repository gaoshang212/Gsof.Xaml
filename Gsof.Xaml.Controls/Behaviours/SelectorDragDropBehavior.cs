using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Gsof.Xaml.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gsof.Xaml.Controls.Behaviours;

public class SelectorDragDropBehavior : Behavior<Selector>
{
    private AdornerLayer? _adornerLayer;

    protected override void OnAttached()
    {
        base.OnAttached();

        this.AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        this.AssociatedObject.QueryContinueDrag += OnQueryContinueDrag;
        this.AssociatedObject.Drop += OnDrop;
        this.AssociatedObject.DragOver += OnDragOver;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        this.AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        this.AssociatedObject.QueryContinueDrag -= OnQueryContinueDrag;
        this.AssociatedObject.Drop -= OnDrop;
        this.AssociatedObject.DragOver -= OnDragOver;
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (Mouse.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var pos = e.GetPosition(this.AssociatedObject);

        var element = this.GetSelectorItem(pos);

        if (element == null || this.AssociatedObject.SelectedItem is null)
        {
            return;
        }

        var adorner = new DragDropAdorner(element);
        _adornerLayer = AdornerLayer.GetAdornerLayer(this.AssociatedObject);
        _adornerLayer?.Add(adorner);

        var dataItem = element.DataContext;

        DragDrop.DoDragDrop(this.AssociatedObject, dataItem, DragDropEffects.Move);

        _adornerLayer?.Remove(adorner);
        _adornerLayer = null;
    }

    private void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
        _adornerLayer?.Update();
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        var parent = this.AssociatedObject;

        var item = this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;

        if (item == null)
        {
            return;
        }

        var data = e.Data.GetData(item.DataContext.GetType());

        var objectToPlaceBefore = GetObjectDataFromPoint(item.GetType(), e.GetPosition(parent));
        if (data == null || objectToPlaceBefore == null)
        {
            return;
        }

        if (this.AssociatedObject.ItemsSource is not IList list)
        {
            return;
        }

        var index = list.IndexOf(objectToPlaceBefore);
        list.Remove(data);
        list.Insert(index, data);
        this.AssociatedObject.SelectedItem = data;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;

        var pos = e.GetPosition(this.AssociatedObject);
        var element = this.GetSelectorItem(pos);

        if (element == null)
        {
            return;
        }

        this.AssociatedObject.SelectedItem = element;
        e.Effects = DragDropEffects.Copy;
    }

    private object? GetObjectDataFromPoint(Type itemType, Point point)
    {
        if (this.AssociatedObject?.InputHitTest(point) is not UIElement element)
        {
            return null;
        }

        var item = element.ParentOfType<FrameworkElement>(itemType.IsInstanceOfType);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (item == null)
        {
            return null;
        }

        var data = this.AssociatedObject.ItemContainerGenerator.ItemFromContainer(item);

        return data;
    }

    private FrameworkElement? GetSelectorItem(Point point)
    {
        HitTestResult result = VisualTreeHelper.HitTest(this.AssociatedObject, point);
        if (result == null)
        {
            return null;
        }

        var item = this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(0);

        var type = item.GetType();
        var element = result.VisualHit.ParentOfType<FrameworkElement>(i => type.IsInstanceOfType(i));

        return element;
    }

}