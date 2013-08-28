using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveCache.App
{
	class ThreadReader
	{

	}

	class Program
	{
		static int count = 10000;
		static object[] GetData()
		{
			if (count <= 0)
				return null;

			var rng1 = new Random().Next(100);
			if (rng1 > 70)
			{
				var rng2 = new Random().Next(100);

				count -= rng2;
				return new[] { new Object() };
			}
			return null;
		}

		static void Main(string[] args)
		{
			var rand = new Random();
			Action<int> loooongJob = (tt) =>
				{
					var rng = new Random().Next(50000000);

					if (rng < 50000000 / 2)
						throw new NotImplementedException("exception test");

					if (rng > 50000000 / 2)
						Enumerable.Range(0, rng)
							.Aggregate((a, b) => a + 1);
					else
						Thread.Sleep(rng/10000);
				};

			var queue = new PriorityQueue<QueueItem, DateTime>(it => it.NextStart);

			var queueItems = PrepareStates.Get()
				.Where(state => state.IsThreaded)
				.SelectMany(state => Enumerable.Repeat(state, state.ThreadCount))
				.Select((state, i) => 
					{
						var item = new QueueItem()
						{
							Id = i,
							Code = state.Code,
							Name = state.Name,

							StateId = state.Id,
							PreapareTypeId = state.PreapareTypeId,
							OrderType = state.OrderType,

							ExecutionSpan = state.ExecutionSpan,
							RecordCount = state.RecordCount,

							NextStart = DateTime.Now.AddMilliseconds(rand.Next(state.ExecutionSpan))
						};

						item.Execute = (data) => 
						{
							Console.WriteLine(string.Format("q:{6}\tstate:[{3}]{4}",
                                Thread.CurrentThread.Name,
								item.PreapareTypeId, item.OrderType, item.StateId, item.Name, item.Code.Substring(0, 8), queue.Size()));

							loooongJob(item.Id);

						};

						return item;
				})
				.ToList();

			queueItems.ForEach(it => queue.Enqueue(it));
			var tf = new TaskFactory();
			while (true)
			{
				var now = DateTime.Now;

				var item = queue.Dequeue();
				if (item == null)
				{
					Console.WriteLine("===EMPTY QUEUE");
					Thread.Sleep(5000);
					continue;
				}

				var startAfter = (int)(item.NextStart - now).TotalMilliseconds;

				if (startAfter > 0)
				{
					Console.WriteLine("===sleep " + startAfter);
					Thread.Sleep(startAfter);
				}

				var data = GetData();

				if (data != null)
				{
					tf.StartNew(() =>
						{
							try
							{
                                Thread.CurrentThread.Name = item.Name;
								item.Execute(null);
							}
							catch { }
							finally
							{
								var threadEnded = DateTime.Now;
								item.NextStart = threadEnded.AddMilliseconds(item.ExecutionSpan);
								queue.Enqueue(item);
							}
						});
				}
				else
				{
					item.NextStart = now.AddMilliseconds(item.ExecutionSpan);
					queue.Enqueue(item);
				}
			}
		}
	}
}
