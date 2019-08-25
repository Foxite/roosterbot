﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract Version ComponentVersion { get; }
		public virtual IEnumerable<DependencyChecker> Dependencies { get; }
		public string Name {
			get {
				string longName = GetType().Name;
				int suffixIndex = longName.IndexOf("Component");
				if (suffixIndex != -1) {
					return longName.Substring(0, suffixIndex);
				} else {
					return longName;
				}
			}
		}

		protected Type ResourcesType { get; set; }

		public ComponentBase() { }

		public virtual Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;
		public virtual Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) => Task.CompletedTask;
		public virtual Task ShutdownAsync() => Task.CompletedTask;

		/// <summary>
		/// Returns a string resource for this component.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public string GetStringResource(string propertyName) {
			// TODO actually throw exceptions instead of error strings like its the 90s
			if (ResourcesType == null) {
				Logger.Error("ComponentBase", $"String resource requested for component {Name} but no ResourcesType was set for its component");
				return "ERROR 1";
			} else {
				PropertyInfo propertyInfo = ResourcesType.GetProperty(propertyName);
				if (propertyInfo == null) {
					Logger.Error("ComponentBase", $"No resource named {propertyName} is defined by {Name}");
					return "ERROR 2";
				} else {
					object value = propertyInfo.GetValue(null);
					if (value is string strValue) {
						return strValue;
					} else {
						Logger.Error("ComponentBase", $"Requested resource {propertyName} defined by {Name} is not a string");
						return "ERROR 3";
					}
				}
			}
		}
	}

	public delegate DependencyResult DependencyChecker(ComponentBase component);

	public class DependencyResult {
		public bool OK { get; }
		public string ErrorMessage { get; }

		public DependencyResult(bool ok, string errorMessage) {
			OK = ok;
			ErrorMessage = errorMessage;
		}
	}
}
