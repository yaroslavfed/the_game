using System.Windows.Input;

namespace the_game.Input;

public interface IKeyBindingService
{
    Key GetKey(GameAction action);
    void SetKey(GameAction action, Key key);
    void Reset();
}
