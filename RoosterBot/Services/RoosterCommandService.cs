using System;
using System.Collections.Generic;
using System.Globalization;
using Discord;
using Qmmands;

namespace RoosterBot {
	public sealed class RoosterCommandService : CommandService {
		private readonly ResourceService m_ResourceService;

		internal RoosterCommandService(ResourceService resourceService) {
			m_ResourceService = resourceService;
		}

		internal RoosterCommandService(ResourceService resourceService, CommandServiceConfiguration config) : base(config) {
			m_ResourceService = resourceService;
		}

		internal bool IsMessageCommand(IMessage message, string prefix, out int argPos) {
			argPos = 0;
			if (message.Content != null && // Message objects created for MessageUpdated events only contain what was modified. Content may be null in certain cases. https://github.com/discord-net/Discord.Net/issues/1409
				message.Source == MessageSource.User &&
				message is IUserMessage userMessage &&
				message.Content.Length > prefix.Length &&
				userMessage.Content.StartsWith(prefix)) {
				// First char after prefix
				char firstChar = message.Content.Substring(prefix.Length)[0];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}

		public Module[] AddLocalizedModule<T>() => AddLocalizedModule(typeof(T));
		public Module[] AddLocalizedModule(Type module) {
			object[] localizedAttributes = module.GetCustomAttributes(typeof(LocalizedModuleAttribute), true);
			if (localizedAttributes.Length == 1) {
				ComponentBase? component = Program.Instance.Components.GetComponentFromAssembly(module.Assembly);
				IReadOnlyList<string> locales = ((LocalizedModuleAttribute) localizedAttributes[0]).Locales;

				Module[] localizedModules = new Module[locales.Count];

				for (int i = 0; i < locales.Count; i++) {
					string locale = locales[i];
					var culture = CultureInfo.GetCultureInfo(locale);

					localizedModules[i] = AddModule(module, (builder) => {
						// TODO (refactor) Localize everything here, not just command names, and don't resolve strings when generating command signatures
						builder.AddCheck(new RequireCultureAttribute(locale, true));

						foreach (string alias in m_ResourceService.ResolveString(culture, component, builder.Name + "_Aliases").Split('|')) {
							builder.AddAlias(alias);
						}
						builder.Name = builder.Name == null ? builder.Name : m_ResourceService.ResolveString(culture, component, builder.Name);

						foreach (CommandBuilder command in builder.Commands) {
							foreach (string alias in m_ResourceService.ResolveString(culture, component, command.Name + "_Aliases").Split('|')) {
								command.AddAlias(alias);
							}
							command.Name = command.Name == null ? command.Name : m_ResourceService.ResolveString(culture, component, command.Name);
						}
					});
				}
				return localizedModules;
			} else {
				throw new ArgumentException("Module class " + module.FullName + " can not be localized because it does not have " + nameof(LocalizedModuleAttribute));
			}
		}
	}
}
