using Discord;
using Discord.Commands;
using System;
using System.Collections.Concurrent;
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
		/// <summary>
		/// Maps command IDs to CommandResponsePairs
		/// </summary>
		private readonly ConcurrentDictionary<ulong, CommandResponsePair> m_Messages;
		private readonly ConfigService m_Config;
		private readonly ResourceService m_ResourceService;

		/// <param name="minimumMemorySeconds">How long it takes at least before old commands are deleted. Old commands are not deleted until a new one from the same user comes in.</param>
		internal RoosterCommandService(ConfigService config, ResourceService resourceService) {
			m_Messages = new ConcurrentDictionary<ulong, CommandResponsePair>();
			m_Config = config;
			m_ResourceService = resourceService;
		}

		internal void AddResponse(IUserMessage userCommand, IUserMessage botResponse) {
			// Remove previous commands from this user if they are older than the minimum memory
			IEnumerable<ulong> oldCommandIds = m_Messages.Where((kvp) => {
				return (DateTime.Now - kvp.Value.Command.Timestamp.UtcDateTime).TotalSeconds > m_Config.MinimumMemorySeconds && kvp.Value.Command.Author.Id == userCommand.Author.Id;
			}).Select(kvp => kvp.Key);

			foreach (ulong commandId in oldCommandIds) {
				m_Messages.TryRemove(commandId, out CommandResponsePair unused);
			}

			IUserMessage[] newSequence = new IUserMessage[] { botResponse };
			m_Messages.AddOrUpdate(userCommand.Id, new CommandResponsePair(userCommand, newSequence), (key, crp) => {
				crp.Responses = crp.Responses.Concat(newSequence).ToArray();
				return crp;
			});
		}

		internal void ModifyResponse(ulong commandId, IUserMessage[] responses) {
			GetResponse(commandId).Responses = responses;
		}

		internal CommandResponsePair GetResponse(ulong userCommandId) {
			if (m_Messages.TryGetValue(userCommandId, out CommandResponsePair ret)) {
				return ret;
			} else {
				return null;
			}
		}

		internal bool RemoveCommand(ulong commandId, out CommandResponsePair crp) {
			return m_Messages.TryRemove(commandId, out crp);
		}

		internal CommandResponsePair GetResponse(IUserMessage userCommand) => GetResponse(userCommand.Id);
		internal bool RemoveCommand(IUserMessage command, out CommandResponsePair crp) => RemoveCommand(command.Id, out crp);
		internal void ModifyResponse(IUserMessage command, IUserMessage[] responses) => ModifyResponse(command.Id, responses);

		internal bool IsMessageCommand(IMessage message, out int argPos) {
			argPos = 0;
			if (message.Source == MessageSource.User &&
				message is IUserMessage userMessage &&
				message.Content.Length > m_Config.CommandPrefix.Length &&
				userMessage.HasStringPrefix(m_Config.CommandPrefix, ref argPos)) {
				// First char after prefix
				char firstChar = message.Content.Substring(m_Config.CommandPrefix.Length)[0];
				if ((firstChar >= 'A' && firstChar <= 'Z') || (firstChar >= 'a' && firstChar <= 'z')) {
					// Probably not meant as a command, but an expression (for example !!! or ?!, depending on the prefix used)
					return true;
				}
			}

			return false;
		}

		private async Task<ModuleInfo[]> AddLocalizedModuleInternalAsync(Type module, Assembly assembly) {
			// For each culture supported by the module, create a ModuleInfo from the type with resolved strings
			if (module.IsSubclassOf(typeof(RoosterModuleBase<>))) {
				throw new ArgumentException(module.Name + " must derive from RoosterModuleBase to support localization.");
			}

			IReadOnlyList<string> locales = module.GetCustomAttributes().OfType<LocalizedModuleAttribute>().Single().Locales;
			ComponentBase component = Program.Instance.Components.GetComponentFromAssembly(assembly);
			ModuleInfo[] localizedModules = new ModuleInfo[locales.Count];

			for (int i = 0; i < locales.Count; i++) {
				string locale = locales[i];
				CultureInfo culture = CultureInfo.GetCultureInfo(locale);

				IEnumerable<(MethodInfo method, CommandAttribute attribute)> commands = module.GetMethods()
					.Where(method => method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<RuntimeResult>))
					.Select(method => (method: method, attribute: method.GetCustomAttribute<CommandAttribute>(false)))
					.Where(commandTuple => commandTuple.attribute != null);

				if (!commands.Any()) {
					throw new ArgumentException(module.Name + " does not have any suitable command methods.");
				}

				localizedModules[i] = await CreateModuleAsync("", moduleBuilder => {
					// Module creation
					moduleBuilder.AddPrecondition(new RequireCultureAttribute(locale, true));

					string name = module.GetCustomAttribute<NameAttribute>()?.Text;
					if (name == null) {
						name = module.Name;
					} else {
						name = m_ResourceService.ResolveString(culture, component, name);
					}
					moduleBuilder.WithName(name);

					string[] aliases = module.GetCustomAttribute<AliasAttribute>()?.Aliases;
					if (aliases != null) {
						moduleBuilder.AddAliases(aliases);
					}

					string remarks = module.GetCustomAttribute<RemarksAttribute>()?.Text;
					if (remarks != null) {
						moduleBuilder.WithRemarks(remarks);
					}

					string summary = module.GetCustomAttribute<SummaryAttribute>()?.Text;
					if (summary != null) {
						moduleBuilder.WithSummary(summary);
					}

					moduleBuilder.AddAttributes(module.GetCustomAttributes().ToArray());

					foreach ((MethodInfo method, CommandAttribute attribute) in commands) {
						moduleBuilder.AddCommand(
							m_ResourceService.ResolveString(culture, component, attribute.Text),
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

									Task task = method.Invoke(moduleInstance, parameters) as Task ?? Task.Delay(0);
									await task;
								} finally {
									moduleInstance.AfterExecuteInternal(command);
									if (moduleInstance is IDisposable disposableModuleInstance) {
										disposableModuleInstance.Dispose();
									}
								}
							},
							(commandBuilder) => {
								// Command creation
								AliasAttribute aliasAttribute = method.GetCustomAttribute<AliasAttribute>(false);
								if (aliasAttribute != null) {
									string[] commandAliases = aliasAttribute.Aliases
										.Select(alias => m_ResourceService.ResolveString(culture, component, alias)).ToArray();
									commandBuilder.AddAliases(commandAliases);
								}

								commandBuilder.AddAttributes(method.GetCustomAttributes().ToArray());

								IEnumerable<PreconditionAttribute> commandPreconditions = method.GetCustomAttributes<PreconditionAttribute>();
								foreach (PreconditionAttribute precondition in commandPreconditions) {
									commandBuilder.AddPrecondition(precondition);
								}

								int? priority = method.GetCustomAttribute<PriorityAttribute>()?.Priority;
								if (priority != null) {
									commandBuilder.WithPriority(priority.Value);
								}

								commandBuilder
									.WithName(m_ResourceService.ResolveString(culture, component, attribute.Text))
									.WithRunMode(attribute.RunMode)
									.WithRemarks(method.GetCustomAttribute<RemarksAttribute>()?.Text)
									.WithSummary(method.GetCustomAttribute<SummaryAttribute>()?.Text);

								ParameterInfo[] parameters = method.GetParameters();
								foreach (var parameter in parameters) {
									// Parameter creation
									string paramName = parameter.GetCustomAttribute<NameAttribute>()?.Text;
									if (paramName == null) {
										paramName = parameter.Name;
									} else {
										paramName = m_ResourceService.ResolveString(culture, component, paramName);
									}

									commandBuilder.AddParameter(
										m_ResourceService.ResolveString(culture, component, paramName),
										parameter.ParameterType,
										(paramBuilder) => {
											paramBuilder
												.AddAttributes(parameter.GetCustomAttributes().ToArray())
												.WithSummary(parameter.GetCustomAttribute<SummaryAttribute>()?.Text)
												.WithIsRemainder(parameter.GetCustomAttribute<RemainderAttribute>() != null)
												.WithIsMultiple(parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
												.WithDefault(parameter.DefaultValue)
												.WithIsOptional(parameter.HasDefaultValue);

											IEnumerable<ParameterPreconditionAttribute> paramPreconditions = parameter.GetCustomAttributes<ParameterPreconditionAttribute>();
											foreach (ParameterPreconditionAttribute paramPrecondition in paramPreconditions) {
												paramBuilder.AddPrecondition(paramPrecondition);
											}

											if (paramBuilder.TypeReader == null) {
												paramBuilder.TypeReader = TypeReaders[paramBuilder.ParameterType].FirstOrDefault();
											}
										}
									);
								} // End parameter creation
							}
						); // End command creation
					}
				}); // End module creation
			} // End locales loop

			return localizedModules;
		}
		
		public Task<ModuleInfo[]> AddLocalizedModuleAsync<T>() => AddLocalizedModuleInternalAsync(typeof(T), Assembly.GetCallingAssembly());
		public Task<ModuleInfo[]> AddLocalizedModuleAsync(Type type) => AddLocalizedModuleInternalAsync(type, Assembly.GetCallingAssembly());
	}

	internal class CommandResponsePair {
		public IUserMessage Command;
		public IUserMessage[] Responses;

		public CommandResponsePair(IUserMessage command, IUserMessage[] responses) {
			Command = command;
			Responses = responses;
		}
	}
}
