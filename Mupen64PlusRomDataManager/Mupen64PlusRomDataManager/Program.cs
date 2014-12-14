using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Mupen64PlusRomDataManager
{
    static class Program
    {
        /// <summary>
        /// The main rom point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
