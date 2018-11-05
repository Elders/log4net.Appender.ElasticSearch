using System;
using Log4stash.Playground.Logging;

namespace Log4stash.Playground
{
    class Program
    {
        static ILog log = LogProvider.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            var gg = log4net.Config.XmlConfigurator.Configure(log4net.LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()), new System.IO.FileInfo("log4net.config"));

            log.Info("Press any key to exit");
            log.Info("Press any key to exit");
            log.Info("Press any key to exit");
            log.Info("Press any key to exit"); 
            log.Info("Press any key to exit");
            log.Info("Press any key to exit");
            log.Info("Press any key to exit");
            log.Info("Press any key to exit");
            log.Info("Press any key to exit");
            log.Info("Press any key to exit");

            Console.ReadLine();
        }
    }
}

