using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using SQLiteTest.Properties;

namespace SQLiteTest
{
    public enum LogLevel : int
    {
        Debug = 1,
        Info = 2,
        Error = 3,
        Exception = 4,
    }

    class Log
    {
        static string LogFileName = "Log.log";
        static bool DoneInit=false;

        static void init()
        {
            if (!DoneInit)
            {
                var folder = Application.LocalUserAppDataPath;
                var asmName = Assembly.GetExecutingAssembly().GetName();
                LogFileName = folder + @"\" + asmName.Name + ".log";
                DoneInit = true;
            }
        }
        static bool out_log_level(LogLevel level)
        {
            switch (level)
            {
                default: return false;
                case LogLevel.Debug:
                    return Settings.Default.LOG_DEBUG;
                case LogLevel.Info:
                    return Settings.Default.LOG_INFO;
                case LogLevel.Error:
                    return Settings.Default.LOG_ERROR;
                case LogLevel.Exception:
                    return Settings.Default.LOG_EXCEPTION;
            }
        }
        private static void Write(LogLevel level, string moduleName="NoModule", string comment="no comment")
        {
            try
            {
                if (!out_log_level(level)) return;
                init();

                var msg = DateTime.Now.ToString("u") + "  ";
                switch (level)
                {
                    case LogLevel.Debug:
                        msg += "[Debug]".PadRight(12); break;
                    case LogLevel.Info:
                        msg += "[Info]".PadRight(12); break;
                    case LogLevel.Error:
                        msg += "[Error]".PadRight(12); break;
                    case LogLevel.Exception:
                        msg += "[Exception]".PadRight(12); break;
                }
                msg += moduleName.PadRight(30);
                msg += comment;
                System.IO.File.AppendAllText(LogFileName, msg + Environment.NewLine);
            }
            catch (Exception) { MessageBox.Show("unable to output log..." + Environment.NewLine + $"path:{LogFileName}"); }
        }
        public static void WriteDebug(string moduleName = "NoModule", string comment = "no comment")
        {
            Write(LogLevel.Debug, moduleName, comment);
        }
        public static void WriteInfo(string moduleName = "NoModule", string comment = "no comment")
        {
            Write(LogLevel.Info, moduleName, comment);
        }
        public static void WriteError(string moduleName = "NoModule", string comment = "no comment")
        {
            Write(LogLevel.Error, moduleName, comment);
        }
        public static void WriteException(string moduleName = "NoModule", string comment = "no comment", Exception ex = null)
        {
            var exp_msg = "";
            if (ex != null) {
                exp_msg += "  " + "msg:" + Environment.NewLine;
                exp_msg += "    " + ex.Message + Environment.NewLine;
                exp_msg += "  " + "source:" + Environment.NewLine;
                exp_msg += "    " + ex.Source + Environment.NewLine;
                exp_msg += "  " + "stack:" + Environment.NewLine;
                exp_msg += "    " + ex.StackTrace + Environment.NewLine;
                comment += Environment.NewLine + exp_msg;
            }

            Write(LogLevel.Exception, moduleName, comment);
        }
    }
    //Public Sub Write2Log(ByVal strSyori As String, ByVal strPos As String, ByVal strText As String)
    //    Try
    //        Dim strBufText As String
    //        If pLogFileNM = "" Then
    //            SetLogFolder("")
    //        End If

    //        Dim strForm As String = If(pStrForm <> "", pStrForm, "Unset")
    //        strBufText = Format(Now, "yyyy/MM/dd HH:mm:ss") _
    //            & " " & n2s(pStrForm).PadRight(30) _
    //            & " " & strSyori.PadRight(30) _
    //            & " " & strPos.PadLeft(5) _
    //            & " " & strText & vbCrLf

    //        My.Computer.FileSystem.WriteAllText(pLogFileNM, strBufText, True)

    //    Catch ex As Exception
    //        MsgBox("ログファイル書込み中に例外が発生しました。" & vbCrLf & ex.Message.ToString())

    //    End Try

    //End Sub
}
