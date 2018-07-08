
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Calendar;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.ViewModels.Calendar {
	/// <summary>
	/// カレンダーViewModel
	/// 1月分
	/// </summary>
	class CalendarViewModel : TabItemViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// カレンダーモデルインスタンス
		/// </summary>
		private readonly CalendarModel _calendar;

		/// <summary>
		/// タブアイテムの選択中フラグ
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// タブアイテム名
		/// </summary>
		public override string Name {
			get;
		} = "カレンダー";

		/// <summary>
		/// 対象月
		/// </summary>
		public ReactiveProperty<DateTime> TargetMonth {
			get;
		}

		/// <summary>
		/// 対象月の日リスト
		/// </summary>
		public ReadOnlyReactiveCollection<CalendarDateViewModel> Dates {
			get;
		}

		/// <summary>
		/// 選択中日
		/// </summary>
		public ReactiveProperty<CalendarDateViewModel> SelectedDate {
			get;
		} = new ReactiveProperty<CalendarDateViewModel>();

		/// <summary>
		/// 表示する日
		/// </summary>
		public ReadOnlyReactiveProperty<CalendarDateViewModel> DateToDisplay {
			get;
		}

		/// <summary>
		/// お買い物表示モードか否か
		/// </summary>
		public ReactiveProperty<bool> ShoppingMode {
			get;
		}

		/// <summary>
		/// カレンダー表示タイプ
		/// </summary>
		public ReactiveProperty<CalendarType> CalendarType {
			get;
		}

		/// <summary>
		/// ビジー中フラグ
		/// </summary>
		public ReadOnlyReactiveProperty<bool> IsBusy {
			get;
		}

		/// <summary>
		/// 前の月へ移動コマンド
		/// </summary>
		public ReactiveCommand GoToPreviousMonth {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// 次の月へ移動コマンド
		/// </summary>
		public ReactiveCommand GoToNextMonth {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// 読み込みコマンド
		/// </summary>
		public ReactiveCommand LoadDatesCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public CalendarViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;
			this._calendar = new CalendarModel(settings, logger).AddTo(this.CompositeDisposable);

			// Property
			this.Dates = this._calendar.Dates.ToReadOnlyReactiveCollection(x => new CalendarDateViewModel(settings, logger, x)).AddTo(this.CompositeDisposable);
			this.TargetMonth = this._calendar.TargetMonth.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);

			this.ShoppingMode =
				this._settings
					.States
					.IngredientDisplayMode
					.ToReactivePropertyAsSynchronized(
						x =>
							x.Value,
						x =>
							x == IngredientDisplayMode.Shopping,
						x =>
							x ?
								IngredientDisplayMode.Shopping :
								IngredientDisplayMode.Normal
					);

			this.CalendarType =
				this._settings
					.States
					.CalendarType
					.ToReactivePropertyAsSynchronized(x => x.Value)
					.AddTo(this.CompositeDisposable);

			this.DateToDisplay = this.SelectedDate.Where(x => x != null).ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			this.IsBusy = this._calendar.IsBusy.ToReadOnlyReactiveProperty();

			// Command
			this.GoToPreviousMonth.Subscribe(this._calendar.GoToPreviousMonth).AddTo(this.CompositeDisposable);
			this.GoToNextMonth.Subscribe(this._calendar.GoToNextMonth).AddTo(this.CompositeDisposable);
			this.LoadDatesCommand.Subscribe(_ => this._calendar.LoadDates()).AddTo(this.CompositeDisposable);
		}
	}
}
