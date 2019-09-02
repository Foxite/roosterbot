using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract Version ComponentVersion { get; }
		public virtual string[] Tags { get; } = new string[] { };

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

		public virtual DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) => new DependencyResult() {
			OK = true
		};
		public virtual Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;
		public virtual Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) => Task.CompletedTask;
		public virtual Task ShutdownAsync() => Task.CompletedTask;

		/// <summary>
		/// Returns a string resource for this component.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public string GetStringResource(string propertyName) {
			if (ResourcesType != null) {
				PropertyInfo propertyInfo = ResourcesType.GetProperty(propertyName);
				if (propertyInfo != null) {
					object value = propertyInfo.GetValue(null);
					if (value is string strValue) {
						return strValue;
					} else {
						throw new ArgumentException($"Requested resource {propertyName} defined by {Name} is not a string");
					}
				} else {
					throw new ArgumentException($"No resource named {propertyName} is defined by {Name}");
				}
			} else {
				throw new ArgumentException($"String resource requested for component {Name} but no ResourcesType was set for its component");
			}
		}
	}

	public class DependencyResult {
		public bool OK { get; set; }
		public string ErrorMessage { get; set; }

		public static Builder Build(IEnumerable<ComponentBase> otherComponents) => new Builder(otherComponents);

		public class Builder {
			private IEnumerable<ComponentBase> m_OtherComponents;
			private bool m_Ok;
			private string m_ErrorMessage;

			internal Builder(IEnumerable<ComponentBase> otherComponents) {
				m_OtherComponents = otherComponents;
				m_Ok = true;
				m_ErrorMessage = "";
			}

			public Builder RequireTag(string tag) {
				if (!m_OtherComponents.Any(comp => comp.Tags.Contains(tag))) {
					m_Ok = false;
					m_ErrorMessage += $"A component tagged with {tag} must be present\n";
				}

				return this;
			}

			public Builder RequireMinimumVersion<T>(Version version) where T : ComponentBase {
				ComponentBase otherComponent = m_OtherComponents.FirstOrDefault(other => other.GetType() == typeof(T));
				if (otherComponent == null || otherComponent.ComponentVersion < version) {
					m_Ok = false;
					m_ErrorMessage += $"{typeof(T).Name} must be present and must be equal to or more recent than {version.ToString()}\n";
				}

				return this;
			}

			public Builder RequireVersion<T>(VersionPredicate predicate) where T : ComponentBase {
				ComponentBase otherComponent = m_OtherComponents.FirstOrDefault(other => other.GetType() == typeof(T));
				if (otherComponent == null || !predicate.Matches(otherComponent.ComponentVersion)) {
					m_Ok = false;
					m_ErrorMessage += $"{typeof(T).Name} must be present and must match {predicate.ToString()}\n";
				}

				return this;
			}

			public DependencyResult Check() {
				return new DependencyResult() {
					OK = m_Ok,
					ErrorMessage = string.IsNullOrEmpty(m_ErrorMessage) ? null : m_ErrorMessage.Trim()
				};
			}
		}
	}
}
