using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MySql.Data.MySqlClient;

using System;

namespace SandBeige.MealRecipes.DataBase {
	public enum DataBaseType {
		MySQL,
		SQLite
	}

	public class MealRecipeDbContext : DbContext {
		private readonly string _dbPath;
		private readonly DataBaseType _dataBaseType;
		private readonly string _server;
		private readonly uint _port;
		private readonly string _user;
		private readonly string _password;
		private readonly string _database;


		/// <summary>
		/// レシピテーブル
		/// </summary>
		public DbSet<Recipe> Recipes {
			get;
			set;
		}

		/// <summary>
		/// レシピ拡張テーブル
		/// </summary>
		public DbSet<RecipeExtension> RecipeExtensions {
			get;
			set;
		}

		/// <summary>
		/// 材料テーブル
		/// </summary>
		public DbSet<Ingredient> Ingredients {
			get;
			set;
		}

		/// <summary>
		/// 材料拡張テーブル
		/// </summary>
		public DbSet<IngredientExtension> IngredientExtensions {
			get;
			set;
		}

		/// <summary>
		/// 手順テーブル
		/// </summary>
		public DbSet<Step> Steps {
			get;
			set;
		}

		/// <summary>
		/// 手順拡張テーブル
		/// </summary>
		public DbSet<StepExtension> StepExtensions {
			get;
			set;
		}

		/// <summary>
		/// 食事テーブル
		/// </summary>
		public DbSet<Meal> Meals {
			get;
			set;
		}

		/// <summary>
		/// 食事タイプIDマスタ
		/// </summary>
		public DbSet<MealType> MealTypes {
			get;
			set;
		}

		/// <summary>
		/// 食事、レシピ中間テーブル
		/// </summary>
		public DbSet<MealRecipe> MealRecipes {
			get;
			set;
		}

		/// <summary>
		/// レシピ、タグ中間テーブル
		/// </summary>
		public DbSet<RecipeTag> RecipeTags {
			get;
			set;
		}

		/// <summary>
		/// タグテーブル
		/// </summary>
		public DbSet<Tag> Tags {
			get;
			set;
		}

		/// <summary>
		/// 祝日テーブル
		/// </summary>
		public DbSet<Holiday> Holidays {
			get;
			set;
		}

		/// <summary>
		/// お買物リストテーブル
		/// </summary>
		public DbSet<ShoppingItem> ShoppingItems {
			get;
			set;
		}

		/// <summary>
		/// ユーザーテーブル
		/// </summary>
		public DbSet<User> Users {
			get;
			set;
		}

		/// <summary>
		/// 評価テーブル
		/// </summary>
		public DbSet<Rating> Ratings {
			get;
			set;
		}

		public MealRecipeDbContext(DataBaseType dataBaseType, string server, uint port, string user, string password, string database) {
			this._dataBaseType = dataBaseType;
			this._server = server;
			this._port = port;
			this._user = user;
			this._password = password;
			this._database = database;
		}

		/// <summary>
		/// コンストラクタ(SQLite)
		/// </summary>
		/// <param name="dbPath"></param>
		public MealRecipeDbContext(DataBaseType dataBaseType, string dbPath) {
			this._dataBaseType = dataBaseType;
			this._dbPath = dbPath;
		}

		/// <summary>
		/// テーブル設定
		/// </summary>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			// 食事
			modelBuilder.Entity<Meal>().HasKey(meal => new {
				meal.Date,
				meal.MealId
			});

			// 食事、レシピ中間テーブル
			modelBuilder.Entity<MealRecipe>().HasKey(mr => new {
				mr.Date,
				mr.MealId,
				mr.RecipeId
			});
			modelBuilder.Entity<MealRecipe>()
				.HasOne(mr => mr.Meal)
				.WithMany(
					m => m.MealRecipes
				)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<MealRecipe>()
				.HasOne(mr => mr.Recipe)
				.WithMany(
					r => r.MealRecipes
				)
				.OnDelete(DeleteBehavior.Restrict);

