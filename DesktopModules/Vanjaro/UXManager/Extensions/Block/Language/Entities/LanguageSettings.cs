using DotNetNuke.UI.Skins;

namespace Vanjaro.UXManager.Extensions.Block.BlockLanguage.Entities
{
    public class LanguageSettings : SkinObjectBase
    {
        private bool _showMenu = true;

        public bool ShowLinks { get; set; }

        public bool ShowMenu
        {
            get
            {
                if ((_showMenu == false) && (ShowLinks == false))
                {
                    //this is to make sure that at least one type of selector will be visible if multiple languages are enabled
                    _showMenu = true;
                }
                return _showMenu;
            }
            set => _showMenu = value;
        }

    }
}
