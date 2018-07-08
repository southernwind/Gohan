using System;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// 設定をまとめるInterface
	/// </summary>
	public interface IBaseSettings : IDisposable {
		/// <summary>
		/// 一般設定
		/// </summary>
		IGeneralSettings GeneralSettings {
			get;
		}

		/// <summary>
		/// 検索設定
		/// </summary>
		ISearchSettings SearchSettings {
			get;
		}

		/// <summary>
		/// ネットワーク設定
		/// </summary>
		INetworkSettings NetworkSettings {
			get;
		}

		/// <summary>
		/// 状態
		/// </summary>
		IStates States {
			get;
		}

		/// <summary>
		/// キャッシュ
		/// </summary>
		ICaches Caches {
			get;
		}

		/// <summary>
		/// データベース変更通知
		/// </summary>
		IDbChangeNotifier DbChangeNotifier {
			get;
		}

		/// <summary>
		/// 保存
		/// </summary>
		void Save();

		/// <summary>
		/// 読み込み
		/// </summary>
		void Load();
	}
}
