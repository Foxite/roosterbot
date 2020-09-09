using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// This lets you apply a function to the result of an existing <see cref="RoosterTypeParser{T}"/> without creating a new class.
	/// This is particularly useful for creating instances of your <see cref="ISnowflake"/> implementations based on your existing, platform-specific snowflake parsers.
	/// </summary>
	public class ConversionParser<TIn, TOut> : RoosterTypeParser<TOut> {
		private readonly RoosterTypeParser<TIn> m_Inner;
		private readonly Func<TIn, TOut> m_Converter;

		/// <inheritdoc/>
		public override string TypeDisplayName { get; }

		/// 
		public ConversionParser(string typeDisplayName, RoosterTypeParser<TIn> inner, Func<TIn, TOut> converter) {
			TypeDisplayName = typeDisplayName;
			m_Inner = inner;
			m_Converter = converter;
		}

		/// <inheritdoc/>
		public async override ValueTask<RoosterTypeParserResult<TOut>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			var result = await m_Inner.ParseAsync(parameter, value, context);

			if (result.IsSuccessful) {
				return Successful(m_Converter(result.Value));
			} else {
				return Unsuccessful(result.InputValid, result.Reason, result.ErrorReasonObjects);
			}
		}
	}
}
