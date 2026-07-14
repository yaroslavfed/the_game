using System.Text.Json;
using System.IO;
using System.Windows.Input;
using TheGame.Core.Storage;

namespace the_game.Input;

public sealed class KeyBindingService : IKeyBindingService
{
    private static readonly IReadOnlyDictionary<GameAction, Key> Defaults =
        new Dictionary<GameAction, Key>
        {
            [GameAction.Attack] = Key.A,
            [GameAction.Heal] = Key.H,
            [GameAction.ExitBattle] = Key.Escape
        };

    private readonly string _filePath;
    private Dictionary<GameAction, Key> _bindings;

    public KeyBindingService(IAppPaths paths)
    {
        _filePath = Path.Combine(paths.UserDataDirectory, "keybindings.json");
        _bindings = Load();
    }

    public Key GetKey(GameAction action) => _bindings.GetValueOrDefault(action, Defaults[action]);

    public void SetKey(GameAction action, Key key)
    {
        GameAction? conflict = _bindings
            .Where(pair => pair.Key != action && pair.Value == key)
            .Select(pair => (GameAction?)pair.Key)
            .FirstOrDefault();

        if (conflict is not null)
        {
            _bindings[conflict.Value] = GetKey(action);
        }

        _bindings[action] = key;
        Save();
    }

    public void Reset()
    {
        _bindings = Defaults.ToDictionary();
        Save();
    }

    private Dictionary<GameAction, Key> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return Defaults.ToDictionary();
            }

            Dictionary<string, string>? stored = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(_filePath));
            var result = Defaults.ToDictionary();
            foreach ((string actionName, string keyName) in stored ?? [])
            {
                if (Enum.TryParse(actionName, out GameAction action) &&
                    Enum.TryParse(keyName, out Key key) && IsAllowed(key))
                {
                    result[action] = key;
                }
            }
            return result;
        }
        catch (JsonException)
        {
            return Defaults.ToDictionary();
        }
        catch (IOException)
        {
            return Defaults.ToDictionary();
        }
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        Dictionary<string, string> data = _bindings.ToDictionary(
            pair => pair.Key.ToString(), pair => pair.Value.ToString());
        File.WriteAllText(_filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static bool IsAllowed(Key key) => key is not (Key.None or Key.System or Key.LeftAlt or Key.RightAlt
        or Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin);
}
