using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using static Caravel.Debugging.Cv_DebugDialog;

namespace Caravel.Debugging
{
    public class Cv_Debug
    {
        public const ushort CV_LOGFLAG_WRITETODEBUGGER = 1 << 0;
        public const ushort CV_LOGFLAG_WRITETOFILE = 1 << 1;

        #if DEBUG
            private const ushort ERRORFLAG_DEFAULT = CV_LOGFLAG_WRITETODEBUGGER | CV_LOGFLAG_WRITETOFILE;
            private const ushort WARNINGFLAG_DEFAULT = CV_LOGFLAG_WRITETODEBUGGER | CV_LOGFLAG_WRITETOFILE;
            private const ushort LOGFLAG_DEFAULT = CV_LOGFLAG_WRITETODEBUGGER | CV_LOGFLAG_WRITETOFILE;
        #else
            private const ushort ERRORFLAG_DEFAULT = 0;
            private const ushort WARNINGFLAG_DEFAULT = 0;
            private const ushort LOGFLAG_DEFAULT = 0;
        #endif

        private static Cv_Debug instance = null;
        private readonly string DebugLogFile = "./Logs/error.log";
        private object m_TagLock = new object();
        private object m_ErrorMessengerLock = new object();

#region ErrorMessenger class definition
        private class ErrorMessenger
        {
            public bool m_bIsEnabled;

            public ErrorMessenger()
            {
                m_bIsEnabled = true;
            }

            public void Show(string errorMessage, bool isFatal, string funcName, string sourceFile, int lineNum)
            {
                if (m_bIsEnabled)
                {
                    if (instance != null && instance.Error(errorMessage, isFatal, funcName, sourceFile, lineNum) == Cv_ErrorDialogResult.CV_LOGERROR_IGNORE)
                    {
                        m_bIsEnabled = false;
                    }
                }
            }
        }
#endregion

        private Dictionary<string, ushort> m_Tags = new Dictionary<string, ushort>();
        private Dictionary<int, ErrorMessenger> m_ErrorMessengers = new Dictionary<int, ErrorMessenger>();

        public void Initialize(string logTagsFile)
        {
            instance = this;

            // set up the default log tags
            SetDisplayFlags("ERROR", ERRORFLAG_DEFAULT);
            SetDisplayFlags("FATAL", ERRORFLAG_DEFAULT);
            SetDisplayFlags("WARNING", WARNINGFLAG_DEFAULT);
            SetDisplayFlags("INFO", LOGFLAG_DEFAULT);

            // Read tags from file
            if (!string.IsNullOrEmpty(logTagsFile) && File.Exists(logTagsFile)) {
                var tagsDoc = new XmlDocument();
                tagsDoc.Load(logTagsFile);

                var xmlNodes = tagsDoc.SelectNodes("//Tag");
                
                foreach(XmlNode node in xmlNodes)
                {
                    string tag = node.Attributes["name"].Value;

                    ushort flags = 0;

                    bool writeToDebugger = false;
                    writeToDebugger = Convert.ToBoolean(node.Attributes["writeToDebugger"].Value);

                    if (writeToDebugger)
                    {
                        flags |= CV_LOGFLAG_WRITETODEBUGGER;
                    }

                    bool writeToFile = false;
                    writeToFile = Convert.ToBoolean(node.Attributes["writeToFile"].Value);

                    if (writeToFile)
                    {
                        flags |= CV_LOGFLAG_WRITETOFILE;
                    }

                    SetDisplayFlags(tag, flags);
                }
            }
        }

