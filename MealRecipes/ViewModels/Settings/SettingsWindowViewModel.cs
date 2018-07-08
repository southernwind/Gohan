using Livet;
using Livet.Messaging.Windows;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Settings;

using System.Linq;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.ViewModels.Settings {
	class SettingsWindowViewModel : ViewModel {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// 設定ページリスト
		/// </summary>
		public SettingsPageViewModelBase[] ContentItems {
			get;
		}

		/// <summary>
		/// 選択した設定ページ
		/// </summary>
		public ReactiveProperty<SettingsPageViewModelBase> SelectedContent {
			get;
		} = new ReactiveProperty<SettingsPageViewModelBase>();

		/// <summary>
		/// 設定保存コマンド
		/// </summary>
		public ReactiveCommand SaveCommand {
			get;
		}

		/// <summary>
		/// 設定保存して終了コマンド
		/// </summary>
		public ReactiveCommand SaveExitCommand {
			get;
		}

		/// <summary>
		/// キャンセルコマンド
		/// </summary>
		public ReactiveCommand CancelExitCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public SettingsWindowViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.ContentItems = new SettingsPageViewModelBase[] {
				new GeneralSettingsViewModel(settings, logger).AddTo(this.CompositeDisposable),
				new SearchSettingsViewModel(settings, logger).AddTo(this.CompositeDisposable),
				new NetworkSettingsViewModel(settings, logger).AddTo(this.CompositeDisposable),
				new MasterEditorViewModel(settings, logger).AddTo(this.CompositeDisposable)
			};

			var valid = this.ContentItems.Select(x => x.IsValidated).CombineLatestValuesAreAllTrue();
			this.SaveCommand = valid.ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.SaveCommand.Subscribe(() => {
				this._settings.Save();
			}).AddTo(this.CompositeDisposable);

			this.SaveExitCommand = valid.ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.SaveExitCommand.Subscribe(() => {
				this._settings.Save();
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			}).AddTo(this.CompositeDisposable);

			this.CancelExitCommand.Subscribe(() => {
				this._settings.Load();
				this.Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
			}).AddTo(this.CompositeDisposable);
		}

		public void Initialize() {
			this.ContentItems.First().IsSelected.Value = true;
		}

	}
}
