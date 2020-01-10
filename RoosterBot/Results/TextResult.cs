using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	/// <summary>
	/// A TextResult consists of an optional emote followed by a string.
	/// </summary>
	public class TextResult : RoosterCommandResult {
		public string? PrefixEmoteName { get; }
		public string Response { get; }

		public TextResult(string? prefixEmoteName, string response) {
			PrefixEmoteName = prefixEmoteName;
			Response = response;
		}

		public override string ToString(RoosterCommandContext rcc) {
			string ret = Response;
			if (PrefixEmoteName != null) {
				if (rcc.ServiceProvider.GetService<EmoteService>().TryGetEmote(rcc.Platform, PrefixEmoteName, out IEmote? emote)) {
					ret = emote.ToString() + " " + ret;
				} else {
					Logger.Error("TextResult", "PlatformComponent " + rcc.Platform.Name + " does not define an emote named " + PrefixEmoteName + ". No emote will be used.");
				}
			}
			return ret;
		}

		public static TextResult Success(string response) => new TextResult("Success", response);
		public static TextResult Info   (string response) => new TextResult("Info",    response);
		public static TextResult Warning(string response) => new TextResult("Warning", response);
		public static TextResult Unknown(string response) => new TextResult("Unknown", response);
		public static TextResult Error  (string response) => new TextResult("Error",   response);
	}
}
