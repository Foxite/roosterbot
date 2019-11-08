using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class MultiReader : RoosterTypeReader {
		private List<RoosterTypeReader> m_Readers;
		private readonly string m_ErrorMessage;
		private readonly ComponentBase m_ResourcesComponent;

		/// <summary>
		/// Allows multiple type readers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if all readers return ParseFailed. If any reader returns another error, then that ErrorReason will be used.</param>
		/// <param name="resourcesComponent">The component to be used when resolving the error message.</param>
		public MultiReader(string errorMessage, ComponentBase resourcesComponent) {
			m_Readers = new List<RoosterTypeReader>();
			m_ErrorMessage = errorMessage;
			m_ResourcesComponent = resourcesComponent;
		}

		internal void AddReader(RoosterTypeReader reader) {
			m_Readers.Add(reader);
		}

		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			foreach (RoosterTypeReader reader in m_Readers) {
				TypeReaderResult result = await reader.ReadAsync(context, input, services);
				if (result.IsSuccess) {
					return result;
				} else if (result.Error != CommandError.ParseFailed) {
					return TypeReaderResult.FromError(CommandError.ParseFailed, result.ErrorReason);
				}
			}
			ResourceService resources = services.GetService<ResourceService>();
			CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild ?? context.UserGuild))!.Culture;
			return TypeReaderResult.FromError(CommandError.ParseFailed, resources.ResolveString(culture, m_ResourcesComponent, m_ErrorMessage));
		}
	}
}
