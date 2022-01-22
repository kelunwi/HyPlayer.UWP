using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HyPlayer.UWP.Models;

public class NavigationMenuItemModel : DependencyObject
{
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        "Content", typeof(string), typeof(NavigationMenuItemModel), new PropertyMetadata(default(string)));

    public string Content
    {
        get => (string)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        "Icon", typeof(IconElement), typeof(NavigationMenuItemModel), new PropertyMetadata(default(IconElement)));

    public IconElement Icon
    {
        get => (IconElement)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IsPageNavigatorProperty = DependencyProperty.Register(
        "IsPageNavigator", typeof(bool), typeof(NavigationMenuItemModel), new PropertyMetadata(default(bool)));

    public bool IsPageNavigator
    {
        get => (bool)GetValue(IsPageNavigatorProperty);
        set => SetValue(IsPageNavigatorProperty, value);
    }

    public static readonly DependencyProperty PageTypeProperty = DependencyProperty.Register(
        "PageType", typeof(Type), typeof(NavigationMenuItemModel), new PropertyMetadata(default(Type)));

    public Type PageType
    {
        get => (Type)GetValue(PageTypeProperty);
        set => SetValue(PageTypeProperty, value);
    }

    public static readonly DependencyProperty PageParameterProperty = DependencyProperty.Register(
        "PageParameter", typeof(object), typeof(NavigationMenuItemModel), new PropertyMetadata(default(object)));

    public object PageParameter
    {
        get => GetValue(PageParameterProperty);
        set => SetValue(PageParameterProperty, value);
    }

    public static readonly DependencyProperty SelectedOperationProperty = DependencyProperty.Register(
        "SelectedOperation", typeof(RoutedEventHandler), typeof(NavigationMenuItemModel),
        new PropertyMetadata(default(RoutedEventHandler)));

    public RoutedEventHandler SelectedOperation
    {
        get => (RoutedEventHandler)GetValue(SelectedOperationProperty);
        set => SetValue(SelectedOperationProperty, value);
    }

    public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(
        "Children", typeof(ObservableCollection<NavigationMenuItemModel>), typeof(NavigationMenuItemModel),
        new PropertyMetadata(default(ObservableCollection<NavigationMenuItemModel>)));

    public ObservableCollection<NavigationMenuItemModel> Children
    {
        get => (ObservableCollection<NavigationMenuItemModel>)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }

    public static readonly DependencyProperty IsBlankProperty = DependencyProperty.Register(
        "IsBlank", typeof(bool), typeof(NavigationMenuItemModel), new PropertyMetadata(false));

    public bool IsBlank
    {
        get => (bool)GetValue(IsBlankProperty);
        set => SetValue(IsBlankProperty, value);
    }
}