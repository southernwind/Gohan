using HtmlAgilityPack;

using Reactive.Bindings;
using Reactive.Bindings.Extensions;

using SandBeige.MealRecipes.Composition;
using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Recipe;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SandBeige.RecipeWebSites.Cookien.Models {
	public class CookienRecipe : RecipeBase {
		private readonly HttpClient _httpClient;

		/// <summary>
		/// 保存期間
		/// </summary>
		public ReactivePropertySlim<TimeSpan> ShelfLife {
			get;
		} = new ReactivePropertySlim<TimeSpan>();

		/// <summary>
		/// メモ
		/// </summary>
		public ReactiveCollection<CookienMemo> CookienMemos {
			get;
		} = new ReactiveCollection<CookienMemo>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner"></param>
		internal CookienRecipe(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner, IRecipeSitePlugin plugin) : base(settings, logger, shoppingListOwner, plugin) {
			this._httpClient = new HttpClient().AddTo(this.Disposable);
		}

		/// <summary>
		/// つくおきメモ挿入
		/// </summary>
		/// <param name="index">挿入する位置</param>
		public void InsertCookienMemo(int index) {
			foreach (var cookienMemo in this.CookienMemos.Skip(index)) {
				cookienMemo.Index.Value++;
			}
			this.CookienMemos.Insert(index, new CookienMemo(index + 1, "", ""));
		}

		/// <summary>
		/// つくおきメモ削除
		/// </summary>
		/// <param name="index">削除する要素のインデックス</param>
		public void RemoveCookienMemoAt(int index) {
			foreach (var cookienMemo in this.CookienMemos.Skip(index)) {
				cookienMemo.Index.Value--;
			}
			this.CookienMemos.RemoveAt(index);
		}

		/// <summary>
		/// レシピダウンロード
		/// </summary>
		/// <returns></returns>
		public override async Task DownloadRecipeAsync() {
			try {
				var htmlString = await this._httpClient.GetStringAsync(this.Url.Value);
				var htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(htmlString);

				this.Title.Value = htmlDoc.QuerySelector("#content header h1").InnerText.Trim();
				using (var stream = await this._httpClient.GetStreamAsync(new Uri(htmlDoc.QuerySelector("#content img[itemprop=image]").Attributes["src"].Value)))
				using (var ms = new MemoryStream()) {
					stream.CopyTo(ms);
					this.Photo.Value = ms.ToArray();
				}
				this.Description.Value = htmlDoc.QuerySelector("#content span[itemprop=description]").InnerText.Trim();
				this.Ingredients.Clear();

				var ingredients = new List<CookienRecipeIngredient>();
				foreach (var ingredient in htmlDoc.QuerySelectorAll("#r_contents p")) {
					var item = new CookienRecipeIngredient(this);
					item.Id.Value = ingredients.Count + 1; // 自動採番

					item.AmountText.Value = ingredient.QuerySelector("span").InnerText.Trim();

					ingredient.RemoveChild(ingredient.QuerySelector("span"));
					item.Name.Value = ingredient.InnerText;

					ingredients.Add(item);
				}
				this.Ingredients.AddRange(ingredients);
				this.Steps.Clear();
				var steps = htmlDoc
					.QuerySelectorAll("#ins_contents > div")
					.Select((x, index) => {
						var step = new CookienRecipeStep(this.Settings, this.Logger);
						step.Number.Value = index + 1;
						step.StepText.Value = x.QuerySelector("p.ins_des").InnerText.Trim();

						return step;
					});
				foreach (var step in steps) {
					this.Steps.Add(step);
				}

				this.ShelfLife.Value = new TimeSpan(int.Parse(htmlDoc.QuerySelector("#content .recipe_info .recipe_info_right span.recipe_info_bold").InnerText.Trim()), 0, 0, 0);

				var memoIndex = 1;
				this.CookienMemos.Clear();
				while (true) {
					var memo = htmlDoc.QuerySelector($"#memo{memoIndex}");
					if (memo == null) {
						break;
					}
					this.CookienMemos.Add(new CookienMemo(memoIndex, memo.InnerText.Trim(), memo.NextSiblingElement().InnerText.Trim()));
					memoIndex++;
				}

				this.Yield.Value = Regex.Replace(htmlDoc.QuerySelector("#r_contents h1").InnerText.Trim(), "^.*?（(.*?)）$", "$1");
			} catch (Exception ex) {
				this._failedNotification.OnNext((this, Behavior.Download, ex));
				return;
			}

			this._completedNotification.OnNext((this, Behavior.Download));
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		protected override void RegisterToExtensionPropertyCollection() {
			this.Extensions.Clear();
			this.Extensions.AddRange(new[] {
				new ExtensionColumn { Key = nameof(this.ShelfLife), Data = this.ShelfLife.Value.Serialize() },
			}.Union(
				this.CookienMemos.Select(
					(x, i) =>
					new ExtensionColumn {
						Key = nameof(this.CookienMemos),
						Data = x.Serialize(),
						Index = i
					}
				)
			));
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		protected override void ReadFromExtensionPropertyCollection() {
			this.CookienMemos.Clear();
			foreach (var ex in this.Extensions.OrderBy(x => x.Key).ThenBy(x => x.Index)) {
				if (ex.Key == nameof(this.ShelfLife)) {
					this.ShelfLife.Value = ex.Data.Deserialize<TimeSpan>();
				} else if (ex.Key == nameof(this.CookienMemos)) {
					this.CookienMemos.Add(ex.Data.Deserialize<CookienMemo>());
				}
			}
		}

		/// <summary>
		/// レシピ材料インスタンス作成
		/// </summary>
		/// <param name="name">材料名</param>
		/// <param name="amount">分量</param>
		/// <returns>レシピ材料インスタンス</returns>
		public override IRecipeIngredient CreateIngredientInstance(int id, string name, string amount) {
			var ingredient = new CookienRecipeIngredient(this);

			ingredient.Id.Value = id;
			ingredient.Name.Value = name;
			ingredient.AmountText.Value = amount;
			return ingredient;
		}

		/// <summary>
		/// レシピ手順インスタンス作成
		/// </summary>
		/// <param name="number">番号</param>
		/// <param name="photoFilePath">写真ファイルパス</param>
		/// <param name="text">手順テキスト</param>
		/// <returns>レシピ手順インスタンス</returns>
		public override IRecipeStep CreateStepInstance(int number, string photoFilePath, string text) {
			var step = new CookienRecipeStep(this.Settings, this.Logger);
			step.Number.Value = number;
			step.PhotoFilePath.Value = photoFilePath;
			step.StepText.Value = text;
			return step;
		}
	}
}
