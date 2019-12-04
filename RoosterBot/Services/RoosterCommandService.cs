using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using Qommon.Events;

namespace RoosterBot {
	/// <summary>
	/// This class wraps multiple Qmmands.CommandService objects, one for each supported culture.
	/// </summary>
	public sealed class RoosterCommandService {
		// Null CultureInfo represents unlocalized modules
		private readonly ConcurrentDictionary<CultureInfo, CommandService> m_ServicesByCulture;
		private readonly CommandService m_DefaultService;
		private readonly ResourceService m_ResourceService;
		private readonly CommandServiceConfiguration m_Config;

		// These events are copied from https://github.com/Quahu/Qmmands/blob/master/src/Qmmands/CommandService.cs
        private readonly AsynchronousEvent<CommandExecutedEventArgs> m_CommandExecuted = new AsynchronousEvent<CommandExecutedEventArgs>(GetEventErrorHandler(nameof(CommandExecuted)));
		private readonly AsynchronousEvent<CommandExecutionFailedEventArgs> m_CommandExecutionFailed = new AsynchronousEvent<CommandExecutionFailedEventArgs>(GetEventErrorHandler(nameof(CommandExecutionFailed)));
		
		/// <summary>
        /// Fires after a <see cref="Command"/> was successfully executed.
        /// You must use this to handle <see cref="RunMode.Parallel"/> <see cref="Command"/>s.
        /// </summary>
        public event AsynchronousEventHandler<CommandExecutedEventArgs> CommandExecuted {
            add => m_CommandExecuted.Hook(value);
            remove => m_CommandExecuted.Unhook(value);
        }

        /// <summary>
        /// Fires after a <see cref="Command"/> failed to execute.
        /// You must use this to handle <see cref="RunMode.Parallel"/> <see cref="Command"/>s.
        /// </summary>
        public event AsynchronousEventHandler<CommandExecutionFailedEventArgs> CommandExecutionFailed {
            add => m_CommandExecutionFailed.Hook(value);
            remove => m_CommandExecutionFailed.Unhook(value);
        }

		internal RoosterCommandService(ResourceService resourceService) {
			m_Config = new CommandServiceConfiguration() {
				// If you get an error here, see the comment in MultiWordCommandMap.
				CommandMap = new MultiWordCommandMap(" "),

				DefaultRunMode = RunMode.Sequential,
				CooldownBucketKeyGenerator = (objectType, context) => {
					if (context is RoosterCommandContext rcc) {
						if (objectType is CooldownType type) {
							return type switch
							{
								CooldownType.User => rcc.User.Id,
								CooldownType.Guild => rcc.GuildConfig.GuildId,
								CooldownType.ModuleUser => rcc.User.Id + "@" + rcc.Command.Module.FullAliases.First(),
								CooldownType.ModuleGuild => rcc.GuildConfig.GuildId + "@" + rcc.Command.Module.FullAliases.First(),
								CooldownType.ComponentUser => rcc.User.Id + "@" + Program.Instance.Components.GetComponentForModule(rcc.Command.Module).Name,
								CooldownType.ComponentGuild => rcc.GuildConfig.GuildId + "@" + Program.Instance.Components.GetComponentForModule(rcc.Command.Module).Name,
								_ => throw new ShouldNeverHappenException("Unknown CooldownType. This should never happen.")
							};
						} else {
							throw new NotSupportedException($"A command has issued a cooldown with an unknown type {objectType.GetType().FullName}. It must use {nameof(CooldownType)}.");
						}
					} else {
						throw new NotSupportedException($"A command has issued a cooldown with an unknown type {context.GetType().FullName}. It must use {nameof(RoosterCommandContext)}.");
					}
				}
			};
			
			m_ServicesByCulture = new ConcurrentDictionary<CultureInfo, CommandService>();
			m_ResourceService = resourceService;
			m_DefaultService = GetNewCommandService();
		}

