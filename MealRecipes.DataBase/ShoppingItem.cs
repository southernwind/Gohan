using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// お買い物テーブル
	/// </summary>
	public class ShoppingItem {
		/// <summary>
		/// 日付
		/// 食事テーブルの主キーの一部
		/// </summary>
		[Column(TypeName = "Date")]
		public DateTime Date {
			get;
			set;
		}

		/// <summary>
		/// 食事ID
		/// 食事テーブルの主キーの一部
		/// </summary>
		public int MealId {
			get;
			set;
		}

		/// <summary>
		/// 食事
		/// </summary>
		public virtual Meal Meal {
			get;
			set;
		}

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
		public virtual Recipe Recipe {
			get;
			set;
		}

		/// <summary>
		/// 材料ID
		/// </summary>
		public int IngredientId {
			get;
			set;
		}

		/// <summary>
		/// お買い物品
		/// </summary>
		public virtual Ingredient Ingredient {
			get;
			set;
		}
	}
}