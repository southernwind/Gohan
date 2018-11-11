using Livet;

using Microsoft.EntityFrameworkCore;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Recipe.Search;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Extensions;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SandBeige.MealRecipes.Models.Recipe {
	/// <summary>
	/// 選択レシピModel
	/// </summary>
	class SearchRecipe : NotificationObject, IDisposable {
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		#region Search

		/// <summary>
		/// 検索ワード
		/// </summary>
		public ReactivePropertySlim<string> SearchWord {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 検索対象フラグ:タイトル
		/// </summary>
		public ReactivePropertySlim<bool> IsTitleSearchTarget {
			get;
		} = new ReactivePropertySlim<bool>(true);

		/// <summary>
		/// 検索対象フラグ:材料
		/// </summary>
		public ReactivePropertySlim<bool> IsIngredientSearchTarget {
			get;
		} = new ReactivePropertySlim<bool>(true);

		/// <summary>
		/// 検索対象フラグ:手順
		/// </summary>
		public ReactivePropertySlim<bool> IsStepSearchTarget {
			get;
		} = new ReactivePropertySlim<bool>(true);

		/// <summary>
		/// タグ候補
		/// </summary>
		public ObservableCollection<SelectableValue<string>> TagList {
			get;
		} = new ObservableCollection<SelectableValue<string>>();

		/// <summary>
		/// 最終利用日
		/// </summary>
		public ReactivePropertySlim<DateTime?> LastUsedDate {
			get;
		} = new ReactivePropertySlim<DateTime?>();

		/// <summary>
		/// 最終利用日以前フラグ
		/// Falseなら以後
		/// </summary>
		public ReactivePropertySlim<bool> IsBeforeLastUsedDate {
			get;
		} = new ReactivePropertySlim<bool>(true);

		/// <summary>
		/// 利用回数
		/// </summary>
		public ReactivePropertySlim<int?> UsageCount {
			get;
		} = new ReactivePropertySlim<int?>();

		/// <summary>
		/// 利用回数以上フラグ
		/// Falseなら以下
		/// </summary>
		public ReactivePropertySlim<bool> IsUsageCountMoreThan {
			get;
		} = new ReactivePropertySlim<bool>(true);

		/// <summary>
		/// アーカイブを含むかどうかのフラグ
		/// </summary>
		public ReactivePropertySlim<bool> IncludingArchive {
			get;
		} = new ReactivePropertySlim<bool>();

		/// <summary>
		/// 評価
		/// </summary>
		public ReadOnlyReactiveCollection<Composition.Recipe.Rating> Ratings {
			get;
		}

		/// <summary>
		/// 検索結果
		/// </summary>
		public ReactivePropertySlim<IRecipe[]> Result {
			get;
		} = new ReactivePropertySlim<IRecipe[]>(new IRecipe[] { });

		/// <summary>
		/// 検索条件プラグインリスト
		/// </summary>
		public ObservableCollection<IRecipeSearchConditionPlugin> SearchConditionPlugins {
			get;
		} = new ObservableCollection<IRecipeSearchConditionPlugin>();

		/// <summary>
		/// 追加されたプラグイン検索条件
		/// </summary>
		public ReactiveCollection<IRecipeSearchConditionModel> PluginSearchConditions {
			get;
		} = new ReactiveCollection<IRecipeSearchConditionModel>();

		/// <summary>
		/// ロード中フラグ
		/// </summary>
		public ReactivePropertySlim<bool> IsBusy {
			get;
		} = new ReactivePropertySlim<bool>();

		#endregion

		#region Sort

		public ObservableCollection<SortCondition> SortConditions {
			get;
		} = new ObservableCollection<SortCondition>();

		public ObservableCollection<ISortItem> SortItems {
			get;
		} = new ObservableCollection<ISortItem>();

		#endregion

		#region Page

		/// <summary>
		/// 現在ページ
		/// </summary>
		public ReactivePropertySlim<int> CurrentPage {
			get;
		} = new ReactivePropertySlim<int>(mode: ReactivePropertyMode.None);

		/// <summary>
		/// 最大ページ数
		/// </summary>
		public ReactivePropertySlim<int> MaxPage {
			get;
		} = new ReactivePropertySlim<int>();

		#endregion

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public SearchRecipe(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			// ソート条件削除時結果更新
			this.SortConditions.CollectionChangedAsObservable()
				.Merge(this.PluginSearchConditions.CollectionChangedAsObservable())
				.Where(x => x.Action == NotifyCollectionChangedAction.Remove)
				.Subscribe(changedItem => {
					this.CurrentPage.Value = 1;
				}).AddTo(this._disposable);

			// ソート条件追加
			this.SortItems.AddRange(new ISortItem[] {
				new SortItem<string>("タイトル", r => r.Title),
				new SortItem<int>("利用回数", r => r.MealRecipes.Count),
				new SortItem<DateTime>("最終利用日", r => r.MealRecipes.Max(mr => mr.Date))
			});

			// プラグイン検索条件追加
			this.SearchConditionPlugins.AddRange(Creator.RecipeSearchConditionPlugins);

			// 検索条件変更時結果更新
			Observable.Merge(
					this.SearchWord.Select(_ => Unit.Default),
					this.LastUsedDate.Select(_ => Unit.Default),
					this.UsageCount.Select(_ => Unit.Default),
					this.IsTitleSearchTarget.Select(_ => Unit.Default),
					this.IsIngredientSearchTarget.Select(_ => Unit.Default),
					this.IsStepSearchTarget.Select(_ => Unit.Default),
					this.IsBeforeLastUsedDate.Select(_ => Unit.Default),
					this.IsUsageCountMoreThan.Select(_ => Unit.Default),
					this.IncludingArchive.Select(_ => Unit.Default))
				.Throttle(new TimeSpan(0, 0, 0, 0, 500))
				.Subscribe(_ => {
					this.CurrentPage.Value = 1;
				}).AddTo(this._disposable);


			// タグ一覧取得
			this.LoadTagList();

			// テーブル更新時結果追従
			var updateTables = new[] {
				nameof(MealRecipeDbContext.IngredientExtensions),
				nameof(MealRecipeDbContext.Ingredients),
				nameof(MealRecipeDbContext.MealRecipes),
				nameof(MealRecipeDbContext.Ratings),
				nameof(MealRecipeDbContext.RecipeExtensions),
				nameof(MealRecipeDbContext.Recipes),
				nameof(MealRecipeDbContext.RecipeTags),
				nameof(MealRecipeDbContext.Tags),
				nameof(MealRecipeDbContext.Users)
			};

			// ページ変更時
			this.CurrentPage
				.Where(x => x != 0)
				.Select(x => Unit.Default)
				.Merge(
					this._settings
					.DbChangeNotifier
					.Received
					.Where(x => x.TableNames.Any(table => updateTables.Contains(table)))
					.Select(x => Unit.Default))
				.Select(x => new Random().Next())
				.ObserveOnDispatcher(System.Windows.Threading.DispatcherPriority.Background)
				.ObserveOn(ThreadPoolScheduler.Instance)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Subscribe(_ => this.Search())
				.AddTo(this._disposable);

			// タグ更新時サジェスト候補追従
			this._settings
				.DbChangeNotifier
				.Received
				.Where(x => x.TableNames.Contains(nameof(MealRecipeDbContext.Tags)))
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Subscribe(_ => this.LoadTagList())
				.AddTo(this._disposable);

			// 評価
			this.Ratings =
				this._settings
					.Caches
					.Users
					.ToReadOnlyReactiveCollection(
						u => {
							var rating = new Composition.Recipe.Rating(u, 0);
							rating.Value.Subscribe(_ => {
								this.CurrentPage.Value = 1;
							});
							return rating;
						}
					).AddTo(this._disposable);

		}

		/// <summary>
		/// 検索
		/// </summary>
		public void Search() {
			this._cancellationTokenSource?.Cancel();
			this._cancellationTokenSource?.Dispose();
			this._cancellationTokenSource = new CancellationTokenSource();
			var token = this._cancellationTokenSource.Token;

			this._logger.Log(LogLevel.Notice, "レシピ検索");
			this.IsBusy.Value = true;

			// 1ページあたりの表示件数
			var resultPerPage = this._settings.SearchSettings.ResultsPerPage;

			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				// 取得条件付加(Where)
				var query = this.SetSearchConditions(db.Recipes);
				this.MaxPage.Value = (int)Math.Ceiling((double)query.Count() / resultPerPage);
				if (this.MaxPage.Value < this.CurrentPage.Value) {
					this.Result.Value = new IRecipe[] { };
					this.IsBusy.Value = false;
					this.CurrentPage.Value = this.MaxPage.Value;
					return;
				}

				// 結果に含むサブテーブルデータを指定
				query = query.Include(x => x.Ingretients);

				// ソート順設定 (Order By)
				query = this.SetSortConditions(query);
				var task = query.Skip((this.CurrentPage.Value - 1) * resultPerPage)
					.Take(resultPerPage)
					.ToListAsync(token);

				try {
					task.Wait(token);
				} catch (Exception ex) when (ex is OperationCanceledException || ex is NullReferenceException || ex is TaskCanceledException) {
					// キャンセル時
					return;
				}

				this.Result.Value =
					task.Result
						.Select(x => {
							var recipe = Creator.CreateRecipeInstanceFromPluginName(this._settings, this._logger, x.PluginName);
							recipe.Id.Value = x.RecipeId;
							recipe.Title.Value = x.Title;
							recipe.ThumbnailFilePath.Value = x.ThumbnailFilePath;
							recipe.Ingredients.AddRange(
								x.Ingretients
									.Select(i => recipe.CreateIngredientInstance(i.IngredientId, i.Name, i.Amount))
									.OrderBy(i => i.Id.Value)
							);
							return recipe;
						}).ToArray();
				this._logger.Log(LogLevel.Notice, "検索完了");

			}
			this.IsBusy.Value = false;
		}

		/// <summary>
		/// 取得条件付加
		/// </summary>
		/// <param name="query">元クエリ</param>
		/// <returns>条件付加後クエリ</returns>
		private IQueryable<DataBase.Recipe> SetSearchConditions(IQueryable<DataBase.Recipe> query) {
			var q = query.Where(x => //ワード検索
				string.IsNullOrWhiteSpace(this.SearchWord.Value) ||
				this.IsTitleSearchTarget.Value && x.Title.Contains(this.SearchWord.Value ?? string.Empty) ||
				this.IsIngredientSearchTarget.Value && x.Ingretients.Any(i => i.Name.Contains(this.SearchWord.Value ?? string.Empty)) ||
				this.IsStepSearchTarget.Value && x.Steps.Any(s => s.StepText.Contains(this.SearchWord.Value ?? string.Empty))
			).Where(x => // タグ検索
				this.TagList.Where(t => t.Selected).Count() == 0 ||
				this.TagList
					.Where(t => t.Selected)
					.Select(t => t.Value)
					.All(s =>
						x.RecipeTags
							.Select(t => t.Tag.TagName)
							.Contains(s)
					)
			).Where(x => // 最終利用日
				this.LastUsedDate.Value == null ||
				x.MealRecipes.Any() &&
				(this.IsBeforeLastUsedDate.Value
					? x.MealRecipes.Max(mr => mr.Date) <= this.LastUsedDate.Value
					: x.MealRecipes.Max(mr => mr.Date) >= this.LastUsedDate.Value
				)
			).Where(x => // 利用回数
				this.UsageCount.Value == null ||
				(this.IsUsageCountMoreThan.Value
					? x.MealRecipes.Count >= this.UsageCount.Value
					: x.MealRecipes.Count <= this.UsageCount.Value
				)
			).Where(x => // アーカイブ
				this.IncludingArchive.Value || !x.IsArchived
			).Where(x => // 評価
				this.Ratings.All(r =>
					x.Ratings.Where(d => d.UserId == r.User.Value.Id.Value).Select(d => d.Value).SingleOrDefault() >= r.Value.Value
				)
			);

			return
				// プラグイン検索条件
				this.PluginSearchConditions
					.Select(x => x.GetFilteringFunction())
					.Aggregate(q, (qq, func) => func(qq));
		}

		/// <summary>
		/// ソート順付加
		/// </summary>
		/// <param name="query">元クエリ</param>
		/// <returns>条件付加後クエリ</returns>
		private IQueryable<DataBase.Recipe> SetSortConditions(IQueryable<DataBase.Recipe> query) {
			var q = query;
			foreach (var sc in this.SortConditions.Where(sc => sc.SortItem != null)) {
				if (sc.SortOrder == SortOrder.Ascending) {
					switch (sc.SortItem) {
						case SortItem<int> item:
							q = q.OrderBy(item.Key);
							break;
						case SortItem<string> item:
							q = q.OrderBy(item.Key);
							break;
						case SortItem<DateTime> item:
							q = q.OrderBy(item.Key);
							break;
					}
				} else {
					switch (sc.SortItem) {
						case SortItem<int> item:
							q = q.OrderByDescending(item.Key);
							break;
						case SortItem<string> item:
							q = q.OrderByDescending(item.Key);
							break;
						case SortItem<DateTime> item:
							q = q.OrderByDescending(item.Key);
							break;
					}
				}
			}
			return q;
		}

		/// <summary>
		/// タグ一覧読み込み
		/// </summary>
		public void LoadTagList() {
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				this.TagList.Clear();
				this.TagList.AddRange(
					db
					.Tags
					.Where(x => x.RecipeTags.Count != 0)
					.OrderByDescending(x => x.RecipeTags.Count)
					.ToList()
					.Select(x => {
						var tag = new SelectableValue<string>(x.TagName);
						Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
							h => tag.PropertyChanged += h,
							h => tag.PropertyChanged -= h)
						.Subscribe(e => {
							this.CurrentPage.Value = 1;
						});
						return tag;
					}).ToList()
				);
			}
		}

		/// <summary>
		/// ソート条件追加
		/// </summary>
		public void AddSortCondition() {
			var sc = new SortCondition();
			Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => sc.PropertyChanged += h,
					h => sc.PropertyChanged -= h)
				.Subscribe(e => {
					this.CurrentPage.Value = 1;
				});
			this.SortConditions.Add(sc);
		}

		/// <summary>
		/// ソート条件削除
		/// </summary>
		/// <param name="sortCondition">削除するソート条件</param>
		public void RemoveSortCondition(SortCondition sortCondition) {
			this.SortConditions.Remove(sortCondition);
		}

		/// <summary>
		/// プラグイン検索条件追加
		/// </summary>
		/// <param name="plugin">追加する検索条件プラグイン</param>
		public void AddPluginSearchCondition(IRecipeSearchConditionPlugin plugin) {
			var sc = plugin.CreateRecipeSearchConditionModel();
			Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
					h => sc.PropertyChanged += h,
					h => sc.PropertyChanged -= h)
				.Subscribe(e => {
					this.CurrentPage.Value = 1;
				});
			this.PluginSearchConditions.Add(sc);
		}

		/// <summary>
		/// プラグイン検索条件削除
		/// </summary>
		/// <param name="condition">削除するプラグイン検索条件</param>
		public void RemovePluginSearchCondition(IRecipeSearchConditionModel condition) {
			this.PluginSearchConditions.Remove(condition);
		}

		#region ページ移動

		/// <summary>
		/// 前のページへ移動
		/// </summary>
		public void GoToPreviousPage() {
			this.CurrentPage.Value--;
		}

		/// <summary>
		/// 次のページへ移動
		/// </summary>
		public void GoToNextPage() {
			this.CurrentPage.Value++;
		}

		public void PageTransition(int page) {
			this.CurrentPage.Value = page;
		}

		#endregion

		public void Dispose() {
			this._disposable.Dispose();
		}
	}

	#region Classes for Search
	public class SelectableValue<T> : NotificationObject {
		private bool _selected;

		public bool Selected {
			get {
				return this._selected;
			}
			set {
				if (this._selected == value) {
					return;
				}
				this._selected = value;
				this.RaisePropertyChanged();
			}
		}

		private T _value;

		public T Value {
			get {
				return this._value;
			}
			set {
				if (this._value?.Equals(value) ?? value == null) {
					return;
				}
				this._value = value;
				this.RaisePropertyChanged();
			}
		}

		public SelectableValue(T value) {
			this.Value = value;
		}
	}
	#endregion

	#region Classes for Sort

	/// <summary>
	/// ソート条件
	/// </summary>
	public class SortCondition : NotificationObject {
		private ISortItem _sortItem;

		public ISortItem SortItem {
			get {
				return this._sortItem;
			}
			set {
				if (this._sortItem == value) {
					return;
				}
				this._sortItem = value;
				this.RaisePropertyChanged();
			}
		}

		private SortOrder _sortOrder;

		public SortOrder SortOrder {
			get {
				return this._sortOrder;
			}
			set {
				if (this._sortOrder == value) {
					return;
				}
				this._sortOrder = value;
				this.RaisePropertyChanged();
			}
		}
	}

	/// <summary>
	/// ソートアイテム
	/// </summary>
	/// <typeparam name="TKey">アイテム型</typeparam>
	public class SortItem<TKey> : ISortItem {
		public SortItem(string name, Expression<Func<DataBase.Recipe, TKey>> key) {
			this.Name = name;
			this.Key = key;
		}

		public string Name {
			get;
		}

		public Expression<Func<DataBase.Recipe, TKey>> Key {
			get;
		}
	}

	public interface ISortItem {
		string Name {
			get;
		}
	}

	#endregion
}
