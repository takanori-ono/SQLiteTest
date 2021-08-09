using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using System.IO;
using SQLiteTest.Properties;

namespace SQLiteTest
{
    public class TimeRecord
    {
        public int No { get; set; } // data ID
        public string CardID { get; set; } // ICカード番号（IDM）
        public DateTime Tm { get; set; } // 時刻
        public int ShutuTai { get; set; } // 出退勤(1:出 2:退)
        public int ShokuNo { get; set; } // 職員番号
        public string Name { get; set; } // 氏名
        public bool Saved { get; set; } // ｄｂ保存されたか
        public static string GetShuttaiCodeS(int code)
        {
            switch (code)
            {
                case 1:
                    return "出勤";
                case 2:
                    return "退勤";
                case 3:
                    return "出勤(時間内)";
                case 4:
                    return "退勤(時間内)";
                default:
                    return "";
            }
        }
    }

    class TbEdyElm
    {
        public int Shokuno { get; set; }
        public string CardId { get; set; }
    }
    class TbUserElm
    {
        public int Shokuno { get; set; }
        public string Name { get; set; }
    }

    class Db
    {
        public static int DoneCount;
        static object lock_obj = new object();
        static Count CountObj;
        static public void SetCountObj(Count c)
        {
            CountObj = c;
        }
        public static int GetCount()
        {
            lock (lock_obj)
            {
                return DoneCount;
            }
        }
        static void CountUp()
        {
            lock (lock_obj)
            {
                DoneCount++;
            }
        }
        static string DBPath;
        static public bool InitDb()
        {
            if (Settings.Default.DB_FOLDER == "")
            {
                MessageBox.Show("DBフォルダの設定を行って下さい。configファイルのDB_FOLDER");
                return false;
            }
            var db_folder = new DirectoryInfo(Settings.Default.DB_FOLDER);
            if (!db_folder.Exists)
            {
                MessageBox.Show("DBフォルダが存在しないので、作成して下さい。configファイルのDB_FOLDER");
                return false;
            }
            var db_filename = db_folder.FullName + @"\" + "time_recorder.db";
            DBPath = db_filename;
            var db_file = new FileInfo(db_filename);
            if (!db_file.Exists)
            {
                if (!create_db_file(db_file)) { MessageBox.Show("cant create database file..."); Application.Exit(); }
            }
            return true;
        }
        //static public void InitDb(string apppath)
        //{
        //    AppPath = apppath;
        //    var fi = new FileInfo(apppath);
        //    //var db_folder = new FileInfo(fi.DirectoryName + @"\" + "db");
        //    var db_folder = new DirectoryInfo(fi.DirectoryName + @"\" + "db");
        //    if (!db_folder.Exists)
        //    {
        //        fi.Directory.CreateSubdirectory("db");
        //    }
        //    var db_filename = db_folder.FullName + @"\" + "time_recorder.db";
        //    var db_file = new FileInfo(db_filename);
        //    if (!db_file.Exists)
        //    {
        //        if (!create_db_file(db_file)) { MessageBox.Show("cant create database file..."); Application.Exit(); }
        //    }
        //}
        static bool create_db_file(FileInfo f)
        {
            string forLog = "";
            try
            {
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = f.FullName };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    using (var cmd = new SQLiteCommand(cn))
                    {
                        // create table(card_id)
                        cmd.CommandText = "create table if not exists card_id(" +
                            "no integer not null primary key," +
                            "id text not null," +       // card no
                            "shokuno integer not null" +
                            ")";
                        cmd.ExecuteNonQuery();
                        forLog = "done card_id";

                        // create table(shokuin)
                        cmd.CommandText = "create table if not exists shokuin(" +
                            "no integer not null primary key," +
                            "shokuno integer not null," +
                            "name text not null" +
                            ")";
                        cmd.ExecuteNonQuery();
                        forLog = "done shokuin";

                        // create table(time_rec)
                        cmd.CommandText = "create table if not exists time_rec(" +
                            "no integer not null primary key," +
                            "id text not null," +
                            "shututai integer not null," +
                            "tm integer not null," +    // DateTime.tick
                            "tm_str text not null," +    // tm のデバッグ確認用
                            "sendsv integer not null" +    // サーバ送信済み
                            ")";
                        cmd.ExecuteNonQuery();
                        forLog = "done time_rec";

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.create_db_file", "cant create db file..." + Environment.NewLine + forLog, ex);
            }
            return false;
        }

