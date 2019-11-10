using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicGrid
{
    public struct Functions
    {
        public const string ToggleStream = "toggle_stream";
        public const string Play = "play";
        public const string Pause = "pause";
        public const string Stop = "stop";
        public const string SetTrack = "set_track";
        public const string SetVolume = "set_volume";

        public const string FitViewToSelection = "fit_view_to_selection";
        public const string FitView = "fit_view";

        public const string ToggleConsole = "toggle_console";
        public const string OpenConsole = "open_console";
        public const string Print = "print";
        public const string CloseConsole = "close_console";

        public const string Set = "set";
        public const string Get = "get";
        public const string Quit = "quit";

        public const string EnableSnap = "enable_snap";
        public const string DisableSnap = "disable_snap";

        public const string SelectAll = "select_all";

        public const string EnableMultiselect = "enable_multiselect";
        public const string DisableMultiselect = "disable_multiselect";

        public const string SaveGrid = "save_grid";
        public const string LoadGrid = "load_grid";
        public const string ImportDistrict = "import_district";
        public const string AskSaveGrid = "ask_save_grid";
        public const string AskLoadGrid = "ask_load_grid";
        public const string AskImportDistrict = "ask_import_district";
    }
}
