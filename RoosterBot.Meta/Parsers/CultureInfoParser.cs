using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Meta {
	public class CultureInfoParser : RoosterTypeParser<CultureInfo> {
		public override string TypeDisplayName => "#CultureInfoReader_TypeDisplayName";

		public override ValueTask<RoosterTypeParserResult<CultureInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (TryGetCultureInfo(input, out CultureInfo? info)) {
				return ValueTaskUtil.FromResult(Successful(info));
			}

			CultureNameService cns = context.ServiceProvider.GetRequiredService<CultureNameService>();
			string? resultCode = cns.Search(parameter.Command.Module.Attributes.OfType<GlobalLocalizationsAttribute>().Any() ? null : context.Culture, input);
			if (resultCode != null) {
				return ValueTaskUtil.FromResult(Successful(CultureInfo.GetCultureInfo(resultCode)));
			}

			return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#CultureInfoReader_ParseFailed"));
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
