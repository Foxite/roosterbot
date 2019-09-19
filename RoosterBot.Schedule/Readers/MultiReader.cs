using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class MultiReader : RoosterTypeReaderBase {
		private IEnumerable<RoosterTypeReaderBase> m_Readers;
		private readonly string m_ErrorMessage;
		private readonly ComponentBase m_ResourcesComponent;

		/// <summary>
		/// Allows multiple type readers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if none of the readers are successful.</param>
		/// <param name="resourcesComponent">The component to be used when resolving the error message.</param>
		public MultiReader(IEnumerable<RoosterTypeReaderBase> readers, string errorMessage, ComponentBase resourcesComponent) {
			m_Readers = readers;
			m_ErrorMessage = errorMessage;
			m_ResourcesComponent = resourcesComponent;
		}

		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			foreach (RoosterTypeReaderBase reader in m_Readers) {
				TypeReaderResult result = await reader.ReadAsync(context, input, services);
				if (result.IsSuccess) {
					return result;
				}
			}
			ResourceService resources = services.GetService<ResourceService>();
			return TypeReaderResult.FromError(CommandError.ParseFailed, resources.ResolveString(context, m_ResourcesComponent, m_ErrorMessage));
		}
	}
}
