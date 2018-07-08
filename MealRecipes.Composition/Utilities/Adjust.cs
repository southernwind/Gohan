using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SandBeige.MealRecipes.Composition.Utilities {
	public static class Adjust {
		private const string Kanji = "〇零一二三四五六七八九十壱弐参拾百佰千阡万萬億兆";
		private const string HalfWidth = "0-9";
		private const string FullWidth = "０-９";
		private const string Symbol = "./．／";
		private const string Other = "半[分玉丁]";
		private enum DigitType {
			Kanji,
			FullWidth,
			HalfWidth,
			Other,
			Unknown,
			NotDigit
		}

		/// <summary>
		/// 調整比率適用
		/// </summary>
		/// <param name="source">元文字列</param>
		/// <param name="adjustment">調整比率</param>
		/// <returns>調整比率適用後文字列</returns>
		public static string Apply(this string input, double adjustment) {
			try {
				if (adjustment == 1) {
					return input;
				}
				var digitRegex = new Regex($"^([{Kanji}]+|[{HalfWidth}{FullWidth}{Symbol}]+|{Other})");
				var words = new List<Word>();
				var temporary = "";
				while (input.Length != 0) {
					if (digitRegex.IsMatch(input)) {
						if (temporary.Length != 0) {
							words.Add(new Word(temporary, 0, DigitType.NotDigit));
							temporary = "";
						}
						words.Add(new Word(digitRegex.Match(input).Result("$1"), 0, DigitType.Unknown));
						input = digitRegex.Replace(input, "");
					} else {
						var firstChar = input[0];
						temporary += firstChar;
						input = Regex.Replace(input, $"^{firstChar}", "");
					}
				}
				if (temporary.Length != 0) {
					words.Add(new Word(temporary, 0, DigitType.NotDigit));
				}
				return string.Join("", words.Select(x => {
					if (x.DigitType == DigitType.Unknown) {
						if (x.Text.Where(c => c == '/' || c == '／').Count() == 1) {
							var splitted = x.Text.Split('/', '／');
							var num0 = ToNumber(splitted[0]);
							var num1 = ToNumber(splitted[1]);

							x.DigitType = num0.DigitType;
							x.Num = num0.Number / num1.Number;
						} else {
							(x.DigitType, x.Num) = ToNumber(x.Text);
						}
					}

					if (x.DigitType != DigitType.NotDigit) {
						x.Num *= adjustment;
					}

					switch (x.DigitType) {
						case DigitType.FullWidth:
							x.Text = Strings.StrConv(x.Num.ToString(), VbStrConv.Wide);
							break;
						case DigitType.HalfWidth:
							x.Text = x.Num.ToString();
							break;
						case DigitType.Other:
							x.Text = x.Num.ToString() + (x.Text[1] != '分' ? x.Text[1].ToString() : "");
							break;
						case DigitType.Kanji:
							x.Text = Kansuji.Convert((long)x.Num);
							break;
						default:
							break;

					}
					return x.Text;
				}));

			} catch (Exception) {
				return input;
			}
		}

		private static (DigitType DigitType, double Number) ToNumber(string Text) {
			if (Regex.IsMatch(Text, $"^[{Kanji}]")) {
				return (DigitType.Kanji, Kansuji.Convert(Text));
			} else if (Regex.IsMatch(Text, $"^[{HalfWidth}]")) {
				return (DigitType.HalfWidth, double.Parse(Strings.StrConv(Text, VbStrConv.Narrow)));
			} else if (Regex.IsMatch(Text, $"^[{FullWidth}]")) {
				return (DigitType.FullWidth, double.Parse(Strings.StrConv(Text, VbStrConv.Narrow)));
			} else if (Regex.IsMatch(Text, Other)) {
				return (DigitType.Other, 0.5);
			} else {
				return (DigitType.NotDigit, 0);
			}
		}

		private class Word {
			public Word(string text = "", double num = 0, DigitType digitType = DigitType.Unknown) {
				this.Text = text;
				this.Num = num;
				this.DigitType = digitType;
			}

			public string Text {
				get;
				set;
			}
			public double Num {
				get;
				set;
			}
			public DigitType DigitType {
				get;
				set;
			}
		}

		private static class Kansuji {

			private static (char Kanji, long Digit)[] convertTable = new[]{
				('一',1),
				('二',2),
				('三',3),
				('四',4),
				('五',5),
				('六',6),
				('七',7),
				('八',8),
				('九',9),
				('壱',1),
				('弐',2),
				('参',3),
				('十',10),
				('拾',10),
				('百',100),
				('佰',100),
				('千',1000),
				('阡',1000),
				('万',10000),
				('萬',10000),
				('億',100000000),
				('兆',1000000000000),
			};

			public static long Convert(string text) {
				if ("零〇".Contains(text.First())) {
					return 0;
				}
				var digit = 0L;
				var result = 0L;
				var num4 = 0L;
				foreach (var ch in text) {
					switch (ch) {
						case '一':
						case '二':
						case '三':
						case '四':
						case '五':
						case '六':
						case '七':
						case '八':
						case '九':
						case '壱':
						case '弐':
						case '参':
							digit = GetDigit(ch);
							break;
						case '十':
						case '百':
						case '千':
						case '拾':
						case '佰':
						case '阡':
							if (digit == 0) {
								digit = 1;
							}
							num4 += digit * GetDigit(ch);
							digit = 0;
							break;
						case '万':
						case '萬':
						case '億':
						case '兆':
							result += (num4 + digit) * GetDigit(ch);
							break;
					}
				}
				return result + num4 + digit;
			}

			public static string Convert(long number) {
				if (number == 0) {
					return "〇";
				}

				var result = "";
				var keta = 0;
				while (number > 0) {
					var n = number % 10;

					if (keta % 4 == 0 && keta != 0) {
						result = GetKanji((long)Math.Pow(10, keta)) + result;
					}

					if (keta % 4 != 0 && n == 1) {
						result = GetKanji((long)Math.Pow(10, keta % 4)) + result;
					} else if (n != 0) {
						result = GetKanji(n) + GetKanji((long)Math.Pow(10, keta % 4)) + result;
					}

					keta++;
					number /= 10;
				}
				return result;
			}

			private static char GetKanji(long digit) {
				return convertTable.First(x => x.Digit == digit).Kanji;
			}

			private static long GetDigit(char ch) {
				return convertTable.First(x => x.Kanji == ch).Digit;
			}
		}
	}
}
