using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RollingAverage
{
	public class StatHistory
	{
		public class Level
		{
			public int Length { get; set; }
			public int Divisions { get; set; }

			public Level(int length, int divisions = -1)
			{
				Length = length;
				Divisions = divisions;
			}
		}

		class StatHistoryLevel
		{
			public readonly int Length;
			private readonly int Divisions;
			private int Counter;

			private Queue<double> Data = new Queue<double>();
			private RollingAverage DataAcc = null;

			public StatHistoryLevel NextPart = null;

			public StatHistoryLevel(Level config, StatHistoryLevel next = null)
			{
				Length = config.Length;
				Divisions = config.Divisions;
				NextPart = next;
				DataAcc = new RollingAverage(config.Length);
			}

			public void Put(double x)
			{
				Data.Enqueue(x);
				DataAcc.Put(x);


				// защита от неограниченного роста
				if (Data.Count > Length)
					Data.Dequeue();

				// когда заполнен кусок
				if (Counter > 0 && Counter % (Length / (Divisions)) == 0)
				{
					var minorAvgInput = DataAcc.Get();
					// есть следующий уровень
					if (NextPart != null)
					{
						// заталкиваем среднее в следующий уровень
						NextPart.Put(minorAvgInput);
					}
				}

				if (Counter >= Length)
					Counter = 0;
				Counter++;
			}

			public IEnumerable<double> GetData(int level)
			{
				return GetRawData(level);
			}

			private IEnumerable<double> GetRawData(int level)
			{
				if (level == 0)
					return Data.Reverse().ToList();
				else
					return NextPart.GetRawData(level - 1);
			}

			private double CommitedAverage(int level)
			{
				if (level == 0)
					return DataAcc.Get();
				else
					return NextPart.CommitedAverage(level - 1);
			}

			public double CurrentAverage(int level)
			{
				if (level == 0)
					return CommitedAverage(level);
				else
				{
					var result = CommitedAverage(0);
					var node = this;
					for (int i = 1; i <= level; i++)
					{
						var length = node.NextPart.Divisions;
						result = result / length +
							   +CommitedAverage(level) * (length - 1) / length;
						node = node.NextPart;
					}
					return result;
				}
			}
		}

		private AccumulatingAverage Average = new AccumulatingAverage();
		private StatHistoryLevel RootPart = null;
		public readonly int Levels = 0;

		public StatHistory(params Level[] levels)
		{
			Levels = levels.Length;

			StatHistoryLevel node = new StatHistoryLevel(levels[0]);
			RootPart = node;
			for (var i = 1; i < levels.Length; i++)
			{
				node.NextPart = new StatHistoryLevel(levels[i]);
				node = node.NextPart;
			}
		}

		public double[] CurrentAverage()
		{
			var result = new List<double>();
			for (var i = 0; i < Levels; i++)
			{
				result.Add(Math.Round(RootPart.CurrentAverage(i), 2));
			}
			return result.ToArray();
		}

		public void Put(double input)
		{
			RootPart.Put(input);
			Average.Put(input);
		}

		public IEnumerable<double> GetData(int level)
		{
			return RootPart.GetData(level);
		}

		public double LevelAverage(int level)
		{
			return RootPart.CurrentAverage(level);
		}

		public double TotalAverage()
		{
			return Average.Get();
		}

	}

}
