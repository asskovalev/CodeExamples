using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CallParser
{
	public class TreeLogItem : LogItem
	{
		public string [] Path { get; set; }
		public bool Unresolved { get; set; }
		public List<TreeLogItem> Children { get; set; }

		public TreeLogItem()
		{
			this.Children = new List<TreeLogItem>();
			this.Unresolved    = true;
		}

		public TreeLogItem(string id) : this()
		{
			this.Path = id.Split('.');
			this.Id = Path.Last();
		}

		public TreeLogItem(LogItem src) :this()
		{
			Unresolved = false;
			Duration   = src.Duration;
			Level      = src.Level;
			Method     = src.Method;
			Start      = src.Start;
		}
	}
}
