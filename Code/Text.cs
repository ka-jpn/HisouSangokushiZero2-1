using static 悲愴三国志Zero2_1.Code.DefType;
using CommanderType = 悲愴三国志Zero2_1.Code.DefType.Commander;
using PersonType = 悲愴三国志Zero2_1.Code.DefType.Person;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Text {
		private record InvadeTextInfo(string Country,string Commander,string Rank);
		private record InvadeText(InvadeTextInfo A,InvadeTextInfo B,EArea C,AttackJudge D);
		internal static string AreaInvadeText(Army attack,Army defense,EArea target,AttackJudge judge,Lang lang) => CalcAreaInvadeText(ArmyInfoToText(attack,lang),ArmyInfoToText(defense,lang),target,judge,lang);
		internal static string CountryInvadeText(Army attack,Army defense,EArea target,AttackJudge judge,Lang lang) => CalcCountryInvadeText(ArmyInfoToText(attack,lang),ArmyInfoToText(defense,lang),target,judge,lang);
		private static InvadeTextInfo ArmyInfoToText(Army army,Lang lang) => new(GetCountryText(army.Country,lang),CommanderToText(army.Commander,lang),army.Rank.ToString());
		private static string CalcCountryInvadeText(InvadeTextInfo src,InvadeTextInfo dest,EArea target,AttackJudge judge,Lang lang) => Ja.CalcCountryInvadeText(new(src,dest,target,judge));
		private static string CalcAreaInvadeText(InvadeTextInfo src,InvadeTextInfo dest,EArea target,AttackJudge judge,Lang lang) => Ja.CalcAreaInvadeText(new(src,dest,target,judge));
		internal static string GetAttackJudgeText(AttackJudge attackJudge,Lang lang) => Ja.GetAttackJudgeText(attackJudge);
		internal static string GetCountryText(ECountry? country,Lang lang) => Ja.GetCountryText(country);
		internal static string CommanderToText(CommanderType commander,Lang lang) => Ja.CommanderToText(commander);
		internal static string BattleDeathPersonText(ERole role,List<PersonType> deathPersons,Lang lang) => Ja.BattleDeathPersonText(role,deathPersons);
		public static string RoleToText(ERole role,Lang lang) => Ja.RoleToText(role);
		internal static string EndPhaseButtonText(Phase phase,Lang lang) => Ja.EndPhaseButtonText(phase);
		internal static string? DeathPersonText(List<PersonType> deathPersons,Lang lang) => Ja.DeathPersonText(deathPersons);
		private static class Ja {
			internal static string CalcCountryInvadeText(InvadeText m) => $"{m.A.Country}の{m.A.Commander}(ランク{m.A.Rank})が{m.C}にて{m.B.Country}の中央軍の{m.B.Commander}(ランク{m.B.Rank})に攻撃して{GetAttackJudgeText(m.D)}";
			internal static string CalcAreaInvadeText(InvadeText m) => $"{m.A.Country}の{m.A.Commander}(ランク{m.A.Rank})が{m.B.Country}領の{m.B.Commander}(ランク{m.B.Rank})が守備する{m.C}に侵攻して{GetAttackJudgeText(m.D)}";
			internal static string GetAttackJudgeText(AttackJudge attackJudge) => attackJudge switch { AttackJudge.crush => "大勝", AttackJudge.win => "辛勝", AttackJudge.lose => "惜敗", AttackJudge.rout => "大敗" };
			internal static string GetCountryText(ECountry? country) => country?.ToString()??"自治";
			internal static string CommanderToText(CommanderType commander) => commander.MainPerson==null&&commander.SubPerson==null ? "無名武官" : $"{commander.MainPerson?.Value??"無名武官"}と{commander.SubPerson?.Value??"無名武官"}";
			internal static string BattleDeathPersonText(ERole role,List<PersonType> deathPersons) => $"{(role==ERole.attack ? "攻撃" : "防衛")}側の{string.Join("と",deathPersons.Select(v => v.Value))}が退却できず戦死";
			internal static string RoleToText(ERole role) => role switch { ERole.central => "中枢", ERole.affair => "内政", ERole.defense => "防衛", ERole.attack => "攻撃" };
			internal static string EndPhaseButtonText(Phase phase) => phase==Phase.Starting ? "勢力決定" : phase==Phase.Planning ? "軍議終了" : "確認";
			internal static string? DeathPersonText(List<PersonType> deathPersons) => deathPersons.Count!=0 ? $"{string.Join("と",deathPersons.Select(v => v.Value))}が死去" : null;
		}
	}
}
