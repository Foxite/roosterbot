namespace RoosterBot {
	public enum CooldownType {
		/// <summary>
		/// Cooldown is specific to this user and shared by all commands.
		/// </summary>
		User,
		
		/// <summary>
		/// Cooldown is specific to this guild and shared by all commands.
		/// </summary>
		Channel,

		/// <summary>
		/// Cooldown is specific to this user and shared by all commands in your module.
		/// </summary>
		ModuleUser,

		/// <summary>
		/// Cooldown is specific to this guild and shared by all commands in your module.
		/// </summary>
		ModuleChannel,

		/// <summary>
		/// Cooldown is specific to this user shared by all commands in your component.
		/// </summary>
		ComponentUser,

		/// <summary>
		/// Cooldown is specific to this guild shared by all command in your component.
		/// </summary>
		ComponentChannel
	}
}
