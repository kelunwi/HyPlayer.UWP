using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HyPlayer.NCMProvider;
using HyPlayer.Uta;
using HyPlayer.UWP.Classes;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace HyPlayer.UWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

        private Task InitializeThings()
        {
            Common.State.PlayCore = new PlayCore();
            Common.State.PlayCore.RegisterAudioService(new AudioGraphService.AudioGraphService());
            Common.State.PlayCore.UseAudioServiceAsync("AudioGraphService");
            Common.State.PlayCore.RegisterPlayController(new HyPlayerBasicPlayController());
            Common.State.NCAPI = new NeteaseCloudMusicProvider();
            Common.State.PlayCore.RegisterMusicProvider(Common.State.NCAPI);
            return Task.CompletedTask;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                Window.Current.SetTitleBar(AppTitleBar);
            }
            else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox")
            {
                var result = ApplicationViewScaling.TrySetDisableLayoutScaling(true);
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }

            InitializeThings();
        }
    }
}