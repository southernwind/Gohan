using SandBeige.MealRecipes.DataBase;

using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Settings {
	/// <summary>
	/// 一般設定
	/// </summary>
	public interface IGeneralSettings : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// データベースファイルパス
		/// </summary>
		string DataBaseFilePath {
			get;
			set;
		}

		/// <summary>
		/// 画像ルートディレクトリパス
		/// </summary>
		string ImageDirectoryPath {
			get;
			set;
		}

		/// <summary>
		/// プラグインディレクトリパス
		/// </summary>
		string PluginsDirectoryPath {
			get;
			set;
		}

		/// <summary>
		/// キャッシュディレクトリパス
		/// </summary>
		string CachesDirectoryPath {
			get;
			set;
		}

		/// <summary>
		/// データベースアドレス
		/// </summary>
		string DataBaseServer {
			get;
			set;
		}

		/// <summary>
		/// データベースポート
		/// </summary>
		uint DataBasePort {
			get;
			set;
		}

		/// <summary>
		/// データベースユーザー
		/// </summary>
		string DataBaseUser {
			get;
			set;
		}

		/// <summary>
		/// データベースパスワード
		/// </summary>
		string DataBasePassword {
			get;
			set;
		}

		/// <summary>
		/// データベース名
		/// </summary>
		string DataBaseName {
			get;
			set;
		}

		/// <summary>
		/// データベースタイプ
		/// </summary>
		DataBaseType DataBaseType {
			get;
			set;
		}

		MealRecipeDbContext GetMealRecipeDbContext();
	}
}
