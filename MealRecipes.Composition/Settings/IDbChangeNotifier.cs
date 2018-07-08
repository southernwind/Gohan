using System;

namespace SandBeige.MealRecipes.Composition.Settings {
	public interface IDbChangeNotifier : IDisposable {
		IObservable<Exception> Error {
			get;
		}

		IObservable<DbChangeArgs> Received {
			get;
		}

		void Notify(params string[] tables);
	}

	public class DbChangeArgs {
		public string Source {
			get;
			set;
		}

		public string[] TableNames {
			get;
			set;
		}

		public DbChangeArgs() {

		}

		public DbChangeArgs(string source, params string[] tableNames) {
			this.Source = source;
			this.TableNames = tableNames;
		}
	}
}