		private CommandService GetService(CultureInfo? culture) {
			if (culture == null) {
				return m_DefaultService;
			} else {
				// A factory is better here because it won't call the function unless it's needed
				return m_ServicesByCulture.GetOrAdd(culture, c => GetNewCommandService() );
			}
		}

		private void AllServices(Action<CommandService> action) {
			action(m_DefaultService);
			foreach (CommandService service in m_ServicesByCulture.Select(kvp => kvp.Value)) {
				action(service);
			}
		}

		public async Task<IResult> ExecuteAsync(string input, RoosterCommandContext context) {
			IResult result = await GetService(context.Culture).ExecuteAsync(input, context);
			if (result is CommandNotFoundResult) {
				return await GetService(null).ExecuteAsync(input, context);
			} else {
				return result;
			}
		}

		public IReadOnlyList<Module> AddModule<T>(Action<ModuleBuilder>? postBuild = null) => AddModule(typeof(T), postBuild);
		public IReadOnlyList<Module> AddModule(Type moduleType, Action<ModuleBuilder>? postBuild = null) {
			Component component = Program.Instance.Components.GetComponentFromAssembly(moduleType.Assembly)!;
			ICollection<CultureInfo> locales = component.SupportedCultures;
			var localizedModules = new List<Module>(locales.Count);

			if (locales.Count == 0) {
				return new[] {
					GetService(null).AddModule(moduleType, (builder) => {
						if (builder.Commands.SelectMany(command => command.Attributes).OfType<RunModeAttribute>().Any()) {
							Logger.Info("RoosterCommandService", $"A command was found module `{builder.Name}` that has a RunMode attribute. " +
								"This is no longer necessary as commands are always executed off-thread.");
						}
						postBuild?.Invoke(builder);
					})
				};
			} else {
				foreach (CultureInfo culture in locales) {
					CommandService service = GetService(culture);

					localizedModules.Add(service.AddModule(moduleType, (module) => {
						LocalizeModule(module, culture, component);

						postBuild?.Invoke(module);
					}));
				}
				return localizedModules;
			}
		}

		private void LocalizeModule(ModuleBuilder module, CultureInfo culture, Component component) {
			string? resolveString(string? key) {
				return key == null ? null : m_ResourceService.ResolveString(culture, component, key);
			}

			module.AddCheck(new RequireCultureAttribute(culture.Name, true));

			foreach (RoosterTextAttribute rta in module.Attributes.OfType<RoosterTextAttribute>()) {
				rta.Text = resolveString(rta.Text)!;
			}

			module.Description = resolveString(module.Description);
			module.Remarks = resolveString(module.Remarks);
			module.Name = resolveString(module.Name);

			if (module.Aliases.Count > 0) {
				string aliasKey = module.Aliases.Single();
				module.Aliases.Remove(aliasKey);
				foreach (string alias in resolveString(aliasKey)!.Split('|')) {
					module.AddAlias(alias);
				}
			}

			foreach (CommandBuilder command in module.Commands) {
				if (command.Attributes.OfType<RunModeAttribute>().Any()) {
					Logger.Info("RoosterCommandService", $"The command `{command.Name}` in `{module.Name}` has a RunMode attribute. " +
						"This is no longer necessary as commands are always executed off-thread.");
				}

				foreach (RoosterTextAttribute rta in command.Attributes.OfType<RoosterTextAttribute>()) {
					rta.Text = resolveString(rta.Text)!;
				}

				if (command.Aliases.Count > 0) {
					string aliasKey = command.Aliases.Single();
					command.Aliases.Remove(aliasKey);
					foreach (string alias in resolveString(aliasKey)!.Split('|')) {
						command.AddAlias(alias);
					}
				}

				command.Description = resolveString(command.Description);
				command.Remarks = resolveString(command.Remarks);
				command.Name = resolveString(command.Name);

				foreach (ParameterBuilder parameter in command.Parameters) {
					foreach (RoosterTextAttribute rta in parameter.Attributes.OfType<RoosterTextAttribute>()) {
						rta.Text = resolveString(rta.Text)!;
					}

					parameter.Description = resolveString(parameter.Description);
					parameter.Remarks = resolveString(parameter.Remarks);
					parameter.Name = resolveString(parameter.Name);
				}
			}

			foreach (ModuleBuilder submodule in module.Submodules) {
				LocalizeModule(submodule, culture, component);
			}
		}

