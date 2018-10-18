using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot {
	public abstract class ComponentBase {
		/// <summary>
		/// Initializes the component.
		/// </summary>
		public abstract void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath);
	}
}
