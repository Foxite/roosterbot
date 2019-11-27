using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	// TODO (refactor) I really hate the Qmmands version of this class, find a better way to do this under Qmmands without using dynamic all over the place
	// I've had this problem before where I needed to have a generic type for a variable but didn't have anything to give as a parameter. I didn't need to use any of the members
	//  that had any generic type in them, it was like having a List<T> but only caring about it's Count property.
	// I think this should totally be possible but I don't know any way to do it, other than using dynamic typing.
	public class MultiParser<T> : RoosterTypeParser<T> {
		private readonly List<dynamic> m_Readers;
		private readonly string m_ErrorMessage;
		private readonly ComponentBase m_ResourcesComponent;

		public override string TypeDisplayName { get; }

		/// <summary>
		/// Allows multiple type readers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if all readers return ParseFailed. If any reader returns another error, then that ErrorReason will be used.</param>
		/// <param name="resourcesComponent">The component to be used when resolving the error message.</param>
		public MultiParser(string errorMessage, string typeDisplayName, ComponentBase resourcesComponent) {
			m_Readers = new List<dynamic>();
			TypeDisplayName = typeDisplayName;
			m_ErrorMessage = errorMessage;
			m_ResourcesComponent = resourcesComponent;
		}

		internal void AddReader<TReader>(RoosterTypeParser<TReader> reader) {
			m_Readers.Add((dynamic) reader);
		}

		protected async override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			foreach (dynamic reader in m_Readers) {
				// This is a TypeParserResult<extends T>
				dynamic result = await reader.ParseAsync(parameter, value, context);
				if (result.IsSuccessful) {
					return TypeParserResult<T>.Successful((T) result.Value);
				}
				/* TODO (feature) If failure reason is not standard
				else if ( ... ) {
					return TypeParserResult<T>.Unsuccessful(result.Reason);
				}*/
			}
			ResourceService resources = context.ServiceProvider.GetService<ResourceService>();
			return TypeParserResult<T>.Unsuccessful(resources.ResolveString(context.Culture, m_ResourcesComponent, m_ErrorMessage));
		}
	}
}
