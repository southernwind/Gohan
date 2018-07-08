using System.Collections.Generic;

namespace SandBeige.MealRecipes.Extensions {
	public static class CompareEx {
		public static bool IsBetween<T>(this T @this, T start, T end) {
			return Comparer<T>.Default.Compare(@this, start) >= 0
				&& Comparer<T>.Default.Compare(@this, end) <= 0;
		}
	}
}
