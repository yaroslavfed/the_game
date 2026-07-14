using ReactiveUI;
using the_game.ViewModels;

namespace the_game.Views;

public partial class BattleView : ReactiveUserControl<BattleViewModel>
{
    public BattleView()
    {
        InitializeComponent();
        this.WhenActivated(_ => { });
    }
}
