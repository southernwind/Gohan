using Livet;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Meal;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.Models.Calendar {
	/// <summary>
	/// カレンダー日Model
	/// </summary>
	public class CalendarDateModel : NotificationObject, IDisposable {
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		/// <summary>
		/// 日付
		/// </summary>
		public ReactivePropertySlim<DateTime> Date {
			get;
		} = new ReactivePropertySlim<DateTime>();

		/// <summary>
		/// 祝日フラグ
		/// </summary>
		public ReadOnlyReactivePropertySlim<bool> IsHoliday {
			get;
		}

		/// <summary>
		/// 祝日名称
		/// </summary>
		public ReactivePropertySlim<string> HolidayName {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 食事リスト
		/// </summary>
		public ReactiveCollection<MealModel> Meals {
			get;
		} = new ReactiveCollection<MealModel>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="date">日</param>
		internal CalendarDateModel(ISettings settings, ILogger logger, DateTime date) {
			this.Date.Value = date;
			this._settings = settings;
			this._logger = logger;

			//Property
			// 祝日フラグ
			this.IsHoliday = this.HolidayName.Select(x => x != null).ToReadOnlyReactivePropertySlim().AddTo(this._disposable);
		}

		/// <summary>
		/// 食事追加
		/// </summary>
		public void AddMeal() {
			var meal = new MealModel(this._settings, this._logger) {
				Date = this
			};

			// MealIdの採番
			meal.MealId.Value = this.Meals.Select(x => x.MealId.Value).Concat(new[] { 0 }).Max() + 1;
			meal.AutoSave.Value = true;

			// 食事種別の初期値として、一番多く使用されている食事種別を選択
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				var id =
					db.Meals
					.GroupBy(x => x.MealTypeId)
					.Select(x => new { Id = x.Key, Count = x.Count() })
					.ToList()
					.Aggregate(
						(a, b) =>
							a.Count >= b.Count ?
							a :
							b
					)?.Id;

				if (id != null) {
					meal.MealType.Value =
						this
							._settings
							.Master
							.MealTypes
							.SingleOrDefault(x =>
								x.MealTypeId == id
									);
				}
			}
			this.Meals.Add(meal);
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
