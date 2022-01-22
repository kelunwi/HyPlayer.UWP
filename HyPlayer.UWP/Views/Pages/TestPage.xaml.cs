using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
using HyPlayer.NCMProvider;
using HyPlayer.NCMProvider.Models;
using HyPlayer.NCMProvider.Returns;
using HyPlayer.Uta.ProvidableItem;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace HyPlayer.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPageViewModel TestPageViewModel = new TestPageViewModel();

        public TestPage()
        {
            this.InitializeComponent();
        }

        private async void ButtonLoadMusic_OnClick(object sender, RoutedEventArgs e)
        {
            Common.State.PlayCore.MoveNext();
            await Common.State.PlayCore.LoadNowPlayingItemMedia();
        }

        private void ButtonPlayMusic_OnClick(object sender, RoutedEventArgs e)
        {
            Common.State.PlayCore.Play();
        }

        private async void ButtonNextMusic_OnClick(object sender, RoutedEventArgs e)
        {
            await Common.State.PlayCore.Stop();
            Common.State.PlayCore.MoveNext();
            await Common.State.PlayCore.LoadNowPlayingItemMedia();
            await Common.State.PlayCore.Play();
        }

        private async void ButtonLoadPlayList_OnClick(object sender, RoutedEventArgs e)
        {
            var list = (await Common.State.NCAPI.GetProvidableItemByInProviderId("pl" + TestPageViewModel.PlayListId)) as NCMUserPlayList;
            Common.State.PlayCore.ReplacePlayListSource(list!);
            Common.State.PlayCore.LoadPlaySource();
        }

        private void ButtonPauseMusic_OnClick(object sender, RoutedEventArgs e)
        {
            Common.State.PlayCore.Pause();
        }

        private async void ButtonLogin_OnClick(object sender, RoutedEventArgs e)
        {
            var ret = await Common.State.NCAPI.RequestAsync<LoginReturn>(NeteaseApis.LoginCellphone,
                new Dictionary<string, object>()
                {
                    { "phone", TestPageViewModel.Account },
                    { "password", TestPageViewModel.Password }
                });
            if (ret.Code != 200)
                TestPageViewModel.UserName = ret.Message;
            else
                TestPageViewModel.UserName = ret.Profile.Nickname;
        }


        private void PositionSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Math.Abs(e.OldValue - e.NewValue) > 2000)
                Common.State.PlayCore.Seek(TimeSpan.FromMilliseconds(e.NewValue));
        }

        private async void ButtonPreviousMusic_OnClick(object sender, RoutedEventArgs e)
        {
            await Common.State.PlayCore.Stop();
            Common.State.PlayCore.MovePrevious();
            await Common.State.PlayCore.LoadNowPlayingItemMedia();
            await Common.State.PlayCore.Play();   
        }
    }

    public class PlayListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var playList = value as List<SingleSongBase>;
            if (playList == null)
                return null;
            return playList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class PlayPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).TotalMilliseconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromMilliseconds((double)value);
        }
    }

    public class PositionSliderThumbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromMilliseconds((double)value).ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class TestPageViewModel : DependencyObject
    {
        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            "UserName", typeof(string), typeof(TestPageViewModel), new PropertyMetadata(default(string)));

        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        public static readonly DependencyProperty AccountProperty = DependencyProperty.Register(
            "Account", typeof(string), typeof(TestPageViewModel), new PropertyMetadata(default(string)));

        public string Account
        {
            get { return (string)GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register(
            "Password", typeof(string), typeof(TestPageViewModel), new PropertyMetadata(default(string)));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PlayListIdProperty = DependencyProperty.Register(
            "PlayListId", typeof(string), typeof(TestPageViewModel), new PropertyMetadata(default(string)));

        public string PlayListId
        {
            get { return (string)GetValue(PlayListIdProperty); }
            set { SetValue(PlayListIdProperty, value); }
        }

        public static readonly DependencyProperty AlbumImageProperty = DependencyProperty.Register(
            "AlbumImage", typeof(BitmapImage), typeof(TestPageViewModel), new PropertyMetadata(default(BitmapImage)));

        public BitmapImage AlbumImage
        {
            get { return (BitmapImage)GetValue(AlbumImageProperty); }
            set { SetValue(AlbumImageProperty, value); }
        }
    }
}