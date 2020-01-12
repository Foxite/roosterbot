namespace RoosterBot.Telegram {
	public abstract class TelegramSnowflake : ISnowflake {
		public PlatformComponent Platform => TelegramComponent.Instance;

		object ISnowflake.Id => throw new System.NotImplementedException();
	}
}