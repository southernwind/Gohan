using System.Collections.Generic;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// 手順テーブル
	/// </summary>
	public class Step {
		/// <summary>
		/// レシピID
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// 手順Id
		/// レシピ毎に一意な連番が割り振られる
		/// </summary>
		public int StepId {
			get;
			set;
		}

		/// <summary>
		/// 画像
		/// </summary>
		public string PhotoFilePath {
			get;
			set;
		}

		/// <summary>
		/// サムネイル画像
		/// </summary>
		public string ThumbnailFilePath {
			get;
			set;
		}

		/// <summary>
		/// 手順テキスト
		/// </summary>
		public string StepText {
			get;
			set;
		}

		/// <summary>
		/// 独自拡張データ
		/// </summary>
		public virtual ICollection<StepExtension> Extensions {
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
	}
}
