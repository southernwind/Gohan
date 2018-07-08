namespace SandBeige.MealRecipes.Composition.Recipe {
	/// <summary>
	/// 拡張列
	/// </summary>
	public class ExtensionColumn {
		/// <summary>
		/// キー
		/// </summary>
		public string Key {
			get;
			set;
		}

		/// <summary>
		/// キー内で一意になるIndex
		/// </summary>
		public int Index {
			get;
			set;
		}

		/// <summary>
		/// データ
		/// </summary>
		public byte[] Data {
			get;
			set;
		}
	}
}
