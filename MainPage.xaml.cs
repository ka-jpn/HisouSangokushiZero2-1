using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using MyUtil;
using Windows.Foundation;
using Windows.UI;
using 悲愴三国志Zero2_1.Code;
using static 悲愴三国志Zero2_1.Code.DefType;
using Point = 悲愴三国志Zero2_1.Code.DefType.Point;
using PostType = 悲愴三国志Zero2_1.Code.DefType.Post;
using PersonType = 悲愴三国志Zero2_1.Code.DefType.Person;
using System.Linq;
namespace 悲愴三国志Zero2_1 {
	public sealed partial class MainPage:Page {
		internal enum ViewMode { fit,fix };
		private enum InfoPanelState { Instruction, Explain, WinCond, PersonData, ChangeLog, Setting };
		internal static readonly double fixModeWidth = 1000;
		internal static readonly Size mapSize = new(2000,1750);
		private static readonly Point mapGridCount = new(9,10);
		internal static readonly GridLength infoFrameWidth = new(50);
		private static readonly Size areaSize = new(204,155);
		private static readonly CornerRadius areaCornerRadius = new(30);
		private static readonly Size personPutSize = new(99,70);
		internal static readonly double countryPersonPutPanelHeight = personPutSize.Height*4+BasicStyle.textHiehgt;
		internal static readonly double personRankFontScale = 1.5;
		internal static readonly double personNameFontScale = 1.75;
		internal static readonly double infoTextWidth = BasicStyle.fontsize*55;
		private static readonly Color defaultCountryColor = Color.FromArgb(255,240,240,240);
		private static readonly Color landRoadColor = Color.FromArgb(150,120,120,50);
		private static readonly Color waterRoadColor = Color.FromArgb(150,50,50,150);
		internal static readonly double attackJudgePointSize = 10;
		private static readonly double postFrameWidth=1.25;
		internal static readonly double dataListFrameWidth = 0.5;
		internal static readonly Color dataListFrameColor = Color.FromArgb(255,150,150,150);
		private static Game game = null!;
		private static (Panel panel, PostType post)? pointerover = null;
		private static (Panel panel, PersonType person)? pick = null;	
		private static InfoPanelState? showInfoPanelState = null;
		internal static ViewMode viewMode = ViewMode.fix;
		public MainPage() {
			InitializeComponent();
			MyInit(this);
		}
		private static void MyInit(MainPage page) {
			SetUIElements(page);
			AttachEvent(page);
			RefreshViewMode(page);
			static void SetUIElements(MainPage page) {
				page.AttackCrushFillColor.Background=new SolidColorBrush(ThresholdFillColor(AttackJudge.crush));
				page.AttackCrushEdgeColor.Background=new SolidColorBrush(ThresholdEdgeColor(AttackJudge.crush));
				page.AttackWinFillColor.Background=new SolidColorBrush(ThresholdFillColor(AttackJudge.win));
				page.AttackWinEdgeColor.Background=new SolidColorBrush(ThresholdEdgeColor(AttackJudge.win));
				page.AttackLoseFillColor.Background=new SolidColorBrush(ThresholdFillColor(AttackJudge.lose));
				page.AttackLoseEdgeColor.Background=new SolidColorBrush(ThresholdEdgeColor(AttackJudge.lose));
				page.AttackRoutFillColor.Background=new SolidColorBrush(ThresholdFillColor(AttackJudge.rout));
				page.AttackRoutEdgeColor.Background=new SolidColorBrush(ThresholdEdgeColor(AttackJudge.rout));
				page.AttackCrushShape.MyApplyA(v => v.Fill=new SolidColorBrush(ThresholdFillColor(AttackJudge.crush))).MyApplyA(v => v.Points= [.. GetJudgeShapeCrds(AttackJudge.crush),.. GetJudgeShapeCrds(AttackJudge.win).Reverse()]);
				page.AttackWinShape.MyApplyA(v => v.Fill=new SolidColorBrush(ThresholdFillColor(AttackJudge.win))).MyApplyA(v => v.Points= [.. GetJudgeShapeCrds(AttackJudge.win),.. GetJudgeShapeCrds(AttackJudge.lose).Reverse()]);
				page.AttackLoseShape.MyApplyA(v => v.Fill=new SolidColorBrush(ThresholdFillColor(AttackJudge.lose))).MyApplyA(v => v.Points= [.. GetJudgeShapeCrds(AttackJudge.lose),.. GetJudgeShapeCrds(AttackJudge.rout).Reverse()]);
				page.AttackRoutShape.MyApplyA(v => v.Fill=new SolidColorBrush(ThresholdFillColor(AttackJudge.rout))).MyApplyA(v => v.Points= [.. GetJudgeShapeCrds(AttackJudge.rout),.. GetJudgeShapeCrds(null).Reverse()]);
				page.AttackJudgePointVisualPanel.MySetChildren([.. CreateRects(null),.. CreateRects(AttackJudge.crush),.. CreateRects(AttackJudge.win),.. CreateRects(AttackJudge.lose),.. CreateRects(AttackJudge.rout),.. CreateTexts(AttackJudge.win),.. CreateTexts(AttackJudge.lose),.. CreateTexts(AttackJudge.rout)]);
				page.AttackRankDiffTextPanel.MySetChildren([.. CreateRankDiffTexts()]);				
				page.PersonDataListPanel.MySetChildren([.. CreatePersonDataList(0,12)]);
				page.CountryDataListPanel.MySetChildren([.. CreateCountryDataList(0,2)]);
				static Color ThresholdEdgeColor(AttackJudge? attackJudge) => attackJudge==AttackJudge.crush ? Color.FromArgb(255,240,135,135) : attackJudge==AttackJudge.win ? Color.FromArgb(255,230,230,65) : attackJudge==AttackJudge.lose ? Color.FromArgb(255,105,225,105) : attackJudge==AttackJudge.rout ? Color.FromArgb(255,135,135,240) : Color.FromArgb(255,165,165,165);
				static Color ThresholdFillColor(AttackJudge attackJudge) => attackJudge==AttackJudge.crush ? Color.FromArgb(255,240,190,190) : attackJudge==AttackJudge.win ? Color.FromArgb(255,235,235,160) : attackJudge==AttackJudge.lose ? Color.FromArgb(255,175,240,175) : Color.FromArgb(255,190,190,240);
				static Windows.Foundation.Point GetJudgePoint(AttackJudge? attackJudge,double rankDiff) => new((rankDiff*9+50),Battle.GetThreshold(attackJudge,rankDiff));
				static Windows.Foundation.Point[] GetJudgeShapeCrds(AttackJudge? attackJudge) => [.. new double[] { -5.5,-5,-4.5,-4,-3.5,-3,-2.5,-2,-1,1,2,2.5,3,3.5,4,4.5,5,5.5 }.Select(i => CookPoint(GetJudgePoint(attackJudge,i)))];
				static Windows.Foundation.Point[] GetJudgePoints(AttackJudge? attackJudge) => [.. new double[] { -5,-4,-3,-2,-1,0,1,2,3,4,5 }.Select(i => GetJudgePoint(attackJudge,i))];
				static Windows.Foundation.Point CookPoint(Windows.Foundation.Point point) => point with { X=point.X*11.5,Y=point.Y*6 };
				static UIElement SetJudgePointCrds(UIElement elem,Windows.Foundation.Point crd,Size size) => elem.MyApplyA(elem => { Canvas.SetLeft(elem,crd.X-size.Width/2); Canvas.SetTop(elem,crd.Y-size.Height/2); });
				static TextBlock[] CreateTexts(AttackJudge? attackJudge) => [.. GetJudgePoints(attackJudge).Select(crd => new TextBlock { Text=crd.Y.ToString() }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd),new(CalcFullWidthLength(crd.Y.ToString())*BasicStyle.fontsize,BasicStyle.textHiehgt))))];
				static Rectangle[] CreateRects(AttackJudge? attackJudge) => [.. GetJudgePoints(attackJudge).Select(crd => new Rectangle { Width=attackJudgePointSize,Height=attackJudgePointSize,Fill=new SolidColorBrush(ThresholdEdgeColor(attackJudge)) }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd),new(attackJudgePointSize,attackJudgePointSize))))];
				static TextBlock[] CreateRankDiffTexts() => [.. new double[] { -5,-4,-3,-2,-1,0,1,2,3,4,5 }.Select(i => GetJudgePoint(null,i).MyApplyF(crd => new TextBlock { Text=i.ToString() }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd with { Y=0 }),new(CalcFullWidthLength(i.ToString())*BasicStyle.fontsize,BasicStyle.textHiehgt)))))];
			}
			static void AttachEvent(MainPage page) {
				page.OpenLogButton.Click+=(_,_) => ClickOpenLogButton(page);
				page.OpenInfoButton.Click+=(_,_) => ClickOpenInfoButton(page);
				page.Page.SizeChanged+=(_,_) => ScalingElements(page,GetScaleFactor(page));
				page.Page.Loaded+=(_,_) => LoadedPage(page);
				page.PointerMoved+=(_,e) => MovePersonPanel(page,e);
				page.PointerReleased+=(_,e) => PutPersonPanel(page);
				page.InstructionButton.Click+=(_,_) => ClickSwitchInfoButton(page,InfoPanelState.Instruction);
				page.ExplainButton.Click+=(_,_) => ClickSwitchInfoButton(page,InfoPanelState.Explain);
				page.WinCondButton.Click+=(_,_) => ClickSwitchInfoButton(page,InfoPanelState.WinCond);
				page.PersonDataButton.Click+=(_,_) => ClickSwitchInfoButton(page,InfoPanelState.PersonData);
				page.ChangeLogButton.Click+=(_,_) => ClickSwitchInfoButton(page,InfoPanelState.ChangeLog);
				page.SettingButton.Click+=(_,_) => ClickSwitchInfoButton(page,InfoPanelState.Setting);
				page.InitGameButton.Click+=(_,_) => InitGame(page,game.NowScenario);
				page.TopSwitchViewModeButton.Click+=(_,_) => SwitchViewMode(page);
				page.InnerSwitchViewModeButton.Click+=(_,_) => SwitchViewMode(page);
				page.CountryCentralPostPanel.PointerEntered+=(_,_) => PointerEnterCountryPostPanel(page,ERole.central);
				page.CountryAffairPostPanel.PointerEntered+=(_,_) => PointerEnterCountryPostPanel(page,ERole.affair);
				page.CountryDefensePostPanel.PointerEntered+=(_,_) => PointerEnterCountryPostPanel(page,ERole.defense);
				page.CountryAttackPostPanel.PointerEntered+=(_,_) => PointerEnterCountryPostPanel(page,ERole.attack);
				static void ClickOpenLogButton(MainPage page) => (page.LogScrollPanel.Visibility=page.LogScrollPanel.Visibility==Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.InfoPanel.Visibility=Visibility.Collapsed);
				static void ClickOpenInfoButton(MainPage page) => (page.InfoPanel.Visibility=page.InfoPanel.Visibility==Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.LogScrollPanel.Visibility=Visibility.Collapsed);
				static void ScalingElements(MainPage page,double scaleFactor) {
					page.CountryPostsPanel.Margin=new(0,0,0,countryPersonPutPanelHeight*(scaleFactor-1));
					page.CountryPostsPanel.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					page.MapPanel.Width=mapSize.Width*scaleFactor;
					page.MapPanel.Height=mapSize.Height*scaleFactor;
					page.MapInnerPanel.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					page.MovePersonCanvas.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					page.CountryInfoPanel.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor,CenterX=page.CountryInfoPanel.Width,CenterY=page.CountryInfoPanel.Height };
					page.LogScrollPanel.ZoomToFactor((float)scaleFactor);
					page.InfoScrollPanel.ZoomToFactor((float)scaleFactor);
					double buttonMargin = infoFrameWidth.Value*(scaleFactor-1);
					page.InfoButtonsPanel.Margin=new(0,0,-page.InfoLayoutPanel.ActualWidth/scaleFactor+page.InfoLayoutPanel.ActualWidth-buttonMargin,buttonMargin);
					page.InfoButtonsPanel.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					page.OpenLogButton.Margin=new(0,0,-page.InfoLayoutPanel.ActualWidth/scaleFactor+page.InfoLayoutPanel.ActualWidth-buttonMargin,buttonMargin);
					page.OpenLogButton.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					page.OpenInfoButton.Margin=new(0,0,buttonMargin,-page.InfoLayoutPanel.ActualHeight/scaleFactor+page.InfoLayoutPanel.ActualHeight-buttonMargin);
					page.OpenInfoButton.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					page.TopSwitchViewModeButton.Margin=new(0,0,buttonMargin,buttonMargin);
					page.TopSwitchViewModeButton.RenderTransform=new ScaleTransform() { ScaleX=scaleFactor,ScaleY=scaleFactor };
					double PostPanelLeftUnit = mapSize.Width/4+(page.CountryPostsPanel.ActualWidth-mapSize.Width*scaleFactor)/scaleFactor/3;
					new List<UIElement> { page.CountryCentralPostPanel,page.CountryAffairPostPanel,page.CountryDefensePostPanel,page.CountryAttackPostPanel }.Select((elem,index)=>(elem,index)).ToList().ForEach(v=> Canvas.SetLeft(v.elem,PostPanelLeftUnit*v.index));
				}
				static void LoadedPage(MainPage page) {
					GameInfo.scenarios.FirstOrDefault()?.MyApplyA(scenario=>InitGame(page,scenario));
					ScalingElements(page,GetScaleFactor(page));
				}
				static void MovePersonPanel(MainPage page,PointerRoutedEventArgs e) {
					if(pick!=null) {
						page.MovePersonCanvas.Children.SingleOrDefault()?.MyApplyA(personPanel => MovePerson(page,personPanel,e));
					}
				}
				static void PutPersonPanel(MainPage page) {
					if(pick!=null) {
						game=game.MyApplyF(game=> swapPerson(page,game)).MyApplyF(game => putPerson(page,game));
						page.MovePersonCanvas.MySetChildren([]);
						pick=null;
						ShowCountryInfo(page,game);
					}
					static Game swapPerson(MainPage page,Game game) {
						KeyValuePair<PersonType,PersonParam>? maybeDestPersonInfo = game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Country==game.PlayCountry&&v?.Value.Post==pointerover?.post);
						return UpdateGame.PutPersonFromUI(game,maybeDestPersonInfo?.Key,pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key==maybeDestPersonInfo?.Key)?.MyApplyF(destPersonInfo => pick?.panel.MySetChildren([CreatePersonPanel(page,destPersonInfo)])));
					}
					static Game putPerson(MainPage page,Game game) {
						return UpdateGame.PutPersonFromUI(game,pick?.person,pointerover?.post??pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key==pick?.person)?.MyApplyF(putPersonInfo => (pointerover?.panel??pick?.panel)?.MySetChildren([CreatePersonPanel(page,putPersonInfo)])));
					}
				}
				static void ClickSwitchInfoButton(MainPage page,InfoPanelState clickButtonInfoPanelState) {
					showInfoPanelState=showInfoPanelState==clickButtonInfoPanelState ? null : clickButtonInfoPanelState;
					page.InstructionPanel.Visibility=showInfoPanelState==InfoPanelState.Instruction ? Visibility.Visible : Visibility.Collapsed;
					page.ExplainPanel.Visibility=showInfoPanelState==InfoPanelState.Explain ? Visibility.Visible : Visibility.Collapsed;
					page.WinCondPanel.Visibility=showInfoPanelState==InfoPanelState.WinCond ? Visibility.Visible : Visibility.Collapsed;
					page.PersonDataPanel.Visibility=showInfoPanelState==InfoPanelState.PersonData ? Visibility.Visible : Visibility.Collapsed;
					page.ChangeLogPanel.Visibility=showInfoPanelState==InfoPanelState.ChangeLog ? Visibility.Visible : Visibility.Collapsed;
					page.SettingPanel.Visibility=showInfoPanelState==InfoPanelState.Setting ? Visibility.Visible : Visibility.Collapsed;
				}
				static void InitGame(MainPage page,Scenario scenario) {
					game=GetGame.GetInitGameScenario(scenario);
					UpdateAreaUI(page,game,[]);
					UpdateCountryPostPersons(page,game);
					page.CountryInfoContentsPanel.MySetChildren([
						new TextBlock{ Text="プレイ勢力選択後に",FontSize=20,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text="情報が表示されます",FontSize=20,TextAlignment=TextAlignment.Center },
					]);
				}
				static void SwitchViewMode(MainPage page) {
					viewMode=viewMode==ViewMode.fix ? ViewMode.fit : ViewMode.fix;
					RefreshViewMode(page);
				}
				static void PointerEnterCountryPostPanel(MainPage page,ERole target) {
					Canvas.SetZIndex(page.CountryCentralPostPanel,GetPiecePanelZIndex(target,ERole.central));
					Canvas.SetZIndex(page.CountryAffairPostPanel,GetPiecePanelZIndex(target,ERole.affair));
					Canvas.SetZIndex(page.CountryDefensePostPanel,GetPiecePanelZIndex(target,ERole.defense));
					Canvas.SetZIndex(page.CountryAttackPostPanel,GetPiecePanelZIndex(target,ERole.attack));
					static int GetPiecePanelZIndex(ERole focus,ERole elem) {
						return elem switch { ERole.central => focus==ERole.central ? 3 : 0, ERole.affair => focus is ERole.central or ERole.affair ? 2 : 1, ERole.defense => focus is ERole.central or ERole.affair ? 1 : 2, ERole.attack => focus==ERole.attack ? 3 : 0 };
					}
				}
			}
			static void UpdateCountryPostPersons(MainPage page,Game game) {
			page.CentralPanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.central),CreatePersonPostPanelElems(page,game,ERole.central)]);
			page.AffairPanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.affair),CreatePersonPostPanelElems(page,game,ERole.affair)]);
			page.DefensePanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.defense),CreatePersonPostPanelElems(page,game,ERole.defense)]);
			page.AttackPanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.attack),CreatePersonPostPanelElems(page,game,ERole.attack)]);
			static StackPanel CreatePersonHeadPostPanel(MainPage page,Game game,ERole role) {
				Button autoPutPersonButton = new Button { Width=personPutSize.Width*3,VerticalAlignment=VerticalAlignment.Stretch,Background=new SolidColorBrush(Color.FromArgb(100,100,100,100)) }.MyApplyA(v => v.Content=new TextBlock { Text="オート配置" });
				autoPutPersonButton.Click+=(_,_) => MainPage.game=AutoPutPersonButtonClick(MainPage.game);
				return new StackPanel { Orientation=Orientation.Horizontal }.MySetChildren([
					new StackPanel { Orientation=Orientation.Horizontal,BorderBrush=new SolidColorBrush(GetPostFrameColor(game,null)),BorderThickness=new(postFrameWidth) }.MySetChildren([
						CreatePersonPutPanel(page,game,new(role,new(PostHead.main)),game.PersonMap.Where(v => v.Value.Country==game.PlayCountry).ToDictionary(),"筆頭"),
						CreatePersonPutPanel(page,game,new(role,new(PostHead.sub)),game.PersonMap.Where(v => v.Value.Country==game.PlayCountry).ToDictionary(),"次席"),
					]),
					autoPutPersonButton
				]);
				Game AutoPutPersonButtonClick(Game game) => game.PlayCountry?.MyApplyF(country => Code.Post.GetAutoPutPost(game,country,role)).MyApplyF(postMap => UpdateGame.SetPersonPost(game,postMap)).MyApplyA(v => UpdateAreaUI(page,v,[])).MyApplyA(game => UpdateCountryPostPersons(page,game))??game;
			}
				static StackPanel CreatePersonPostPanelElems(MainPage page,Game game,ERole role) {
					return new StackPanel { BorderBrush=new SolidColorBrush(GetPostFrameColor(game,null)),BorderThickness=new(postFrameWidth) }.MySetChildren([.. Enumerable.Range(0,3).Select(row => GetPersonPostLinePanel(page,game,role,row,game.PersonMap.Where(v => v.Value.Country==game.PlayCountry).ToDictionary()))]);
					static StackPanel GetPersonPostLinePanel(MainPage page,Game game,ERole role,int rowNo,Dictionary<PersonType,PersonParam> personMap) => new StackPanel { Orientation=Orientation.Horizontal }.MySetChildren([.. Enumerable.Range(0,5).Select(i => CreatePersonPutPanel(page,game,new(role,new(rowNo*5+i)),personMap,(rowNo*5+i+1).ToString()))]);
				}
			}
			static void UpdateLogMessage(MainPage page,Game game) {
				page.LogContentPanel.MySetChildren([.. game.LogMessage.Select(logText => new TextBlock() { Text=logText })]);
			}
			static Grid CreatePersonPutPanel(MainPage page,Game game,PostType post,Dictionary<PersonType,PersonParam> putPersonMap,string backText) {
				Grid personPutPanel = new() { Width=personPutSize.Width,Height=personPutSize.Height,BorderBrush=new SolidColorBrush(GetPostFrameColor(game,post.PostKind.MaybeArea)),BorderThickness=new(postFrameWidth),Background=new SolidColorBrush(Colors.Transparent) };
				StackPanel personPutInnerPanel = new StackPanel().MySetChildren(putPersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post==post) is KeyValuePair<PersonType,PersonParam> param ? [CreatePersonPanel(page,param)] : []);
				TextBlock personPutBackText = new() { Text=backText,Foreground=new SolidColorBrush(Color.FromArgb(100,100,100,100)),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center,RenderTransform=new ScaleTransform() { ScaleX=2,ScaleY=2,CenterX=CalcFullWidthLength(backText)*BasicStyle.fontsize/2,CenterY=BasicStyle.textHiehgt/2 } };
				personPutPanel.PointerEntered+=(_,_) => EnterPersonPanel(MainPage.game,personPutInnerPanel,post);
				personPutPanel.PointerExited+=(_,_) => ExitPersonPanel(personPutInnerPanel);
				return personPutPanel.MySetChildren([personPutBackText,personPutInnerPanel]);
				static void EnterPersonPanel(Game game,StackPanel personPutInnerPanel,PostType post) {
					if(game.Phase!=Phase.Starting&&(post.PostKind.MaybeArea?.MyApplyF(area => game.AreaMap.GetValueOrDefault(area)?.Country==game.PlayCountry)??true)) {
						personPutInnerPanel.Background=new SolidColorBrush(Color.FromArgb(150,255,255,255));
						pointerover=(personPutInnerPanel, post);
					}
				}
				static void ExitPersonPanel(StackPanel personPutInnerPanel) {
					if(pointerover!=null) {
						personPutInnerPanel.Background=new SolidColorBrush(Colors.Transparent);
						pointerover=null;
					}
				}
			}
			static void UpdateAreaUI(MainPage page,Game game,Dictionary<ECountry,EArea?> armyTargetMap) {
				page.MapElementsCanvas.MySetChildren([.. ScenarioData.scenarios.GetValueOrDefault(game.NowScenario)?.RoadConnections.Select(road => CreateRoadLine(game,road))?? [],.. game.AreaMap.Select(info => CreateAreaPanel(page,game,info,armyTargetMap))]);
				static Line CreateRoadLine(Game game,ScenarioData.Road road) {
					Point from = game.AreaMap.GetValueOrDefault(road.From)?.Position??new(0,0);
					Point to = game.AreaMap.GetValueOrDefault(road.To)?.Position??new(0,0);
					return new Line() { X1=CookPositionX(from.X),Y1=CookPositionY(from.Y),X2=CookPositionX(to.X),Y2=CookPositionY(to.Y),Stroke=new SolidColorBrush(road.Kind==RoadKind.land ? landRoadColor : waterRoadColor),StrokeThickness=10*Math.Pow(road.Easiness,1.6)+20 };
					static double CookPositionX(double x) => x*(mapSize.Width-areaSize.Width-infoFrameWidth.Value)/(mapGridCount.X-1)+infoFrameWidth.Value+areaSize.Width/2;
					static double CookPositionY(double y) => y*(mapSize.Height-areaSize.Height-infoFrameWidth.Value)/(mapGridCount.Y-1)+infoFrameWidth.Value+areaSize.Height/2;
				}
				static Grid CreateAreaPanel(MainPage page,Game game,KeyValuePair<EArea,AreaInfo> info,Dictionary<ECountry,EArea?> armyTarget) {
					double capitalBorderWidth = 3;
					Grid areaPanel = new() { Width=areaSize.Width,Height=areaSize.Height,CornerRadius=areaCornerRadius };
					Border areaBorder = new() { Width=areaSize.Width,Height=areaSize.Height,BorderThickness=new(game.CountryMap.Values.Select(v => v.CapitalArea).Contains(info.Key) ? capitalBorderWidth : 0),CornerRadius=areaCornerRadius,BorderBrush=new SolidColorBrush(Colors.Red),Background=new SolidColorBrush(Country.GetCountryColor(game,info.Value.Country)??defaultCountryColor) };
					Grid areaBackPanel = new() { Width=areaSize.Width,Height=areaSize.Height,Background=new SolidColorBrush(Area.IsPlayerSelectable(game,info.Key) ? Colors.Transparent : Color.FromArgb(100,100,100,100)) };
					StackPanel areaInnerPanel = new() { Width=areaSize.Width,VerticalAlignment=VerticalAlignment.Center };
					Canvas.SetLeft(areaPanel,info.Value.Position.X*(mapSize.Width-areaSize.Width-infoFrameWidth.Value)/(mapGridCount.X-1)+infoFrameWidth.Value);
					Canvas.SetTop(areaPanel,info.Value.Position.Y*(mapSize.Height-areaSize.Height-infoFrameWidth.Value)/(mapGridCount.Y-1)+infoFrameWidth.Value);
					areaPanel.PointerPressed+=(_,_) => MainPage.game=PushAreaPanel(page,MainPage.game,info);
					return areaPanel.MySetChildren([areaBorder,areaBackPanel,areaInnerPanel.MySetChildren(GetAreaElems(page,game,info,armyTarget))]);
					static List<UIElement> GetAreaElems(MainPage page,Game game,KeyValuePair<EArea,AreaInfo> areaInfo,Dictionary<ECountry,EArea?> armyTargetMap) => [
						new StackPanel { HorizontalAlignment=HorizontalAlignment.Center,Orientation=Orientation.Horizontal}.MySetChildren([
							new TextBlock { Text=areaInfo.Key.ToString() },
							new TextBlock { Text=$" {areaInfo.Value.Country?.ToString()??$"自治"}領" },
						]),
						new StackPanel { HorizontalAlignment=HorizontalAlignment.Center,Orientation=Orientation.Horizontal,BorderBrush=new SolidColorBrush(GetPostFrameColor(game,areaInfo.Key)),BorderThickness=new(postFrameWidth) }.MySetChildren([
							CreatePersonPutPanel(page,game,new(ERole.defense,new(areaInfo.Key)),game.PersonMap,"防"),CreatePersonPutPanel(page,game,new(ERole.affair,new(areaInfo.Key)),game.PersonMap,"政")
						]),
						new StackPanel { HorizontalAlignment=HorizontalAlignment.Center,Orientation=Orientation.Horizontal}.MySetChildren([
							new TextBlock { Text=areaInfo.Value.AffairParam.AffairNow.ToString("0") },
							new TextBlock { Text="/" },
							new TextBlock { Text=areaInfo.Value.AffairParam.AffairsMax.ToString("0") },
						]),
						new TextBlock { Text=areaInfo.Value.Country?.MyApplyF(country=>game.CountryMap.GetValueOrDefault(country)?.SleepTurnNum.MyApplyF(v=>v>0?$"休み {v}":null)+(Country.IsFocusDefense(armyTargetMap,country)?"(防)":null)),TextAlignment=TextAlignment.Center }
					];
					static Game PushAreaPanel(MainPage page,Game game,KeyValuePair<EArea,AreaInfo> areaInfo) {
						return game.Phase==Phase.Starting ? areaInfo.Value.Country?.MyApplyF(country => SelectPlayCountry(page,game,country))??game : Area.IsPlayerSelectable(game,areaInfo.Key) ? SelectTarget(page,game,areaInfo.Value.Country!=game.PlayCountry ? areaInfo.Key : null) : game;
						static Game SelectPlayCountry(MainPage page,Game game,ECountry playCountry) => UpdateGame.AttachGameStartData(game,playCountry).MyApplyA(game => { UpdateCountryPostPersons(page,game); ShowCountryInfo(page,game); });
						static Game SelectTarget(MainPage page,Game game,EArea? area) => (game with { SelectTarget=area }).MyApplyA(game => { ShowCountryInfo(page,game); });
					}
				}
			}
			static Grid CreatePersonPanel(MainPage page,KeyValuePair<PersonType,PersonParam> person) {
				double minFullWidthLength = 2.25;
				Grid panel = new Grid { Width=personPutSize.Width,Height=personPutSize.Height,Background=new SolidColorBrush(Country.GetCountryColor(game,person.Value.Country)??defaultCountryColor) }.MySetChildren([
					new StackPanel { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=new SolidColorBrush(Color.FromArgb((byte)(20*person.Value.Rank),0,0,0)) }.MySetChildren([
						GetRankPanel(page,person),
						new TextBlock { Text=person.Key.Value,TextAlignment=TextAlignment.Center,Margin=new(-page.FontSize*(CalcFullWidthLength(person.Key.Value)-2)/2,0,-page.FontSize*(CalcFullWidthLength(person.Key.Value)-2)/minFullWidthLength,0),RenderTransform=new ScaleTransform{ ScaleX=minFullWidthLength/Math.Max(minFullWidthLength,CalcFullWidthLength(person.Key.Value))*personNameFontScale,ScaleY=personNameFontScale,CenterX=personPutSize.Width/2+page.FontSize*(CalcFullWidthLength(person.Key.Value)-2)/minFullWidthLength  }  }
					])
				]);
				panel.PointerPressed+=(_,e) => PickPersonPanel(page,e,panel,person.Key);
				return panel;
				static StackPanel GetRankPanel(MainPage page,KeyValuePair<PersonType,PersonParam> person) {
					bool matchRole = person.Value.Role==person.Value.Post?.PostRole;
					TextBlock baseTextBlock = new() { Margin=new(0,-3,0,3) };
					return new StackPanel { Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Center,RenderTransform=new ScaleTransform() { ScaleX=personRankFontScale,ScaleY=personRankFontScale,CenterX=page.FontSize/2 } }.MySetChildren(matchRole ? GetMatchRankTextBlock() : GetUnMatchRankTextBlock());
					List<UIElement> GetMatchRankTextBlock() => [baseTextBlock.MyApplyA(v => v.Text=person.Value.Rank.ToString())];
					List<UIElement> GetUnMatchRankTextBlock() => [baseTextBlock.MyApplyA(v => { v.Text=(person.Value.Rank-1).ToString(); v.Foreground=new SolidColorBrush(Colors.Red); }),new Image{ Source=new SvgImageSource(new($"ms-appx:///Assets/Img/{person.Value.Role}.svg")),VerticalAlignment=VerticalAlignment.Top,Width=BasicStyle.textHiehgt*0.75,Height=BasicStyle.textHiehgt*0.75 }];
				}
				static void PickPersonPanel(MainPage page,PointerRoutedEventArgs e,Panel personPanel,PersonType person) {
					if(game.Phase!=Phase.Starting&&game.PersonMap.GetValueOrDefault(person)?.Country==game.PlayCountry&&personPanel.Parent is Panel parentPanel) {
						personPanel.IsHitTestVisible=false;
						parentPanel.MySetChildren([]);
						page.MovePersonCanvas.MySetChildren([personPanel]);
						MovePerson(page,personPanel,e);
						pick=(parentPanel, person);
					}
				}
			}
			static void MovePerson(MainPage page,UIElement personPanel,PointerRoutedEventArgs e) {
				Canvas.SetLeft(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.X-personPutSize.Width/2);
				Canvas.SetTop(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.Y-personPutSize.Height/2);
			}
			static Color GetPostFrameColor(Game game,EArea? area) => area!=null&&(ScenarioData.scenarios.GetValueOrDefault(game.NowScenario)?.ChinaAreas?? []).Contains(area.Value) ? Color.FromArgb(150,100,100,30) : Color.FromArgb(150,0,0,0);
			static double GetScaleFactor(MainPage page) => Math.Max(page.MainLayoutPanel.ActualWidth/mapSize.Width,page.MainLayoutPanel.ActualHeight/mapSize.Height);
			static void RefreshViewMode(MainPage page) {
				page.SwitchViewModeButtonText.Text=viewMode==ViewMode.fix ? "▼" : "▲";
				page.ViewModeText.Text=viewMode==ViewMode.fix ? "固定幅" : "ウィンドウフィット";
				page.Width=viewMode==ViewMode.fix ? fixModeWidth : double.NaN;
			}
			static void ShowCountryInfo(MainPage page,Game game) {
				double countryInfoFontSize = 22;
				Button nextPhaseButton = new() { HorizontalAlignment=HorizontalAlignment.Stretch,Background=new SolidColorBrush(Color.FromArgb(100,100,100,100)),Height=page.FontSize*4.75 };
				nextPhaseButton.Click+=(_,_) => { MainPage.game=MainPage.game.Phase==Phase.Starting ? StartGame(page,MainPage.game) : MainPage.game.Phase==Phase.Planning ? EndPlanningPhase(page,MainPage.game) : EndExecutionPhase(page,MainPage.game); ShowCountryInfo(page,MainPage.game); };
				page.CountryInfoContentsPanel.MySetChildren([
					new TextBlock{ Text=Turn.GetCalendarText(game),TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}",TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"首都:{game.PlayCountry?.MyApplyF(country=>Area.GetCapitalArea(game,country))}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"資金:{game.PlayCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.Fund.ToString("0.####")}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"内政力:{game.PlayCountry?.MyApplyF(country=>Country.GetAffairPower(game,country)).ToString("0.####")}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"内政難度:{game.PlayCountry?.MyApplyF(country=>Country.GetAffairDifficult(game,country)).ToString("0.####")}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"総内政値:{game.PlayCountry?.MyApplyF(country=>Country.GetTotalAffair(game,country)).ToString("0.####")}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"支出:{game.PlayCountry?.MyApplyF(country=>Country.GetOutFunds(game,country)).ToString("0.####")}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"収入:{game.PlayCountry?.MyApplyF(country=>Country.GetInFunds(game,country)).ToString("0.####")}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						new TextBlock{ Text=$"侵攻:{Country.GetTargetArea(game)?.ToString()??"なし"}",FontSize=countryInfoFontSize,TextAlignment=TextAlignment.Center },
						nextPhaseButton.MyApplyA(v=>v.Content=new TextBlock{ Text=Code.Text.EndPhaseButtonText(game.Phase,Lang.ja) }),
					]); static Game StartGame(MainPage page,Game game) => (game with { Phase=Phase.Planning }).MyApplyA(v => UpdateAreaUI(page,v,[]));
				static Game EndPlanningPhase(MainPage page,Game game) {
					return game.MyApplyF(CalcArmyTarget).MyApplyA(v => UpdateAreaUI(page,game,v)).MyApplyF(v => ArmyAttack(game,v)).MyApplyF(v => v with { Phase=Phase.Execution });
					static Dictionary<ECountry,EArea?> CalcArmyTarget(Game game) {
						return game.CountryMap.Where(country=>country.Value.SleepTurnNum==0).ToDictionary(country => country.Key,country => country.Key==ECountry.漢 ? null : country.Key!=game.PlayCountry ? RandomSelectNPCAttackTarget(game,country.Key) : game.SelectTarget);
						static EArea? RandomSelectNPCAttackTarget(Game game,ECountry country) => Area.GetAdjacentAnotherCountryAllAreas(game,country).MyNullable().Append(null).MyPickAny().MyApplyF(area=> area?.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country!=null&&MyRandom.RandomJudge(0.9)?null:area);
					}
					static Game ArmyAttack(Game game,Dictionary<ECountry,EArea?> targetAreaMap) {
						return game.CountryMap.Aggregate(game,(game,countryInfo) => {
							EArea? targetArea = countryInfo.Value.Fund>=Country.CalcAttackFunds(game,countryInfo.Key) ? targetAreaMap.GetValueOrDefault(countryInfo.Key) : null;
							ECountry? defenseCountry = targetArea?.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country;
							return targetArea!=null ? ExeAttack(game,targetAreaMap,countryInfo.Key,targetArea.Value,defenseCountry) : targetAreaMap.ContainsKey(countryInfo.Key) ? ExeDefense(game,countryInfo.Key) : ExeRest(game,countryInfo.Key,countryInfo.Value.SleepTurnNum);
							static Game ExeAttack(Game game,Dictionary<ECountry,EArea?> targetAreaMap,ECountry country,EArea targetArea,ECountry? defenseCountry) => game.MyApplyF(game => UpdateGame.Attack(game,country,targetArea,defenseCountry,Country.IsFocusDefense(targetAreaMap,defenseCountry)));
							static Game ExeDefense(Game game,ECountry country) => game.MyApplyF(game => UpdateGame.Defense(game,country));
							static Game ExeRest(Game game,ECountry country,int remainRestTurn) => game.MyApplyF(game => UpdateGame.Rest(game,country,remainRestTurn));
						});
					}
				}
				static Game EndExecutionPhase(MainPage page,Game game) {
					return game.MyApplyF(UpdateGame.NextTurn).MyApplyA(v => UpdateAreaUI(page,v,[])).MyApplyA(game => UpdateCountryPostPersons(page,game)).MyApplyA(game => UpdateLogMessage(page,game)).MyApplyF(v => v with { Phase=Phase.Planning });
				}
			}
		}
		static StackPanel[] CreatePersonDataList(int scenarioNo,int chunkBlockNum) {
			ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
			Dictionary<PersonType,PersonParam>[] chunkedPersonInfoMaps = maybeScenarioInfo?.PersonMap.MyApplyF(elems => elems.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).Chunk((int)Math.Ceiling((double)elems.Count/chunkBlockNum))).Select(v => v.ToDictionary()).ToArray()?? [];
			return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedPersonInfoMaps.Select(chunkedPersonInfoMap => CreatePersonDataPanel(scenarioInfo,chunkedPersonInfoMap)).ToArray())?? [];
			static StackPanel CreatePersonDataPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<PersonType,PersonParam> includePersonInfoMap) {
				return new StackPanel { BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor),Background=new SolidColorBrush(dataListFrameColor) }.MySetChildren([
					CreatePersonDataLine(
						Color.FromArgb(255,240,240,240),
						new TextBlock { Text="陣営",HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text="人物名",HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text="ロール",Margin=new(0,0,-BasicStyle.fontsize*1.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
						new TextBlock { Text="ランク",Margin=new(0,0,-BasicStyle.fontsize*1.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
						new TextBlock { Text="登場",HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text="没年",HorizontalAlignment=HorizontalAlignment.Center }
					), .. includePersonInfoMap.Select(personInfo => CreatePersonDataLine(
						scenarioInfo.CountryMap.GetValueOrDefault(personInfo.Value.Country)?.ViewColor??Colors.Transparent,
						new TextBlock { Text=personInfo.Value.Country.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text=personInfo.Key.Value,Margin=new(0,0,-BasicStyle.fontsize*(CalcFullWidthLength(personInfo.Key.Value)-3),0),RenderTransform=new ScaleTransform() { ScaleX=Math.Min(1,3/CalcFullWidthLength(personInfo.Key.Value)) } },
						new Image{ Source=new SvgImageSource(new($"ms-appx:///Assets/Img/{personInfo.Value.Role}.svg")),Width=BasicStyle.fontsize,Height=BasicStyle.fontsize,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center },
						new TextBlock { Text=personInfo.Value.Rank.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text=Code.Person.GetAppearYear(personInfo.Value).MyApplyF(appearYear=>appearYear>=scenarioInfo.StartYear?appearYear.ToString():"登場"),HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text=personInfo.Value.DeathYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center }
					))
				]);
				static StackPanel CreatePersonDataLine(Color backColor,UIElement countryNameElem,UIElement personNameElem,UIElement personRoleElem,UIElement personRankElem,UIElement personAppearYearElem,UIElement personDeathYearElem) {
					return new StackPanel { Orientation=Orientation.Horizontal,Background=new SolidColorBrush(backColor),}.MySetChildren([
						new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
						new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personNameElem),
						new Border{ Width=CalcElemWidth(1.5),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personRoleElem),
						new Border{ Width=CalcElemWidth(1.5),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personRankElem),
						new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personAppearYearElem),
						new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personDeathYearElem),
					]);
					static double CalcElemWidth(double textlength) => BasicStyle.fontsize*textlength+dataListFrameWidth*2;
				}
			}
		}
		static StackPanel[] CreateCountryDataList(int scenarioNo,int chunkBlockNum) {
			ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
			Dictionary<ECountry,CountryInfo>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count/chunkBlockNum))).Select(v => v.ToDictionary()).ToArray()?? [];
			return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateCountryDataPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray())?? [];
			static StackPanel CreateCountryDataPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<ECountry,CountryInfo> includeCountryInfoMap) {
				return new StackPanel { BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor),Background=new SolidColorBrush(dataListFrameColor) }.MySetChildren([
					CreateCountryDataLine(
						Color.FromArgb(255,240,240,240),
						new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text="資金",HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text="所属エリア",HorizontalAlignment=HorizontalAlignment.Center }
					), .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
						scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??Colors.Transparent,
						new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text=countryInfo.Value.Fund.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
						new TextBlock { Text=string.Join(",",scenarioInfo.AreaMap.Where(v=>v.Value.Country==countryInfo.Key).Select(v=>v.Key.ToString())),HorizontalAlignment=HorizontalAlignment.Left }
					))
				]);
				static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,UIElement countryFundElem,UIElement countryAreasElem) {
					return new StackPanel { Orientation=Orientation.Horizontal,Background=new SolidColorBrush(backColor),}.MySetChildren([
						new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
						new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryFundElem),
						new Border{ Width=CalcElemWidth(50),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryAreasElem),
					]);
					static double CalcElemWidth(double textlength) => BasicStyle.fontsize*textlength+dataListFrameWidth*2;
				}
			}
		}
		static double CalcFullWidthLength(string str) => str.Length-str.Where(v => v is '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '-').Count()*0.4;
	}
}