using Windows.UI.Xaml;
using HyPlayer.NCMProvider;
using HyPlayer.Uta;

namespace HyPlayer.UWP.Common;

public static class State
{
    public static PlayCore PlayCore;
    public static NeteaseCloudMusicProvider NCAPI;
    public static readonly Store Store = new Store();
}

public sealed class Store : DependencyObject
{
    public static readonly DependencyProperty CanGoBackProperty = DependencyProperty.Register(
        "CanGoBack", typeof(bool), typeof(Store), new PropertyMetadata(default(bool)));

    public bool CanGoBack
    {
        get { return (bool)GetValue(CanGoBackProperty); }
        set { SetValue(CanGoBackProperty, value); }
    }
}