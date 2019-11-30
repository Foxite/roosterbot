using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Qmmands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Meta {
	public class CultureInfoReader : RoosterTypeParser<CultureInfo> {
		private readonly Regex m_FlagEmoteRegex;

		public override string TypeDisplayName => "#CultureInfoReader_TypeDisplayName";

		public CultureInfoReader(Component component) : base(component) {
			m_FlagEmoteRegex = new Regex(@"\:flag_([a-z]{2})\:");
		}

		protected override ValueTask<RoosterTypeParserResult<CultureInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (TryGetCultureInfo(input, out CultureInfo? info)) {
				return ValueTaskUtil.FromResult(Successful(info));
			}

			Match flagMatch = m_FlagEmoteRegex.Match(input);
			if (flagMatch.Success) {
				string countryCode = flagMatch.Groups[0].Value;
				if (TryGetCultureInfo(countryCode, out info)) {
					return ValueTaskUtil.FromResult(Successful(info));
				} else {
					return ValueTaskUtil.FromResult(Unsuccessful(true, "#CultureInfoReader_ParseFailed_UnknownFlag"));
				}
			}

			CultureNameService cns = context.ServiceProvider.GetService<CultureNameService>();
			string? resultCode = cns.Search(context.Culture, input);
			if (resultCode != null) {
				return ValueTaskUtil.FromResult(Successful(CultureInfo.GetCultureInfo(resultCode)));
			}

			return ValueTaskUtil.FromResult(Unsuccessful(false, "#CultureInfoReader_ParseFailed"));
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
