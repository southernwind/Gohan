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
using System.Web;

namespace SandBeige.RecipeWebSites.Rakuten.Models {
	public class RakutenRecipe : RecipeBase {
		private readonly HttpClient _httpClient;

		/// <summary>
		/// 作者
		/// </summary>
		public ReactivePropertySlim<RakutenRecipeAuthor> Author {
			get;
		} = new ReactivePropertySlim<RakutenRecipeAuthor>();

		/// <summary>
		/// おいしくなるコツ
		/// </summary>
		public ReactivePropertySlim<string> Advice {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// きっかけ
		/// </summary>
		public ReactivePropertySlim<string> History {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 楽天レシピID
		/// </summary>
		public ReactivePropertySlim<string> RakutenRecipeId {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 公開日
		/// </summary>
		public ReactivePropertySlim<string> PublishedDate {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="logger"></param>
		/// <param name="shoppingListOwner"></param>
		internal RakutenRecipe(IBaseSettings settings, ILogger logger, IShoppingListOwner shoppingListOwner, IRecipeSitePlugin plugin) : base(settings, logger, shoppingListOwner, plugin) {
			this._httpClient = new HttpClient().AddTo(this.Disposable);
			this._httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36");
			this._httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
			this._httpClient.DefaultRequestHeaders.Add("Accept-Language", "ja,en-US;q=0.9,en;q=0.8");
			this._httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
			this._httpClient.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
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

				this.Title.Value = htmlDoc.QuerySelector("#topicPath [itemprop=title] strong").InnerText.Trim();
				using (var stream = await this._httpClient.GetStreamAsync(new Uri(htmlDoc.QuerySelector("#detailContents .rcpPhotoBox img").Attributes["src"].Value)))
				using (var ms = new MemoryStream()) {
					stream.CopyTo(ms);
					this.Photo.Value = ms.ToArray();
				}
				this.Description.Value = htmlDoc.QuerySelector("#detailContents .ownerCom .summary").InnerText.Trim();

				this.Author.Value = new RakutenRecipeAuthor {
					Name = HttpUtility.HtmlDecode(htmlDoc.QuerySelector("#recipe_owner_mypage_link").InnerText).Trim(),
					Url = new Uri(this.Url.Value, htmlDoc.QuerySelector("#recipe_owner_mypage_link").Attributes["href"].Value).AbsoluteUri
				};

				this.Yield.Value = htmlDoc.QuerySelector("#detailContents .materialBox [itemprop=recipeYield]")?.InnerText.Trim();

				this.Ingredients.Clear();
				var ingredients = new List<RakutenRecipeIngredient>();
				foreach (var ingredient in htmlDoc.QuerySelectorAll("#detailContents .materialBox li[itemprop=ingredients]")) {
					var item = new RakutenRecipeIngredient(this);
					item.Id.Value = ingredients.Count + 1; // 自動採番
					item.Name.Value = ingredient.QuerySelector(".name").InnerText.Trim();
					item.AmountText.Value = ingredient.QuerySelector(".amount").InnerText;

					ingredients.Add(item);
				}
				this.Ingredients.AddRange(ingredients);
				this.Steps.Clear();
				var steps = htmlDoc
					.QuerySelectorAll("#detailContents .stepBox")
					.Select(async (x, index) => {
						var RakutenRecipeStep = new RakutenRecipeStep(this.Settings, this.Logger);
						RakutenRecipeStep.Number.Value = index + 1;
						var photoUrl = x.QuerySelector(".stepPhoto img")?.Attributes["src"].Value;
						if (photoUrl != null) {
							using (var stream = await this._httpClient.GetStreamAsync(new Uri(photoUrl)))
							using (var ms = new MemoryStream()) {
								stream.CopyTo(ms);
								RakutenRecipeStep.Photo.Value = ms.ToArray();
							}
						}
						RakutenRecipeStep.StepText.Value = x.QuerySelector(".stepMemo").InnerText.Trim();

						return RakutenRecipeStep;
					});
				foreach (var step in steps) {
					this.Steps.Add(await step);
				}
				this.History.Value = htmlDoc.QuerySelector("#detailContents .howtoPointBox:first-child p")?.InnerText.Trim();
				this.Advice.Value = htmlDoc.QuerySelector("#detailContents .howtoPointBox:last-child p")?.InnerText.Trim();
				this.RakutenRecipeId.Value = Regex.Replace(HttpUtility.HtmlDecode(htmlDoc.QuerySelector("#detailContents .rcpId").InnerText.Trim()), "^.+:.", "");
				this.PublishedDate.Value = htmlDoc.QuerySelector("#publish_day_itemprop").InnerText.Trim();
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
				new ExtensionColumn { Key = nameof(this.RakutenRecipeId), Data = this.RakutenRecipeId.Value.Serialize() },
				new ExtensionColumn { Key = nameof(this.PublishedDate), Data = this.PublishedDate.Value.Serialize() }
			});
		}

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		protected override void ReadFromExtensionPropertyCollection() {
			foreach (var ex in this.Extensions) {
				if (ex.Key == nameof(this.Author)) {
					this.Author.Value = ex.Data.Deserialize<RakutenRecipeAuthor>();
				} else if (ex.Key == nameof(this.Advice)) {
					this.Advice.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.History)) {
					this.History.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.RakutenRecipeId)) {
					this.RakutenRecipeId.Value = ex.Data.Deserialize<string>();
				} else if (ex.Key == nameof(this.PublishedDate)) {
					this.PublishedDate.Value = ex.Data.Deserialize<string>();
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
			var ingredient = new RakutenRecipeIngredient(this);

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
			var step = new RakutenRecipeStep(this.Settings, this.Logger);
			step.Number.Value = number;
			step.PhotoFilePath.Value = photoFilePath;
			step.StepText.Value = text;
			return step;
		}
	}
}
