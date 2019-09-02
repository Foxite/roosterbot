using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
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
					m_ErrorMessage += $"{typeof(T).Name} must be present and its version must be at least {version.ToString()}\n";
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

			public Builder MinimumRoosterBotVersion(Version version) {
				if (Constants.RoosterBotVersion < version) {
					m_Ok = false;
					m_ErrorMessage += $"RoosterBot version must be at least {version.ToString()}";
				}

				return this;
			}

			public Builder RequireRoosterBotVersion(VersionPredicate predicate) {
				if (!predicate.Matches(Constants.RoosterBotVersion)) {
					m_Ok = false;
					m_ErrorMessage += $"RoosterBot must match {predicate.ToString()}";
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
