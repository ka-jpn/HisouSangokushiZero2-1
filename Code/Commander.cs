using MyUtil;
using static 悲愴三国志Zero2_1.Code.DefType;
using CommanderType = 悲愴三国志Zero2_1.Code.DefType.Commander;
using PersonType = 悲愴三国志Zero2_1.Code.DefType.Person;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Commander {
		private static Dictionary<PostHead,PersonType?> GetPersonMap(Game game,ECountry country,ERole role) => Enum.GetValues<PostHead>().ToDictionary(v => v,v => Person.GetPostPerson(game,country,new(role,new(v)))?.Key);
		private static Dictionary<PostHead,PersonType?> GetAreaDefensePersonMap(Game game,ECountry defense,EArea battleArea) => new([new(PostHead.main,Person.GetPostPerson(game,defense,new(ERole.defense,new(battleArea)))?.Key)]);
		private static CommanderType PersonMapToCommander(Dictionary<PostHead,PersonType?> targetPersonMap) => new(targetPersonMap.GetValueOrDefault(PostHead.main),targetPersonMap.GetValueOrDefault(PostHead.sub));
		internal static CommanderType GetCommander(Game game,ECountry? country,ERole role) => (country?.MyApplyF(v => GetPersonMap(game,v,role))?? []).MyApplyF(PersonMapToCommander);
		internal static CommanderType GetCentralCommander(Game game,ECountry? country) => GetCommander(game,country,ERole.central);
		internal static CommanderType GetAffairsCommander(Game game,ECountry? affairsCountry) => GetCommander(game,affairsCountry,ERole.affair);
		internal static CommanderType GetDefenseCommander(Game game,ECountry? defenseCountry) => GetCommander(game,defenseCountry,ERole.defense);
		internal static CommanderType GetAttackCommander(Game game,ECountry? attackCountry) => GetCommander(game,attackCountry,ERole.attack);
		internal static CommanderType AreaCommander(Game game,ECountry? defenseCountry,EArea defenseArea) => (defenseCountry?.MyApplyF(v => GetAreaDefensePersonMap(game,v,defenseArea))?? []).MyApplyF(PersonMapToCommander);
		internal static decimal CommanderRank(Game game,CommanderType commander,ERole role) => (commander.MainPerson?.MyApplyF(v => Person.CalcRank(game,v,role))??0)+(commander.SubPerson?.MyApplyF(v => Person.CalcRank(game,v,role)/2)??0);
	}
}
