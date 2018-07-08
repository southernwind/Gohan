using HtmlAgilityPack;

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

namespace SandBeige.RecipeWebSites.WeekCook.Models {
	public class WeekCookRecipe : RecipeBase {
		private readonly HttpClient _httpClient;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner"></param>
		internal WeekCookRecipe(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner, IRecipeSitePlugin plugin) : base(settings, logger, shoppingListOwner, plugin) {
			this._httpClient = new HttpClient().AddTo(this.Disposable);
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

				this.Title.Value = htmlDoc.QuerySelector(".ttlarea .ttlmain").InnerText.Trim();
				using (var stream = await this._httpClient.GetStreamAsync(
					new Uri(this.Url.Value, htmlDoc.QuerySelector(".grid-0 img.picture").Attributes["src"].Value))
				)
				using (var ms = new MemoryStream()) {
					stream.CopyTo(ms);
					this.Photo.Value = ms.ToArray();
				}
				this.Ingredients.Clear();

				var ingredients = new List<WeekCookRecipeIngredient>();
				foreach (var ingredient in htmlDoc.QuerySelectorAll("ul.ingredients_list li.ingredients_li")) {
					var item = new WeekCookRecipeIngredient(this);
					item.Id.Value = ingredients.Count + 1; // 自動採番

					item.AmountText.Value = ingredient.QuerySelector("div.quantity").InnerText.Trim();
					item.Name.Value = Regex.Replace(ingredient.QuerySelector("div.name").InnerText.Trim(), "[\r\n ]+", " ");

					ingredients.Add(item);
				}
				this.Ingredients.AddRange(ingredients);
				this.Steps.Clear();
				var steps = htmlDoc
					.QuerySelectorAll(".howto .howto_list article.howto_li")
					.Select((x, index) => {
						var step = new WeekCookRecipeStep(this.Settings, this.Logger);
						step.Number.Value = index + 1;
						step.StepText.Value = x.InnerText.Trim();

						return step;
					});
				foreach (var step in steps) {
					this.Steps.Add(step);
				}

				this.Yield.Value = htmlDoc.QuerySelector("h3.section_ttl01 span").InnerText.Trim();
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
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		protected override void ReadFromExtensionPropertyCollection() {
		}

		/// <summary>
		/// レシピ材料インスタンス作成
		/// </summary>
		/// <param name="name">材料名</param>
		/// <param name="amount">分量</param>
		/// <returns>レシピ材料インスタンス</returns>
		public override IRecipeIngredient CreateIngredientInstance(int id, string name, string amount) {
			var ingredient = new WeekCookRecipeIngredient(this);

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
			var step = new WeekCookRecipeStep(this.Settings, this.Logger);
			step.Number.Value = number;
			step.PhotoFilePath.Value = photoFilePath;
			step.StepText.Value = text;
			return step;
		}
	}
}
