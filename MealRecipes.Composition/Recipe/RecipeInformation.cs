namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// レシピを復元するための最小情報
	/// </summary>
	public class RecipeInformation {
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public RecipeInformation() {
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="recipeId">レシピID</param>
		/// <param name="adjustment">分量調整</param>
		public RecipeInformation(int recipeId, double adjustment) {
			this.RecipeId = recipeId;
			this.Adjustment = adjustment;
		}

		/// <summary>
		/// レシピID
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// 分量調整
		/// </summary>
		public double Adjustment {
			get;
			set;
		}
	}
}