        static string db_fullpath()
        {
            //if (AppPath == null)
            //{
            //    MessageBox.Show("need db init");
            //    return "";
            //}
            //var fi = new FileInfo(AppPath);
            //var db_filename = fi.DirectoryName + @"\db\" + "time_recorder.db";
            //return db_filename;
            return DBPath;
        }

        static public string GetNameByCardId(string card_id)
        {
            string forLog = "";
            try
            {
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    using (var cmd = new SQLiteCommand(cn))
                    {
                        string idval = card_id;//"01104E00C5175D03";
                                               //cmd.CommandText = $"select * from card_id where id='{idval}'";
                        cmd.CommandText = $"select s.name from card_id c left join shokuin s on s.shokuno = c.shokuno where c.id='{card_id}'";
                        forLog = "sql: " + cmd.CommandText;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var allkeys = string.Join(":", reader.GetValues().AllKeys);
                                //MessageBox.Show(allkeys);
                                var vals = reader.GetValues();
                                var a = vals.Get("name");// integer ではなく string
                                return a;
                                // MessageBox.Show(a);
                                //var shokuno = reader.GetValue("shokuno");
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.WriteException("Db.GetNameByCardId", "failed getting name..." + Environment.NewLine + forLog, ex);
            }

            return "";
        }
        static public bool AddTimeRec(TimeRecord arec)
        {
            return AddTimeRec(arec.CardID, arec.Tm, arec.ShutuTai);
        }
        static public bool AddTimeRec(List<TimeRecord> recList)
        {
            string forLog = "";
            try
            {
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    var trn = cn.BeginTransaction();
                    using (var cmd = new SQLiteCommand(cn))
                    {
                        int cnt = 0;
                        foreach(var rec in recList)
                        {
                            var card_id = rec.CardID;
                            var dt = rec.Tm;
                            var shututai = rec.ShutuTai;

                            string idval = card_id;
                            cmd.CommandText = $"insert into time_rec values((select ifnull(max(no)+1,1) from time_rec), '{card_id}', {shututai}, {dt.Ticks.ToString()}, '{dt.ToString("u")}', 0)";
                            forLog = "sql: " + cmd.CommandText;

                            var rc = cmd.ExecuteNonQuery();
                            if( rc != 1)
                            {
                                Log.WriteError("Db.AddTimeRec", $"failed insert rec to time_rec table" + Environment.NewLine + forLog);
                                trn.Rollback();
                                return false;
                            }
                            cnt++;
                            //CountUp();
                            if (cnt==100)
                            {
                                CountObj.CountUp(100);
                                cnt = 0;
                            }
                        }
                        CountObj.CountUp(cnt);
                    }
                    trn.Commit();
                }
                Log.WriteInfo("Db.AddTimeRec", $"added to db record Count:[{recList.Count()}]" + Environment.NewLine + forLog);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.AddTimeRec", "failed adding to db..." + Environment.NewLine + forLog, ex);
            }
            return false;
        }
        static public bool AddTimeRec(string card_id, DateTime dt, int shututai)
        {
            string forLog = "";
            try
            {
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    using (var cmd = new SQLiteCommand(cn))
                    {
                        string idval = card_id;
                        cmd.CommandText = $"insert into time_rec values((select ifnull(max(no)+1,1) from time_rec), '{card_id}', {shututai}, {dt.Ticks.ToString()}, '{dt.ToString("u")}', 0)";
                        forLog = "sql: " + cmd.CommandText;

                        var rc = cmd.ExecuteNonQuery();
                        Log.WriteInfo("Db.AddTimeRec", $"added to db rc:[{rc}]" + Environment.NewLine + forLog);
                        return rc == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.AddTimeRec", "failed adding to db..." + Environment.NewLine + forLog, ex);
            }
            return false;
        }
        static public List<TimeRecordDebug> TestReadTimeRec()
        {
            string forLog = "";
            try
            {
                var ans = new List<TimeRecordDebug>();
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    using (var cmd = new SQLiteCommand(cn))
                    {
                        cmd.CommandText = $"select * from time_rec";
                        forLog = cmd.CommandText;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                forLog = "";
                                var allkeys = string.Join(":", reader.GetValues().AllKeys);
                                var vals = reader.GetValues();
                                //var a = vals.Get("name");// integer ではなく string

                                var arec = new TimeRecordDebug();
                                arec.No = Convert.ToInt32(vals.Get("no"));
                                forLog = vals.Get("no");
                                arec.CardID = vals.Get("id");
                                arec.RawTm = Convert.ToInt64(vals.Get("tm"));
                                arec.Tm = new DateTime(arec.RawTm);
                                arec.TmDebug = vals.Get("tm_str");
                                arec.ShutuTai = Convert.ToInt32(vals.Get("shututai "));
                                ans.Add(arec);
                            }
                        }
                    }
                }
                return ans;
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.TestReadTimeRec", "failed read timerec..." + Environment.NewLine + forLog, ex);
            }
            return null;
        }
        static public bool RegNewCard(int shokuno, string shokuinName, string cardID)
        {
            var user_elm = new List<TbUserElm> { new TbUserElm { Name = shokuinName, Shokuno = shokuno } };
            var card_elm = new List<TbEdyElm> { new TbEdyElm { CardId = cardID, Shokuno = shokuno } };

            return ImportShokuin(user_elm) && ImportCardId(card_elm);
        }
        static public bool ImportCardId(List<TbEdyElm> importData)
        {
            string forLog = "";
            try
            {
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    foreach (var elm in importData)
                    {
                        using (var cmd = new SQLiteCommand(cn))
                        {
                            cmd.CommandText = $"select count(no) from card_id where id='{elm.CardId}'";
                            forLog = cmd.CommandText;
                            var cnt = (long)cmd.ExecuteScalar();
                            var sql = "";
                            if (cnt == 0)
                            {
                                sql = $"insert into card_id values((select ifnull(max(no)+1,1) from card_id), '{elm.CardId}', {elm.Shokuno})";
                            }
                            else
                            {
                                sql = $"update card_id set shokuno={elm.Shokuno} where id='{elm.CardId}'";
                            }

                            cmd.CommandText = sql;
                            forLog = cmd.CommandText;
                            var rc = cmd.ExecuteNonQuery();
                            if (rc != 1)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.ImportCardId", "failed import to card_id..." + Environment.NewLine + forLog, ex);
            }
            return false;
        }
        static public bool ImportShokuin(List<TbUserElm> importData)
        {
            string forLog = "";
            try
            {
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    foreach (var elm in importData)
                    {
                        using (var cmd = new SQLiteCommand(cn))
                        {
                            cmd.CommandText = $"select count(no) from shokuin where shokuno={elm.Shokuno}";
                            forLog = cmd.CommandText;
                            var cnt = (long)cmd.ExecuteScalar();
                            var sql = "";
                            if (cnt == 0)
                            {
                                sql = $"insert into shokuin values((select ifnull(max(no)+1,1) from shokuin), {elm.Shokuno}, '{elm.Name}')";
                            }
                            else
                            {
                                sql = $"update shokuin set name='{elm.Name}' where shokuno={elm.Shokuno}";
                            }

                            cmd.CommandText = sql;
                            forLog = cmd.CommandText;
                            var rc = cmd.ExecuteNonQuery();
                            if (rc != 1)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.ImportCardId", "failed import to card_id..." + Environment.NewLine + forLog, ex);
            }
            return false;
        }

        // 今日の履歴に読み込む
        // 途中で終了するとメモリ上に履歴が残らないので
        static public bool LoadToday(List<TimeRecord> data)
        {
            string forLog = "";
            try
            {
                var dbpath = db_fullpath();
                var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbpath };
                using (var cn = new SQLiteConnection(sqlConnectionSb.ToString()))
                {
                    cn.Open();

                    using (var cmd = new SQLiteCommand(cn))
                    {
                        var base_date = DateTime.Now.AddDays(-1);
                        cmd.CommandText = $"select * from time_rec where tm >= {base_date}";
                        forLog = cmd.CommandText;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                forLog = "";
                                var allkeys = string.Join(":", reader.GetValues().AllKeys);
                                var vals = reader.GetValues();
                                //var a = vals.Get("name");// integer ではなく string

                                var arec = new TimeRecord();
                                arec.No = Convert.ToInt32(vals.Get("no"));
                                arec.CardID = vals.Get("id");
                                arec.ShutuTai = Convert.ToInt32(vals.Get("shututai"));
                                arec.Tm = new DateTime(Convert.ToInt64(vals.Get("tm")));
                                arec.Saved = true;
                                // 以下はとりあえず不要なので
                                // arec.Shokuno
                                // arec.Name
                                data.Add(arec);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteException("Db.LoadToday", "failed read timerec..." + Environment.NewLine + forLog, ex);
            }
            return false;
        }
    }
    public class TimeRecordDebug
    {
        public int No { get; set; } // data ID
        public string CardID { get; set; } // ICカード番号（IDM）
        public long RawTm { get; set; } // 時刻
        public DateTime Tm { get; set; } // 時刻
        public string TmDebug { get; set; } // 時刻
        public int ShutuTai { get; set; } // 出退勤(1:出 2:退)
    }
}
