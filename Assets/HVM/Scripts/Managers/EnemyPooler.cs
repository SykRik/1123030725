using System.Collections.Generic;

namespace HVM
{
	public class EnemyPooler : BasePooler<EnemyController>
	{
		public List<EnemyController> Enemies => liveItems;
	}
}