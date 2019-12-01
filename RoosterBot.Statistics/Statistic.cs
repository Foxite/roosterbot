namespace RoosterBot.Statistics {
	public abstract class Statistic {
		public Component NameKeyComponent { get; }
		public string NameKey { get; }
		public abstract int Count { get; }

		protected Statistic(Component nameKeyComponent, string nameKey) {
			NameKeyComponent = nameKeyComponent;
			NameKey = nameKey;
		}
	}
}
