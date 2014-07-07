namespace DiversityPhone.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ReactiveUI;
    using BugSense;
    using BugSense.Core.Model;
    using ReactiveUI.NLog;

    class BugSenseLogger : NLogLogger
    {
        public BugSenseLogger(NLog.Logger inner)
            : base(inner)
        {            
        }

        private void BugSenseLogException(string message, Exception exception, LogLevel level)
        {
            if (App.EnableBugSense)
            {
                var Handler = BugSenseHandler.Instance;
                Handler.LogException(exception,
                    new LimitedCrashExtraDataList() {
                        new CrashExtraData()
                        {
                            Key = "level",
                            Value = level.ToString()
                        }
                    }
                );
            }
        }       

        public override void DebugException(string message, Exception exception)
        {
            BugSenseLogException(message, exception, LogLevel.Debug);
            base.DebugException(message, exception);
        }


        public override void ErrorException(string message, Exception exception)
        {
            BugSenseLogException(message, exception, LogLevel.Error);
            base.ErrorException(message, exception);
        }

        public override void FatalException(string message, Exception exception)
        {
            BugSenseLogException(message, exception, LogLevel.Fatal);
            base.FatalException(message, exception);
        }

        public override void InfoException(string message, Exception exception)
        {
            BugSenseLogException(message, exception, LogLevel.Info);
            base.InfoException(message, exception);
        }

        public override void WarnException(string message, Exception exception)
        {
            BugSenseLogException(message, exception, LogLevel.Warn);
            base.WarnException(message, exception);
        }
    }
}