        public void SetDisplayFlags(string tag, ushort flags)
        {
            lock (m_TagLock)
            {
                if (flags != 0)
                {
                    m_Tags[tag] = flags;
                }
                else
                {
                    m_Tags.Remove(tag);
                }
            }
        }

#region Public Debug API
        public static void Fatal(string str)
        {
            #if DEBUG
                if(instance == null)
                {
                    return;
                }

                int hash;
                string func = _Function();
                string file = _File();
                int line = _Line();
                ErrorMessenger msg = null;

                lock (instance.m_ErrorMessengerLock)
                {
                    if (instance.HasErrorMessenger(func, file, line, out hash))
                    {
                        msg = instance.GetErrorMessenger(hash);
                    }
                    else
                    {
                        msg = new ErrorMessenger();
                        instance.AddErrorMessenger(hash, msg);
                    }
                }

                msg.Show(str, true, func, file, line);
            #endif
        }

        public static void Error(string str)
        {
            #if DEBUG
                if (instance == null)
                {
                    return;
                }

                int hash;
                string func = _Function();
                string file = _File();
                int line = _Line();
                ErrorMessenger msg = null;

                lock (instance.m_ErrorMessengerLock)
                {
                    if (instance.HasErrorMessenger(func, file, line, out hash))
                    {
                        msg = instance.GetErrorMessenger(hash);
                    }
                    else
                    {
                        msg = new ErrorMessenger();
                        instance.AddErrorMessenger(hash, msg);
                    }
                }

                msg.Show(str, false, func, file, line);
            #endif
        }

        public static void Warning(string str)
        {
            #if DEBUG
                if (instance == null)
                {
                    return;
                }

                instance.Log("WARNING", str, _Function(), _File(), _Line());
            #endif
        }

        public static void Info(string str)
        {
            #if DEBUG
                if (instance == null)
                {
                    return;
                }

                instance.Log("INFO", str, _Function(), _File(), _Line());
            #endif
        }

        public static void Log(string tag, string str)
        {
            #if DEBUG
                if (instance == null)
                {
                    return;
                }

                instance.Log(tag, str, _Function(), _File(), _Line());
            #endif
        }

        public static void Assert(bool expression, string str)
        {
            #if DEBUG
                if (instance == null)
                {
                    return;
                }

                if (!expression)
                {
                    int hash;
                    string func = _Function();
                    string file = _File();
                    int line = _Line();
                    ErrorMessenger msg = null;

                    lock (instance.m_ErrorMessengerLock)
                    {
                        if (instance.HasErrorMessenger(func, file, line, out hash))
                        {
                            msg = instance.GetErrorMessenger(hash);
                        }
                        else
                        {
                            msg = new ErrorMessenger();
                            instance.AddErrorMessenger(hash, msg);
                        }
                    }

                    msg.Show(str, false, func, file, line);
                }
            #endif
        }
#endregion

#region Private helper methods
        private ErrorMessenger GetErrorMessenger(int hash)
        {
            ErrorMessenger msg = null;
            if (m_ErrorMessengers.TryGetValue(hash, out msg))
            {
                return msg;
            }

            return null;
        }

        private bool HasErrorMessenger(string function, string file, int line, out int hash)
        {
            string id = function + file + line;
            hash = id.GetHashCode();

            if (GetErrorMessenger(hash) != null)
            {
                return true;
            }

            return false;
        }

        private void Log(string tag, string message, string funcName, string sourceFile, int lineNum)
        {
            lock (m_TagLock)
            {
                ushort flags;
                if (m_Tags.TryGetValue(tag, out flags))
                {
                    if ((flags & CV_LOGFLAG_WRITETODEBUGGER) <= 0 && (flags & CV_LOGFLAG_WRITETOFILE) <= 0)
                    {
                        return;
                    }

                    string finalString = GetOutputString(tag, message, funcName, sourceFile, lineNum);
                    if ((flags & CV_LOGFLAG_WRITETOFILE) > 0)
                    {
                        WriteToLogFile(finalString);
                    }
                        
                    if ((flags & CV_LOGFLAG_WRITETODEBUGGER) > 0)
                    {
                        OutputToDebug(finalString);
                    }
                }
            }
        }

