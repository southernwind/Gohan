using System;

namespace SandBeige.MealRecipes.Composition.Logging {
	/// <summary>
	/// エラー出力Interface
	/// </summary>
	public interface ILogger {
		/// <summary>
		/// ログ出力
		/// </summary>
		/// <param name="level">ログレベル</param>
		/// <param name="message">内容</param>
		/// <param name="exception">例外オブジェクト</param>
		void Log(LogLevel Level, object message, Exception exception = null);
	}
}
