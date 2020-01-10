using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RoosterBot {
	public static class Util {
		public static readonly Random RNG = new Random();

		public static bool CompareNumeric(object left, object right) {
			Type[] numerics = new[] {
				typeof(byte),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(sbyte),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
			};
			if (numerics.Contains(left.GetType()) && numerics.Contains(right.GetType())) {
				return ((IConvertible) left).ToDecimal(null).Equals(((IConvertible) left).ToDecimal(null));
			} else {
				return left.Equals(right);
			}
		}

		/// <summary>
		/// This will deserialize a Json file based on a template class. If the file does not exist, it will be created and the template will be serialized and written to the file.
		/// </summary>
		public static T LoadJsonConfigFromTemplate<T>(string filePath, T template, JsonSerializerSettings? jss = null) {
			if (File.Exists(filePath)) {
				return JsonConvert.DeserializeObject<T>(filePath, jss);
			} else {
				using var sw = File.CreateText(filePath);
				sw.Write(JsonConvert.SerializeObject(template));
				return template;
			}
		}
	}
}
