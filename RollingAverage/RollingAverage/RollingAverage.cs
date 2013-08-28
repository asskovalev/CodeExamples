using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RollingAverage
{
	public class RollingAverage
	{
		private double Average = 0;
		private readonly int MaxLength = 0;
		Queue<double> Snapshot = new Queue<double>();

		public RollingAverage(int maxLength)
		{
			MaxLength = maxLength;
		}

		public double Put(double x)
		{
			Snapshot.Enqueue(x);

			var isFull = Snapshot.Count > MaxLength;
			var decrement = isFull
				? Snapshot.Dequeue()
				: Average;

			if (isFull)
				Average -= decrement / MaxLength;

			Average += x / MaxLength;

			return Average;
		}

		public double Get()
		{
			return Average;
		}

		public int Count
		{
			get { return Snapshot.Count; }
		}

		public void Reset()
		{
			Snapshot.Clear();
			Average = 0;
		}
	}

}
