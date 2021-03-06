﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// An AspectListResult has multiple discrete pieces of information that should be displayed together. This class is <see cref="IEnumerable{AspectListItem}"/>.
	/// </summary>
	public class AspectListResult : RoosterCommandResult, IEnumerable<AspectListItem> {
		private readonly IEnumerable<AspectListItem> m_Aspects;

		/// <summary>
		/// The text to prepend to the aspect list.
		/// </summary>
		public string Caption { get; }

		/// <summary>
		/// Indicates if <see cref="ToString(RoosterCommandContext)"/> should include the aspect names.
		/// </summary>
		public bool IncludeAspectNames { get; }

		/// 
		public AspectListResult(string caption, IEnumerable<AspectListItem> aspects, bool includeAspectNames = false) {
			Caption = caption;
			m_Aspects = aspects;
			IncludeAspectNames = includeAspectNames;
		}

		/// <inheritdoc/>
		public override string ToString(RoosterCommandContext r) => Caption + "\n" + string.Join('\n', m_Aspects.Select(item => item.Present(IncludeAspectNames)));

		/// <inheritdoc/>
		public IEnumerator<AspectListItem> GetEnumerator() => m_Aspects.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => m_Aspects.GetEnumerator();
	}

	/// <summary>
	/// An presented AspectListItem must start with its <see cref="PrefixEmote"/> and end with its <see cref="Value"/>. It name is optional, but if shown it must come between PrefixEmote and Value.
	/// </summary>
	public class AspectListItem {
		/// <summary>
		/// The emote to prefix to the aspect.
		/// </summary>
		public IEmote PrefixEmote { get; }

		/// <summary>
		/// The name of the aspect.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The value of the aspect.
		/// </summary>
		public string Value { get; }

		///
		public AspectListItem(IEmote prefixEmote, string name, string value) {
			PrefixEmote = prefixEmote;
			Name = name;
			Value = value;
		}

		///
		public string Present(bool includeName) => PrefixEmote.ToString() + (includeName ? Name + ": " : " ") + Value;
	}
}
