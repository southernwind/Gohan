
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.DataBase;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.Linq;
using System.Reactive.Linq;

using Holiday = SandBeige.MealRecipes.Utilities.Holiday;

namespace SandBeige.MealRecipes.ViewModels.Settings {
	class MasterEditorViewModel : SettingsPageViewModelBase {
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly Master _master;
		private readonly ICaches _caches;

		/// <summary>
		/// 設定名
		/// </summary>
		public override string Name {
			get;
		} = "マスタ編集";

		/// <summary>
		/// 選択状態
		/// </summary>
		public override ReactiveProperty<bool> IsSelected {
			get;
		} = new ReactiveProperty<bool>();

		/// <summary>
		/// 値検証結果
		/// </summary>
		public override ReactiveProperty<bool> IsValidated {
			get;
		}

		/// <summary>
		/// 祝日データ作成コマンド
		/// </summary>
		public AsyncReactiveCommand CreateHolidayDataCommand {
			get;
		} = new AsyncReactiveCommand();

		/// <summary>
		/// 追加用食事タイプ名
		/// </summary>
		public ReactivePropertySlim<string> MealNameToAdd {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 食事タイプリスト
		/// </summary>
		public ReadOnlyReactiveCollection<MealType> MealTypes {
			get;
		}
		/// <summary>
		/// 食事タイプ追加コマンド
		/// </summary>
		public ReactiveCommand AddMealTypeCommand {
			get;
		}

		/// <summary>
		/// 食事タイプ削除コマンド
		/// </summary>
		public ReactiveCommand<int> RemoveMealTypeCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// 追加用ユーザー名
		/// </summary>
		public ReactivePropertySlim<string> UserNameToAdd {
			get;
		} = new ReactivePropertySlim<string>();


		/// <summary>
		/// ユーザーリスト
		/// </summary>
		public ReadOnlyReactiveCollection<Composition.User.User> Users {
			get;
		}

		/// <summary>
		/// ユーザー追加コマンド
		/// </summary>
		public ReactiveCommand AddUserCommand {
			get;
		}

		/// <summary>
		/// ユーザー削除コマンド
		/// </summary>
		public ReactiveCommand<int> RemoveUserCommand {
			get;
		} = new ReactiveCommand<int>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		public MasterEditorViewModel(ISettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;
			this._master = this._settings.Master;
			this._caches = this._settings.Caches;

			this.MealTypes = this._master.MealTypes.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			this.Users = this._caches.Users.ToReadOnlyReactiveCollection().AddTo(this.CompositeDisposable);

			this.CreateHolidayDataCommand.Subscribe(async () => {
				await Holiday.CreateHolidayMaster(settings, logger);
			});

			this.AddMealTypeCommand = this.MealNameToAdd.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.AddMealTypeCommand.Subscribe(_ => {
				this._master.AddMealType(this.MealNameToAdd.Value);
				this.MealNameToAdd.Value = "";
			}).AddTo(this.CompositeDisposable);

			this.RemoveMealTypeCommand.Subscribe(id => {
				this._master.RemoveMealType(id);
			}).AddTo(this.CompositeDisposable);

			this.AddUserCommand = this.UserNameToAdd.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand().AddTo(this.CompositeDisposable);
			this.AddUserCommand.Subscribe(_ => {
				this._caches.Users.Add(new Composition.User.User(0, this.UserNameToAdd.Value));
				this.UserNameToAdd.Value = "";
			}).AddTo(this.CompositeDisposable);

			this.RemoveUserCommand.Subscribe(id => {
				this._caches.Users.Remove(this.Users.Single(x => x.Id.Value == id));
			}).AddTo(this.CompositeDisposable);

			this.IsValidated = new ReactiveProperty<bool>(true);
		}
	}
}
