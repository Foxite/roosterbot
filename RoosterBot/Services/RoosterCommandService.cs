﻿using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace RoosterBot {
	/// <summary>
	/// RoosterCommandService keeps track of executed commands and their replies.
	/// </summary>
	public sealed class RoosterCommandService : CommandService {
		private readonly ResourceService m_ResourceService;

		/// <param name="minimumMemorySeconds">How long it takes at least before old commands are deleted. Old commands are not deleted until a new one from the same user comes in.</param>
		internal RoosterCommandService(ResourceService resourceService) {
			m_ResourceService = resourceService;
		}

		internal bool IsMessageCommand(IMessage message, string prefix, out int argPos) {
			argPos = 0;
			if (message.Source == MessageSource.User &&
				message is IUserMessage userMessage &&
				message.Content.Length > prefix.Length &&
				userMessage.HasStringPrefix(prefix, ref argPos)) {
				// First char after prefix
				char firstChar = message.Content.Substring(prefix.Length)[0];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}

		public Task<ModuleInfo[]> AddLocalizedModuleAsync<T>() => AddLocalizedModuleAsync(typeof(T));
		public async Task<ModuleInfo[]> AddLocalizedModuleAsync(Type module) {
			// For each culture supported by the module, create a ModuleInfo from the type with resolved strings
			if (module.IsSubclassOf(typeof(RoosterModuleBase<>))) {
				throw new ArgumentException(module.Name + " must derive from RoosterModuleBase to support localization.");
			}

			IReadOnlyList<string> locales = module.GetCustomAttributes().OfType<LocalizedModuleAttribute>().Single().Locales;
			ComponentBase component = Program.Instance.Components.GetComponentFromAssembly(module.Assembly);
			ModuleInfo[] localizedModules = new ModuleInfo[locales.Count];

			for (int i = 0; i < locales.Count; i++) {
				string locale = locales[i];
				localizedModules[i] = await CreateModuleAsync("", GetModuleBuildFunction(module, component, locale));
			}

			return localizedModules;
		}

		private Action<ModuleBuilder> GetModuleBuildFunction(Type module, ComponentBase component, string locale) {
			return (moduleBuilder) => {
				// TODO (refactoring) this code is still pretty messy, take an example from how Discord.NET does it https://github.com/discord-net/Discord.Net/blob/dev/src/Discord.Net.Commands/Builders/ModuleClassBuilder.cs
				IEnumerable<(MethodInfo method, CommandAttribute attribute)> commands = module.GetMethods()
					.Where(method => method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<RuntimeResult>))
					.Select(method => (method: method, attribute: method.GetCustomAttribute<CommandAttribute>(false)))
					.Where(commandTuple => commandTuple.attribute != null);

				if (!commands.Any()) {
					throw new ArgumentException(module.Name + " does not have any suitable command methods.");
				}

				CultureInfo culture = CultureInfo.GetCultureInfo(locale);
				moduleBuilder.AddPrecondition(new RequireCultureAttribute(locale, true));

				foreach (Attribute attr in module.GetCustomAttributes()) {
					switch (attr) {
						case NameAttribute name:
							moduleBuilder.WithName(name.Text); // Don't resolve it here because we want to automatically add localized ones if there's no AliasAttribute
							break;                             // So we do it after the foreach loop
						case AliasAttribute alias:
							if (alias.Aliases.Length == 1 && alias.Aliases[0].StartsWith("#")) {
								moduleBuilder.AddAliases(m_ResourceService.ResolveString(culture, component, alias.Aliases[0]).Split('|'));
							} else {
								moduleBuilder.AddAliases(alias.Aliases);
							}
							break;
						case RemarksAttribute remarks:
							moduleBuilder.WithRemarks(m_ResourceService.ResolveString(culture, component, remarks.Text));
							break;
						case SummaryAttribute summary:
							moduleBuilder.WithSummary(m_ResourceService.ResolveString(culture, component, summary.Text));
							break;
						case GroupAttribute group:
							moduleBuilder.Group = m_ResourceService.ResolveString(culture, component, group.Prefix);
							break;
						case PreconditionAttribute precondition:
							moduleBuilder.AddPrecondition(precondition);
							break;
						default:
							moduleBuilder.AddAttributes(attr);
							break;
					}
				}

				if (!moduleBuilder.Aliases.Any() && moduleBuilder.Name.StartsWith("#")) {
					moduleBuilder.AddAliases(m_ResourceService.ResolveString(culture, component, moduleBuilder.Name + "_Aliases").Split('|'));
				}
				if (string.IsNullOrWhiteSpace(moduleBuilder.Name)) {
					moduleBuilder.Name = module.Name;
				}
				moduleBuilder.Name = m_ResourceService.ResolveString(culture, component, moduleBuilder.Name);

				foreach ((MethodInfo method, CommandAttribute commandAttribute) in commands) {
					string primaryAlias = m_ResourceService.ResolveString(culture, component, commandAttribute.Text);
					if (!string.IsNullOrWhiteSpace(moduleBuilder.Group)) {
						primaryAlias = (moduleBuilder.Group + " " + primaryAlias).Trim();
					}
					if (string.IsNullOrWhiteSpace(primaryAlias)) {
						primaryAlias = m_ResourceService.ResolveString(culture, component, method.Name);
					}
					moduleBuilder.AddCommand(
						primaryAlias,
						async (context, parameters, commandServices, command) => {
							// Command execution
							PropertyInfo[] properties = module.GetProperties();
							IRoosterModuleBase moduleInstance = (IRoosterModuleBase) Activator.CreateInstance(module);
							foreach (PropertyInfo prop in properties.Where(prop => prop.SetMethod != null && prop.SetMethod.IsPublic && !prop.SetMethod.IsAbstract)) {
								object service = commandServices.GetService(prop.PropertyType);
								prop.SetValue(moduleInstance, service);
							}

							module.GetProperty("Context").SetValue(moduleInstance, context);

							try {
								moduleInstance.BeforeExecuteInternal(command);

								Task task = method.Invoke(moduleInstance, parameters) as Task ?? Task.CompletedTask;
								await task;
							} finally {
								moduleInstance.AfterExecuteInternal(command);
								if (moduleInstance is IDisposable disposableModuleInstance) {
									disposableModuleInstance.Dispose();
								}
							}
						},
						GetCommandBuildFunction(method, commandAttribute, moduleBuilder, culture, component)
					);
				}

				// Submodule creation
				IEnumerable<Type> submodules = module.GetNestedTypes().Where(nestedClass => nestedClass.IsSubclassOf(typeof(ModuleBase<>)));

				foreach (Type submodule in submodules) {
					if (submodule.GetCustomAttribute<LocalizedModuleAttribute>() != null) {
						throw new ArgumentException("Submodules of localized modules can not be localized. They are always localized in the same culture as their parent.");
					}

					moduleBuilder.AddModule(submodule.GetCustomAttribute<NameAttribute>()?.Text ?? submodule.Name, GetModuleBuildFunction(submodule, component, locale));
				}
			};
		}

		private Action<CommandBuilder> GetCommandBuildFunction(MethodInfo method, CommandAttribute commandAttribute, ModuleBuilder moduleBuilder, CultureInfo culture, ComponentBase component) {
			return (commandBuilder) => {
				if (!string.IsNullOrWhiteSpace(commandAttribute.Text)) {
					commandBuilder.WithName(commandAttribute.Text);
				}

				foreach (Attribute attr in method.GetCustomAttributes()) {
					switch (attr) {
						case NameAttribute name:
							commandBuilder.Name = m_ResourceService.ResolveString(culture, component, name.Text);
							break;
						case AliasAttribute alias:
							string[] aliases;
							if (alias.Aliases.Length == 1 && alias.Aliases[0].StartsWith("#")) {
								aliases = m_ResourceService.ResolveString(culture, component, alias.Aliases[0]).Split('|');
							} else {
								aliases = alias.Aliases;
							}
							if (!string.IsNullOrWhiteSpace(moduleBuilder.Group)) {
								moduleBuilder.AddAliases(aliases.Select(aliasText => moduleBuilder.Group + " " + aliasText).ToArray());
							} else {
								moduleBuilder.AddAliases(aliases);
							}
							break;
						case PriorityAttribute priority:
							commandBuilder.WithPriority(priority.Priority);
							break;
						case PreconditionAttribute precondition:
							commandBuilder.AddPrecondition(precondition);
							break;
						case SummaryAttribute summary:
							commandBuilder.WithSummary(m_ResourceService.ResolveString(culture, component, summary.Text));
							break;
						case RemarksAttribute remarks:
							commandBuilder.WithRemarks(m_ResourceService.ResolveString(culture, component, remarks.Text));
							break;
						default:
							commandBuilder.AddAttributes(attr);
							break;
					}
				}

				if (!commandBuilder.Aliases.Any() && commandBuilder.Name.StartsWith("#")) {
					commandBuilder.AddAliases(m_ResourceService.ResolveString(culture, component, commandBuilder.Name + "_Aliases").Split('|'));
				}
				if (string.IsNullOrWhiteSpace(commandBuilder.Name)) {
					commandBuilder.Name = method.Name;
				}
				if (commandAttribute.IgnoreExtraArgs.HasValue) {
					commandBuilder.IgnoreExtraArgs = commandAttribute.IgnoreExtraArgs.Value;
				}

				commandBuilder.Name = m_ResourceService.ResolveString(culture, component, commandBuilder.Name);
				commandBuilder.RunMode = commandAttribute.RunMode;

				ParameterInfo[] parameters = method.GetParameters();
				foreach (var parameter in parameters) {
					string paramName = parameter.GetCustomAttribute<NameAttribute>()?.Text;
					if (paramName == null) {
						paramName = parameter.Name;
					} else {
						paramName = m_ResourceService.ResolveString(culture, component, paramName);
					}

					commandBuilder.AddParameter(
						m_ResourceService.ResolveString(culture, component, paramName),
						parameter.ParameterType,
						GetParamBuildFunction(parameter, culture, component)
					);
				}
			};
		}

		private Action<ParameterBuilder> GetParamBuildFunction(ParameterInfo parameter, CultureInfo culture, ComponentBase component) {
			return (paramBuilder) => {
				foreach (Attribute attr in parameter.GetCustomAttributes()) {
					switch (attr) {
						case SummaryAttribute summary:
							paramBuilder.Summary = m_ResourceService.ResolveString(culture, component, summary.Text);
							break;
						case ParameterPreconditionAttribute precondition:
							paramBuilder.AddPrecondition(precondition);
							break;
						case RemainderAttribute remainder:
							paramBuilder.IsRemainder = true;
							break;
						case ParamArrayAttribute _:
							paramBuilder.WithIsMultiple(true);
							break;
						default:
							paramBuilder.AddAttributes(attr);
							break;
					}
				}
				paramBuilder
					.WithDefault(parameter.DefaultValue)
					.WithIsOptional(parameter.HasDefaultValue);

				if (paramBuilder.TypeReader == null) {
					paramBuilder.TypeReader = TypeReaders[paramBuilder.ParameterType].FirstOrDefault();
				}
			};
		}
	}
}
