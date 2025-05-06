namespace 悲愴三国志Zero2_1.Code {
	internal static class BaseData {
		internal static readonly int capitalPieceRowNum = 3;
		internal static readonly int capitalPieceColumnNum = 5;
		internal static readonly int capitalPieceCellNum = capitalPieceRowNum*capitalPieceColumnNum;
		internal static readonly double areaButtonXPadding = 0.75;
		internal static readonly double areaButtonYPadding = 0.75;
		internal static readonly double areaButtonLeft = (100+areaButtonXPadding)/9;
		internal static readonly double areaButtonTop = (100+areaButtonYPadding)/10;
		internal static readonly double areaButtonWidth = areaButtonLeft-areaButtonXPadding;
		internal static readonly double areaButtonHeight = areaButtonTop-areaButtonYPadding;
		internal static readonly string[] yearItems = ["春","夏","秋","冬"];
		internal static readonly int majorityAge = 16;
	}
}