		#region CommandService wrappers
		public void AddArgumentParser(IArgumentParser ap) {
			AllServices((service) => service.AddArgumentParser(ap));
		}

		public void AddTypeParser<T>(TypeParser<T> parser, bool replacePrimitive = false) {
			AllServices((service) => service.AddTypeParser<T>(parser, replacePrimitive));
		}

		public IReadOnlyList<CommandMatch> FindCommands(CultureInfo? culture, string path) {
			return GetService(culture).FindCommands(path);
		}

		public IReadOnlyList<Command> GetAllCommands(CultureInfo? culture) {
			return GetService(culture).GetAllCommands();
		}

		public IReadOnlyList<Module> GetAllModules(CultureInfo? culture) {
			IEnumerable<Module> modules = GetService(culture).GetAllModules();
			if (culture != null) {
				modules = modules.Concat(GetService(null).GetAllModules());
			}
			return modules.ToList().AsReadOnly();
		}

		public IArgumentParser GetArgumentParser<T>() => GetArgumentParser(typeof(T));
		public IArgumentParser GetArgumentParser(Type type) {
			return GetService(null).GetArgumentParser(type);
		}

		public TParser GetSpecificTypeParser<T, TParser>() where TParser : TypeParser<T> {
			return GetService(null).GetSpecificTypeParser<T, TParser>();
		}

		public TypeParser<T> GetTypeParser<T>(bool replacingPrimitive = false) {
			return GetService(null).GetTypeParser<T>(replacingPrimitive);
		}

		public void RemoveAllModules() {
			AllServices((service) => service.RemoveAllModules());
		}

		public void RemoveAllTypeParsers() {
			AllServices((service) => service.RemoveAllTypeParsers());
		}

		public void RemoveArgumentParser<T>() => RemoveArgumentParser(typeof(T));
		public void RemoveArgumentParser(Type type) {
			AllServices((service) => service.RemoveArgumentParser(type));
		}

		public void RemoveTypeParser<T>(TypeParser<T> typeParser) {
			AllServices((service) => service.RemoveTypeParser(typeParser));
		}

		public void SetDefaultArgumentParser<T>() => SetDefaultArgumentParser(typeof(T));
		public void SetDefaultArgumentParser(Type type) {
			AllServices((service) => service.SetDefaultArgumentParser(type)) ;
		}

		public void SetDefaultArgumentParser(IArgumentParser parser) {
			AllServices((service) => service.SetDefaultArgumentParser(parser));
		}
		#endregion

		private static Func<Exception, Task> GetEventErrorHandler(string name) {
			string logMessage = $"A {name} handler has thrown an exception.";
			return e => Logger.LogSync(new Discord.LogMessage(Discord.LogSeverity.Error, "RoosterCommandService", logMessage, e));
		}

		private async Task HandleCommandExecutedAsync(CommandExecutedEventArgs args) {
			try {
				await m_CommandExecuted.InvokeAsync(args);
			} catch (Exception e) {
				Logger.Error("RoosterCommandService", "A CommandExecuted handler has thrown an exception.", e);
				throw;
			}
		}

		private async Task HandleCommandExecutionFailedAsync(CommandExecutionFailedEventArgs args) {
			try {
				await m_CommandExecutionFailed.InvokeAsync(args);
			} catch (Exception e) {
				Logger.Error("RoosterCommandService", "A CommandExecutionFailed handler has thrown an exception.", e);
				throw;
			}
		}

		private CommandService GetNewCommandService() {
			var ret = new CommandService(m_Config);

			ret.CommandExecuted += HandleCommandExecutedAsync;
			ret.CommandExecutionFailed += HandleCommandExecutionFailedAsync;

			return ret;
		}
	}
}