        private string GetOutputString(string tag, string message, string funcName, string fileName, int lineNum)
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(tag))
            {
                sb.Append("[").Append(tag).Append("][").Append(DateTime.Now).Append("] ").Append(message);
            }
            else
            {
                sb.Append("[").Append(DateTime.Now).Append("] ").Append(message);
            }

            bool addedNewLine = false;
            if (!string.IsNullOrEmpty(funcName))
            {
                addedNewLine = true;
                sb.Append(Environment.NewLine).Append('\t').Append("Function: ").Append(funcName);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                if (!addedNewLine)
                {
                    sb.Append(Environment.NewLine).Append('\t');
                    addedNewLine = true;
                }

                sb.Append(" (").Append(fileName).Append(") ");
            }

            if (lineNum != 0)
            {
                if (!addedNewLine)
                {
                    sb.Append(Environment.NewLine).Append('\t');
                }

                sb.Append("(").Append("Line: ").Append(lineNum).Append(")");;
            }

            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private void WriteToLogFile(string message)
        {
            (new FileInfo(DebugLogFile)).Directory.Create();
            File.AppendAllText(DebugLogFile, message);
        }

        private void OutputToDebug(string message)
        {
            Debug.Write(message);
        }

        private Cv_ErrorDialogResult Error(string message, bool isFatal, string funcName, string sourceFile, int lineNum)
        {
            string tag = ((isFatal) ? "FATAL" : "ERROR");

            string finalString = GetOutputString(tag, message, funcName, sourceFile, lineNum);

            lock (m_TagLock)
            {
                ushort flags;
                if (m_Tags.TryGetValue(tag, out flags))
                {
                    if ((flags & CV_LOGFLAG_WRITETOFILE) > 0)
                    {
                        WriteToLogFile(finalString);
                    }

                    if ((flags & CV_LOGFLAG_WRITETODEBUGGER) > 0)
                    {
                        OutputToDebug(finalString);
                    }
                }
            }

            var mBoxParams = new Cv_MessageBoxParams();
            mBoxParams.buttons = new Cv_ButtonType[]
            {
                Cv_ButtonType.CV_BUTTON_ABORT,
                Cv_ButtonType.CV_BUTTON_RETRY,
                Cv_ButtonType.CV_BUTTON_IGNORE
            };

            mBoxParams.defaultButton = Cv_ButtonType.CV_BUTTON_RETRY;
            mBoxParams.bgColor = Color.Red;
            mBoxParams.textColor = Color.Green;
            mBoxParams.btBorderColor = Color.Yellow;
            mBoxParams.btBgColor = Color.Blue;
            mBoxParams.btSelectedColor = Color.Purple;
            mBoxParams.title = tag;
            mBoxParams.message = finalString;
            mBoxParams.messageType = Cv_MessageType.CV_MESSAGE_ERROR;

            Cv_ButtonType result;

            if (!ShowMessageBox(mBoxParams, out result)) {
                throw new Exception("Error opening dialog box.");
            }

            switch (result)
            {
                case Cv_ButtonType.CV_BUTTON_ABORT:
                    Debugger.Break();
                    return Cv_ErrorDialogResult.CV_LOGERROR_ABORT;
                case Cv_ButtonType.CV_BUTTON_IGNORE:
                    return Cv_ErrorDialogResult.CV_LOGERROR_IGNORE;
                default:
                    return Cv_ErrorDialogResult.CV_LOGERROR_RETRY;
            }
        }

        private void AddErrorMessenger(int hash, ErrorMessenger errorMessenger)
        {
            m_ErrorMessengers[hash] = errorMessenger;
        }

        private static string _Function()
        {
            StackTrace stackTrace = new StackTrace(true);
            return stackTrace.GetFrame(2).GetMethod().Name;
        }

        private static string _File()
        {
            StackTrace stackTrace = new StackTrace(true);
            return stackTrace.GetFrame(2).GetFileName();
        }

        private static int _Line()
        {
            StackTrace stackTrace = new StackTrace(true);
            return stackTrace.GetFrame(2).GetFileLineNumber();
        }
#endregion
    }
}