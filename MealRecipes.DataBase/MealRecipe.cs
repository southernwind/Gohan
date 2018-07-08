using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// 食事、レシピの中間テーブル
	/// </summary>
	public class MealRecipe {
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
		public Meal Meal {
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
		public Recipe Recipe {
			get;
			set;
		}

		/// <summary>
		/// レシピ分量調整
		/// </summary>
		public double Adjustment {
			get;
			set;
		}
	}
}
