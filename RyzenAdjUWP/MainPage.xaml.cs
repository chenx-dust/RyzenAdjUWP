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
            _ = Backend.LaunchBackend();
        }

        private void TdpSlider_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var valueSet = new ValueSet();
            valueSet["TDP"] = (int)e.NewValue;
            _ = Backend.Instance.SendRequestAsync(valueSet);
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
