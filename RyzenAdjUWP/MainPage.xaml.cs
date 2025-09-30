using Microsoft.Gaming.XboxGameBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace RyzenAdjUWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Backend.Instance.MessageReceivedEvent += Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent += Backend_OnClosedOrFailed;
            PanelSwitch(Backend.Instance.IsConnected);
        }

        private void Backend_OnMessageReceived(object sender, string message)
        {
            string[] args = message.Split(' ');
            if (args.Length == 0)
                return;
            switch (message)
            {
                case "pong":
                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(true));
                    break;
            }
        }

        private void Backend_OnClosedOrFailed(object sender, EventArgs args)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(false));
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

        private void TdpSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-tdp {TdpSlider.Value}");
        }

        private void LaunchBackendButton_OnClick(object sender, RoutedEventArgs e)
        {
            _ = Backend.LaunchBackend();
        }
    }

    public class WattConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{value} W";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AdjustSliderCommand : ICommand
    {
        public double Step { get; set; }

        public bool CanExecute(object parameter)
        {
            return parameter is Slider;
        }

        public void Execute(object parameter)
        {
            if (parameter is Slider slider)
            {
                double newValue = slider.Value + Step;

                if (newValue > slider.Maximum)
                    newValue = slider.Maximum;
                if (newValue < slider.Minimum)
                    newValue = slider.Minimum;

                slider.Value = newValue;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
