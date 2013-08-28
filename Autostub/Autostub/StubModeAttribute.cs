using System;
using Autostub.Entity;

namespace Autostub
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class StubModeAttribute : Attribute
	{
		public StubModeType Mode { get; private set; }
		public StubModeAttribute(StubModeType mode)
		{
			Mode = mode;
		}
	}
}