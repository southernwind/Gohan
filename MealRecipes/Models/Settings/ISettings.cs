using SandBeige.MealRecipes.Composition.Settings;

namespace SandBeige.MealRecipes.Models.Settings {
	public interface ISettings : IBaseSettings {
		/// <summary>
		/// マスタキャッシュ
		/// </summary>
		Master Master {
			get;
		}
	}
}
