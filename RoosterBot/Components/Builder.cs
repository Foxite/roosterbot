﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	public partial class DependencyResult {
		public class Builder {
			private IEnumerable<ComponentBase> m_OtherComponents;
			private string m_ErrorMessage;

			internal Builder(IEnumerable<ComponentBase> otherComponents) {
				m_OtherComponents = otherComponents;
				m_ErrorMessage = "";
			}

			public Builder RequireTag(string tag) {
				if (!m_OtherComponents.Any(comp => comp.Tags.Contains(tag))) {
					m_ErrorMessage += $"A component tagged with {tag} must be present\n";
				}

				return this;
			}

			public Builder RequireMinimumVersion<T>(Version version) where T : ComponentBase {
				ComponentBase otherComponent = m_OtherComponents.FirstOrDefault(other => other.GetType() == typeof(T));
				if (otherComponent == null || otherComponent.ComponentVersion < version) {
					m_ErrorMessage += $"{typeof(T).Name} must be present and its version must be at least {version.ToString()}\n";
				}

				return this;
			}

			public Builder RequireVersion<T>(VersionPredicate predicate) where T : ComponentBase {
				ComponentBase otherComponent = m_OtherComponents.FirstOrDefault(other => other.GetType() == typeof(T));
				if (otherComponent == null || !predicate.Matches(otherComponent.ComponentVersion)) {
					m_ErrorMessage += $"{typeof(T).Name} must be present and must match {predicate.ToString()}\n";
				}

				return this;
			}

			public Builder RequireMinimumRoosterBotVersion(Version version) {
				if (Constants.RoosterBotVersion < version) {
					m_ErrorMessage += $"RoosterBot version must be at least {version.ToString()}";
				}

				return this;
			}

			public Builder RequireRoosterBotVersion(VersionPredicate predicate) {
				if (!predicate.Matches(Constants.RoosterBotVersion)) {
					m_ErrorMessage += $"RoosterBot must match {predicate.ToString()}";
				}

				return this;
			}

			public DependencyResult Check() {
				return new DependencyResult() {
					m_ErrorMessage = string.IsNullOrEmpty(m_ErrorMessage) ? null : m_ErrorMessage.Trim()
				};
			}
		}
	}
}