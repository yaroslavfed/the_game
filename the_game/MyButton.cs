using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace the_game
{
    class MyButton : Button
    {
        public async void PressAsync()
        {
            var chrome = GetTemplateChild("Chrome") as Decorator;
            if (chrome == null) return;
            var chromeType = chrome.GetType();
            var propertyInfo = chromeType.GetProperty("RenderPressed");
            propertyInfo.SetValue(chrome, true);
            await Task.Delay(100);
            propertyInfo.SetValue(chrome, false);
        }
    }
}
