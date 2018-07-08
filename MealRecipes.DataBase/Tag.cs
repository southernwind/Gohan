using System.Collections.Generic;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// タグ
	/// </summary>
	public class Tag {
		/// <summary>
		/// タグID
		/// </summary>
		public int TagId {
			get;
			set;
		}

		/// <summary>
		/// タグ名
		/// </summary>
		public string TagName {
			get;
			set;
		}

		/// <summary>
		/// レシピリスト
		/// </summary>
		public virtual ICollection<RecipeTag> RecipeTags {
			get;
			set;
		}
	}
}
