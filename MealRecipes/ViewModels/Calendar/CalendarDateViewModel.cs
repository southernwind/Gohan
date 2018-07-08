
using Livet;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Calendar;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.ViewModels.Meal;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.ViewModels.Calendar {
	/// <summary>
	/// カレンダー日ViewModel
	/// カレンダーの1日分のViewModel
	/// </summary>
	class CalendarDateViewModel : ViewModel {
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		/// <summary>
		/// カレンダー日モデルインスタンス
		/// </summary>
		private readonly CalendarDateModel _calendarDate;

		/// <summary>
		/// 日
		/// </summary>
		public ReactiveProperty<DateTime> Date {
			get;
		}

		/// <summary>
		/// 祝日フラグ
		/// </summary>
		public ReadOnlyReactiveProperty<bool> IsHoliday {
			get;
		}

		/// <summary>
		/// 祝日名称
		/// </summary>
		public ReadOnlyReactiveProperty<string> HolidayName {
			get;
		}

		/// <summary>
		/// 曜日
		/// </summary>
		public ReadOnlyReactiveProperty<DayOfWeek> DayOfWeek {
			get;
		}

		/// <summary>
		/// 食事リスト
		/// </summary>
		public ReadOnlyReactiveCollection<MealViewModel> Meals {
			get;
		}

		/// <summary>
		/// 食事追加コマンド
		/// </summary>
		public ReactiveCommand AddMealCommand {
			get;
		} = new ReactiveCommand();

		/// <summary>
		/// 食事を横に並べる個数
		/// </summary>
		public ReadOnlyReactiveProperty<int> MealColumns {
			get;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="calendarDate">カレンダー日Model</param>
		public CalendarDateViewModel(ISettings settings, ILogger logger, CalendarDateModel calendarDate) {
			this._settings = settings;
			this._logger = logger;
			// モデルインスタンス
			this._calendarDate = calendarDate.AddTo(this.CompositeDisposable);

			// Property
			// 日
			this.Date = this._calendarDate.Date.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(this.CompositeDisposable);
			// 食事リスト
			this.Meals = this._calendarDate.Meals.ToReadOnlyReactiveCollection(
				x => new MealViewModel(settings, logger, x).AddTo(this.CompositeDisposable)
			).AddTo(this.CompositeDisposable);

			// 曜日
			this.DayOfWeek = this.Date.Select(x => x.DayOfWeek).ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// 祝日フラグ
			this.IsHoliday = this._calendarDate.IsHoliday.ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// 祝日名称
			this.HolidayName = this._calendarDate.HolidayName.Select(x => x ?? "").ToReadOnlyReactiveProperty().AddTo(this.CompositeDisposable);

			// Command
			// 食事追加コマンド
			this.AddMealCommand.Subscribe(this._calendarDate.AddMeal).AddTo(this.CompositeDisposable);

			// 食事列数
			this.MealColumns = this.Meals
				.CollectionChangedAsObservable()
				.Where(x => new[] {
					NotifyCollectionChangedAction.Add,
					NotifyCollectionChangedAction.Remove,
					NotifyCollectionChangedAction.Reset
				}.Contains(x.Action))
				.Select(x => Math.Min(3, this.Meals.Count))
				.ToReadOnlyReactiveProperty(Math.Min(3, this.Meals.Count))
				.AddTo(this.CompositeDisposable);
		}
	}
}