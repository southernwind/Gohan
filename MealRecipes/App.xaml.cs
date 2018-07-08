using Livet;

using Reactive.Bindings;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Models.Settings;
using SandBeige.MealRecipes.Utilities;
using SandBeige.MealRecipes.ViewModels;

using System;
using System.IO;
using System.Windows;

namespace SandBeige.MealRecipes {
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application {
		private ILogger _logger;
		/// <summary>
		/// 初期処理
		/// </summary>
		/// <param name="e"></param>
		protected override void OnStartup(StartupEventArgs e) {
			DispatcherHelper.UIDispatcher = this.Dispatcher;
			UIDispatcherScheduler.Initialize();
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			this._logger = new Logger();
			var settings = new Settings(this._logger, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MealRecipes.settings"));
			settings.Load();

			// 各サイト読み込み
			Creator.PluginDirectory = settings.GeneralSettings.PluginsDirectoryPath;
			Creator.Load();

			using (var dataContext = settings.GeneralSettings.GetMealRecipeDbContext()) {
				// dataContext.Database.EnsureDeleted();
				if (dataContext.Database.EnsureCreated()) {

					// マスタデータ登録 とりあえずな
					dataContext.MealTypes.AddRange(
						new MealType {
							MealTypeId = 1,
							Name = "あさごはん"
						}, new MealType {
							MealTypeId = 2,
							Name = "ひるごはん"
						}, new MealType {
							MealTypeId = 3,
							Name = "ばんごはん"
						});
					dataContext.SaveChanges();
				}
			}
			settings.MasterLoad();
			settings.CachesLoad();

			this.MainWindow = new Views.MainWindow() {
				DataContext = new MainWindowViewModel(settings, this._logger)
			};
			this.MainWindow.ShowDialog();
		}

		/// <summary>
		/// 集約エラーハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			if (e.ExceptionObject is Exception ex) {
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				Console.WriteLine(ex.Message);
				this._logger.Log(LogLevel.Fatal, "ハンドリングしていない例外が発生", ex);
			} else {
				Console.WriteLine(e.ToString());
			}

			//TODO:ロギング処理など
			MessageBox.Show(
				"不明なエラーが発生しました。アプリケーションを終了します。",
				"エラー",
				MessageBoxButton.OK,
				MessageBoxImage.Error);

			Environment.Exit(1);
		}
	}
}
