using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace RyzenAdjUWP
{
	internal class MainPageModelWrapper : INotifyPropertyChanged, IDisposable
	{
		private MainPageModel _base;
		private CoreDispatcher _dispatcher;

		public MainPageModelWrapper(MainPageModel baseModel, CoreDispatcher dispatcher)
		{
			_base = baseModel;
			_dispatcher = dispatcher;
		}

		public double Tdp
		{
			get { lock (_base) { return _base.tdp; } }
			set
			{
                lock (_base)
                {
					value = Math.Min(Math.Max(value, _base.tdpMin), _base.tdpMax);
					if (_base.tdp != value)
					{
						_base.tdp = value;
						_base.Notify("Tdp");
						Backend.Instance.Send($"set-tdp {value}");
					}
				}
			}
		}
		public void SetTdpVar(double value)
		{
			lock (_base)
			{
				if (_base.tdp != value)
				{
					_base.tdp = value;
					_base.Notify("Tdp");
				}
			}
		}
		public double TdpMax
		{
			get { lock (_base) { return _base.tdpMax; } }
			set
			{
				lock (_base)
				{
					if (_base.tdpMax != value)
					{
						_base.tdpMax = value;
						if (_base.tdp > value)
							SetTdpVar(value);
						_base.Notify("TdpMax");
					}
				}
			}
		}
		public double TdpMin
		{
			get { lock (_base) { return _base.tdpMin; } }
			set
			{
				lock (_base)
				{
					if (_base.tdpMin != value)
					{
						_base.tdpMin = value;
						if (_base.tdp < value)
							SetTdpVar(value);
						_base.Notify("TdpMin");
					}
				}
			}
		}
		public bool AutoStart
		{
			get { lock (_base) { return _base.autoStart; } }
			set
			{
                lock (_base)
                {
					if (_base.autoStart != value)
					{
						_base.autoStart = value;
						_base.Notify("AutoStart");
						Backend.Instance.Send($"autostart {value}");
					}
				}
			}
        }
        public void SetAutoStartVar(bool value)
        {
            lock (_base)
            {
                if (_base.autoStart != value)
                {
                    _base.autoStart = value;
                    _base.Notify("AutoStart");
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

		public async Task Notify(string propertyName)
		{
			await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				if (PropertyChanged != null)
				{
					this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			});
		}

		public void Dispose()
		{
			_base.Remove(this);
		}
	}

	class MainPageModel
    {
		public double tdp = 30;
		public double tdpMax = 100;
		public double tdpMin = 0;
		public bool autoStart = false;
		public bool isConnected = false;

        private List<MainPageModelWrapper> _wrappers = new List<MainPageModelWrapper>();

		public MainPageModelWrapper GetWrapper(CoreDispatcher dispatcher)
		{
			var wrapper = new MainPageModelWrapper(this, dispatcher);
			_wrappers.Add(wrapper);
			return wrapper;
		}

		public void Notify(string propertyName)
		{
			foreach (var wrapper in _wrappers)
				_ = wrapper.Notify(propertyName);
		}

		public void Remove(MainPageModelWrapper wraper)
		{
			_wrappers.Remove(wraper);
		}
	}
}
