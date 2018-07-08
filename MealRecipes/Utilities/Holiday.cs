using SandBeige.MealRecipes.Composition.Logging;
using SandBeige.MealRecipes.Models.Settings;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SandBeige.MealRecipes.Utilities {
	public static class Holiday {
		internal static async Task CreateHolidayMaster(ISettings settings, ILogger logger) {
			using (var hc = new HttpClient())
			using (var db = settings.GeneralSettings.GetMealRecipeDbContext())
			using (var transaction = db.Database.BeginTransaction())
			using (var stream = await hc.GetStreamAsync("http://www8.cao.go.jp/chosei/shukujitsu/syukujitsu.csv"))
			using (var sr = new StreamReader(stream, Encoding.GetEncoding("Shift-JIS"))) {
				var csv = await sr.ReadToEndAsync();

				var holidays =
					csv.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
						.Skip(1)
						.Select(x => x.Split(','))
						.Select(x => (Date: DateTime.Parse(x[0]), Name: x[1])).ToList();

				// 振替休日
				foreach (var (date, name) in holidays.Where(x => x.Date.DayOfWeek == DayOfWeek.Sunday).ToList()) {
					var day = date;
					while (true) {
						day = day.AddDays(1);
						if (day.DayOfWeek == DayOfWeek.Sunday || holidays.Select(x => x.Date).Contains(day)) {
							continue;
						}
						holidays.Add((day, "振替休日"));
						break;
					}
				}

				foreach (var (date, name) in holidays) {
					db.Holidays.RemoveRange(db.Holidays.Where(x => x.Date == date));
					await db.SaveChangesAsync();
					db.Holidays.Add(new DataBase.Holiday {
						Date = date,
						Name = name
					});
					await db.SaveChangesAsync();
				}

				transaction.Commit();
				settings.DbChangeNotifier.Notify(nameof(db.Holidays));
			}
		}
	}
}
