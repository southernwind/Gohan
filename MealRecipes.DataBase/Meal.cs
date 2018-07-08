using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// 食事テーブル
	/// </summary>
	public class Meal {
		/// <summary>
		/// 日付
		/// </summary>
		[Column(TypeName = "Date")]
		public DateTime Date {
			get;
			set;
		}

		/// <summary>
		/// 食事ID
		/// 日付毎に一意な連番が割り振られる
		/// </summary>
		public int MealId {
			get;
			set;
		}

		/// <summary>
		/// 食事タイプID
		/// 食事タイプテーブルの主キー
		/// </summary>
		public int MealTypeId {
			get;
			set;
		}

		/// <summary>
		/// 食事タイプ
		/// </summary>
		public MealType MealType {
			get;
			set;
		}

		/// <summary>
		/// レシピリスト
		/// </summary>
		public virtual ICollection<MealRecipe> MealRecipes {
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
