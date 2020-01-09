using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	// If you get an error that you can't find ICommandMap, add this to your nuget sources:
	// https://www.myget.org/F/foxite/api/v3/index.json 
	// Then delete Qmmands from your nuget cache (usually located at ~/.nuget/packages/qmmands, or you can clear your entire cache) and do a package restore.
	// 
	// That feed contains a fork of Qmmands which lets you provide your own command map. The builtin command map does not support spaces in command/module aliases, and the library owner
	//  does not want to support custom command maps nor spaces in aliases.
	internal class MultiWordCommandMap : ICommandMap {
		private readonly string m_Separator;
		private readonly List<Command> m_Commands;

		// TODO (test) Other separators
		// It should totally work but test it anyway
		internal MultiWordCommandMap(string separator = " ") {
			m_Commands = new List<Command>();
			m_Separator = separator;
		}

		public IReadOnlyList<CommandMatch> FindCommands(string input) {
			var matches = new ConcurrentBag<CommandMatch>();
			var checkedModules = new ConcurrentDictionary<Module, bool>();

			// This might be faster if we only keep a list of top level modules, and iterate over that, recursively checking the submodules.
			Parallel.ForEach(m_Commands, command => { // This will run for quite a lot of iterations and is pretty heavy, so making it fast is important.
				var checkModules = new Stack<Module>(4);
				checkModules.Push(command.Module);
				bool proceed = true;
				while (checkModules.Peek().Parent != null) { // Typically no more than two iterations, almost never 4. The initial capacity of the stack is 4
					if (checkedModules.TryGetValue(checkModules.Peek(), out proceed)) {
						break;
					}

					checkModules.Push(checkModules.Peek().Parent);
				}

				if (!proceed) {
					return;
				}

				bool match = false;
				var path = new List<string>();
				ReadOnlySpan<char> remainingInput = input.AsSpan();
				while (checkModules.TryPop(out Module? check)) { // See above, this generally does not loop more than twice
					if (check.Aliases.Count == 0) {
						match = true;
						continue;
					} else {
						foreach (string alias in check.Aliases) {
							if (remainingInput.StartsWith(alias, StringComparison.InvariantCultureIgnoreCase)) {
								remainingInput = remainingInput.Slice(alias.Length);
								path.Add(alias);
								match = true;
								break;
							}
						}
					}

					if (match) {
						ReadOnlySpan<char> trimmedRemainingInput = remainingInput.TrimStart(m_Separator);
						if (trimmedRemainingInput != remainingInput) {
							remainingInput = trimmedRemainingInput;
							checkedModules.TryAdd(check, true);
						} else {
							match = false;
							checkedModules.TryAdd(check, false);
							break;
						}
					} else {
						checkedModules.TryAdd(check, false);
					}
				}
				if (match) {
					if (command.Aliases.Count > 0) {
						foreach (string alias in command.Aliases) {
							if (remainingInput.StartsWith(alias, StringComparison.InvariantCultureIgnoreCase)) {
								remainingInput = remainingInput.Slice(alias.Length);
								if (remainingInput.Length == 0 || remainingInput.StartsWith(m_Separator, StringComparison.InvariantCultureIgnoreCase)) {
									matches.Add(new CommandMatch(command, alias, path.AsReadOnly(), new string(remainingInput.TrimStart(m_Separator))));
									break;
								}
							}
						}
					} else {
						matches.Add(new CommandMatch(command, string.Empty, path.AsReadOnly(), new string(remainingInput.TrimStart(m_Separator))));
					}
				}
			});
			return matches.ToArray();
		}

		public void MapModule(Module module) {
			m_Commands.AddRange(module.Commands);
			foreach (Module sub in module.Submodules) {
				MapModule(sub);
			}
		}

		public void UnmapModule(Module module) {
			m_Commands.RemoveAll(command => command.Module == module);
			foreach (Module sub in module.Submodules) {
				UnmapModule(sub);
			}
		}
	}
}