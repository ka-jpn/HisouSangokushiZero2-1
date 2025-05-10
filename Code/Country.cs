using MyUtil;
using Windows.UI;
using static 悲愴三国志Zero2_1.Code.DefType;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Country {
		internal static Color? GetCountryColor(Game game,ECountry? country) => country?.MyApplyF(game.CountryMap.GetValueOrDefault)?.ViewColor;
		internal static decimal GetTotalAffair(Game game,ECountry country) => game.AreaMap.Where(v => v.Value.Country==country).Sum(v => v.Value.AffairParam.AffairNow*(v.Key==game.CountryMap.GetValueOrDefault(country)?.CapitalArea ? 1.5m : 1m));
		internal static decimal GetAffairPower(Game game,ECountry country) => Commander.GetAffairsCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.affair)).MyApplyF(affairsRank => affairsRank/5m+1);
		internal static decimal GetAffairDifficult(Game game,ECountry country) => Math.Round((decimal)Math.Pow(GetAreaNum(game,country),0.5),4);
		internal static decimal GetInFunds(Game game,ECountry country) => GetAreaNum(game,country)!=0 ? Math.Round(GetTotalAffair(game,country)*GetAffairPower(game,country)/GetAffairDifficult(game,country)+10m/GetAreaNum(game,country),4) : 0;
		internal static decimal GetOutFunds(Game game,ECountry country) {
			List<PersonParam> deployedPersonParams = [..Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role).ExceptBy(Person.GetWaitPostPersonMap(game,country,role).Keys,v => v.Key).Select(v => v.Value))];
			decimal backCost = Math.Round((decimal)(1-Math.Pow(0.9,(double)GetAffairDifficult(game,country)))*GetTotalAffair(game,country)/GetAffairDifficult(game,country),4);
			decimal roleCost = deployedPersonParams.Sum(v => v.Post?.PostKind.MaybeHead==PostHead.main ? 1 : v.Post?.PostKind.MaybeHead==PostHead.sub ? 0.5m : v.Post?.PostKind.MaybeArea!=null ? 0.5m : 0);
			decimal affairCost = deployedPersonParams.Sum(v => v.Post?.PostKind.MaybeArea!=null&&v.Post?.PostRole==ERole.affair ? v.Rank*2 : 0);
			decimal personCost = deployedPersonParams.Sum(v => v.Rank/10m);
			return backCost+roleCost+affairCost+personCost;
		}
		internal static decimal CalcAttackFunds(Game game,ECountry country) => Commander.GetAttackCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.attack)).MyApplyF(attackRank => (attackRank+1)*50);
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
