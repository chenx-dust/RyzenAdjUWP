using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RyzenAdjUWP
{
    class MainPageModel : INotifyPropertyChanged
    {
		private double m_tdp;
		public double Tdp
		{
			get => m_tdp;
			set
			{
				if (m_tdp != value)
				{
					m_tdp = value;
					Notify("Tdp");
				}
			}
		}

        public event PropertyChangedEventHandler PropertyChanged;
		private void Notify(string propertyName)
		{
			if (PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
