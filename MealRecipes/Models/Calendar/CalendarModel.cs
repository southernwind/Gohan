using Livet;

using Microsoft.EntityFrameworkCore;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Extensions;
using SandBeige.MealRecipes.Models.Meal;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace SandBeige.MealRecipes.Models.Calendar {
	/// <summary>
	/// カレンダーModel
	/// </summary>
	public class CalendarModel : NotificationObject, IDisposable {
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();
		private CancellationTokenSource _cancellationTokenSource;

		/// <summary>
		/// 対象月
		/// DateTime型のうち利用しているのは年月まで
		/// </summary>
		public ReactivePropertySlim<DateTime> TargetMonth {
			get;
		} = new ReactivePropertySlim<DateTime>();

		/// <summary>
		/// 日リスト
		/// </summary>
		public ObservableCollection<CalendarDateModel> Dates {
			get;
		} = new ObservableCollection<CalendarDateModel>();

		/// <summary>
		/// ビジー中フラグ
		/// </summary>
		public ReactivePropertySlim<bool> IsBusy {
			get;
		} = new ReactivePropertySlim<bool>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		internal CalendarModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;
			this.TargetMonth.Value = DateTime.Now;

			// テーブル更新時追従
			var tables = new[] {
				nameof(MealRecipeDbContext.Holidays),
				nameof(MealRecipeDbContext.MealRecipes),
				nameof(MealRecipeDbContext.Meals),
				nameof(MealRecipeDbContext.ShoppingItems),
			};

			this._settings
				.DbChangeNotifier
				.Received
				.Where(x => x.TableNames.Any(t => tables.Contains(t)))
				.Select(x => Unit.Default)
				.Merge(this._settings.Master.MealTypes.CollectionChangedAsObservable().Select(x => Unit.Default))
				.Merge(this.TargetMonth.Select(x => Unit.Default))
				.ObserveOnDispatcher(System.Windows.Threading.DispatcherPriority.Background)
				.ObserveOn(ThreadPoolScheduler.Instance)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Subscribe(_ => this.LoadDates())
				.AddTo(this._disposable);
		}

		/// <summary>
		/// 日読み込み
		/// 対象月に対応した日リストを生成し、
		/// 生成した日に対応した食事がDBに登録されていればそれも読み込む
		/// </summary>
		public void LoadDates() {
			this._logger.Log(LogLevel.Notice, $"{this.TargetMonth.Value.Month}月カレンダー読み込み");

			this._cancellationTokenSource?.Cancel();
			this._cancellationTokenSource?.Dispose();
			this._cancellationTokenSource = new CancellationTokenSource();
			var token = this._cancellationTokenSource.Token;

			this.IsBusy.Value = true;

			// 月のはじめの日
			var targetMonthFirstDate = new DateTime(this.TargetMonth.Value.Year, this.TargetMonth.Value.Month, 1);
			// 月の初めの週の日曜日
			var targetMonthFirstWeekSunday = targetMonthFirstDate.AddDays(DayOfWeek.Sunday - targetMonthFirstDate.DayOfWeek);
			// 月の最後の日
			var targetMonthLastDate = new DateTime(
				this.TargetMonth.Value.Year,
				this.TargetMonth.Value.Month,
				DateTime.DaysInMonth(this.TargetMonth.Value.Year, this.TargetMonth.Value.Month)
			);
			// 月の最後の週の土曜日
			var targetMonthLastWeekSaturday = targetMonthLastDate.AddDays(DayOfWeek.Saturday - targetMonthLastDate.DayOfWeek);

			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				// 表示範囲日付
				var dateRange =
					Enumerable
						.Range(0, (targetMonthLastWeekSaturday - targetMonthFirstWeekSunday).Days + 1)
						.Select(x => targetMonthFirstWeekSunday.AddDays(x)).ToArray();

				// 表示範囲DBデータ
				var dbDataTask = db.Meals
					.Where(
						m => targetMonthFirstWeekSunday <= m.Date && m.Date <= targetMonthLastWeekSaturday
					)
					.Where(m => m.Date.IsBetween(dateRange.Min(), dateRange.Max()))
					.Include(m => m.MealRecipes)
					.ThenInclude(mr => mr.Recipe)
					.ThenInclude(r => r.Ingretients)
					.Include(m => m.MealType)
					.Include(m => m.ShoppingList)
					.ToListAsync(token);

				try {
					dbDataTask.Wait(token);
				} catch (Exception) {
					return;
				}

				// 祝日一覧取得
				var holidays = db.Holidays.Where(h => h.Date.IsBetween(dateRange.Min(), dateRange.Max())).ToArray();

				// 取得したDBデータを各Modelクラスに変換
				this.Dates.Clear();
				this.Dates.AddRange(
					dateRange
						.Select(x => {
							var cd = new CalendarDateModel(this._settings, this._logger, x);
							cd.HolidayName.Value = holidays.SingleOrDefault(h => h.Date == x)?.Name;
							cd.Meals.AddRange(
								dbDataTask
									.Result
									.Where(m => m.Date == x)
									.Select(m => {
										var meal = new MealModel(this._settings, this._logger) {
											Date = cd
										};
										meal.MealId.Value = m.MealId;
										meal.MealType.Value = meal.MealTypes.FirstOrDefault(mt => mt.MealTypeId == m.MealType.MealTypeId);
										meal.Recipes.AddRange(
											m.MealRecipes.Select(mr => (mr, mr.Recipe)).Select(mrr => {
												var r = mrr.Recipe;
												var mr = mrr.mr;
												var recipe = Creator.CreateRecipeInstanceFromPluginName(this._settings, this._logger, r.PluginName, meal);
												recipe.Id.Value = r.RecipeId;
												recipe.Title.Value = r.Title;
												recipe.ThumbnailFilePath.Value = r.ThumbnailFilePath;
												recipe.Adjustment.Value = mr.Adjustment;
												recipe.IsArchived.Value = r.IsArchived;
												recipe.Ingredients.AddRange(r.Ingretients.Select(i => recipe.CreateIngredientInstance(i.IngredientId, i.Name, i.Amount)));
												return recipe;
											})
										);
										meal.ShoppingList.AddRange(
											meal.Recipes
												.SelectMany(r => r.Ingredients)
												.Where(r => m.ShoppingList.Any(si => si.RecipeId == r.Recipe.Id.Value && si.IngredientId == r.Id.Value))
										);
										meal.AutoSave.Value = true;
										return meal;
									})
							);
							return cd;
						})
						.OrderBy(x => x.Date.Value)
					);

			}

			this.IsBusy.Value = false;

			this._logger.Log(LogLevel.Notice, $"{this.TargetMonth.Value.Month}月カレンダー読み込み完了");
		}

		/// <summary>
		/// 先月へ移動
		/// </summary>
		public void GoToPreviousMonth() {
			this._logger.Log(LogLevel.Notice, $"{this.TargetMonth.Value.Month}月");
			this.TargetMonth.Value = this.TargetMonth.Value.AddMonths(-1);
			this._logger.Log(LogLevel.Notice, $"{this.TargetMonth.Value.Month}月");
		}

		/// <summary>
		/// 来月へ移動
		/// </summary>
		public void GoToNextMonth() {
			this._logger.Log(LogLevel.Notice, $"{this.TargetMonth.Value.Month}月");
			this.TargetMonth.Value = this.TargetMonth.Value.AddMonths(1);
			this._logger.Log(LogLevel.Notice, $"{this.TargetMonth.Value.Month}月");
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
