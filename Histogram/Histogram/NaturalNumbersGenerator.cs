using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Histogram
{
	public class NaturalNumbersGenerator
	{
		private Random random = new Random();

		public IEnumerable<int> Generate(int minValue, int maxValue)
		{
			if (maxValue <= minValue)
				throw new ArgumentOutOfRangeException("minValue", "range's lowest bound should be greater than right one");

			while (true)
				yield return random.Next(maxValue - minValue + 1) + minValue;
		}
	}
}
