using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;

namespace Histogram
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			Main app = new Main();
			var viewModel = new DisplayModel();
			app.DataContext = viewModel;
			app.Show();

			DispatcherTimer dispatcher = new DispatcherTimer();
			dispatcher.Interval = TimeSpan.FromMilliseconds(500);
			dispatcher.IsEnabled = true;

			dispatcher.Tick += delegate
			{
				CommandManager.InvalidateRequerySuggested();
			}; 
		}
	}
}
