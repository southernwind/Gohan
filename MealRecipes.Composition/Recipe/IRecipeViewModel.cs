using Livet.Messaging;

using Reactive.Bindings;

using System;
using System.ComponentModel;

namespace SandBeige.MealRecipes.Composition.Recipe {
	public interface IRecipeViewModel : INotifyPropertyChanged {
		/// <summary>
		/// レシピモデル
		/// </summary>
		IRecipe Recipe {
			get;
			set;
		}

		/// <summary>
		/// 材料表示モード
		/// </summary>
		IReadOnlyReactiveProperty<IngredientDisplayMode> IngredientDisplayMode {
			get;
		}

		/// <summary>
		/// レシピID
		/// </summary>
		IReactiveProperty<int> Id {
			get;
		}

		/// <summary>
		/// レシピURL
		/// </summary>
		IReactiveProperty<Uri> Url {
			get;
		}

		/// <summary>
		/// レシピ名
		/// </summary>
		IReactiveProperty<string> Title {
			get;
		}

		/// <summary>
		/// レシピメインイメージ
		/// </summary>
		IReactiveProperty<byte[]> Photo {
			get;
		}

		/// <summary>
		/// レシピメインイメージファイルパス
		/// </summary>
		IReadOnlyReactiveProperty<string> PhotoFilePath {
			get;
		}

		/// <summary>
		/// サムネイル
		/// </summary>
		IReactiveProperty<byte[]> Thumbnail {
			get;
		}

		/// <summary>
		/// サムネイル
		/// </summary>
		IReadOnlyReactiveProperty<string> ThumbnailFilePath {
			get;
		}

		/// <summary>
		/// レシピ説明
		/// </summary>
		IReactiveProperty<string> Description {
			get;
		}

		/// <summary>
		/// 容量(XX人分)
		/// </summary>
		IReactiveProperty<string> Yield {
			get;
		}

		/// <summary>
		/// 材料
		/// </summary>
		ReadOnlyReactiveCollection<IRecipeIngredient> Ingredients {
			get;
		}

		/// <summary>
		/// お買物リスト
		/// </summary>
		IReadOnlyReactiveProperty<IRecipeIngredient[]> ShoppingList {
			get;
		}

		/// <summary>
		/// お買物リスト情報付きレシピ材料リスト
		/// </summary>
		IReadOnlyReactiveProperty<ShoppingInformationIncludedIngredient[]> ShoppingInformationIncludedIngredients {
			get;
		}

		/// <summary>
		/// 手順
		/// </summary>
		ReadOnlyReactiveCollection<IRecipeStep> Steps {
			get;
		}

		/// <summary>
		/// 調整比率
		/// </summary>
		IReactiveProperty<double> Adjustment {
			get;
		}

		/// <summary>
		/// メモ
		/// </summary>
		IReactiveProperty<string> Memo {
			get;
		}

		/// <summary>
		/// 所要時間
		/// </summary>
		IReactiveProperty<TimeSpan> RequiredTime {
			get;
		}

		/// <summary>
		/// 登録日
		/// </summary>
		IReactiveProperty<DateTime> RegistrationDate {
			get;
		}

		/// <summary>
		/// アーカイブ済みフラグ
		/// </summary>
		IReactiveProperty<bool> IsArchived {
			get;
		}

		/// <summary>
		/// タグリスト
		/// </summary>
		IReactiveProperty<string[]> Tags {
			get;
		}

		/// <summary>
		/// 評価一覧
		/// </summary>
		ReadOnlyReactiveCollection<Rating> Ratings {
			get;
		}

		/// <summary>
		/// ダウンロードコマンド
		/// </summary>
		AsyncReactiveCommand<InteractionMessenger> DownloadCommand {
			get;
		}

		/// <summary>
		/// レシピ登録
		/// </summary>
		AsyncReactiveCommand<InteractionMessenger> RegisterRecipeCommand {
			get;
		}

		/// <summary>
		/// レシピ削除
		/// </summary>
		AsyncReactiveCommand<InteractionMessenger> DeleteRecipeCommand {
			get;
		}

		/// <summary>
		/// レシピビュワー起動
		/// </summary>
		ReactiveCommand OpenRecipeDetailCommand {
			get;
		}

		/// <summary>
		/// レシピ読み込みコマンド
		/// </summary>
		ReactiveCommand LoadRecipeCommand {
			get;
		}

		/// <summary>
		/// 材料挿入コマンド
		/// </summary>
		ReactiveCommand<int> InsertIngredientCommand {
			get;
		}

		/// <summary>
		/// 手順挿入コマンド
		/// </summary>
		ReactiveCommand<int> InsertStepCommand {
			get;
		}

		/// <summary>
		/// 材料削除コマンド
		/// </summary>
		ReactiveCommand<int> RemoveIngredientCommand {
			get;
		}

		/// <summary>
		/// 手順削除コマンド
		/// </summary>
		ReactiveCommand<int> RemoveStepCommand {
			get;
		}

		/// <summary>
		/// アーカイブコマンド
		/// </summary>
		ReactiveCommand ArchiveCommand {
			get;
		}

		/// <summary>
		/// アーカイブ解除コマンド
		/// </summary>
		ReactiveCommand UnarchiveCommand {
			get;
		}
	}
}
