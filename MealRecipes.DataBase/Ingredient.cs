using System.Collections.Generic;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// 材料テーブル
	/// </summary>
	public class Ingredient {
		/// <summary>
		/// レシピID
		/// レシピテーブルの主キー
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// 材料ID
		/// レシピ毎に一意な連番が割り振られる
		/// </summary>
		public int IngredientId {
			get;
			set;
		}

		/// <summary>
		/// 材料名
		/// </summary>
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// 分量
		/// </summary>
		public string Amount {
			get;
			set;
		}

		/// <summary>
		/// 独自拡張データ
		/// </summary>
		public virtual ICollection<IngredientExtension> Extensions {
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
		/// お買物リスト
		/// </summary>
		public virtual ICollection<ShoppingItem> ShoppingList {
			get;
			set;
		}
	}
}
