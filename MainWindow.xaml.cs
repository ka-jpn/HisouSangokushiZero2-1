using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace îﬂú∆éOçëéuZero2_1 {
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>    
	public sealed partial class MainWindow:Window {
		public MainWindow() {
			InitializeComponent();
			Frame rootFrame = new() { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,HorizontalContentAlignment=HorizontalAlignment.Stretch,VerticalContentAlignment=VerticalAlignment.Stretch };
			rootFrame.Navigate(typeof(MainPage));
			Content=rootFrame;
		}
	}
}