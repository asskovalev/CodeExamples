using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActiveCache.App
{
	class PrepareState
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }

		public string OrderType { get; set; }
		public int PreapareTypeId { get; set; }

		public int ThreadCount { get; set; }
		public int RecordCount { get; set; }
		public int ExecutionSpan { get; set; }
		public int CheckSpan { get; set; }
		public bool IsThreaded { get; set; }
	}
}
