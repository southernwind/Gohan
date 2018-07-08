using Livet;

using Reactive.Bindings;

using System;
using System.Reactive.Disposables;

namespace SandBeige.MealRecipes.Composition.Recipe {
	public class Rating : NotificationObject, IDisposable {
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public IReactiveProperty<User.User> User {
			get;
		} = new ReactivePropertySlim<User.User>(mode: ReactivePropertyMode.DistinctUntilChanged);

		public IReactiveProperty<int> Value {
			get;
		} = new ReactivePropertySlim<int>(mode: ReactivePropertyMode.DistinctUntilChanged);

		public Rating(User.User user, int value) {
			this.User.Value = user;
			this.Value.Value = value;
		}

		public void AddToCompositeDisposable(IDisposable disposable) {
			this._disposable.Dispose();
		}

		public void Dispose() {
			this._disposable.Dispose();
		}
	}

	public static class RatingExtensions {
		public static IDisposable AddToRatingCompositeDisposable(this IDisposable disposable, Rating rating) {
			rating.AddToCompositeDisposable(disposable);
			return disposable;
		}
	}
}
