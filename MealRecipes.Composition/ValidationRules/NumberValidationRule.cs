using System;
using System.Globalization;
using System.Windows.Controls;

namespace SandBeige.MealRecipes.Composition.ValidationRules {
	public class NumberValidationRule : ValidationRule {
		public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
			if(value is string str) {
				if (str.EndsWith(".")) {
					return new ValidationResult(false, "フォーマット不正");
				}
				if (double.TryParse(str, out var _)) {
					return new ValidationResult(true, null);
				}
			}
			return new ValidationResult(false, "フォーマット不正");
		}
	}
}
