using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//sing Simple7;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using Jayrock.JsonRpc;

namespace BitMiner
{
    public partial class Form1 : Form
    {
        
        System.Windows.Forms.Timer statsUpdateTimer = new System.Windows.Forms.Timer();
        int miners = 0;
       // public System.IO.StreamReader mine1sr;
       // public int mine1tc = 1;
        string[] khash = new string[10];
        object[] m = new object[50];
        StatsUpdater Stats = new StatsUpdater();
        int[] minerspeed = new int[50];
        int[] minersshares = new int[50];
        int[] minerashares = new int[50];
        int[] failovercount = new int[50];
        const int FailOverMax = 5;
        int totalspeed = 1;
        int totalashares = 0;
        int totalsshares = 0;

        public Form1()
        {
            
            InitializeComponent();
        }

   
        protected override void OnPaint(PaintEventArgs e)
        {
            if (File.Exists("Simple7.dll"))
            {
               //Simple7.NonClientArea.PaintBackground(e, this); // COMMENT FOR XP
            }
            else
            {
                base.OnPaint(e);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("Simple7.dll"))
            {
               //Simple7.NonClientArea.ExtendFrame(this, 4, 361, 20, 2); // COMMENT FOR XP
            } 
            if (!(System.IO.Directory.Exists("accounts")))
            {
                System.IO.Directory.CreateDirectory("accounts");
            }
            

           // BitMiner.Monitors.GPU gpu = new Monitors.GPU();

            statsUpdateTimer.Tick += new EventHandler(statsUpdateTimer_Tick);
            statsUpdateTimer.Interval = 10;
            Stats.StatsChanged += new StatsUpdater.StatsChangedHandler(Stats_StatsChanged);

            statsUpdateTimer.Start();
            Stats.Start();
        }

