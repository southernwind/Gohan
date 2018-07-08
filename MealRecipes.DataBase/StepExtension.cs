namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// 手順独自拡張データ
	/// </summary>
	public class StepExtension {
		/// <summary>
		/// レシピID
		/// レシピテーブルの主キー
		/// </summary>
		public int RecipeId {
			get;
			set;
		}

		/// <summary>
		/// 手順ID
		/// レシピ毎に一意な連番が割り振られる
		/// </summary>
		public int StepId {
			get;
			set;
		}

		/// <summary>
		/// 拡張データキー(プロパティ名)
		/// </summary>
		public string Key {
			get;
			set;
		}

		/// <summary>
		/// 配列インデックス番号
		/// </summary>
		public int Index {
			get;
			set;
		}

		/// <summary>
		/// データ
		/// </summary>
		public byte[] Data {
			get;
			set;
		}
	}
}
