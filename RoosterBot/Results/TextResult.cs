using Discord;

namespace RoosterBot {
	/// <summary>
	/// A TextResult consists of an optional emote followed by a string.
	/// </summary>
	public class TextResult : RoosterCommandResult {
		public IEmote? PrefixEmote { get; }
		public string Response { get; }

		public TextResult(IEmote? prefixEmote, string response) {
			PrefixEmote = prefixEmote;
			Response = response;
		}

		public override string ToString() {
			return (PrefixEmote != null ? (PrefixEmote.ToString() + " ") : "") + Response;
		}

		public static TextResult Success(string response) => new TextResult(Constants.Success, response);
		public static TextResult Info   (string response) => new TextResult(Constants.Info,    response);
		public static TextResult Warning(string response) => new TextResult(Constants.Warning, response);
		public static TextResult Unknown(string response) => new TextResult(Constants.Unknown, response);
		public static TextResult Error  (string response) => new TextResult(Constants.Error,   response);
	}
}
