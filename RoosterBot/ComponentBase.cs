using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot {
	public abstract class ComponentBase {
		/// <summary>
		/// Initializes the component.
		/// </summary>
		/// <returns>true if initialization was successful. If false, the bot will not load any other components.</returns>
		public abstract bool Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath);
	}
}
