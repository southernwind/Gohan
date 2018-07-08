using System;
using System.Collections.Generic;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// レシピテーブル
	/// </summary>
	public class Recipe {
		/// <summary>
		/// レシピID
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// URL
		/// </summary>
		public string Url {
			get;
			set;
		}

		/// <summary>
		/// レシピタイトル
		/// </summary>
		public string Title {
			get;
			set;
		}

		/// <summary>
		/// レシピメイン画像
		/// </summary>
		public string PhotoFilePath {
			get;
			set;
		}

		/// <summary>
		/// レシピメイン画像サムネイル
		/// </summary>
		public string ThumbnailFilePath {
			get;
			set;
		}

		/// <summary>
		/// 詳細
		/// </summary>
		public string Description {
			get;
			set;
		}

		/// <summary>
		/// 分量
		/// </summary>
		public string Yield {
			get;
			set;
		}

		/// <summary>
		/// メモ
		/// </summary>
		public string Memo {
			get;
			set;
		}

		/// <summary>
		/// 所要時間
		/// </summary>
		public TimeSpan RequiredTime {
			get;
			set;
		}

		/// <summary>
		/// 登録日
		/// </summary>
		public DateTime RegistrationDate {
			get;
			set;
		}

		/// <summary>
		/// 作成プラグイン名
		/// </summary>
		public string PluginName {
			get;
			set;
		}

		/// <summary>
		/// アーカイブ済みフラグ
		/// </summary>
		public bool IsArchived {
			get;
			set;
		}

		/// <summary>
		/// 材料リスト
		/// </summary>
		public virtual ICollection<Ingredient> Ingretients {
			get;
			set;
		}

		/// <summary>
		/// 手順リスト
		/// </summary>
		public virtual ICollection<Step> Steps {
			get;
			set;
		}

		/// <summary>
		/// 独自拡張データ
		/// </summary>
		public virtual ICollection<RecipeExtension> Extensions {
			get;
			set;
		}

		/// <summary>
		/// 材料、レシピ中間テーブル
		/// </summary>
		public virtual ICollection<MealRecipe> MealRecipes {
			get;
			set;
		}

		/// <summary>
		/// レシピ、タグの中間テーブル
		/// </summary>
		public virtual ICollection<RecipeTag> RecipeTags {
			get;
			set;
		}

		/// <summary>
		/// 評価
		/// </summary>
		public virtual ICollection<Rating> Ratings {
			get;
			set;
		}
	}
}
