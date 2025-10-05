using Microsoft.Gaming.XboxGameBar;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RyzenAdjUWP
{
    public sealed partial class MainPage : IDisposable
    {
        private static MainPageModel _modelBase = new MainPageModel();
        private MainPageModelWrapper _model;
        private ResourceDictionary _compactFontSizeResource = new ResourceDictionary
        {
            Source = new Uri("ms-appx:///Styles/FontSizeCompact.xaml")
        };

        public MainPage()
        {
            InitializeComponent();
            _model = _modelBase.GetWrapper(Dispatcher);
            this.DataContext = _model;

            Backend.Instance.MessageReceivedEvent += Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent += Backend_OnClosedOrFailed;
            if (Backend.Instance.IsConnected)
                ConnectedInitialize();
            else
                PanelSwitch(false);
        }

        public void Dispose()
        {
            Backend.Instance.MessageReceivedEvent -= Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent -= Backend_OnClosedOrFailed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var widget = e.Parameter as XboxGameBarWidget;
            if (widget != null)
            {
                SwitchCompactMode(widget.CompactModeEnabled);
                widget.CompactModeEnabledChanged += OnCompactModeChanged;
            }
            base.OnNavigatedTo(e);
        }

        private void OnCompactModeChanged(XboxGameBarWidget widget, object _)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SwitchCompactMode(widget.CompactModeEnabled);
                var lastTheme = this.RequestedTheme;
                this.RequestedTheme = ElementTheme.Light;
                this.RequestedTheme = ElementTheme.Dark;
                this.RequestedTheme = lastTheme;
            });
        }

        private void SwitchCompactMode(bool enabled)
        {
            if (enabled)
            {
                if (!this.Resources.MergedDictionaries.Contains(_compactFontSizeResource))
                    this.Resources.MergedDictionaries.Add(_compactFontSizeResource);
            }
            else
            {
                this.Resources.MergedDictionaries.Remove(_compactFontSizeResource);
            }
        }

        private void ConnectedInitialize()
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(true));
            Backend.Instance.Send("init");
        }

        private void PanelSwitch(bool isBackendAlive)
        {
            if (isBackendAlive)
            {
                MainPanel.Visibility = Visibility.Visible;
                BackendPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainPanel.Visibility = Visibility.Collapsed;
                BackendPanel.Visibility = Visibility.Visible;
            }
        }

        private void Backend_OnMessageReceived(object sender, string message)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Backend_OnMessageReceived_Impl(sender, message));
        }

        private void Backend_OnMessageReceived_Impl(object sender, string message)
        {
            var backend = sender as Backend;
            string[] args = message.Split(' ');
            if (args.Length == 0)
                return;
            switch (args[0])
            {
                case "connected":
                    ConnectedInitialize();
                    break;
                case "tdp-limit":
                    _model.TdpMax = double.Parse(args[1]);
                    _model.TdpMin = double.Parse(args[2]);
                    break;
                case "tdp":
                    _model.SetTdpVar(double.Parse(args[1]));
                    break;
                case "autostart":
                    _model.SetAutoStartVar(bool.Parse(args[1]));
                    break;
            }
        }

        private void Backend_OnClosedOrFailed(object _, EventArgs args)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(false));
        }

        private void LaunchBackendButton_OnClick(object sender, RoutedEventArgs e)
        {
            _ = Backend.LaunchBackend();
        }
    }
}