        void statsUpdateTimer_Tick(object sender, EventArgs e)
        {
            totalspeed = 0;
            totalashares=  0;
            totalsshares = 0;
            foreach (int i in minerspeed)
            {
                totalspeed += i;
            }

            foreach (int i in minerashares)
            {
                totalashares += i;
            }
            foreach (int i in minersshares)
            {
                totalsshares += i;
            }
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[1].Controls["groupBox5"].Controls["label23"].Text = totalspeed.ToString();
            });
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[1].Controls["groupBox5"].Controls["label29"].Text = totalashares.ToString();
            });
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[1].Controls["groupBox5"].Controls["label30"].Text = totalsshares.ToString();
            });
        }

        void Stats_StatsChanged(object sender, StatsUpdater.StatsUpdatedArgs e)
        {
            label31.Text = e.networkSpeed.ToString();
            label44.Text = e.networkBTC.ToString() + " mill";
            label33.Text = e.currentBlock.ToString();
            label35.Text = e.currentDifficulty.ToString();
            label39.Text = e.exchangeRate.ToString();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            label19.Text = hScrollBar1.Value.ToString();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox1.Text);
            if (!(System.IO.Directory.Exists("accounts")))
            {
                System.IO.Directory.CreateDirectory("accounts");
            }
            System.IO.StreamWriter sw = new System.IO.StreamWriter("accounts\\" + textBox1.Text + ".txt");
            sw.WriteLine(textBox2.Text);
            sw.WriteLine(textBox3.Text);
            sw.WriteLine(comboBox1.SelectedItem.ToString());
            if ((textBox5.Text != string.Empty) || (textBox5.Text != "ip:port") || (textBox5.Text != "host:port"))
            {
                sw.WriteLine(textBox5.Text);
            }
            sw.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter("settings.txt");
            sw.WriteLine(textBox4.Text);
            sw.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                string pooluser = "test";
                string poolpass = "test";
                int counter = 0;
                string line;
                System.IO.StreamReader file = new System.IO.StreamReader("accounts/" + listBox2.SelectedItem + ".txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (counter == 0)
                    {
                        pooluser = line;
                    }
                    if (counter == 1)
                    {
                        poolpass = line;
                    }
                    counter++;
                }

                int nm = newMiner(listBox2.SelectedItem.ToString(), 0);
            }
            else
            {
                MessageBox.Show("Please select an account.");
            }

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Don't Use.");

            string[] accounts = System.IO.Directory.GetFiles("accounts\\", "*.txt");
            foreach (string s in accounts)
            {
                listBox1.Items.Add(s.Substring(9).Replace(".txt", ""));
                listBox2.Items.Add(s.Substring(9).Replace(".txt", ""));
                comboBox2.Items.Add(s.Substring(9).Replace(".txt", ""));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {


            string account = listBox1.SelectedItem.ToString();
            System.IO.StreamReader sr = new System.IO.StreamReader("accounts/" + account + ".txt");
            textBox5.Text = "";
            textBox1.Text = account;


            int counter = 0;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (counter == 0)
                {
                    textBox2.Text = line;
                }
                if (counter == 1)
                {
                    textBox3.Text = line;
                }
                if (counter == 2)
                {
                    comboBox1.SelectedItem = line;
                }
                if (counter == 3)
                {
                    if (line != String.Empty)
                    {
                        textBox5.Text = line;
                    }
                }
                counter++;
            }

            sr.Close();

        }

        int newMiner(string nick, int failover)
        {
            Miner miner;
            miners++;
            bool vectors  = false;



            if (checkBox3.Checked)
            {
                vectors = true;
            }

            StreamReader sr = new StreamReader("accounts/" + listBox2.SelectedItem.ToString() + ".txt");
            int counter = 0;
            string line;
            string pUser = "setauser";
            string pPass = "setapass";
            string pPool = "DeepBit";
            string pHost = "nil";

            while ((line = sr.ReadLine()) != null)
            {
                if (counter == 0)
                {
                    pUser = line;
                }
                if (counter == 1)
                {
                    pPass = line;
                }
                if (counter == 2)
                {
                    pPool = line;
                }
                if (counter == 3)
                {
                    if (line != string.Empty)
                    {
                        pHost = line;
                    }
                }

                counter++;
            }

            int d, f;
            d = (int)numericUpDown2.Value;
            f = (int)numericUpDown3.Value;

            Pool p = Pool.DeepBit;
            MinerProgram mp = MinerProgram.poclbm;


            if (pPool == "DeepBit")
            {
                p = Pool.DeepBit;
            }
            else if (pPool == "Slush's Pool")
            {
                p = Pool.Slush;
            }
            else if (pPool == "BTC Guild")
            {
                p = Pool.BTCGuild;
            }
            else if (pPool == "BTC Mine")
            {
                p = Pool.BTCMine;
            }
            else if (pPool == "Eligius")
            {
                p = Pool.Eligius;
            }
            else if (pPool == "Custom")
            {
                p = Pool.Custom;
            }
            else if (pPool == "Ozco.in")
            {
                p = Pool.Ozco;
            }
                 
            
                miner = new Miner(d, f, vectors, hScrollBar1.Value, nick + " - Miner " + miners, p, mp, pUser, pPass, pHost);

                m[miners] = miner;
            miner.SpeedChanged += new Miner.SpeedChangedHandler(SetSpeed);
            miner.StaleShare += new Miner.StaleShareHandler(SetSShares);
            miner.AcceptedShare += new Miner.AcceptedShareHandler(SetAShares);
            miner.NewBlock += new Miner.NewBlockHandler(SetBlock);
            miner.Error += new Miner.ErrorHandler(Error);
            miner.FailOver += new Miner.FailOverHandler(FailOver);


            tabControl1.TabPages.Add(miner.newMinerPage(nick));
            miner.Tab = tabControl1.TabPages[5];
            miner.TabIndex = 4 + miners;
            miner.Initialize();
            miner.Start();


            

            

           


            return 1;




        }

        private void SetSpeed(object sender, Miner.SpeedChangedArgs e)
        {
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[e.TabIndex].Controls["l8"].Text = e.CurrentSpeed;
            });
            int speed;
            bool b = int.TryParse(e.CurrentSpeed, out speed);
            if (b)
                minerspeed[e.TabIndex] = speed;



            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[1].Controls["groupBox5"].Controls["label23"].Text = totalspeed.ToString();
            });
        }

        private void SetBlock(object sender, Miner.NewBlockArgs e)
        {
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[e.TabIndex].Controls["l5"].Text = e.BlockID.ToString();
                tabControl1.TabPages["Stats"].Controls["groupBox8"].Controls["label33"].Text = e.BlockID.ToString();

            });


        }

        private void SetAShares(object sender, Miner.AcceptedShareArgs e)
        {
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[e.TabIndex].Controls["l6"].Text = e.Accepted.ToString();
            });

            minersshares[e.TabIndex] = 0;
            minerashares[e.TabIndex] = e.Accepted;
        }

        private void SetSShares(object sender, Miner.StaleShareArgs e)
        {
            tabControl1.BeginInvoke((MethodInvoker)delegate
            {
                tabControl1.TabPages[e.TabIndex].Controls["l7"].Text = e.Stale.ToString();
            });

            minersshares[e.TabIndex] += e.Stale;
        }

        private void Error(object sender, Miner.ErrorArgs e)
        {
            MessageBox.Show(e.ErrorMessage);
        }

        private void FailOver(object sender, Miner.FailOverArgs e)
        {
            failovercount[e.TabPage]++;

            if (failovercount[e.TabPage] > FailOverMax)
            {
                Miner miner = new Miner(e.device, e.framerate, e.vectors, e.worksize, e.nick, e.pool, e.mp, e.pooluser, e.poolpass, e.phost);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Miner mi in m)
            {
                if(mi != null)
                    mi.Stop(this, new EventArgs());
            } 

            ProcessStartInfo psi = new ProcessStartInfo("taskkill.exe", "/IM poclbm.exe /F");
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();


          
            
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string pool = comboBox1.SelectedItem.ToString();

            textBox2.Enabled = false;
            textBox3.Enabled = false;
            button6.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;


            if (pool == "DeepBit")
            {
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }
            else if (pool == "Slush's Pool")
            {
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }
            else if (pool == "BTC Guild")
            {
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }
            else if (pool == "BTC Mine")
            {
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }
            else if (pool == "Eligius")
            {
                textBox4.Enabled = true;
                button6.Enabled = true; 
            }
            else if (pool == "Ozco.in")
            {
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }
            else if (pool == "Custom")
            {
                textBox2.Enabled = true;
                textBox3.Enabled = true;
                textBox5.Enabled = true;
            }

        }

        private void tabPage6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stats.TimerTick();
        }


       
    }
}
