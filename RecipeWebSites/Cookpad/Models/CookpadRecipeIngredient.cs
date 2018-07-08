
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Extensions;

namespace SandBeige.RecipeWebSites.Cookpad.Models {
	/// <summary>
	/// 材料
	/// </summary>
	public class CookpadRecipeIngredient : RecipeIngredientBase {
		public CookpadRecipeIngredient(IRecipe recipe) : base(recipe) {
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		public override void RegisterToExtensionPropertyCollection() {
			this.Extensions = new[] {
				new ExtensionColumn { Key = nameof(this.Category), Data = this.Category.Value?.Serialize() }
			};
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		public override void ReadFromExtensionPropertyCollection() {
			foreach (var ex in this.Extensions) {
				if (ex.Key == nameof(this.Category)) {
					this.Category.Value = ex.Data.Deserialize<string>();
				}
			}
		}
	}
}
