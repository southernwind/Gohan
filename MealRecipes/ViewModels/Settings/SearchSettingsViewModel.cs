using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Settings;

using System.ComponentModel.DataAnnotations;

namespace SandBeige.MealRecipes.ViewModels.Settings {
	class SearchSettingsViewModel : SettingsPageViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// 設定名
		/// </summary>
		public override string Name {
			get;
		} = "検索設定";

		/// <summary>
		/// ページの選択状態
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// 値検証結果
		/// </summary>
		public override ReactiveProperty<bool> IsValidated {
			get;
		}

		/// <summary>
		/// 1ページあたりの結果表示件数
		/// </summary>
		[Range(1, 300)]
		public ReactiveProperty<int> ResultsPerPage {
			get;
		}

		/// <summary>
		/// レシピ詳細の自動表示
		/// </summary>
		public ReactiveProperty<bool> AutomaticDisplayRecipeDetail {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public SearchSettingsViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.ResultsPerPage =
				this._settings
					.SearchSettings
					.ToReactivePropertyAsSynchronized(x => x.ResultsPerPage)
					.SetValidateAttribute(() => this.ResultsPerPage)
					.AddTo(this.CompositeDisposable);

			this.AutomaticDisplayRecipeDetail =
				this._settings
					.SearchSettings
					.ToReactivePropertyAsSynchronized(x => x.AutomaticDisplayRecipeDetail)
					.AddTo(this.CompositeDisposable);

			this.IsValidated = new[] {
					this.ResultsPerPage.ObserveHasErrors
				}.CombineLatestValuesAreAllFalse()
				.ToReactiveProperty()
				.AddTo(this.CompositeDisposable);
		}
	}
}
