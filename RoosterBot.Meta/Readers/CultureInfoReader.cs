using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Meta {
	public class CultureInfoReader : RoosterTypeReader {
		private Regex m_FlagEmoteRegex;

		public override Type Type => typeof(CultureInfo);
		public override string TypeDisplayName => "#CultureInfoReader_TypeDisplayName";

		public CultureInfoReader() {
			m_FlagEmoteRegex = new Regex(@"\:flag_([a-z]{2})\:");
		}

		protected override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			if (TryGetCultureInfo(input, out CultureInfo? info)) {
				return Task.FromResult(TypeReaderResult.FromSuccess(info));
			}
			ResourceService resources = services.GetService<ResourceService>();

			Match flagMatch = m_FlagEmoteRegex.Match(input);
			if (flagMatch.Success) {
				string countryCode = flagMatch.Groups[0].Value;
				if (TryGetCultureInfo(countryCode, out info)) {
					return Task.FromResult(TypeReaderResult.FromSuccess(info));
				} else {
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context.Culture, "CultureInfoReader_ParseFailed_UnknownFlag")));
				}
			}

			CultureNameService cns = services.GetService<CultureNameService>();
			string? resultCode = cns.Search(context.Culture, input);
			if (resultCode != null) {
				return Task.FromResult(TypeReaderResult.FromSuccess(CultureInfo.GetCultureInfo(resultCode)));
			}

			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, resources.GetString(context.Culture, "CultureInfoReader_ParseFailed")));
		}

		private bool TryGetCultureInfo(string name, [NotNullWhen(true), MaybeNullWhen(false)] out CultureInfo? info) {
			try {
				info = CultureInfo.GetCultureInfo(name);
				return true;
			} catch (CultureNotFoundException) {
				// This is ugly but I'm pretty sure it's no worse than doing CultureInfo.GetCultures(All) and searching for the one you want.
				// In fact it's better because you don't need to create a whole array just to get one item out of it.
				info = null;
				return false;
			}
		}
	}
}
