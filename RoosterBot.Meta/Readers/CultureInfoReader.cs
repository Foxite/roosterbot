using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Qmmands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Meta {
	public class CultureInfoReader : RoosterTypeParser<CultureInfo> {
		private Regex m_FlagEmoteRegex;

		public override string TypeDisplayName => "#CultureInfoReader_TypeDisplayName";

		public CultureInfoReader() {
			m_FlagEmoteRegex = new Regex(@"\:flag_([a-z]{2})\:");
		}

		protected override ValueTask<TypeParserResult<CultureInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (TryGetCultureInfo(input, out CultureInfo? info)) {
				return new ValueTask<TypeParserResult<CultureInfo>>(TypeParserResult<CultureInfo>.Successful(info));
			}

			var resources = context.ServiceProvider.GetService<ResourceService>();
			Match flagMatch = m_FlagEmoteRegex.Match(input);
			if (flagMatch.Success) {
				string countryCode = flagMatch.Groups[0].Value;
				if (TryGetCultureInfo(countryCode, out info)) {
					return new ValueTask<TypeParserResult<CultureInfo>>(TypeParserResult<CultureInfo>.Successful(info));
				} else {
					return new ValueTask<TypeParserResult<CultureInfo>>(TypeParserResult<CultureInfo>.Unsuccessful(resources.GetString(context.Culture, "CultureInfoReader_ParseFailed_UnknownFlag")));
				}
			}

			CultureNameService cns = context.ServiceProvider.GetService<CultureNameService>();
			string? resultCode = cns.Search(context.Culture, input);
			if (resultCode != null) {
				return new ValueTask<TypeParserResult<CultureInfo>>(TypeParserResult<CultureInfo>.Successful(CultureInfo.GetCultureInfo(resultCode)));
			}

			return new ValueTask<TypeParserResult<CultureInfo>>(TypeParserResult<CultureInfo>.Unsuccessful(resources.GetString(context.Culture, "CultureInfoReader_ParseFailed")));
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
