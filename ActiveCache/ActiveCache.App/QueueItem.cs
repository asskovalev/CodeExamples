using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActiveCache.App
{
	class QueueItem
	{
		public int Id { get; set; }
		public int StateId { get; set; }
		public string Code { get; set; }
		public string Name { get; set; }

		public DateTime NextStart { get; set; }

		public Action<object[]> Execute { get; set; }

		public string OrderType { get; set; }
		public int PreapareTypeId { get; set; }

		public int RecordCount { get; set; }
		public int ExecutionSpan { get; set; }
	}
}
