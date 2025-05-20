using MyUtil;
using static 悲愴三国志Zero2_1.Code.DefType;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Turn {
		internal static int GetYear(Game game) => (game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.StartYear??0)+(game.PlayTurn??0)/BaseData.yearItems.Length;
		internal static int GetInYear(Game game) => (game.PlayTurn??0)%BaseData.yearItems.Length;
		internal static string? GetCalendarInYear(Game game) => BaseData.yearItems.ElementAtOrDefault(GetInYear(game));
		internal static string? GetCalendarText(Game game) => $"{GetYear(game)}年 {GetCalendarInYear(game)}";
	}
}
