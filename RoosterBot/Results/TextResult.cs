using System;
using System.Collections.Generic;
using Discord;

namespace RoosterBot {
	/// <summary>
	/// A TextResult consists of an optional emote followed by a string.
	/// </summary>
	public class TextResult : RoosterCommandResult {
		public Emote? PrefixEmote { get; }
		public string Response { get; }

		public override IReadOnlyList<object> ErrorReasonObjects => throw new NotImplementedException();

		public override ComponentBase? ErrorReasonComponent => throw new NotImplementedException();

		public override bool IsSuccessful => throw new NotImplementedException();

		public TextResult(Emote? prefixEmote, string response) {
			PrefixEmote = prefixEmote;
			Response = response;
		}

		public override string Present() => (PrefixEmote != null ? (PrefixEmote + " ") : "") + Response;

		private static TextResult GetResult(string emote, string response) => new TextResult(Emote.Parse(emote.Trim()), response);

		public static TextResult Success(string response) => GetResult(Util.Success, response);
		public static TextResult Info   (string response) => GetResult(Util.Info,    response);
		public static TextResult Warning(string response) => GetResult(Util.Warning, response);
		public static TextResult Unknown(string response) => GetResult(Util.Unknown, response);
		public static TextResult Error  (string response) => GetResult(Util.Error,   response);
	}
}
