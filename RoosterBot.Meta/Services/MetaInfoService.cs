namespace RoosterBot.Meta {
	public class MetaInfoService {
		internal string GithubLink  { get; }
		internal string DiscordLink { get; }

		public MetaInfoService(string githubLink, string discordLink) {
			GithubLink = githubLink;
			DiscordLink = discordLink;
		}
	}
}
