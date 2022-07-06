using System;
using System.Text;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;

namespace Dough.Core
{
    public static class Log
    {
        private static Logger engineLogger;
        private static Logger appLogger;
        
        private static FileTarget logFile;
        private static ColoredConsoleTarget logConsole;
        
        [ConfigValue("LogEnabled", ConfigFiles.EngineCore)]
        private static bool _logEnabled = true;
        
        public static void Init(string appName = "App")
        {
            var logConfig = new LoggingConfiguration();
            
            logFile = new FileTarget("logFile")
            {
                FileName = "Logs/log.txt",
                Encoding = Encoding.UTF8,
                ArchiveOldFileOnStartup = true,
            };
            logConsole = new ColoredConsoleTarget("logConsole")
            {
                Encoding = Encoding.UTF8
            };
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange));
            
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logConsole);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

            LogManager.Configuration = logConfig;
            
            engineLogger = LogManager.Setup().GetLogger("Dough");
            appLogger = LogManager.Setup().GetLogger(appName);
            
            EngineInfo("Initialised log!");
        }

        public static void Configure()
        {
            // This is a bit of a hack. Since we have to init the logger before the config system to
            // allow the config system to log (which I believe is more important), we have to configure
            // based on configvalues after we have already called Log.Init(). 
            // For now, the solution is just this Reconfigure function which is called directly after
            // the config systems initialisation, however there are a couple other things we could do instead:
            // 1. Not have config for the log. As long as we're not opening a console for distribution builds, it's fine.
            // 2. Have the log manage its own config. Could still just use Tomlet, serialise a "LogConfig" class. ez.
            if (!_logEnabled)
            {
                // If log is disabled, just null loggers.
                engineLogger = LogManager.CreateNullLogger();
                appLogger = LogManager.CreateNullLogger();
            }
        }

        public static void Shutdown()
        {
            LogManager.Shutdown();
        }

        public static void Trace(string msg) => appLogger.Trace(msg);
        public static void Info(string msg) => appLogger.Info(msg);
        public static void Warn(string msg) => appLogger.Warn(msg);
        public static void Error(string msg) => appLogger.Error(msg);
        public static void Fatal(string msg) => appLogger.Fatal(msg);
        public static void Fatal(Exception e, string msg) => appLogger.Fatal(e, msg);

        internal static void EngineTrace(string msg) => engineLogger.Trace(msg);
        internal static void EngineInfo(string msg) => engineLogger.Info(msg);
        internal static void EngineWarn(string msg) => engineLogger.Warn(msg);
        internal static void EngineError(string msg) => engineLogger.Error(msg);
        internal static void EngineFatal(string msg) => engineLogger.Fatal(msg);
        internal static void EngineFatal(Exception e, string msg) => engineLogger.Fatal(e, msg);
    }
}
