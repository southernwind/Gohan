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

namespace SandBeige.RecipeWebSites.Cookpad.Models {
	public class CookpadRecipe : RecipeBase {
		private readonly HttpClient _httpClient;

		/// <summary>
		/// 作者
		/// </summary>
		public ReactivePropertySlim<CookpadRecipeAuthor> Author {
			get;
		} = new ReactivePropertySlim<CookpadRecipeAuthor>();

		/// <summary>
		/// コツ・ポイント
		/// </summary>
		public ReactivePropertySlim<string> Advice {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// レシピの生い立ち
		/// </summary>
		public ReactivePropertySlim<string> History {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// クックパッドレシピID
		/// </summary>
		public ReactivePropertySlim<string> CookpadRecipeId {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 公開日
		/// </summary>
		public ReactivePropertySlim<string> PublishedDate {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 更新日
		/// </summary>
		public ReactivePropertySlim<string> UpdateDate {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner"></param>
		internal CookpadRecipe(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner, IRecipeSitePlugin plugin) : base(settings, logger, shoppingListOwner, plugin) {
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

				this.Title.Value = htmlDoc.QuerySelector("#recipe-title h1").InnerText.Trim();
				using (var stream = await this._httpClient.GetStreamAsync(new Uri(htmlDoc.QuerySelector("#main-photo img").Attributes["data-large-photo"].Value)))
				using (var ms = new MemoryStream()) {
					stream.CopyTo(ms);
					this.Photo.Value = ms.ToArray();
				}
				this.Description.Value = htmlDoc.QuerySelector("#description .description_text").InnerText.Trim();
				this.Ingredients.Clear();
				var category = "";

				var ingredients = new List<CookpadRecipeIngredient>();
				foreach (var ingredient in htmlDoc.QuerySelectorAll("#ingredients_list .ingredient_row")) {
					if (ingredient.QuerySelector(".ingredient_name") != null) {
						if (ingredient.ChildNodes.Any(x => x.HasClass("ingredient_category"))) {
							var categoryNode = ingredient.QuerySelector(".ingredient_category");
							categoryNode.RemoveChild(categoryNode.QuerySelector("span"));
							category = categoryNode.InnerText.Trim();
							continue;
						}
						var item = new CookpadRecipeIngredient(this);
						item.Id.Value = ingredients.Count + 1; // 自動採番
						item.Category.Value = category;
						item.Name.Value = ingredient.QuerySelector(".ingredient_name").InnerText.Trim();
						item.AmountText.Value = ingredient.QuerySelector(".ingredient_quantity").InnerText;

						ingredients.Add(item);
					}
				}
				this.Ingredients.AddRange(ingredients);
				this.Steps.Clear();
				var steps = htmlDoc
					.QuerySelectorAll("#steps > [class^=step]")
					.Select(async (x, index) => {
						var cookpadRecipeStep = new CookpadRecipeStep(this.Settings, this.Logger);
						cookpadRecipeStep.Number.Value = index + 1;
						var photoUrl = x.QuerySelector(".image img")?.Attributes["data-large-photo"]?.Value;
						if (photoUrl != null) {
							using (var stream = await this._httpClient.GetStreamAsync(new Uri(photoUrl)))
							using (var ms = new MemoryStream()) {
								stream.CopyTo(ms);
								cookpadRecipeStep.Photo.Value = ms.ToArray();
							}
						}
						cookpadRecipeStep.StepText.Value = x.QuerySelector(".step_text").InnerText.Trim();

						return cookpadRecipeStep;
					});
				foreach (var step in steps) {
					this.Steps.Add(await step);
				}

				this.Author.Value = new CookpadRecipeAuthor {
					Name = htmlDoc.QuerySelector("#recipe_author_name").InnerText,
					Url = new Uri(this.Url.Value, htmlDoc.QuerySelector("#recipe_author_name").Attributes["href"].Value).AbsoluteUri
				};
				this.Advice.Value = htmlDoc.QuerySelector("#advice")?.InnerText.Trim();
				this.History.Value = htmlDoc.QuerySelector("#history")?.InnerText.Trim();

				var yield = htmlDoc.QuerySelector("#ingredients .yield")?.InnerText.Trim();
				if (yield != null) {
					yield = Regex.Replace(yield, @"^（(.*)）$", "$1");
				}
				this.Yield.Value = yield;
				this.CookpadRecipeId.Value = Regex.Replace(htmlDoc.QuerySelector("#recipe_id_and_published_date .recipe_id").InnerText.Trim(), "^.+: ", "");
				this.PublishedDate.Value = Regex.Replace(htmlDoc.QuerySelector("#published_date").InnerText.Trim(), "^.+: ", "");
				this.UpdateDate.Value = Regex.Replace(htmlDoc.QuerySelector("#recipe_id_and_published_date span:not([class])").InnerText.Trim(), "^.+: ", "");

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
				new ExtensionColumn { Key = nameof(this.Author), Data = this.Author.Value.Serialize() },
				new ExtensionColumn { Key = nameof(this.Advice), Data = this.Advice.Value.Serialize() },
				new ExtensionColumn { Key = nameof(this.History), Data = this.History.Value.Serialize() },
				new ExtensionColumn { Key = nameof(this.CookpadRecipeId), Data = this.CookpadRecipeId.Value.Serialize() },
				new ExtensionColumn { Key = nameof(this.PublishedDate), Data = this.PublishedDate.Value.Serialize() },
				new ExtensionColumn { Key = nameof(this.UpdateDate), Data = this.UpdateDate.Value.Serialize() }
			});
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		protected override void ReadFromExtensionPropertyCollection() {
			foreach (var ex in this.Extensions) {
				if (ex.Key == nameof(this.Author)) {
					this.Author.Value = ex.Data.Deserialize<CookpadRecipeAuthor>();
				} else if (ex.Key == nameof(this.Advice)) {
					this.Advice.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.History)) {
					this.History.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.CookpadRecipeId)) {
					this.CookpadRecipeId.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.PublishedDate)) {
					this.PublishedDate.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.UpdateDate)) {
					this.UpdateDate.Value = ex.Data.Deserialize<string>();
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
			var ingredient = new CookpadRecipeIngredient(this);

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
			var step = new CookpadRecipeStep(this.Settings, this.Logger);
			step.Number.Value = number;
			step.PhotoFilePath.Value = photoFilePath;
			step.StepText.Value = text;
			return step;
		}
	}
}
