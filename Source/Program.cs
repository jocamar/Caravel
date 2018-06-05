using Caravel.TestSamples;
using System;

namespace Caravel
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var app = new SimpleGame(1280, 720))
            {
                app.Run();
            }
        }
    }
}
