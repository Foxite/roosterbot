using System;
using System.Collections.Generic;
using System.Text;

namespace RoosterBot.Statistics {
	public class ExternalStatistic : Statistic {
		private readonly Func<int> m_GetCount;

		public override int Count => m_GetCount();

		public ExternalStatistic(Func<int> getCount, Component nameKeyComponent, string nameKey) : base(nameKeyComponent, nameKey) {
			m_GetCount = getCount;
		}
	}
}
