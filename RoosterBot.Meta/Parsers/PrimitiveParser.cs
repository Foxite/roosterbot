using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	public class PrimitiveParser<T> : RoosterTypeParser<T> {
		public override string TypeDisplayName => "#Meta_Primitive_" + m_TypeKey;

		private readonly string m_TypeKey;

		public PrimitiveParser(string typeKey) {
			m_TypeKey = typeKey;
		}

		public override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			try {
				var result = (T) Convert.ChangeType(input, typeof(T));
				return ValueTaskUtil.FromResult(Successful((T) result));
			} catch {
				return ValueTaskUtil.FromResult(Unsuccessful(false, context, "#Meta_PrimitiveFail_" + m_TypeKey));
			}
		}
	}

	public delegate bool TryParsePrimitive<T>(string input, out T result);
}
