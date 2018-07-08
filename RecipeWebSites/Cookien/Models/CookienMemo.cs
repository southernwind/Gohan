using Livet;

using Reactive.Bindings;

namespace SandBeige.RecipeWebSites.Cookien.Models {
	/// <summary>
	/// つくおきメモ
	/// </summary>
	public class CookienMemo : NotificationObject {
		/// <summary>
		/// インデックス
		/// </summary>
		public ReactiveProperty<int> Index {
			get;
			set;
		} = new ReactiveProperty<int>();

		/// <summary>
		/// タイトル
		/// </summary>
		public ReactiveProperty<string> Title {
			get;
			set;
		} = new ReactiveProperty<string>();

		/// <summary>
		/// 詳細
		/// </summary>
		public ReactiveProperty<string> Description {
			get;
			set;
		} = new ReactiveProperty<string>();

		public CookienMemo(int index, string title, string description) {
			this.Index.Value = index;
			this.Title.Value = title;
			this.Description.Value = description;
		}

		// シリアライズ用
		public CookienMemo() {

		}
	}
}
