using Livet;

using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.DataBase;

using System;
using System.IO;
using System.Reactive.Disposables;

namespace SandBeige.MealRecipes.Models.Settings {
	/// <summary>
	/// 一般設定
	/// </summary>
	public class GeneralSettings : NotificationObject, IGeneralSettings {
		private readonly CompositeDisposable _disposable = new CompositeDisposable();
		/// <summary>
		/// データベースファイルパス
		/// </summary>
		public string DataBaseFilePath {
			get;
			set;
		} = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./Gohan.db");

		/// <summary>
		/// 画像ルートディレクトリパス
		/// </summary>
		public string ImageDirectoryPath {
			get;
			set;
		} = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./images");

		/// <summary>
		/// プラグインディレクトリパス
		/// </summary>
		public string PluginsDirectoryPath {
			get;
			set;
		} = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./Plugins");

		/// <summary>
		/// データベースアドレス
		/// </summary>
		public string DataBaseServer {
			get;
			set;
		} = "127.0.0.1";

		/// <summary>
		/// データベースポート
		/// </summary>
		public uint DataBasePort {
			get;
			set;
		} = 3306;

		/// <summary>
		/// データベースユーザー
		/// </summary>
		public string DataBaseUser {
			get;
			set;
		} = "Gohan";

		/// <summary>
		/// データベースパスワード
		/// </summary>
		public string DataBasePassword {
			get;
			set;
		} = "Gohan";

		/// <summary>
		/// データベース名
		/// </summary>
		public string DataBaseName {
			get;
			set;
		} = "Gohan";

		/// <summary>
		/// データベースタイプ
		/// </summary>
		public DataBaseType DataBaseType {
			get;
			set;
		} = DataBaseType.SQLite;

		public MealRecipeDbContext GetMealRecipeDbContext() {
			switch (this.DataBaseType) {
				case DataBaseType.SQLite:
					return new MealRecipeDbContext(this.DataBaseType, this.DataBaseFilePath);
				case DataBaseType.MySQL:
					return new MealRecipeDbContext(
						this.DataBaseType,
						this.DataBaseServer,
						this.DataBasePort,
						this.DataBaseUser,
						this.DataBasePassword,
						this.DataBaseName
					);
				default:
					throw new NotImplementedException();
			}
		}

		public void Dispose() {
			this._disposable.Dispose();
		}

	}
}
