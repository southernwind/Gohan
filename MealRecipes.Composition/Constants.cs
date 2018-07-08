namespace SandBeige.MealRecipes.Composition {
	/// <summary>
	/// 材料表示モード
	/// </summary>
	public enum IngredientDisplayMode {
		/// <summary>
		/// 通常表示
		/// </summary>
		Normal,

		/// <summary>
		/// お買い物表示
		/// </summary>
		Shopping
	}

	/// <summary>
	/// カレンダー表示モード
	/// </summary>
	public enum CalendarType {
		/// <summary>
		/// 詳細表示
		/// </summary>
		Details,

		/// <summary>
		/// カレンダー表示
		/// </summary>
		Calendar
	}

	/// <summary>
	/// 動作
	/// </summary>
	public enum Behavior {
		Delete,
		Download,
		Load,
		Register
	}

	/// <summary>
	/// ログレベル
	/// </summary>
	public enum LogLevel {
		Notice,
		Warning,
		Fatal
	}

	public class Constants {
	}
}
