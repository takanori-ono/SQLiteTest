using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQLiteTest
{
    public partial class Form1 : Form
    {
        int progress_value;
        List<TimeRecord> recList;
        Thread aThread;
        Thread bThread;
        delegate void DelegateUpdateProgress();
        int MaxCount;
        Count Count;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.WriteInfo("init db");
            Db.InitDb();
        }
        private void btnCreateDB_Click(object sender, EventArgs e)
        {
        }

        private void btnAddRec_Click(object sender, EventArgs e)
        {
            //var d = new TimeRecord();
            //d.CardID = "123456ABCDEFG";
            //d.Tm = DateTime.Now;
            //d.ShutuTai = 1;
            //Db.AddTimeRec(d);

            progressBar.Value = 0;

            int max_cnt;
            if (int.TryParse(txtRecCnt.Text, out max_cnt))
            {
            }
            else
            {
                max_cnt = 1;
            }
            recList = new List<TimeRecord>();
            var r = new Random();
            for (int i = 0; i < max_cnt; ++i)
            {
                var dt = new DateTime(2021, 1, 1).AddMinutes(r.Next(365*24*60));
                var d = new TimeRecord();
                d.CardID = "123456ABCDEFG";
                d.Tm = dt;
                d.ShutuTai = 1;
                recList.Add(d);
            }
            MaxCount = max_cnt;
            Count = new Count();
            aThread = new Thread(new ThreadStart(add_rec_thread));
            aThread.Start();
            bThread = new Thread(new ThreadStart(progress_countup_thread2));
            bThread.Start();

        }

        void add_rec_thread()
        {
            Db.SetCountObj(Count);
            Db.AddTimeRec(recList);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //for(int i=1; i<=100; ++i)
            //{
            //    progressBar.Value = i;
            //    System.Threading.Thread.Sleep(100);
            //}

            //progress_countup_thread();

            aThread = new Thread(new ThreadStart(progress_countup_thread));
            aThread.Start();

        }

        void progress_countup_thread()
        {
            progress_value = 0;
            while (progress_value<100)
            {
                progress_value++;
                UpdateProgress();
                System.Threading.Thread.Sleep(100);
            }
        }
        void progress_countup_thread2()
        {
            progress_value = 0;
            while (progress_value < 100)
            {
                if (MaxCount > 0)
                {
                    progress_value = (int)((double)Count.GetCount() / MaxCount * 100);
                    UpdateProgress();
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        void UpdateProgress()
        {
            if (this.InvokeRequired)
            {
                // 別スレッドから呼ばれた場合は、こっち
                this.Invoke(new DelegateUpdateProgress(this.UpdateProgress));
                return;
            }

            progressBar.Value = progress_value;
        }

    }
    
    class Count
    {
        object lock_obj = new object();
        volatile int Cnt=0;

        public int GetCount()
        {
            lock (lock_obj)
            {
                return Cnt;
            }
        }
        public void CountUp(int n)
        {
            lock (lock_obj)
            {
                Cnt += n;
            }
        }
    }
}