			// 食事、お買い物リスト(レシピ材料)中間テーブル
			modelBuilder.Entity<ShoppingItem>().
				HasKey(si => new {
					si.Date,
					si.MealId,
					si.RecipeId,
					si.IngredientId
				});
			modelBuilder.Entity<ShoppingItem>()
				.HasOne(si => si.Meal)
				.WithMany(m => m.ShoppingList)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<ShoppingItem>()
				.HasOne(si => si.Ingredient)
				.WithMany(i => i.ShoppingList)
				.OnDelete(DeleteBehavior.Cascade);

			// 食事タイプ
			modelBuilder.Entity<MealType>().HasKey(mealType => new {
				mealType.MealTypeId
			});

			// レシピ
			modelBuilder.Entity<Recipe>().HasKey(recipe => new {
				recipe.RecipeId
			});

			// レシピ拡張
			modelBuilder.Entity<RecipeExtension>().HasKey(extension => new {
				extension.RecipeId,
				extension.Key,
				extension.Index
			});

			// レシピ材料
			modelBuilder.Entity<Ingredient>().HasKey(ingredient => new {
				ingredient.RecipeId,
				ingredient.IngredientId
			});

			// レシピ材料拡張
			modelBuilder.Entity<IngredientExtension>().HasKey(extension => new {
				extension.RecipeId,
				extension.IngredientId,
				extension.Key,
				extension.Index
			});

			// レシピ手順
			modelBuilder.Entity<Step>().HasKey(step => new {
				step.RecipeId,
				step.StepId
			});

			// レシピ手順拡張
			modelBuilder.Entity<StepExtension>().HasKey(extension => new {
				extension.RecipeId,
				extension.StepId,
				extension.Key,
				extension.Index
			});

			// レシピ、タグ中間テーブル
			modelBuilder.Entity<RecipeTag>().HasKey(rt => new {
				rt.RecipeId,
				rt.TagId
			});

			modelBuilder.Entity<RecipeTag>()
				.HasOne(rt => rt.Recipe)
				.WithMany(
					r => r.RecipeTags
				)
				.OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<RecipeTag>()
				.HasOne(rt => rt.Tag)
				.WithMany(
					t => t.RecipeTags
				)
				.OnDelete(DeleteBehavior.Restrict);

			// タグ
			modelBuilder.Entity<Tag>().HasKey(tag => new {
				tag.TagId
			});

			// 祝日
			modelBuilder.Entity<Holiday>().HasKey(h => new {
				h.Date
			});

			// 評価
			modelBuilder.Entity<Rating>().HasKey(r => new {
				r.RecipeId,
				r.UserId
			});

			// ユーザー
			modelBuilder.Entity<User>().HasKey(u => new {
				u.UserId
			});
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			switch (this._dataBaseType) {
				case DataBaseType.SQLite:
					optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder { DataSource = this._dbPath }.ToString());
					break;
				case DataBaseType.MySQL:
					optionsBuilder.UseMySql(new MySqlConnectionStringBuilder {
						Server = this._server,
						Port = this._port,
						UserID = this._user,
						Password = this._password,
						Database = this._database
					}.ToString());
					break;
			}
#if DEBUG
			var factory = new LoggerFactory(new[] { new MealRecipeDbLoggerProvider() });
			optionsBuilder.UseLoggerFactory(factory);
#endif
		}
	}

	/// <summary>
	/// ログ出力クラス
	/// </summary>
	public class MealRecipeDbLoggerProvider : ILoggerProvider {
		public ILogger CreateLogger(string categoryName) {
			return new ConsoleLogger();
		}

		public void Dispose() {
		}

		private class ConsoleLogger : ILogger {
			public IDisposable BeginScope<TState>(TState state) {
				return null;
			}

			public bool IsEnabled(LogLevel logLevel) {
				return true;
			}

			public void Log<TState>(
				LogLevel logLevel,
				EventId eventId,
				TState state,
				Exception exception,
				Func<TState, Exception, string> formatter) {
				Console.WriteLine(formatter(state, exception));
			}
		}
	}
}
