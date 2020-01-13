namespace RoosterBot.Telegram {
	public abstract class TelegramSnowflake : ISnowflake {
		public PlatformComponent Platform => TelegramComponent.Instance;

		public abstract long Id { get; }

		object ISnowflake.Id => Id;
	}
}