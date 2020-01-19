using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	/// <summary>
	/// A TextResult consists of an optional emote followed by a string.
	/// </summary>
	public class TextResult : RoosterCommandResult {
		/// <summary>
		/// The emote name to prefix this result with. Use <see cref="EmoteService"/> to get the <see cref="IEmote"/>.
		/// </summary>
		public string? PrefixEmoteName { get; }

		/// <summary>
		/// The actual text for this TextResult.
		/// </summary>
		public string Response { get; }

		/// <summary>
		/// Construct a new TextResult.
		/// </summary>
		public TextResult(string? prefixEmoteName, string response) {
			PrefixEmoteName = prefixEmoteName;
			Response = response;
		}

		/// <inheritdoc/>
		public override string ToString(RoosterCommandContext rcc) {
			string ret = Response;
			if (PrefixEmoteName != null) {
				if (rcc.ServiceProvider.GetRequiredService<EmoteService>().TryGetEmote(rcc.Platform, PrefixEmoteName, out IEmote? emote)) {
					ret = emote.ToString() + " " + ret;
				} else {
					Logger.Error("TextResult", "PlatformComponent " + rcc.Platform.Name + " does not define an emote named " + PrefixEmoteName + ". No emote will be used.");
				}
			}
			return ret;
		}

		/// <summary>
		/// Get a TextResult with an emote indicating success.
		/// </summary>
		public static TextResult Success(string response) => new TextResult("Success", response);

		/// <summary>
		/// Get a TextResult with an emote indicating an informational response.
		/// </summary>
		public static TextResult Info   (string response) => new TextResult("Info",    response);

		/// <summary>
		/// Get a TextResult with an emote indicating a warning.
		/// </summary>
		public static TextResult Warning(string response) => new TextResult("Warning", response);

		/// <summary>
		/// Get a TextResult with an emote indicating an unknown result.
		/// </summary>
		public static TextResult Unknown(string response) => new TextResult("Unknown", response);

		/// <summary>
		/// Get a TextResult with an emote indicating an error.
		/// </summary>
		public static TextResult Error  (string response) => new TextResult("Error",   response);
	}
}
