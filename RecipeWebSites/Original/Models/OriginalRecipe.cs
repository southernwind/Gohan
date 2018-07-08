using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;

using System.Threading.Tasks;

namespace SandBeige.RecipeWebSites.Original.Models {
	/// <summary>
	/// レシピModel
	/// </summary>
	class OriginalRecipe : RecipeBase {
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner">お買物リスト保持インスタンス</param>
		/// <param name="plugin">親プラグイン</param>
		internal OriginalRecipe(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner, IRecipeSitePlugin plugin) : base(settings, logger, shoppingListOwner, plugin) {
		}

		/// <summary>
		/// 材料インスタンス作成
		/// </summary>
		/// <param name="id">材料ID</param>
		/// <param name="name">材料名</param>
		/// <param name="amount">量</param>
		/// <returns>材料インスタンス</returns>
		public override IRecipeIngredient CreateIngredientInstance(int id, string name, string amount) {
			var ingredient = new OriginalRecipeIngredient(this);

			ingredient.Id.Value = id;
			ingredient.Name.Value = name;
			ingredient.AmountText.Value = amount;
			return ingredient;
		}

		/// <summary>
		/// 手順インスタンス作成
		/// </summary>
		/// <param name="number">手順番号</param>
		/// <param name="photoFilePath">写真ファイルパス</param>
		/// <param name="text">テキスト</param>
		/// <returns>手順インスタンス</returns>
		public override IRecipeStep CreateStepInstance(int number, string photoFilePath, string text) {
			var step = new OriginalRecipeStep(this.Settings, this.Logger);
			step.Number.Value = number;
			step.PhotoFilePath.Value = photoFilePath;
			step.StepText.Value = text;
			return step;
		}

		/// <summary>
		/// ダウンロードしないため実装不要
		/// </summary>
		/// <returns></returns>
		public override Task DownloadRecipeAsync() {
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		protected override void ReadFromExtensionPropertyCollection() {
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		protected override void RegisterToExtensionPropertyCollection() {
		}
	}
}
