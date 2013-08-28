using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RollingAverage
{
	public class AccumulatingAverage
	{
		private double Average = 0;
		private int Length = 0;

		public double Put(double data)
		{
			Length++;
			Average -= Average / Length;
			Average += data / Length;
			return Average;
		}

		public double Get()
		{
			return Average;
		}

		public int Count
		{
			get { return Length; }
		}

		public void Reset()
		{
			Length = 0;
			Average = 0;
		}
	}

}
