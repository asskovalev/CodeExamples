using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Histogram.Common.Wpf
{
	public abstract class ObservableObject : INotifyPropertyChanged
	{
		protected void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		bool _isUiBlocked;
		public bool IsUiBlocked
		{
			get { return _isUiBlocked; }
			set
			{
				_isUiBlocked = value;
				RaisePropertyChanged("IsUiBlocked");
			}
		}
	}
}
