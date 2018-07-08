using System;

namespace SandBeige.MealRecipes.DataBase {
	/// <summary>
	/// 祝日
	/// </summary>
	public class Holiday {
		/// <summary>
		/// 日付
		/// </summary>
		public DateTime Date {
			get;
			set;
		}

		/// <summary>
		/// 祝日名称
		/// </summary>
		public string Name {
			get;
			set;
		}
	}
}
