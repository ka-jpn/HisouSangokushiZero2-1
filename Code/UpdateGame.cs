using MyUtil;
using static 悲愴三国志Zero2_1.Code.DefType;
using CommanderType = 悲愴三国志Zero2_1.Code.DefType.Commander;
using PersonType = 悲愴三国志Zero2_1.Code.DefType.Person;
using PostType = 悲愴三国志Zero2_1.Code.DefType.Post;
namespace 悲愴三国志Zero2_1.Code {
	internal static class UpdateGame {
		internal static Game SetPersonPost(Game game,Dictionary<PersonType,PostType> postMap) => game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => postMap.TryGetValue(v.Key,out PostType? post) ? v.Value with { Post=post } : v.Value) };
		internal static Game RemovePersonPost(Game game,List<PersonType> removePersons) => game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => removePersons.Contains(v.Key) ? v.Value with { Post=null } : v.Value) };
		internal static Game InitAlivePersonPost(Game game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetInitPost(game,country,role))).ToDictionary().MyApplyF(v => SetPersonPost(game,v));
		internal static Game InitAppearPersonPost(Game game) {
			Dictionary<ECountry,Dictionary<PersonType,PostType>> appearPersonMap = game.CountryMap.Keys.Select(country => (country, Enum.GetValues<ERole>().SelectMany(role => Post.GetInitAppearPost(game,country,role)).ToDictionary())).ToDictionary();
			Dictionary<ECountry,KeyValuePair<PersonType,PersonParam>> findPersonMap = game.CountryMap.Keys.Select(country => (appearPersonMap.GetValueOrDefault(country)?? []).Count==0&&Country.SuccessFindPersonRank(game,country) is int personRank ? Person.GenerateFindPerson(game,country,personRank) : (KeyValuePair<PersonType,PersonParam>?)null).MyNonNull().ToDictionary(v => v.Value.Country,v => v);
			return appearPersonMap.Values.SelectMany(v => v).ToDictionary().MyApplyF(v => SetPersonPost(game,v)).MyApplyF(game => game with { PersonMap=game.PersonMap.Concat(findPersonMap.Values).ToDictionary(),CountryMap=game.CountryMap.ToDictionary(v => v.Key,v => findPersonMap.ContainsKey(v.Key) ? v.Value with { AnonymousPersonNum=v.Value.AnonymousPersonNum+1 } : v.Value) });
		}
		internal static Game PutWaitPersonPost(Game game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetPutWaitPost(game,country,role))).ToDictionary().MyApplyF(v => SetPersonPost(game,v));
		internal static Game RemoveNaturalDeathPersonPost(Game game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetDeathPostPersonMap(game,country,role).Keys)).ToList().MyApplyF(deathPersons => RemovePersonPost(game,deathPersons).MyApplyF(game => game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => deathPersons.Contains(v.Key) ? v.Value with { GameDeathTurn=game.PlayTurn } : v.Value) }).MyApplyF(game => AppendLogMessage(game,[Text.DeathPersonText([.. deathPersons],Lang.ja)])));
		internal static Game AutoPutPostCPU(Game game) => game.CountryMap.Keys.Except(game.PlayCountry!=null ? [game.PlayCountry.Value] : []).Except([ECountry.漢]).SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetAutoPutPost(game,country,role))).ToDictionary().MyApplyF(v => SetPersonPost(game,v));
		internal static Game PutPersonFromUI(Game game,PersonType? putPerson,PostType? putPost) => putPerson!=null&&putPost!=null ? SetPersonPost(game,new() { { putPerson,putPost } }).MyApplyA(v => v.StateHasChanged?.Invoke()) : game;
		internal static Game AttachGameStartData(Game game,ECountry? countryName) => countryName is ECountry country ? game with { PlayCountry=country,PlayTurn=0 } : game;
		internal static Game UpdateCapitalArea(Game game) => game with { CountryMap=game.CountryMap.ToDictionary(v => v.Key,countryInfo => countryInfo.Value with { CapitalArea=Area.ComputeCapitalArea(game,countryInfo.Key) }) };
		internal static Game PayAttackFunds(Game game,ECountry country) => game with { CountryMap=game.CountryMap.MyUpdate(country,(_,countryInfo) => countryInfo with { Fund=countryInfo.Fund-Country.CalcAttackFund(game,country) }) };
		internal static Game AppendLogMessage(Game game,List<string?> appendMessages) => game with { LogMessage= [.. game.LogMessage,.. appendMessages.MyNonNull()] };
		internal static Game CountryAttack(Game game,ECountry attackSide,EArea target,Army attack,Army defense,AttackJudge judge) {
			return judge switch { AttackJudge.crush => Crush(game,attackSide,target,defense), AttackJudge.win => Win(game,attackSide,target,defense), AttackJudge.lose => Lose(game,attackSide), AttackJudge.rout => Rout(game,attackSide,attack) };
			static Game Crush(Game game,ECountry attackSide,EArea target,Army defense) => DeathCommander(game,defense.Commander,ERole.defense);
			static Game Win(Game game,ECountry attackSide,EArea target,Army defense) => SleepCountry(game,attackSide,1);
			static Game Lose(Game game,ECountry attackSide) => SleepCountry(game,attackSide,1);
			static Game Rout(Game game,ECountry attackSide,Army attack) => SleepCountry(game,attackSide,3).MyApplyF(game => DeathCommander(game,attack.Commander,ERole.attack));
		}
		internal static Game AreaAttack(Game game,ECountry attackSide,EArea target,Army attack,Army defense,AttackJudge judge) {
			return judge switch { AttackJudge.crush => Crush(game,attackSide,target,defense), AttackJudge.win => Win(game,attackSide,target,defense), AttackJudge.lose => Lose(game,attackSide,target), AttackJudge.rout => Rout(game,attackSide,target,attack) };
			static Game Crush(Game game,ECountry attackSide,EArea target,Army defense) => ChangeHasCountry(game,attackSide,defense.Country,target).MyApplyF(game => FallArea(game,target)).MyApplyF(game => DeathCommander(game,defense.Commander,ERole.defense));
			static Game Win(Game game,ECountry attackSide,EArea target,Army defense) => ChangeHasCountry(game,attackSide,defense.Country,target).MyApplyF(game => FallArea(game,target)).MyApplyF(game => SleepCountry(game,attackSide,1));
			static Game Lose(Game game,ECountry attackSide,EArea target) => SleepCountry(game,attackSide,1).MyApplyF(game => DamageArea(game,target));
			static Game Rout(Game game,ECountry attackSide,EArea target,Army attack) => SleepCountry(game,attackSide,3).MyApplyF(game => DeathCommander(game,attack.Commander,ERole.attack)).MyApplyF(game => DamageArea(game,target));
			static Game ChangeHasCountry(Game game,ECountry attackCountry,ECountry? defenseCountry,EArea targetArea) {
				return AppendLogMessage(game,[Text.ChangeHasCountryText(attackCountry,defenseCountry,targetArea,Lang.ja)]).MyApplyF(game => UpdateAreaMap(game,attackCountry,targetArea)).MyApplyF(game => MakeEmptyPost(game,targetArea)).MyApplyF(game => IsFallCapital(game,targetArea,defenseCountry) ? FallCapital(game,defenseCountry) : game);
				static Game UpdateAreaMap(Game game,ECountry attackCountry,EArea targetArea) => game with { AreaMap=game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { Country=attackCountry }) };
				static Game MakeEmptyPost(Game game,EArea targetArea) => game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => v.Value.Post?.PostKind!=new PostKind(targetArea) ? v.Value : v.Value with { Post=null }) };
				static bool IsFallCapital(Game game,EArea targetArea,ECountry? defenseCountry) => defenseCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.CapitalArea==targetArea;
				static Game FallCapital(Game game,ECountry? defenseCountry) {
					List<PersonType> defenseCountryCapitalPerson = [.. game.PersonMap.Where(v => v.Value.Country==defenseCountry&&v.Value.Post.MyApplyF(v => v?.PostKind.MaybeHead!=null||v?.PostKind.MaybePostNo!=null)).Select(v => v.Key)];
					List<PersonType> deathPerson = [.. defenseCountryCapitalPerson.Where(_ => MyRandom.RandomJudge(0.5))];
					return (game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => deathPerson.Contains(v.Key) ? v.Value with { Post=null,DeathYear=Turn.GetYear(game) } : v.Value) }).MyApplyF(game => AppendLogMessage(game,[Text.FallCapitalText(defenseCountry,deathPerson,Lang.ja)]));
				}
			}
			static Game FallArea(Game game,EArea targetArea) {
				return game with { AreaMap=game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { AffairParam=areaInfo.AffairParam with { AffairNow=Math.Round(areaInfo.AffairParam.AffairNow*0.9m,4),AffairsMax=Math.Round(areaInfo.AffairParam.AffairsMax*0.95m,4) } }) };
			}
			static Game DamageArea(Game game,EArea targetArea) {
				return game with { AreaMap=game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { AffairParam=areaInfo.AffairParam with { AffairNow=Math.Round(areaInfo.AffairParam.AffairNow*0.95m,4) } }) };
			}
		}
		private static Game SleepCountry(Game game,ECountry attackCountry,int sleepTurnNum) => game with { CountryMap=game.CountryMap.MyUpdate(attackCountry,(_,countryInfo) => countryInfo with { SleepTurnNum=sleepTurnNum }) };
		private static Game DeathCommander(Game game,CommanderType commander,ERole role) {
			List<PersonType> deathPersons = new PersonType?[] { DeathJudge() ? commander.MainPerson : null,DeathJudge() ? commander.SubPerson : null }.MyNonNull();
			return deathPersons.Count==0 ? game : AppendLogMessage(game,[Text.BattleDeathPersonText(role,deathPersons,Lang.ja)]).MyApplyF(game => UpdatePersonMap(game,deathPersons));
			static bool DeathJudge() => MyRandom.RandomJudge(0.25);
			static Game UpdatePersonMap(Game game,List<PersonType> deathPersons) => game with { PersonMap=deathPersons.Aggregate(game.PersonMap,(fold,value) => fold.MyUpdate(value,(_,param) => param with { Post=null,GameDeathTurn=game.PlayTurn })) };
		}
		internal static Game NextTurn(Game game) {
			return game.MyApplyF(UpdateCapitalArea).MyApplyF(AddTurn).MyApplyF(InOutFunds).MyApplyF(AddAffair).MyApplyF(InitAppearPersonPost).MyApplyF(RemoveDeathPersonPost).MyApplyF(PutWaitPersonPost).MyApplyF(AutoPutPostCPU);
			static Game AddTurn(Game game) => game with { PlayTurn=game.PlayTurn+1 };
			static Game InOutFunds(Game game) => game with { CountryMap=game.CountryMap.ToDictionary(v => v.Key,v => v.Value with { Fund=v.Value.Fund+Country.GetInFund(game,v.Key)-Country.GetOutFund(game,v.Key) }) };
			static Game AddAffair(Game game) => game with {
				AreaMap=game.AreaMap.ToDictionary(area => area.Key,area => area.Value with {
					AffairParam=area.Value.AffairParam with {
						AffairNow=Math.Clamp(Math.Round(area.Value.AffairParam.AffairNow+(area.Value.Country?.MyApplyF(country => Country.GetAffairPower(game,country)/Country.GetAffairDifficult(game,country))??0)*(game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post?.PostRole==ERole.affair&&v?.Value.Post?.PostKind==new PostKind(area.Key))?.Value.MyApplyF(v => Person.CalcRank(v,ERole.affair))??0)*(1-area.Value.AffairParam.AffairNow/area.Value.AffairParam.AffairsMax),4),0,area.Value.AffairParam.AffairsMax),
						AffairsMax=Math.Round(area.Value.AffairParam.AffairsMax*1.001m+0.01m,4)
					}
				})
			};
			static Game RemoveDeathPersonPost(Game game) => Turn.GetInYear(game)==BaseData.yearItems.Length/2 ? RemoveNaturalDeathPersonPost(game) : game;
		}
		internal static Game Attack(Game game,ECountry attackCountry,EArea targetArea,ECountry? defenseCountry,bool defenseSideFocusDefense) {
			Army attackArmy = Commander.GetAttackCommander(game,attackCountry).MyApplyF(commander => new Army(attackCountry,commander,Commander.CommanderRank(game,commander,ERole.attack)));
			AttackResult? countryBattle = Battle.Country.Attack(game,defenseCountry,targetArea,attackArmy,defenseSideFocusDefense,Lang.ja);
			AttackResult areaBattle = Battle.Area.Attack(game,defenseCountry,targetArea,attackArmy,defenseSideFocusDefense,Lang.ja);
			return game.MyApplyF(game => PayAttackFunds(game,attackCountry))
				.MyApplyF(game => BattleDefensesideCentralDefense(game,countryBattle,attackCountry,targetArea,attackArmy))
				.MyApplyF(game => countryBattle?.Judge is AttackJudge.lose or AttackJudge.rout ? game : BattleDefensesideAreaDefense(game,areaBattle,attackCountry,targetArea,attackArmy));
			static Game BattleDefensesideCentralDefense(Game game,AttackResult? countryBattle,ECountry attackCountry,EArea targetArea,Army attackArmy) => countryBattle?.Judge.MyApplyF(judge => AppendLogMessage(game,[countryBattle.InvadeText]).MyApplyF(game => CountryAttack(game,attackCountry,targetArea,attackArmy,countryBattle.Defense,judge)))??game;
			static Game BattleDefensesideAreaDefense(Game game,AttackResult areaBattle,ECountry attackCountry,EArea targetArea,Army attackArmy) => AppendLogMessage(game,[areaBattle.InvadeText]).MyApplyF(game => areaBattle.Judge.MyApplyF(judge => AreaAttack(game,attackCountry,targetArea,attackArmy,areaBattle.Defense,judge)));
		}
		internal static Game Defense(Game game,ECountry country,bool isTryAttack) => AppendLogMessage(game,[Text.DefenseText(country,isTryAttack,Lang.ja)]);
		internal static Game Rest(Game game, ECountry country) {
			return AppendLogMessage(game, [Text.RestText(country, Country.GetSleepTurn(game,country),Lang.ja)]) with { CountryMap = game.CountryMap.MyUpdate(country, (_, info) => info with { SleepTurnNum = info.SleepTurnNum - 1 }) };
		}
	}
}