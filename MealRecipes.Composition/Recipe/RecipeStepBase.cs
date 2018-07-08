using Reactive.Bindings;

using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Composition.Settings;
using SandBeige.MealRecipes.Composition.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;

namespace SandBeige.MealRecipes.Composition.Recipe {
	public abstract class RecipeStepBase : IRecipeStep {
		private readonly IBaseSettings _settings;
		private readonly ILogger _logger;

		public IReactiveProperty<int> Number {
			get;
		} = new ReactivePropertySlim<int>();

		/// <summary>
		/// 写真
		/// </summary>
		public IReactiveProperty<byte[]> Photo {
			get;
		} = new ReactivePropertySlim<byte[]>();

		/// <summary>
		/// サムネイルファイルパス
		/// </summary>
		public IReactiveProperty<string> PhotoFilePath {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// サムネイルファイルフルパス
		/// </summary>
		public IReactiveProperty<string> PhotoFileFullPath {
			get;
		}

		/// <summary>
		/// サムネイル写真
		/// </summary>
		public IReactiveProperty<byte[]> Thumbnail {
			get;
		} = new ReactivePropertySlim<byte[]>();

		/// <summary>
		/// サムネイルファイルパス
		/// </summary>
		public IReactiveProperty<string> ThumbnailFilePath {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// サムネイルファイルフルパス
		/// </summary>
		public IReactiveProperty<string> ThumbnailFileFullPath {
			get;
		}


		/// <summary>
		/// テキスト
		/// </summary>
		public IReactiveProperty<string> StepText {
			get;
		} = new ReactivePropertySlim<string>();

		/// <summary>
		/// 拡張プロパティリスト
		/// </summary>
		public IEnumerable<ExtensionColumn> Extensions {
			get;
			set;
		}

		public RecipeStepBase(IBaseSettings settings, ILogger logger) {
			this._settings = settings;
			this._logger = logger;

			this.PhotoFileFullPath = this.PhotoFilePath.Where(x => x != null).Select(x => Path.Combine(this._settings.GeneralSettings.ImageDirectoryPath, x)).ToReactiveProperty();
			this.ThumbnailFileFullPath = this.ThumbnailFilePath.Where(x => x != null).Select(x => Path.Combine(this._settings.GeneralSettings.ImageDirectoryPath, x)).ToReactiveProperty();

			this.Photo.Where(x => x != null).Subscribe(x => {
				using (var crypto = new SHA256CryptoServiceProvider()) {
					var filePath = string.Join("", crypto.ComputeHash(x).Select(b => $"{b:X2}")) + ".png";
					this.PhotoFilePath.Value = filePath;

					this.Thumbnail.Value = ThumbnailCreator.Create(x, 100, 100);
					var thumbFilePath = string.Join("", crypto.ComputeHash(this.Thumbnail.Value).Select(b => $"{b:X2}")) + ".png";
					this.ThumbnailFilePath.Value = thumbFilePath;
				}
			});

			this.PhotoFilePath.Where(x => x != null).Subscribe(x => {
				var fullpath = Path.Combine(this._settings.GeneralSettings.ImageDirectoryPath, x);

				if (!File.Exists(fullpath)) {
					File.WriteAllBytes(fullpath, this.Photo.Value);
				} else {
					this.Photo.Value = File.ReadAllBytes(fullpath);
				}
			});

			this.ThumbnailFilePath.Where(x => x != null).Subscribe(x => {
				var fullpath = Path.Combine(this._settings.GeneralSettings.ImageDirectoryPath, x);
				if (this.Thumbnail.Value != null && !File.Exists(fullpath)) {
					File.WriteAllBytes(fullpath, this.Thumbnail.Value);
				}
			});
		}

		/// <summary>
		/// 拡張プロパティコレクションへ登録
		/// </summary>
		public abstract void ReadFromExtensionPropertyCollection();

		/// <summary>
		/// 拡張プロパティコレクションから読み込み
		/// </summary>
		public abstract void RegisterToExtensionPropertyCollection();
	}
}
