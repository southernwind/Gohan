using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Settings;

using System.IO;

namespace SandBeige.MealRecipes.ViewModels.Settings {
	class GeneralSettingsViewModel : SettingsPageViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// 設定名
		/// </summary>
		public override string Name {
			get;
		} = "一般設定";

		/// <summary>
		/// 選択状態
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// 値検証結果
		/// </summary>
		public override ReactiveProperty<bool> IsValidated {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// データベースファイルパス
		/// </summary>
		public ReactiveProperty<string> DataBaseFilePath {
			get;
		}

		/// <summary>
		/// 画像ルートディレクトリパス
		/// </summary>
		public ReactiveProperty<string> ImageDirectoryPath {
			get;
		}

		/// <summary>
		/// プラグインディレクトリパス
		/// </summary>
		public ReactiveProperty<string> PluginsDirectoryPath {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public GeneralSettingsViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.DataBaseFilePath =
				this._settings
				.GeneralSettings
				.ToReactivePropertyAsSynchronized(x => x.DataBaseFilePath).SetValidateNotifyError(x => File.Exists(x) ? null : "ファイルが存在しません")
				.AddTo(this.CompositeDisposable);

			this.ImageDirectoryPath =
				this._settings
				.GeneralSettings
				.ToReactivePropertyAsSynchronized(x => x.ImageDirectoryPath).SetValidateNotifyError(x => Directory.Exists(x) ? null : "ディレクトリが存在しません")
				.AddTo(this.CompositeDisposable);

			this.PluginsDirectoryPath =
				this._settings
					.GeneralSettings
					.ToReactivePropertyAsSynchronized(x => x.PluginsDirectoryPath).SetValidateNotifyError(x => Directory.Exists(x) ? null : "ディレクトリが存在しません")
					.AddTo(this.CompositeDisposable);

			this.IsValidated = new[] {
					this.DataBaseFilePath.ObserveHasErrors,
					this.ImageDirectoryPath.ObserveHasErrors,
					this.PluginsDirectoryPath.ObserveHasErrors
				}.CombineLatestValuesAreAllFalse()
				.ToReactiveProperty()
				.AddTo(this.CompositeDisposable);
		}
	}
}
