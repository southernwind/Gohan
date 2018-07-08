namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// レシピ評価テーブル
	/// </summary>
	public class Rating {
		/// <summary>
		/// レシピID
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// ユーザーID
		/// </summary>
		public int UserId {
			get;
			set;
		}

		/// <summary>
		/// 評価値
		/// </summary>
		public int Value {
			get;
			set;
		}

		/// <summary>
		/// レシピID
		/// </summary>
		public Recipe Recipe {
			get;
			set;
		}

		/// <summary>
		/// ユーザー
		/// </summary>
		public User User {
			get;
			set;
		}
	}
}
