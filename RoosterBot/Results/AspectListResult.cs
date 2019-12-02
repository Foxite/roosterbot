using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace RoosterBot {
	/// <summary>
	/// An AspectListResult has multiple discrete pieces of information that should be displayed together. This class is <see cref="IEnumerable{AspectListItem}"/>.
	/// </summary>
	public class AspectListResult : RoosterCommandResult, IEnumerable<AspectListItem> {
		private readonly IEnumerable<AspectListItem> m_Aspects;

		public string Caption { get; }
		public bool IncludeAspectNames { get; }

		public AspectListResult(string caption, IEnumerable<AspectListItem> aspects, bool includeAspectNames = false) {
			Caption = caption;
			m_Aspects = aspects;
			IncludeAspectNames = includeAspectNames;
		}

		public override string ToString() => Caption + "\n" + string.Join('\n', m_Aspects.Select(item => item.Present(IncludeAspectNames)));

		public IEnumerator<AspectListItem> GetEnumerator() => m_Aspects.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => m_Aspects.GetEnumerator();
	}

	/// <summary>
	/// An presented AspectListItem must start with its <see cref="PrefixEmote"/> and end with its <see cref="Value"/>. It name is optional, but if shown it must come between PrefixEmote and Value.
	/// </summary>
	public class AspectListItem {
		public IEmote PrefixEmote { get; }
		public string Name { get; }
		public string Value { get; }

		public AspectListItem(IEmote prefixEmote, string name, string value) {
			PrefixEmote = prefixEmote;
			Name = name;
			Value = value;
		}

		public string Present(bool includeName) => PrefixEmote.ToString() + (includeName ? Name + ": " : " ") + Value;
	}
}
