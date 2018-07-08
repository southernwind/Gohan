using Livet;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Composition.User;
using SandBeige.MealRecipes.Extensions;

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.Models.Settings {
	public class Caches : NotificationObject, ICaches {
		private ISettings _settings;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		private bool _isLoaded = false;

		/// <summary>
		/// ユーザ一覧
		/// </summary>
		public ReactiveCollection<User> Users {
			get;
		} = new ReactiveCollection<User>();

		public Caches(ISettings settings) {
			this._settings = settings;

			// 追加時DB追従
			this.Users
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Add && this._isLoaded)
				.Subscribe(ncc => {
					using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
						var items = ncc.NewItems.Cast<User>().Select(x => (obj: x, db: new DataBase.User() {
							Name = x.Name.Value
						})).ToList();
						// AddRangeでは主キーが更新されないようなので、地道にAddしていく
						foreach (var item in items) {
							db.Users.Add(item.db);
						}
						db.SaveChanges();
						foreach (var item in items) {
							item.obj.Id.Value = item.db.UserId;
						}
						this._settings.DbChangeNotifier.Notify(nameof(db.Users));
					}
				}).AddTo(this._disposable);

			// 削除時DB追従
			this.Users
				.CollectionChangedAsObservable()
				.Where(x => x.Action == NotifyCollectionChangedAction.Remove && this._isLoaded)
				.Subscribe(ncc => {
					using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
						db.Users.RemoveRange(db.Users.Where(x => ncc.OldItems.Cast<User>().Any(u => u.Id.Value == x.UserId)));
						db.SaveChanges();
						this._settings.DbChangeNotifier.Notify(nameof(db.Users));
					}
				}).AddTo(this._disposable);

			// DB変更時キャッシュ追従
			this._settings
				.DbChangeNotifier
				.Received.Where(x => x.TableNames.Contains(nameof(DataBase.MealRecipeDbContext.Users)))
				.Throttle(TimeSpan.FromMilliseconds(50))
				.Subscribe(_ => {
					this.Load();
				}).AddTo(this._disposable);


		}

		/// <summary>
		/// パーマネントストレージからキャッシュ元データのロード
		/// </summary>
		public void Load() {
			this._isLoaded = false;
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				this.Users.Clear();
				this.Users.AddRange(db.Users.Select(x => new User(x.UserId, x.Name)));
			}
			this._isLoaded = true;
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
