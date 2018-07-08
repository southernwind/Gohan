using Reactive.Bindings;

using System.Collections.Generic;

namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// レシピ手順
	/// </summary>
	public interface IRecipeStep {
		IReactiveProperty<int> Number {
			get;
		}

		/// <summary>
		/// 写真
		/// </summary>
		IReactiveProperty<byte[]> Photo {
			get;
		}

		/// <summary>
		/// 写真ファイルパス
		/// </summary>
		IReactiveProperty<string> PhotoFilePath {
			get;
		}

		/// <summary>
		/// 写真ファイルフルパス
		/// </summary>
		IReactiveProperty<string> PhotoFileFullPath {
			get;
		}

		/// <summary>
		/// 写真
		/// </summary>
		IReactiveProperty<byte[]> Thumbnail {
			get;
		}

		/// <summary>
		/// サムネイル写真ファイルパス
		/// </summary>
		IReactiveProperty<string> ThumbnailFilePath {
			get;
		}

		/// <summary>
		/// サムネイル写真ファイルフルパス
		/// </summary>
		IReactiveProperty<string> ThumbnailFileFullPath {
			get;
		}

		/// <summary>
		/// 手順テキスト
		/// </summary>
		IReactiveProperty<string> StepText {
			get;
		}

		/// <summary>
		/// 拡張プロパティリスト
		/// </summary>
		IEnumerable<ExtensionColumn> Extensions {
			get;
			set;
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		void RegisterToExtensionPropertyCollection();

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		void ReadFromExtensionPropertyCollection();
	}
}
