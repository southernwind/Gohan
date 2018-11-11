using Livet;
using Livet.Messaging;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.ViewModels.Calendar;
using SandBeige.MealRecipes.ViewModels.Recipe;
using SandBeige.MealRecipes.ViewModels.Settings;
using System.IO;
using System.Linq;

namespace SandBeige.MealRecipes.ViewModels {
	/// <summary>
	/// メインウィンドウViewModel
	/// </summary>
	class MainWindowViewModel : ViewModel {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// タブに表示するコンテンツ一覧
		/// </summary>
		public TabItemViewModelBase[] ContentItems {
			get;
		}

		/// <summary>
		/// 選択中のコンテンツ
		/// </summary>
		public ReactiveProperty<TabItemViewModelBase> SelectedContent {
			get;
		} = new ReactiveProperty<TabItemViewModelBase>();

		/// <summary>
		/// 設定ウィンドウオープンコマンド
		/// </summary>
		public ReactiveCommand OpenSettingsWindowCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public MainWindowViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;
			var recipeViewrContainerViewModel = new RecipeViewerContainerViewModel(settings, logger).AddTo(this.CompositeDisposable);
			this.ContentItems = new TabItemViewModelBase[] {
				new CalendarViewModel(settings, logger).AddTo(this.CompositeDisposable),
				recipeViewrContainerViewModel
			};

			this.OpenSettingsWindowCommand.Subscribe(() => {
				using (var vm = new SettingsWindowViewModel(settings, logger)) {
					this.Messenger.Raise(new TransitionMessage(vm, "OpenSettingsWindow"));
				}
			}).AddTo(this.CompositeDisposable);
		}

		public void Initialize() {
			this.ContentItems.First().IsSelected.Value = true;

			var directoryPathes = new[] {
				this._settings.GeneralSettings.ImageDirectoryPath ,
				this._settings.GeneralSettings.CachesDirectoryPath ,
				this._settings.GeneralSettings.PluginsDirectoryPath
			};

			if (directoryPathes.Any(x => !Directory.Exists(x))) {
				this.OpenSettingsWindowCommand.Execute();
			}
		}

		public void SaveSettings() {
			this._settings.Save();
		}
	}
}
