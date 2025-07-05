using System.Collections.Generic;

namespace CompleteProject
{
	public class EnemyPooler : BasePooler<Enemy>
	{
		public List<Enemy> Enemies => liveItems;
	}
}