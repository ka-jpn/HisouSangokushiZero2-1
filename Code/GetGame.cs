using MyUtil;
using static 悲愴三国志Zero2_1.Code.DefType;
namespace 悲愴三国志Zero2_1.Code {
	internal static class GetGame {
		private static ScenarioData.ScenarioInfo? GetScenario(Scenario newScenario) => ScenarioData.scenarios.GetValueOrDefault(newScenario);
		private static Game InitGame(Scenario scenario,ScenarioData.ScenarioInfo? scenarioInfo) => new(scenario,scenarioInfo?.AreaMap.ToDictionary()?? [],scenarioInfo?.CountryMap.ToDictionary()?? [],scenarioInfo?.PersonMap.ToDictionary()?? [],null,null,0,Phase.SelectScenario,[],false,[],[],[],[]);
		private static Game InitState(Game game) => game.MyApplyF(UpdateGame.InitAlivePersonPost).MyApplyF(UpdateGame.AutoPutPostCPU).MyApplyF(UpdateGame.UpdateCapitalArea);
		internal static Game GetInitGameScenario(Scenario scenario) => GetScenario(scenario).MyApplyF(maybeScenarioInfo => InitGame(scenario,maybeScenarioInfo)).MyApplyF(InitState);
	}
}
