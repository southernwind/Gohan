using Reactive.Bindings;

using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// 別の場所に保存されているデータのキャッシュ
	/// </summary>
	public interface ICaches : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// ユーザ一覧
		/// </summary>
		ReactiveCollection<User.User> Users {
			get;
		}

		/// <summary>
		/// パーマネントストレージからキャッシュ元データのロード
		/// </summary>
		void Load();
	}
}
