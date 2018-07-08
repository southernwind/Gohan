namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// レシピタグ中間テーブル
	/// </summary>
	public class RecipeTag {
		/// <summary>
		/// レシピID
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// レシピ
		/// </summary>
		public Recipe Recipe {
			get;
			set;
		}

		/// <summary>
		/// タグID
		/// </summary>
		public int TagId {
			get;
			set;
		}

		/// <summary>
		/// タグ
		/// </summary>
		public Tag Tag {
			get;
			set;
		}
	}
}
