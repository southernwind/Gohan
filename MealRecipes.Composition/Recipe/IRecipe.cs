using Reactive.Bindings;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SandBeige.MealRecipes.Composition.Recipe {
	public interface IRecipe : INotifyPropertyChanged, IDisposable {
		/// <summary>
		/// 完了通知
		/// </summary>
		IObservable<(IRecipe Sender, Behavior Behavior)> CompletedNotification {
			get;
		}

		/// <summary>
		/// 失敗通知
		/// </summary>
		IObservable<(IRecipe Sender, Behavior Behavior, Exception Exception)> FailedNotification {
			get;
		}

		/// <summary>
		///	お買物リスト保有インスタンス
		/// </summary>
		IReactiveProperty<IShoppingListOwner> ShoppingListOwner {
			get;
		}

		/// <summary>
		/// 自動更新フラグ
		/// </summary>
		bool AutoReload {
			get;
			set;
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
		IReactiveProperty<string> PhotoFilePath {
			get;
		}

		/// <summary>
		/// サムネイル
		/// </summary>
		IReactiveProperty<byte[]> Thumbnail {
			get;
		}

		/// <summary>
		/// サムネイルファイルパス
		/// </summary>
		IReactiveProperty<string> ThumbnailFilePath {
			get;
		}

		/// <summary>
		/// レシピ説明
		/// </summary>
		IReactiveProperty<string> Description {
			get;
		}

		/// <summary>
		/// 容量
		/// </summary>
		IReactiveProperty<string> Yield {
			get;
		}

		IReadOnlyReactiveProperty<string> AdjustedYeild {
			get;
		}

		/// <summary>
		/// 材料
		/// </summary>
		ObservableCollection<IRecipeIngredient> Ingredients {
			get;
		}

		/// <summary>
		/// お買物リスト
		/// </summary>
		ReadOnlyReactivePropertySlim<IRecipeIngredient[]> ShoppingList {
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
		ObservableCollection<IRecipeStep> Steps {
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
		/// レシピデータダウンロード
		/// </summary>
		/// <returns><see cref="Task"/></returns>
		Task DownloadRecipeAsync();

		/// <summary>
		/// レシピDB登録
		/// </summary>
		/// <returns><see cref="Task"/></returns>
		Task RegistAsync();

		/// <summary>
		/// レシピ読み込み
		/// </summary>
		void Load();

		Task DeleteAsync();

		/// <summary>
		/// 材料挿入
		/// </summary>
		/// <param name="index">挿入する位置</param>
		void InsertIngredient(int index);

		/// <summary>
		/// 手順挿入
		/// </summary>
		/// <param name="index">挿入する位置</param>
		void InsertStep(int index);

		/// <summary>
		/// 材料削除
		/// </summary>
		/// <param name="index">削除する要素のインデックス</param>
		void RemoveIngredientAt(int index);

		/// <summary>
		/// 材料削除
		/// </summary>
		/// <param name="index">削除する要素のインデックス</param>
		void RemoveStepAt(int index);

		/// <summary>
		/// アーカイブ
		/// </summary>
		void Archive();

		/// <summary>
		/// アーカイブ解除
		/// </summary>
		void Unarchive();

		/// <summary>
		/// レシピ材料インスタンス作成
		/// </summary>
		/// <param name="id">材料Id</param>
		/// <param name="name">材料名</param>
		/// <param name="amount">分量</param>
		/// <returns>レシピ材料インスタンス</returns>
		IRecipeIngredient CreateIngredientInstance(int id, string name, string amount);

		/// <summary>
		/// レシピ手順インスタンス作成
		/// </summary>
		/// <param name="number">番号</param>
		/// <param name="photoFilePath">写真</param>
		/// <param name="text">手順テキスト</param>
		/// <returns>レシピ手順インスタンス</returns>
		IRecipeStep CreateStepInstance(int number, string photoFilePath, string text);
	}
}
