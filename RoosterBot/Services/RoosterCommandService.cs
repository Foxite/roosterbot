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
		private readonly ConcurrentDictionary<CultureInfo, CommandService> m_ServicesByCulture;
		private readonly CommandService m_DefaultService;
		private readonly ResourceService m_ResourceService;
		private readonly CommandServiceConfiguration m_Config;
		private readonly List<Action<CommandService>> m_SetupActions;

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
			m_SetupActions = new List<Action<CommandService>>();

			m_Config = new CommandServiceConfiguration() {
				DefaultRunMode = RunMode.Sequential,
				CooldownBucketKeyGenerator = (objectType, context) => {
					if (context is RoosterCommandContext rcc) {
						if (objectType is CooldownType type) {
							return type switch
							{
								CooldownType.User => rcc.User.GetReference().ToString(),
								CooldownType.Channel => rcc.ChannelConfig.ChannelReference.ToString(),
								CooldownType.ModuleUser => rcc.User.GetReference().ToString() + "@" + rcc.Command.Module.FullAliases.First(),
								CooldownType.ModuleChannel => rcc.ChannelConfig.ChannelReference.ToString() + "@" + rcc.Command.Module.FullAliases.First(),
								CooldownType.ComponentUser => rcc.User.GetReference().ToString() + "@" + Program.Instance.Components.GetComponentForModule(rcc.Command.Module).Name,
								CooldownType.ComponentChannel => rcc.ChannelConfig.ChannelReference.ToString() + "@" + Program.Instance.Components.GetComponentForModule(rcc.Command.Module).Name,
								_ => type.ToString()
							};
						} else {
							// This stuff shouldn't happen anymore with the added type safety of our wrapper functions, but we still have to return something or throw.
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

			AddTypeParser(new PlatformSpecificParser<IUser>("#UserParser_TypeDisplayName"));
			AddTypeParser(new PlatformSpecificParser<IChannel>("#ChannelParser_TypeDisplayName"));
			AddTypeParser(new PlatformSpecificParser<IMessage>("#MessageParser_TypeDisplayName"));
		}

		internal async Task<IResult> ExecuteAsync(string input, RoosterCommandContext context) {
			IResult result = await GetService(context.Culture).ExecuteAsync(input, context);
			if (result is CommandNotFoundResult ||
				(result is ChecksFailedResult cfr && cfr.FailedChecks.Count == 1 && cfr.FailedChecks.First().Check is RequireCultureAttribute rca && rca.Hide)) {
				return await GetService(null).ExecuteAsync(input, context);
			} else {
				return result;
			}
		}

		/// <summary>
		/// Get the <see cref="PlatformSpecificParser{T}"/> for <typeparamref name="T"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">If no <see cref="PlatformSpecificParser{T}"/> has been registered for <typeparamref name="T"/>.</exception>
		public PlatformSpecificParser<T> GetPlatformSpecificParser<T>() where T : ISnowflake {
			PlatformSpecificParser<T>? psp = GetSpecificTypeParser<T, PlatformSpecificParser<T>>();
			if (psp == null) {
				throw new InvalidOperationException("A PlatformSpecificParser has not been registered for type " + typeof(T).FullName + ". This shouldn't happen if you're using IUser, IChannel, or IMessage.");
			}
			return psp;
		}

		#region AddModule and LocalizeModule
		/// <summary>
		/// Add a module.
		/// </summary>
		/// <param name="postBuild">Delegate that modifies the module before it gets added to the RoosterCommandService.</param>
		/// <typeparam name="T">Must be <see cref="RoosterModule{T}"/>.</typeparam>
		// Can't really have proper type checking here because RoosterModule is generic. We can still do it but we would have to add a second parameter
		//  just for the context type which I think is undesirable.
		public IReadOnlyList<Module> AddModule<T>(Action<ModuleBuilder>? postBuild = null) => AddModule(typeof(T), postBuild);

		/// <summary>
		/// Add a module.
		/// </summary>
		/// <param name="moduleType">The <see cref="Type"/> of module to add.</param>
		/// <param name="postBuild">Delegate that modifies the module before it gets added to the RoosterCommandService.</param>
		public IReadOnlyList<Module> AddModule(Type moduleType, Action<ModuleBuilder>? postBuild = null) {
			if (!IsRoosterModule(moduleType)) {
				throw new ArgumentException("Modules must derive from RoosterModule<T>.");
			}

			Component component = Program.Instance.Components.GetComponentFromAssembly(moduleType.Assembly)!;
			IReadOnlyCollection<CultureInfo> cultures = ResourceService.GetAvailableCultures(component);
			if (cultures.Count == 0) {
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
				var localizedModules = new List<Module>(cultures.Count);
				foreach (CultureInfo culture in cultures) {
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
				string aliasKey = module.Aliases.First();
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
					string aliasKey = command.Aliases.First();
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
		#endregion

		#region CommandService wrappers
		/// <summary>
		/// Add an <see cref="IArgumentParser"/>.
		/// </summary>
		public void AddArgumentParser(IArgumentParser ap) {
			AllServices((service) => service.AddArgumentParser(ap));
		}

		/// <summary>
		/// Add a <see cref="RoosterTypeParser{T}"/>.
		/// </summary>
		/// <param name="parser">The parser to add.</param>
		/// <param name="replacePrimitive">Should this parser replace the built-in parser for <typeparamref name="T"/>.</param>
		public void AddTypeParser<T>(RoosterTypeParser<T> parser, bool replacePrimitive = false) {
			AllServices((service) => service.AddTypeParser(parser, replacePrimitive));
		}

		/// <summary>
		/// Find all commands matching a <paramref name="path"/>.
		/// </summary>
		public IReadOnlyList<CommandMatch> FindCommands(CultureInfo? culture, string path) {
			return GetService(culture).FindCommands(path);
		}

		/// <summary>
		/// Get all commands for a culture.
		/// </summary>
		public IReadOnlyList<Command> GetAllCommands(CultureInfo? culture) {
			return GetService(culture).GetAllCommands();
		}

		/// <summary>
		/// Get all modules for a culture.
		/// </summary>
		public IReadOnlyList<Module> GetAllModules(CultureInfo? culture) {
			IEnumerable<Module> modules = GetService(culture).GetAllModules();
			if (culture != null) {
				modules = modules.Concat(GetService(null).GetAllModules());
			}
			return modules.ToList().AsReadOnly();
		}

		/// <summary>
		/// Get an <see cref="IArgumentParser"/>.
		/// </summary>
		public IArgumentParser? GetArgumentParser<T>() where T : IArgumentParser => GetArgumentParser(typeof(T));

		/// <summary>
		/// Get an <see cref="IArgumentParser"/>.
		/// </summary>
		public IArgumentParser? GetArgumentParser(Type type) {
			return GetService(null).GetArgumentParser(type);
		}

		/// <summary>
		/// Get a specific <see cref="RoosterTypeParser{T}"/>.
		/// </summary>
		public TParser? GetSpecificTypeParser<T, TParser>() where TParser : RoosterTypeParser<T> {
			return GetService(null).GetSpecificTypeParser<T, TParser>();
		}

		/// <summary>
		/// Get a <see cref="RoosterTypeParser{T}"/> for a specific <typeparamref name="T"/>.
		/// </summary>
		public RoosterTypeParser<T>? GetTypeParser<T>(bool replacingPrimitive = false) {
			return GetService(null).GetTypeParser<T>(replacingPrimitive) as RoosterTypeParser<T>;
		}

		/// <summary>
		/// Remove an <see cref="IArgumentParser"/>.
		/// </summary>
		public void RemoveArgumentParser<T>() where T : IArgumentParser => RemoveArgumentParser(typeof(T));

		/// <summary>
		/// Remove an <see cref="IArgumentParser"/>.
		/// </summary>
		public void RemoveArgumentParser(Type type) {
			AllServices((service) => service.RemoveArgumentParser(type));
		}

		/// <summary>
		/// Remove a <see cref="RoosterTypeParser{T}"/>.
		/// </summary>
		public void RemoveTypeParser<T>(RoosterTypeParser<T> typeParser) {
			AllServices((service) => service.RemoveTypeParser(typeParser));
		}

		/// <summary>
		/// Set a <see cref="IArgumentParser"/> as the default.
		/// </summary>
		public void SetDefaultArgumentParser<T>() where T : IArgumentParser => SetDefaultArgumentParser(typeof(T));

		/// <summary>
		/// Set a <see cref="IArgumentParser"/> as the default.
		/// </summary>
		public void SetDefaultArgumentParser(Type type) {
			if (typeof(IArgumentParser).IsAssignableFrom(type)) {
				AllServices((service) => service.SetDefaultArgumentParser(type));
			} else {
				throw new ArgumentException("Argument parsers must derive from " + nameof(IArgumentParser) + ".");
			}
		}

		/// <summary>
		/// Set a <see cref="IArgumentParser"/> as the default.
		/// </summary>
		public void SetDefaultArgumentParser(IArgumentParser parser) {
			AllServices((service) => service.SetDefaultArgumentParser(parser));

		}
		#endregion

		#region Private methods
		private static Func<Exception, Task> GetEventErrorHandler(string name) {
			string logMessage = $"A {name} handler has thrown an exception.";
			return e => {
				Logger.Error("RoosterCommandService", logMessage, e);
				return Task.CompletedTask;
			};
		}

		private CommandService GetService(CultureInfo? culture) {
			if (culture == null) {
				return m_DefaultService;
			} else {
				// A factory is better here because it won't call the function unless it's needed
				return m_ServicesByCulture.GetOrAdd(culture, c => GetNewCommandService());
			}
		}

		private void AllServices(Action<CommandService> action) {
			action(m_DefaultService);
			foreach (CommandService service in m_ServicesByCulture.Values) {
				action(service);
			}
			m_SetupActions.Add(action);
		}

		private CommandService GetNewCommandService() {
			// If you get an error here, see the comment in MultiWordCommandMap.
			m_Config.CommandMap = new MultiWordCommandMap(" "); // Have to create a new instance every time, otherwise the map will be shared between all command services
			var ret = new CommandService(m_Config);

			ret.CommandExecuted += async (args) => await m_CommandExecuted.InvokeAsync(args);
			ret.CommandExecutionFailed += async (args) => await m_CommandExecutionFailed.InvokeAsync(args);

			foreach (Action<CommandService> action in m_SetupActions) {
				action(ret);
			}

			return ret;
		}

		private bool IsRoosterModule(Type objectType) {
			while (objectType.BaseType != null) {
				if (objectType.BaseType.IsGenericType && typeof(RoosterModule<>).Equals(objectType.BaseType.GetGenericTypeDefinition())) {
					return true;
				} else {
					objectType = objectType.BaseType;
				}
			}
			return false;
		}
		#endregion
	}
}
