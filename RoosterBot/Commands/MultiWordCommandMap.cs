using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	// If you get an error that you can't find ICommandMap, add this to your nuget sources:
	// https://www.myget.org/F/foxite/api/v3/index.json 
	// That feed contains a fork of Qmmands which lets you provide your own command map. The builtin command map does not support spaces in command/module aliases, and the library owner
	//  does not want to support custom command maps nor spaces in aliases.
	internal class MultiWordCommandMap : ICommandMap {
		private readonly string m_Separator;
		private readonly List<Command> m_Commands;

		// I've not actually tested this with any separator other than a space but it should totally work.
		public MultiWordCommandMap(string separator = " ") {
			m_Commands = new List<Command>();
			m_Separator = separator;
		}

		public IReadOnlyList<CommandMatch> FindCommands(string input) {
			var matches = new List<CommandMatch>();
			// TODO (optimize) This is very slow because it is a direct copy of the proof of concept.
			// For starters we can keep a list of modules that we've already checked, and skip re-checking those.
			foreach (Command command in m_Commands) {
				var path = new List<string>();
				var checkModules = new Stack<Module>();
				string remainingInput = input;
				checkModules.Push(command.Module);
				while (checkModules.Peek().Parent != null) {
					checkModules.Push(checkModules.Peek().Parent);
				}
				bool match = false;
				while (checkModules.TryPop(out Module? check)) {
					if (check.Aliases.Count == 0) {
						match = true;
						continue;
					}
					foreach (string alias in check.Aliases) {
						if (remainingInput.StartsWith(alias)) {
							remainingInput = remainingInput.Substring(alias.Length);
							path.Add(alias);
							match = true;
							break;
						}
					}

					if (match) {
						string trimmedRemainingInput = remainingInput.TrimStart(m_Separator);
						if (trimmedRemainingInput != remainingInput) {
							remainingInput = trimmedRemainingInput;
						} else {
							match = false;
							break;
						}
					}
				}
				if (match) {
					if (command.Aliases.Count > 0) {
						foreach (string alias in command.Aliases) {
							if (remainingInput.StartsWith(alias)) {
								remainingInput = remainingInput.Substring(alias.Length);
								if (remainingInput.Length == 0 || remainingInput.StartsWith(m_Separator)) {
									matches.Add(new CommandMatch(command, alias, path.AsReadOnly(), remainingInput.TrimStart(m_Separator)));
									break;
								}
							}
						}
					} else {
						matches.Add(new CommandMatch(command, "", path.AsReadOnly(), remainingInput.TrimStart(m_Separator)));
					}
				}
			}
			return matches.AsReadOnly();
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