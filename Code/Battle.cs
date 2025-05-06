using MyUtil;
using static 悲愴三国志Zero2_1.Code.DefType;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Battle {
		private record ThresholdInfo(KeyValuePair<int,int> Lower,KeyValuePair<int,int> Upper);
		private static readonly Dictionary<int,int> crushingThresholds = new() { { int.MinValue,0 },{ int.MaxValue,0 } };
		private static readonly Dictionary<int,int> winThresholds = new() { { int.MinValue,2 },{ -5,2 },{ -3,4 },{ 3,16 },{ 5,24 },{ int.MaxValue,24 } };
		private static readonly Dictionary<int,int> defeatThresholds = new() { { int.MinValue,8 },{ -5,8 },{ -3,16 },{ 3,64 },{ 5,80 },{ int.MaxValue,80 } };
		private static readonly Dictionary<int,int> rowtThresholds = new() { { int.MinValue,76 },{ -5,76 },{ -3,84 },{ 3,96 },{ 5,98 },{ int.MaxValue,98 } };
		private static readonly Dictionary<AttackJudge,Dictionary<int,int>> thresholdMap = Enum.GetValues<AttackJudge>().Zip([crushingThresholds,winThresholds,defeatThresholds,rowtThresholds]).ToDictionary();
		internal static readonly int thresholdMax = 100;
		private static ThresholdInfo GetThresholdInfo(Dictionary<int,int> v,decimal attackRankSuperiority) => new(v.LastOrDefault(v => v.Key<=attackRankSuperiority),v.FirstOrDefault(v => v.Key>attackRankSuperiority));
		private static double GetThresholdFromInfo(ThresholdInfo t,decimal attackRankSuperiority) => double.Lerp(t.Lower.Value,t.Upper.Value,((double)attackRankSuperiority-t.Lower.Key)/(t.Upper.Key-t.Lower.Key));
		private static double GetThreshold(Dictionary<int,int> thresholdMap,decimal attackRankSuperiority) => GetThresholdInfo(thresholdMap,attackRankSuperiority).MyApplyF(v => GetThresholdFromInfo(v,attackRankSuperiority));
		internal static double GetThreshold(AttackJudge? attackJudge,decimal attackRankSuperiority) => attackJudge?.MyApplyF(thresholdMap.GetValueOrDefault)?.MyApplyF(v => GetThreshold(v,attackRankSuperiority))??thresholdMax;
		internal static AttackJudge JudgeAttack(decimal attackRank,decimal defenseRank) => MyRandom.GenerateDouble(0,thresholdMax).MyApplyF(rand => Enum.GetValues<AttackJudge>().FirstOrDefault(v => rand<=GetThreshold(v,attackRank-defenseRank)));
		internal static class Area {
			internal static AttackResult Attack(Game game,ECountry? defenseSide,EArea target,Army attack,Lang lang) => DefenseArmy(game,defenseSide,target).MyApplyF(defense => AttackJudge(attack,defense).MyApplyF(judge => new AttackResult(defense,judge,Text.AreaInvadeText(attack,defense,target,judge,lang))));
			private static Army DefenseArmy(Game game,ECountry? defense,EArea target) => Commander.AreaCommander(game,defense,target).MyApplyF(commander => new Army(defense,commander,Commander.CommanderRank(game,commander,ERole.defense)));
			private static AttackJudge AttackJudge(Army attack,Army defense) => JudgeAttack(attack.Rank,defense.Rank);
		}
		internal static class Country {
			internal static AttackResult? Attack(Game game,ECountry? defenseSide,EArea target,Army attack,Lang lang) => DefenseArmy(game,defenseSide).MyApplyF(defense => AttackJudge(attack,defense,Distance(game,defenseSide,target))?.MyApplyF(judge => new AttackResult(defense,judge,Text.CountryInvadeText(attack,defense,target,judge,lang))));
			private static Army DefenseArmy(Game game,ECountry? defense) => Commander.GetDefenseCommander(game,defense).MyApplyF(commander => new Army(defense,commander,Commander.CommanderRank(game,commander,ERole.defense)));
			private static int? Distance(Game game,ECountry? defense,EArea target) => defense?.MyApplyF(defense => Code.Area.GetCapitalArea(game,defense)?.MyApplyF(capitalArea => Code.Area.GetAreaDistance(game,defense,capitalArea,target)));
			private static AttackJudge? AttackJudge(Army attack,Army defense,int? dist) => (dist switch { 0 => 1, _ => (decimal?)null })?.MyApplyF(coefficient => JudgeAttack(attack.Rank,defense.Rank*coefficient));
		}
	}
}
