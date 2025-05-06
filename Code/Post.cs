using static 悲愴三国志Zero2_1.Code.DefType;
using PostType = 悲愴三国志Zero2_1.Code.DefType.Post;
using PersonType = 悲愴三国志Zero2_1.Code.DefType.Person;
using MyUtil;
using System.Data;
namespace 悲愴三国志Zero2_1.Code {
	internal static class Post {																																																																			
		private static List<PostType> GetCapitalRolePosts(ECountry country,ERole role) => country==ECountry.漢 ? [] : [new PostType(role,new(PostHead.main)),new PostType(role,new(PostHead.sub))];
		private static List<PostType> GetCapitalRoleStockPosts(ECountry country,ERole role) => country==ECountry.漢 ? [] : [.. Enumerable.Range(0,BaseData.capitalPieceCellNum).Select(i => new PostType(role,new(i)))];
		private static List<PostType> GetAreaRolePosts(Game game,ECountry country,ERole role) => [.. (role==ERole.defense ? Area.CalcOrdDefenseAreas(game,country) : role==ERole.affair ? Area.CalcOrdAffairAreas(game,country) : []).Select(area => new PostType(role,new(area)))];
		private static List<PostType> GetEmptyCapitalStockPosts(Game game,ECountry country,ERole role) => [.. GetCapitalRoleStockPosts(country,role).Except(game.PersonMap.Values.Where(v => v.Country==country).Select(v => v.Post))];
		internal static Dictionary<PersonType,PostType> GetInitPost(Game game,ECountry country,ERole role) => Person.GetInitPersonMap(game,country,role).ToDictionary(v => v.Key,v => new PostType(role,new()));
		internal static Dictionary<PersonType,PostType> GetInitAppearPost(Game game,ECountry country,ERole role) => Person.GetAppearPersonMap(game,country,role).ToDictionary(v => v.Key,v => new PostType(role,new()));
		internal static Dictionary<PersonType,PostType> GetPutWaitPost(Game game,ECountry country,ERole role) => Person.GetWaitPostPersonMap(game,country,role).Keys.Zip(GetEmptyCapitalStockPosts(game,country,role)).ToDictionary();
		internal static Dictionary<PersonType,PostType> GetAutoPutPost(Game game,ECountry country,ERole role) => Person.GetAlivePersonMap(game,country,role).OrderByDescending(personInfo =>Person.CalcRank(personInfo.Value,role)).Select(person => person.Key).Zip([.. GetCapitalRolePosts(country,role),.. GetAreaRolePosts(game,country,role),.. GetCapitalRoleStockPosts(country,role)]).ToDictionary();
	}
}
