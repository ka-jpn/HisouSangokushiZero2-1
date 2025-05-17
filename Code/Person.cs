using MyUtil;
using static 悲愴三国志Zero2_1.Code.DefType;
using PostType = 悲愴三国志Zero2_1.Code.DefType.Post;
using PersonType = 悲愴三国志Zero2_1.Code.DefType.Person;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Person {
		private static bool IsInitPerson(Game game,PersonParam personParam) => GetAppearYear(personParam)<Turn.GetYear(game);
		private static bool IsAppearPerson(Game game,PersonParam personParam) => GetAppearYear(personParam)==Turn.GetYear(game)&&Turn.GetInYear(game)==BaseData.yearItems.Length/2;
		private static bool IsAlivePerson(PersonParam personParam) => personParam.Post!=null;
		private static bool IsWaitPostPerson(PersonParam personParam) => personParam.Post is PostType post&&post.PostKind.MyApplyF(v => v.MaybeArea==null&&v.MaybeHead==null&&v.MaybePostNo==null);
		private static bool IsNaturalDeathPerson(int year,PersonParam personParam) => year>=personParam.DeathYear&&personParam.Post!=null&&personParam.GameDeathTurn==null&&MyRandom.RandomJudge(0.1+(year-personParam.DeathYear)*0.09);
		private static Dictionary<PersonType,PersonParam> GetRolePersonMap(Game game,ECountry country,ERole role) => game.PersonMap.Where(v => v.Value.Country==country&&(v.Value.Post?.MyApplyF(v => v.PostRole==role)??v.Value.Role==role)).ToDictionary();
		internal static Dictionary<PersonType,PersonParam> GetInitPersonMap(Game game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsInitPerson(game,v.Value)).ToDictionary();
		internal static Dictionary<PersonType,PersonParam> GetAppearPersonMap(Game game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsAppearPerson(game,v.Value)).ToDictionary();
		internal static Dictionary<PersonType,PersonParam> GetAlivePersonMap(Game game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsAlivePerson(v.Value)).ToDictionary();
		internal static Dictionary<PersonType,PersonParam> GetWaitPostPersonMap(Game game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsWaitPostPerson(v.Value)).ToDictionary();
		internal static Dictionary<PersonType,PersonParam> GetNaturalDeathPostPersonMap(Game game,ECountry country,ERole role) => GetRolePersonMap(game,country,role).Where(v => IsNaturalDeathPerson(Turn.GetYear(game),v.Value)).ToDictionary();
		internal static KeyValuePair<PersonType,PersonParam>? GetPostPerson(Game game,ECountry country,PostType post) => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Country==country&&v?.Value.Post==post);
		internal static decimal CalcRank(Game game,PersonType person,ERole role) => game.PersonMap.GetValueOrDefault(person)?.MyApplyF(param => CalcRank(param,role))??0m;
		internal static decimal CalcRank(PersonParam personParam,ERole role) => personParam.Rank+(personParam.Role==role ? 0 : -1);
		internal static int GetAppearYear(PersonParam personParam) => personParam.GameAppearYear??personParam.BirthYear+BaseData.majorityAge;
		internal static KeyValuePair<PersonType,PersonParam> GenerateFindPerson(Game game,ECountry country,int personRank) {
			ERole personRole = Enum.GetValues<ERole>().MyPickAny();
			int birthYear = Turn.GetYear(game)-BaseData.majorityAge-MyRandom.GenerateInt(0,40);
			int deathYear = Math.Max(birthYear+BaseData.majorityAge+MyRandom.GenerateInt(0,60),Turn.GetYear(game)+3);
			return new(new($"{country}無名{game.CountryMap.GetValueOrDefault(country)?.AnonymousPersonNum+1}"),new(personRole,personRank,birthYear,deathYear,country,null,null,new(personRole,new())));
		}
	}
}
