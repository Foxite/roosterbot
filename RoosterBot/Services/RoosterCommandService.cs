using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
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
		
		public event AsynchronousEventHandler<CommandExecutedEventArgs>? CommandExecuted;
		public event AsynchronousEventHandler<CommandExecutionFailedEventArgs>? CommandExecutionFailed;

		internal RoosterCommandService(ResourceService resourceService) : this(resourceService, new CommandServiceConfiguration()) { }

		internal RoosterCommandService(ResourceService resourceService, CommandServiceConfiguration config) {
			m_ServicesByCulture = new ConcurrentDictionary<CultureKey, CommandService>();
			m_ResourceService = resourceService;
			m_Config = config;
		}

		// TODO (refactor) Move this to a Util file
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

		private CommandService GetService(CultureInfo? culture) => m_ServicesByCulture.GetOrAdd(culture, (c) => {
			var ret = new CommandService(m_Config);
			ret.CommandExecuted += (args) => CommandExecuted?.Invoke(args) ?? Task.CompletedTask;
			ret.CommandExecutionFailed += (args) => CommandExecutionFailed?.Invoke(args) ?? Task.CompletedTask;
			return ret;
		});

		private void AllServices(Action<CommandService> action) {
			foreach (CommandService service in m_ServicesByCulture.Select(kvp => kvp.Value)) {
				action(service);
			}
		}

		public async Task<IResult> ExecuteAsync(string input, RoosterCommandContext context) {
			IResult result = await GetService(context.Culture).ExecuteAsync(input, context);
			if (result.IsSuccessful) {
				return result;
			} else {
				return await GetService(null).ExecuteAsync(input, context);
			}
		}

		public Module[] AddModule<T>(Action<ModuleBuilder>? postBuild = null) => AddModule(typeof(T), postBuild);
		public Module[] AddModule(Type moduleType, Action<ModuleBuilder>? postBuild = null) {
			object[] localizedAttributes = moduleType.GetCustomAttributes(typeof(LocalizedModuleAttribute), true);
			if (localizedAttributes.Length == 0) {
				return new[] { GetService(null).AddModule(moduleType, postBuild) };
			} else if (localizedAttributes.Length == 1) {
				ComponentBase? component = Program.Instance.Components.GetComponentFromAssembly(moduleType.Assembly);
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
