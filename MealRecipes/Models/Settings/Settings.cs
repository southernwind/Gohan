using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Models.Notifier;

using System;
using System.IO;
using System.Reactive.Disposables;
using System.Xaml;

namespace SandBeige.MealRecipes.Models.Settings {
	/// <summary>
	/// 設定
	/// </summary>
	public class Settings : ISettings {
		private readonly string _settingsFilePath;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		/// <summary>
		/// 一般設定
		/// </summary>
		public IGeneralSettings GeneralSettings {
			get;
			set;
		}

		/// <summary>
		/// 検索設定
		/// </summary>
		public ISearchSettings SearchSettings {
			get;
			set;
		}

		/// <summary>
		/// ネットワーク設定
		/// </summary>
		public INetworkSettings NetworkSettings {
			get;
			set;
		}

		/// <summary>
		/// 状態保持
		/// </summary>
		public IStates States {
			get;
			set;
		}

		/// <summary>
		/// キャッシュ
		/// </summary>
		public ICaches Caches {
			get;
		}

		/// <summary>
		/// マスタキャッシュ
		/// </summary>
		public Master Master {
			get;
		}

		public IDbChangeNotifier DbChangeNotifier {
			get;
		}

		[Obsolete("シリアライズ用")]
		public Settings() {
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public Settings(ILogger logger, string settingsFilePath) {
			this.GeneralSettings = new GeneralSettings().AddTo(this._disposable);
			this.SearchSettings = new SearchSettings().AddTo(this._disposable);
			this.NetworkSettings = new NetworkSettings().AddTo(this._disposable);
			this.States = new States().AddTo(this._disposable);
			this.DbChangeNotifier = new DbChangeNotifier(logger, this).AddTo(this._disposable);
			this.Master = new Master(this).AddTo(this._disposable);
			this.Caches = new Caches(this).AddTo(this._disposable);
			this._settingsFilePath = settingsFilePath;
		}

		/// <summary>
		/// 保存
		/// </summary>
		public void Save() {
			using (var ms = new MemoryStream()) {
				XamlServices.Save(ms, this);
				using (var fs = File.Create(this._settingsFilePath)) {
					ms.WriteTo(fs);
				}
			}
		}

		/// <summary>
		/// 読み込み
		/// </summary>
		public void Load() {
			if (!File.Exists(this._settingsFilePath)) {
				return;
			}
			if (!(XamlServices.Load(this._settingsFilePath) is Settings settings)) {
				return;
			}
			this.GeneralSettings?.Dispose();
			this.GeneralSettings = settings.GeneralSettings.AddTo(this._disposable);
			this.SearchSettings?.Dispose();
			this.SearchSettings = settings.SearchSettings.AddTo(this._disposable);
			this.NetworkSettings?.Dispose();
			this.NetworkSettings = settings.NetworkSettings.AddTo(this._disposable);
			this.States?.Dispose();
			this.States = settings.States.AddTo(this._disposable);
		}

		public void MasterLoad() {
			this.Master.Load();
		}

		public void CachesLoad() {
			this.Caches.Load();
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
