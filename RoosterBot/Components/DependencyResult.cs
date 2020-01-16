using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// A class that helps when checking if <see cref="Component"/> dependencies are met.
	/// You should use this class within your <see cref="Component.CheckDependencies(IEnumerable{Component})"/> method.
	/// </summary>
	public class DependencyResult {
		private string? m_ErrorMessage;

		/// <summary>
		/// Indicates if all dependencies are met.
		/// </summary>
		public bool OK => m_ErrorMessage == null;

		/// <summary>
		/// Indicates the human-readable reason why the dependencies are not met.
		/// </summary>
		/// <exception cref="InvalidOperationException">If <see cref="OK"/> is true.</exception>
		public string ErrorMessage => m_ErrorMessage ?? throw new InvalidOperationException("Can't get the error message of a successful operation");

		/// <summary>
		/// Creates an instance of the <see cref="Builder"/> class which lets you specify your dependencies.
		/// </summary>
		/// <param name="otherComponents">The enumeration of other components that are installed, as received in your <see cref="Component.CheckDependencies(IEnumerable{Component})"/> method.</param>
		public static Builder Build(IEnumerable<Component> otherComponents) => new Builder(otherComponents);

		/// <summary>
		/// The class that lets you specify your dependencies.
		/// </summary>
		public class Builder {
			private readonly IEnumerable<Component> m_OtherComponents;
			private string m_ErrorMessage = "";

			internal Builder(IEnumerable<Component> otherComponents) {
				m_OtherComponents = otherComponents;
			}

			/// <summary>
			/// Specify that at least one component with a specific <paramref name="tag"/> must be installed.
			/// </summary>
			public Builder RequireTag(string tag) {
				if (!m_OtherComponents.Any(comp => comp.Tags.Contains(tag))) {
					m_ErrorMessage += $"A component tagged with {tag} must be present\n";
				}

				return this;
			}

			/// <summary>
			/// Specify that a certain component must be installed with a minimum version requirement.
			/// </summary>
			public Builder RequireMinimumVersion<T>(Version version) where T : Component {
				Component otherComponent = m_OtherComponents.FirstOrDefault(other => other.GetType() == typeof(T));
				if (otherComponent == null || otherComponent.ComponentVersion < version) {
					m_ErrorMessage += $"{typeof(T).Name} must be present and its version must be at least {version.ToString()}\n";
				}

				return this;
			}

			/// <summary>
			/// Specify that a certain component must be installed with an exact version requirement.
			/// </summary>
			public Builder RequireVersion<T>(VersionPredicate predicate) where T : Component {
				Component otherComponent = m_OtherComponents.FirstOrDefault(other => other.GetType() == typeof(T));
				if (otherComponent == null || !predicate.Matches(otherComponent.ComponentVersion)) {
					m_ErrorMessage += $"{typeof(T).Name} must be present and must match {predicate.ToString()}\n";
				}

				return this;
			}

			/// <summary>
			/// Specify that RoosterBot must be at least a specific version.
			/// </summary>
			public Builder RequireMinimumRoosterBotVersion(Version version) {
				if (Program.Version < version) {
					m_ErrorMessage += $"RoosterBot version must be at least {version.ToString()}";
				}

				return this;
			}

			/// <summary>
			/// Specify that RoosterBot must match a specific version predicate.
			/// </summary>
			/// <param name="predicate"></param>
			/// <returns></returns>
			public Builder RequireRoosterBotVersion(VersionPredicate predicate) {
				if (!predicate.Matches(Program.Version)) {
					m_ErrorMessage += $"RoosterBot must match {predicate.ToString()}";
				}

				return this;
			}

			/// <summary>
			/// Create a <see cref="DependencyResult"/> from this <see cref="Builder"/>.
			/// </summary>
			/// <returns></returns>
			public DependencyResult Check() {
				return new DependencyResult() {
					m_ErrorMessage = string.IsNullOrEmpty(m_ErrorMessage) ? null : m_ErrorMessage.Trim()
				};
			}
		}
	}
}
