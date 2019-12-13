using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	public class PrimitiveParser<T> : RoosterTypeParser<T> {
		public override string TypeDisplayName => "#Meta_Primitive_" + m_TypeKey;

		private readonly TryParsePrimitive<T> m_ParseDelegate;
		private readonly string m_TypeKey;

		public PrimitiveParser(TryParsePrimitive<T> parseDelegate, string typeKey) {
			m_ParseDelegate = parseDelegate;
			m_TypeKey = typeKey;
		}

		protected override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (m_ParseDelegate(input, out T result)) {
				return ValueTaskUtil.FromResult(Successful(result));
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#Meta_PrimitiveFail_" + m_TypeKey, StringUtil.EscapeString(input)));
			}
		}
	}

	public delegate bool TryParsePrimitive<T>(string input, out T result);
}
