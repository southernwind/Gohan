using System;
using System.Globalization;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.Composition.ValidationRules {
	/// <summary>
	/// /日付検証Validation
	/// </summary>
	public class DateTimeValidationRule : ValidationRule {
		public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
			switch (value) {
				case null:
					return new ValidationResult(true, null);
				case string str:
					if (
						string.IsNullOrEmpty(str) ||
						DateTime.TryParseExact(str, "yyyy/M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)) {
						return new ValidationResult(true, null);
					}
					return new ValidationResult(false, "型不正");
				default:
					return new ValidationResult(false, "型不正");
			}
		}
	}
}
