using MyUtil;
using Windows.UI;
using static 悲愴三国志Zero2_1.Code.Battle;
using static 悲愴三国志Zero2_1.Code.DefType;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Country {
		internal static Color? GetCountryColor(Game game,ECountry? country) => country?.MyApplyF(game.CountryMap.GetValueOrDefault)?.ViewColor;
		internal static decimal GetTotalAffairs(Game game,ECountry country) => game.AreaMap.Where(v => v.Value.Country==country).Sum(v => v.Value.AffairParam.AffairNow*(v.Key==game.CountryMap.GetValueOrDefault(country)?.CapitalArea ? 1.5m : 1m));
		internal static decimal GetAffairsPower(Game game,ECountry country) => Commander.GetAffairsCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.affair)).MyApplyF(affairsRank => affairsRank/5m+1);
		internal static decimal GetAffairsDifficult(Game game,ECountry country) => Math.Round((decimal)Math.Pow(GetAreaNum(game,country),0.5),4);
		internal static decimal GetInFunds(Game game,ECountry country) => (GetAreaNum(game,country)!=0 ? Math.Round(GetTotalAffairs(game,country)*GetAffairsPower(game,country)/GetAffairsDifficult(game,country),4) : 0)+10;
		internal static decimal GetOutFunds(Game game,ECountry country) {
			List<PersonParam> deployedPersonParams = [..Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role).ExceptBy(Person.GetWaitPostPersonMap(game,country,role).Keys,v => v.Key).Select(v => v.Value))];
			int roleCost = deployedPersonParams.Sum(v => v.Post?.PostKind.MaybeHead==PostHead.main ? 2 : v.Post?.PostKind.MaybeHead==PostHead.sub ? 1 : v.Post?.PostKind.MaybeArea!=null ? 1 : 0);
			int affairCost = deployedPersonParams.Sum(v => v.Post?.PostKind.MaybeArea!=null&&v.Post?.PostRole==ERole.affair ? v.Rank*5 : 0);
			decimal personCost = deployedPersonParams.Sum(v => v.Rank/5m);
			return roleCost+affairCost+personCost;
		}
		internal static decimal CalcAttackFunds(Game game,ECountry country) => Commander.GetAttackCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.attack)).MyApplyF(attackRank => (attackRank+2)*100);
		internal static EArea? GetTargetArea(Game game) => game.SelectTarget;
		internal static EArea? GetInvadeArea(Game game,ECountry? country) => country?.MyApplyF(game.CountryMap.GetValueOrDefault)?.Invade;
		internal static int GetAreaNum(Game game,ECountry country) => game.AreaMap.Values.Where(v => v.Country==country).Count();
		internal static ECountry? GetAreaCountry(Game game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.Country;
		internal static int? SuccessFindPersonRank(Game game,ECountry country) {
			decimal mainRank = Person.GetPostPerson(game,country,new(ERole.central,new(PostHead.main)))?.Value.MyApplyF(v => Person.CalcRank(v,ERole.central))??0;
			decimal subRank = Person.GetPostPerson(game,country,new(ERole.central,new(PostHead.sub)))?.Value.MyApplyF(v => Person.CalcRank(v,ERole.central))??0;
			decimal findPersonRank = mainRank+subRank/2;
			return !MyRandom.RandomJudge((double)(findPersonRank+1)/30) ? null : MyRandom.RandomJudge((double)findPersonRank/100) ? 2 : 1;
		}
		internal static bool IsFocusDefense(Dictionary<ECountry,EArea?> armyTargetMap,ECountry? country) => country!=null&&armyTargetMap.TryGetValue(country.Value,out EArea? target)&&target==null;
	}
}
