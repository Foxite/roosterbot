using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
		private readonly ConcurrentDictionary<CultureKey, CommandService> m_ServicesByCulture;
		private readonly ResourceService m_ResourceService;
		private readonly CommandServiceConfiguration m_Config;

		// These events are copied from https://github.com/Quahu/Qmmands/blob/master/src/Qmmands/CommandService.cs
        private readonly AsynchronousEvent<CommandExecutedEventArgs> m_CommandExecuted = new AsynchronousEvent<CommandExecutedEventArgs>();
        private readonly AsynchronousEvent<CommandExecutionFailedEventArgs> m_CommandExecutionFailed = new AsynchronousEvent<CommandExecutionFailedEventArgs>();
		
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

		internal RoosterCommandService(ResourceService resourceService) : this(resourceService, new CommandServiceConfiguration()) { }

		internal RoosterCommandService(ResourceService resourceService, CommandServiceConfiguration config) {
			m_ServicesByCulture = new ConcurrentDictionary<CultureKey, CommandService>();
			m_ResourceService = resourceService;
			m_Config = config;
		}

		private CommandService GetService(CultureInfo? culture) => m_ServicesByCulture.GetOrAdd(culture, (c) => {
			var ret = new CommandService(m_Config);
			ret.CommandExecuted += async (args) => await m_CommandExecuted.InvokeAsync(args);
			ret.CommandExecutionFailed += async (args) => await m_CommandExecutionFailed.InvokeAsync(args);
			return ret;
		});

		private void AllServices(Action<CommandService> action) {
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

		public Module[] AddModule<T>(Action<ModuleBuilder>? postBuild = null) => AddModule(typeof(T), postBuild);
		public Module[] AddModule(Type moduleType, Action<ModuleBuilder>? postBuild = null) {
			object[] localizedAttributes = moduleType.GetCustomAttributes(typeof(LocalizedModuleAttribute), true);

			if (localizedAttributes.Length == 0) {
				return new[] {
					GetService(null).AddModule(moduleType, (builder) => {
						if (builder.Commands.SelectMany(command => command.Attributes).OfType<RunModeAttribute>().Any()) {
							Logger.Warning("RoosterCommandService", $"A command was found module `{builder.Name}` that has a RunMode attribute. " +
								"This is no longer necessary as commands are always executed off-thread. " +
								"Parallel commands cannot receive proper post-execution handling. It is highly recommended that you remove all RunMode attributes from your code.");
						}
						postBuild?.Invoke(builder);
					})
				};
			} else if (localizedAttributes.Length == 1) {
				Component? component = Program.Instance.Components.GetComponentFromAssembly(moduleType.Assembly);
				IReadOnlyList<string> locales = ((LocalizedModuleAttribute) localizedAttributes[0]).Locales;

				Module[] localizedModules = new Module[locales.Count];

				for (int i = 0; i < locales.Count; i++) {
					string locale = locales[i];
					var culture = CultureInfo.GetCultureInfo(locale);

					// A factory is more performant because it won't create a whole new service if it's not going to be used
					CommandService service = GetService(culture);

					string? resolveString(string? key) {
						return key == null ? null : m_ResourceService.ResolveString(culture, component, key);
					}

					localizedModules[i] = service.AddModule(moduleType, (module) => {
						// TODO (review) Is this function called for all submodules of the {moduleType} we're adding?
						// Otherwise we need to foreach module.Submodules
						module.AddCheck(new RequireCultureAttribute(locale, true));

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
								Logger.Warning("RoosterCommandService", $"A command was found module `{module.Name}` that has a RunMode attribute. " +
									"This is no longer necessary as commands are always executed off-thread.");
							}

							foreach (RoosterTextAttribute rta in module.Attributes.OfType<RoosterTextAttribute>()) {
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
								foreach (RoosterTextAttribute rta in module.Attributes.OfType<RoosterTextAttribute>()) {
									rta.Text = resolveString(rta.Text)!;
								}

								parameter.Description = resolveString(parameter.Description);
								parameter.Remarks = resolveString(parameter.Remarks);
								parameter.Name = resolveString(parameter.Name);
							}
						}

						postBuild?.Invoke(module);
					});
				}
				return localizedModules;
			} else {
				throw new ArgumentException("Module class " + moduleType.FullName + " can not be localized because it does not have " + nameof(LocalizedModuleAttribute));
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

		/// <summary>
		/// A non-nullable wrapper for a nullable CultureInfo that we can use as a dictionary key.
		/// </summary>
		[DebuggerDisplay("{ToString()}")]
		private struct CultureKey : IEquatable<CultureKey>, IEquatable<CultureInfo> {
			public CultureInfo? Culture { get; }

			public CultureKey(CultureInfo? culture) {
				Culture = culture;
			}

			public override bool Equals(object? obj) {
				if (obj != null && obj is CultureKey key) {
					return Equals(key);
				} else {
					return false;
				}
			}

			public override int GetHashCode() => Culture?.GetHashCode() ?? 0;

			public bool Equals(CultureInfo? other) => Culture == other;
			public bool Equals(CultureKey other) =>   Culture == other.Culture;

			public static bool operator ==(CultureKey   left, CultureInfo? right) => left .Culture == right;
			public static bool operator ==(CultureInfo? left, CultureKey right)   => right.Culture == left;
			public static bool operator ==(CultureKey   left, CultureKey right)   => left.Equals(right);

			public static bool operator !=(CultureKey   left, CultureInfo? right) => !(left == right);
			public static bool operator !=(CultureInfo? left, CultureKey right)   => !(left == right);
			public static bool operator !=(CultureKey   left, CultureKey right)   => !(left == right);
			
			public static implicit operator CultureInfo?(CultureKey key) => key.Culture;
			public static implicit operator CultureKey(CultureInfo? info) => new CultureKey(info);

			public override string ToString() => Culture?.ToString() ?? "null";
		}
	}
}
