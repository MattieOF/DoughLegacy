using System.Text;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;

namespace Dough.Core
{
    public class Log
    {
        public static void Init()
        {
            var logConfig = new LoggingConfiguration();
            
            var logFile = new FileTarget("logFile")
            {
                FileName = "log.txt",
                Encoding = Encoding.UTF8
            };
            var logConsole = new ColoredConsoleTarget("logConsole")
            {
                Encoding = Encoding.UTF8
            };
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange));
            
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logConsole);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

            LogManager.Configuration = logConfig;
            
            LogManager.GetCurrentClassLogger().Info("Hello!");
            LogManager.GetCurrentClassLogger().Warn("uh oh");
            LogManager.GetCurrentClassLogger().Fatal("fuck");
        }

        public static void Shutdown()
        {
            LogManager.Shutdown();
        }
    }
}