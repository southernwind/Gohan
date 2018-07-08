using Livet;

using Microsoft.EntityFrameworkCore;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Extensions;

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace SandBeige.MealRecipes.Models.Settings {
	public class Master : NotificationObject, IDisposable {
		private readonly ISettings _settings;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public ReactiveCollection<MealType> MealTypes {
			get;
		} = new ReactiveCollection<MealType>();

		public Master(ISettings settings) {
			this._settings = settings;
			this._settings
				.DbChangeNotifier
				.Received
				.Where(x => x.TableNames.Contains(nameof(MealRecipeDbContext.MealTypes)))
				.Throttle(TimeSpan.FromMilliseconds(50))
				.ObserveOnUIDispatcher()
				.Subscribe(_ => this.Load())
				.AddTo(this._disposable);
		}

		public void AddMealType(string name) {
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext())
			using (var tran = db.Database.BeginTransaction()) {
				var mt = new MealType { Name = name };
				db.MealTypes.Add(mt);
				db.SaveChanges();
				tran.Commit();
				this.MealTypes.Add(mt);
				this._settings.DbChangeNotifier.Notify(nameof(db.MealTypes));
			}
		}

		public void RemoveMealType(int mealTypeId) {
			using (var db = this._settings.GeneralSettings.GetMealRecipeDbContext())
			using (var tran = db.Database.BeginTransaction()) {
				db.Database.ExecuteSqlCommand($"DELETE FROM {nameof(db.MealTypes)} WHERE {nameof(MealType.MealTypeId)} = {{0}}", mealTypeId);
				tran.Commit();
				this.MealTypes.Remove(this.MealTypes.Single(x => x.MealTypeId == mealTypeId));
				this._settings.DbChangeNotifier.Notify(nameof(db.MealTypes));
			}
		}

		public void Load() {
			using (var dataContext = this._settings.GeneralSettings.GetMealRecipeDbContext()) {
				this.MealTypes.Clear();
				this.MealTypes.AddRange(dataContext.MealTypes);
			}
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}
}
