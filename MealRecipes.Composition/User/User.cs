using Livet;

using Reactive.Bindings;

namespace SandBeige.MealRecipes.Composition.User {
	public class User : NotificationObject {
		/// <summary>
		/// ID
		/// </summary>
		public IReactiveProperty<int> Id {
			get;
		} = new ReactivePropertySlim<int>(mode: ReactivePropertyMode.DistinctUntilChanged);

		/// <summary>
		/// 名前
		/// </summary>
		public IReactiveProperty<string> Name {
			get;
		} = new ReactivePropertySlim<string>(mode: ReactivePropertyMode.DistinctUntilChanged);

		public User(int userId, string name) {
			this.Id.Value = userId;
			this.Name.Value = name;
		}
	}
}
