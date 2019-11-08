using System;

namespace RoosterBot.PublicTransit {
	public static class PublicTransitUtil {
		/// <summary>
		/// Returns the Levenshtein distance from {source} to {target}.
		/// From https://stackoverflow.com/a/6944095/3141917
		/// </summary>
		public static int Levenshtein(string source, string target) {
			if (source.Length == 0) {
				if (target.Length == 0) {
					return 0;
				} else {
					return target.Length;
				}
			} else if (target.Length == 0) {
				return source.Length;
			}

			int n = source.Length;
			int m = target.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 1; j <= m; d[0, j] = j++) ;

			for (int i = 1; i <= n; i++) {
				for (int j = 1; j <= m; j++) {
					int cost = (target[j - 1] == source[i - 1]) ? 0 : Math.Min(n - i, m - j);
					int min1 = d[i - 1, j] + n - i;
					int min2 = d[i, j - 1] + m - j;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}
	}
}
