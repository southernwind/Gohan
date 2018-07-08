using Livet;

using Microsoft.EntityFrameworkCore;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Models.Calendar;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace SandBeige.MealRecipes.Models.Meal {
	/// <summary>
	/// 食事Model
	/// </summary>
	public class MealModel : NotificationObject, IShoppingListOwner, IDisposable {
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		private Subject<Unit> _registerCompleted = new Subject<Unit>();
		/// <summary>
		/// 登録完了通知
		/// </summary>
		public IObservable<Unit> RegisterCompleted {
			get {
				return this._registerCompleted.AsObservable();
			}
		}

		/// <summary>
		/// 自身を保持しているカレンダー日インスタンス
		/// </summary>
		public CalendarDateModel Date {
			get;
			set;
		}

		/// <summary>
		/// 食事ID
		/// </summary>
		public ReactivePropertySlim<int> MealId {
			get;
		} = new ReactivePropertySlim<int>();

		/// <summary>
		/// 食事タイプ
		/// </summary>
		public ReactivePropertySlim<MealType> MealType {
			get;
		} = new ReactivePropertySlim<MealType>();

		/// <summary>
		/// 食事タイプリスト
		/// </summary>
		public ReadOnlyReactiveCollection<MealType> MealTypes {
			get;
		}

		/// <summary>
		/// レシピの自動保存
		/// </summary>
		public ReactivePropertySlim<bool> AutoSave {
			get;
		} = new ReactivePropertySlim<bool>();

		/// <summary>
		/// レシピリスト
		/// </summary>
		public ObservableCollection<IRecipe> Recipes {
			get;
		} = new ObservableCollection<IRecipe>();

		/// <summary>
		/// お買い物リスト
		/// </summary>
		public ReactiveCollection<IRecipeIngredient> ShoppingList {
			get;
		} = new ReactiveCollection<IRecipeIngredient>();

		private ReadOnlyReactiveCollection<IDisposable> _adjustmentChange;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		internal MealModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.MealTypes = this._settings.Master.MealTypes.ToReadOnlyReactiveCollection();

			this.ShoppingList
				.CollectionChangedAsObservable()
				.Where(_ => this.AutoSave.Value)
				.Throttle(new TimeSpan(0, 0, 1))
				.Subscribe(async x => {
					await this.RegisterShoppingListAsync();
				}).AddTo(this._disposable);

			// 食事の内容に変化があれば保存
			this.MealType.Where(x => x != null).Select(x => Unit.Default)
				.Merge(this.Recipes.CollectionChangedAsObservable().Select(x => Unit.Default))
				.Where(_ => this.AutoSave.Value)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Subscribe(_ => this.Register())
				.AddTo(this._disposable);

			// 食事の倍率に変化があれば保存
			this._adjustmentChange = this.Recipes.ToReadOnlyReactiveCollection(
				x => x.Adjustment
					.Where(_ => this.AutoSave.Value)
					.Throttle(TimeSpan.FromMilliseconds(50))
					.Subscribe(_ => this.Register()))
					.AddTo(this._disposable);

			// 不要になったものから破棄する
			this._adjustmentChange
				.CollectionChangedAsObservable()
				.Where(x =>
					new[] {
						NotifyCollectionChangedAction.Replace,
						NotifyCollectionChangedAction.Remove,
						NotifyCollectionChangedAction.Reset
					}.Contains(x.Action)
					&& (x.OldItems?.Count ?? 0) != 0
				).Subscribe(x => {
					foreach (var item in x.OldItems.Cast<IDisposable>()) {
						item.Dispose();
					}
				}).AddTo(this._disposable);

			this._registerCompleted.AddTo(this._disposable);
		}

		/// <summary>
		/// DB登録
		/// 更新時は削除→登録を行う
		/// </summary>
		public void Register() {
			this._logger.Log(LogLevel.Notice, $"食事登録 日={this.Date.Date.Value} 連番={this.MealId.Value}");
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext())
			using (var transaction = db.Database.BeginTransaction()) {
				var meal =
					db.Meals
						.SingleOrDefault(
							x =>
								x.Date == this.Date.Date.Value &&
								x.MealId == this.MealId.Value
						);
				if (meal == null) {
					meal = new DataBase.Meal {
						Date = this.Date.Date.Value,
						MealId = this.MealId.Value
					};

					db.Meals.Add(meal);
				} else {
					// 子テーブルの削除
					var targetTables = new[] {
						nameof(db.MealRecipes),
						nameof(db.ShoppingItems)
					};

					foreach (var table in targetTables) {
						db.Database.ExecuteSqlCommand($"DELETE FROM {table} WHERE {nameof(MealRecipe.Date)}={{0}} AND {nameof(MealRecipe.MealId)}={{1}}",
							this.Date.Date.Value,
							this.MealId.Value);
					}
				}

				meal.MealTypeId = this.MealType.Value.MealTypeId;
				meal.MealRecipes = this.Recipes.Select(recipe => new MealRecipe {
					RecipeId = recipe.Id.Value,
					Adjustment = recipe.Adjustment.Value
				}).ToList();
				meal.ShoppingList = this.ShoppingList.Select(i =>
					new ShoppingItem {
						RecipeId = i.Recipe.Id.Value,
						IngredientId = i.Id.Value
					}).ToList();

				db.SaveChanges();
				transaction.Commit();
				this._settings.DbChangeNotifier.Notify(nameof(db.Meals), nameof(db.MealRecipes), nameof(db.ShoppingItems));
				// 登録完了通知
				this._registerCompleted.OnNext(Unit.Default);
			}
			this._logger.Log(LogLevel.Notice, $"食事登録完了 日={this.Date.Date.Value} 連番={this.MealId.Value}");
		}

		/// <summary>
		/// お買物リスト登録
		/// </summary>
		/// <returns></returns>
		private async Task RegisterShoppingListAsync() {
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext())
			using (var transaction = db.Database.BeginTransaction()) {
				try {
					await db.Database.ExecuteSqlCommandAsync(
						$@"DELETE FROM {nameof(db.ShoppingItems)} WHERE {nameof(ShoppingItem.Date)}={{0}} AND {nameof(ShoppingItem.MealId)}={{1}}",
						this.Date.Date.Value,
						this.MealId.Value);
					db.ShoppingItems.AddRange(
						this.ShoppingList.Select(i =>
								new ShoppingItem {
									Date = this.Date.Date.Value,
									MealId = this.MealId.Value,
									RecipeId = i.Recipe.Id.Value,
									IngredientId = i.Id.Value
								}).
							ToList());
					await db.SaveChangesAsync();
					transaction.Commit();
					this._settings.DbChangeNotifier.Notify(nameof(db.ShoppingItems));
				} catch (DbUpdateException) {
					// 未登録のレシピの場合、外部キー制約エラーが発生するが、食事登録後に再度登録を行うため問題なし。
				}
			}
		}

		/// <summary>
		/// レシピ追加
		/// </summary>
		/// <param name="recipe"></param>
		public void AddRecipe(IRecipe recipe) {
			recipe.ShoppingListOwner.Value = this;
			this.Recipes.Add(recipe);
		}

		/// <summary>
		/// レシピ削除
		/// </summary>
		/// <param name="recipe"></param>
		public void RemoveRecipe(IRecipe recipe) {
			foreach (var s in this.ShoppingList.Where(x => x.Recipe.Id.Value == recipe.Id.Value).ToArray()) {
				this.ShoppingList.Remove(s);
			}
			this.Recipes.Remove(recipe);
		}

		/// <summary>
		/// 食事削除
		/// </summary>
		public void RemoveMeal() {
			this.Date.Meals.Remove(this);
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				var removeRow = db.Meals.SingleOrDefault(x => x.Date == this.Date.Date.Value && x.MealId == this.MealId.Value);
				if (removeRow == null) {
					return;
				}
				db.Meals.Remove(removeRow);
				db.SaveChanges();
				this._settings.DbChangeNotifier.Notify(nameof(db.Meals));
			}
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
