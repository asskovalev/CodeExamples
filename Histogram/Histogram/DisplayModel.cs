using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using OxyPlot;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Concurrent;
using Histogram.Common.Wpf;
using Histogram.Common;

namespace Histogram
{
	public class DisplayModel : ObservableObject
	{
		string _filePath;
		public string FilePath
		{
			get { return _filePath; }
			set 
			{
				_filePath = value;
				RaisePropertyChanged("FilePath");
			}
		}

		string _createdFilePath;
		public string CreatedFilePath
		{
			get { return _createdFilePath; }
			set
			{
				_createdFilePath = value;
				RaisePropertyChanged("CreatedFilePath");
			}
		}


		private ObservableCollection<HistogramElement> _histogramData { get; set; }
		public ObservableCollection<HistogramElement> HistogramData
		{
			get { return _histogramData; }
			set
			{
				_histogramData = value;
				RaisePropertyChanged("HistogramData");
			}
		}



		ICommand _openFileCommand;
		public ICommand OpenFileCommand
		{
			get
			{
				return _openFileCommand = _openFileCommand ??
					new Command(
						p => Task.Factory
								.StartNew(() => IsUiBlocked = true)
								.ContinueWith(t => OpenAndCount())
								.ContinueWith(t =>
									{
										var ex = t.Exception.Flatten();
										var messages = ex.InnerExceptions
											.Select(it => it.Message)
											.Aggregate((m1, m2) => string.Join(Environment.NewLine, m1, m2));
										MessageBox.Show(messages, string.Empty);
									},
									TaskContinuationOptions.OnlyOnFaulted)
								.ContinueWith(t => IsUiBlocked = false ),
						p => !IsUiBlocked);
			}
		}

		ICommand _createFileCommand;
		public ICommand CreateFileCommand
		{
			get
			{
				return _createFileCommand = _createFileCommand ??
					new Command(
						p => Task.Factory
								.StartNew(() => IsUiBlocked = true)
								.ContinueWith(t => CreateFile())
								.ContinueWith(t =>
									{
										var ex = t.Exception.Flatten();
										var messages = ex.InnerExceptions
											.Select(it => it.Message)
											.Aggregate((m1, m2) => string.Join(Environment.NewLine, m1, m2));
										MessageBox.Show(messages, string.Empty);
									},
									TaskContinuationOptions.OnlyOnFaulted)
								.ContinueWith(t => IsUiBlocked = false ),
						p => !IsUiBlocked);
			}
		}


		private void OpenAndCount()
		{
			var openDialog = new OpenFileDialog();
			if (openDialog.ShowDialog() ?? false)
			{
				FilePath = openDialog.FileName;

				var histogram = CalcHistogram(FilePath);

				HistogramData = new ObservableCollection<HistogramElement>(
					histogram
					.OrderByDescending(it => it.Count)
					.Take(10)
					.OrderBy(it => it.Value));

				SaveHistogram(Path.ChangeExtension(openDialog.FileName, ".histogram"), histogram);

			}
		}

		private void CreateFile()
		{
			var generator = new NaturalNumbersGenerator();
			var sequence = generator
				.Generate(1, 100000)
				.Take(10000000);

			var fi = new FileInfo("numbers.txt");
			
			using (var f = File.Open(fi.FullName, FileMode.Create, FileAccess.Write))
			using (var sw = new StreamWriter(f))
				sequence.Foreach(sw.WriteLine);

			CreatedFilePath = fi.FullName;
		}

		/// <summary>
		/// сохраняет гистограмму
		/// </summary>
		private void SaveHistogram(string filepath, IList<HistogramElement> histogram)
		{
			using (var f = File.Open(filepath, FileMode.Create, FileAccess.Write))
			using (var sw = new StreamWriter(f))
				histogram.Foreach(item => 
					sw.WriteLine(item));
		}

		/// <summary>
		/// считает гистограмму по данным из файла
		/// </summary>
		private IList<HistogramElement> CalcHistogram(string filename)
		{
			var rawCounts = File.ReadLines(filename)
				.AsParallel()
				.Select(it => Int32.Parse(it, System.Globalization.NumberStyles.None))
				.Aggregate(
					() => new Dictionary<int, int>(),
					(acc, it) =>
					{
						if (!acc.ContainsKey(it))
							acc[it] = 0;
						acc[it] += 1;
						return acc;
					},
					(d1, d2) => d1.Concat(d2)
						.GroupBy(it => it.Key, it => it.Value)
						.ToDictionary(it => it.Key, it => it.Sum()),
					it => it);

			return rawCounts
				.OrderBy(it => it.Key)
				.Select(it => new HistogramElement()
				{
					Value = (it.Key),
					Count = it.Value
				})
				.ToList();
		}
	}
}
