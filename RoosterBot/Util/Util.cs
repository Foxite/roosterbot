using System;

namespace RoosterBot {
	public static class Util {
		public static readonly string Error   = Constants.Error  .ToString() + " ";
		public static readonly string Success = Constants.Success.ToString() + " ";
		public static readonly string Warning = Constants.Warning.ToString() + " ";
		public static readonly string Unknown = Constants.Unknown.ToString() + " ";
		public static readonly string Info    = Constants.Info   .ToString() + " ";
		public static readonly Random RNG     = new Random();
	}
}
