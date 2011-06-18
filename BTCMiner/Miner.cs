using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;


namespace BitMiner
{


    enum Pool
    {
        DeepBit,
        Slush,
        BTCMine,
        BTCGuild,
        Eligius,
        Ozco,
        Custom
    }

    enum MinerProgram
    {
        poclbm,
        pheonix
    }

    class Miner
    {
        bool running = false;
        string arguments;
        string nickname;
        private Pool pool;
        private MinerProgram program;
        private Process p = new Process();
        public TabPage Tab;
        public int TabIndex;
        private int id = 0;
        private int DeviceID = 0;
        private string pURI;
        private int pPort;
        private string pUser;
        private string pPass;
        
        public int FailOverMiner = 0;
        public int FailOverCount = 5;
        private int foc = 0;
        private int ASHARES = 0;
        private int BLOCKID = 31337;
        private int SSHARES = 0;

        int d;
        int f;
        int w;
        bool v;
        string pu;
        string pp;
        string ph;


        public int PID
        {
            get
            {
                return id;
            }
        }



        #region Handlers

        

        public class MinerStoppedArgs : System.EventArgs
        {

        }

        public class FailOverArgs : System.EventArgs
        {
            public int TabPage;
            public string nick;
            public int device;
            public int framerate;
            public bool vectors;
            public int worksize;
            public Pool pool;
            public MinerProgram mp;
            public string pooluser;
            public string poolpass;
            public string phost;
        }


        public class SpeedChangedArgs : System.EventArgs
        {
            public string CurrentSpeed;
            public int TabIndex;

        }

        public class NewBlockArgs : System.EventArgs
        {
            public int BlockID = 0;
            public int CurrentBlock = 0;
            public int TabIndex;

        }

        public class AcceptedShareArgs : System.EventArgs
        {
            public int Accepted = 0;
            public int TabIndex;
        }

        public class StaleShareArgs : System.EventArgs
        {
            public int Stale = 0;
            public int TabIndex;

        }

        public class ErrorArgs : System.EventArgs
        {
            public string ErrorMessage = "";
        }

        public delegate void SpeedChangedHandler(object sender, SpeedChangedArgs e);

        public event SpeedChangedHandler SpeedChanged;

        public delegate void FailOverHandler(object sender, FailOverArgs e);

        public event FailOverHandler FailOver;

        public delegate void NewBlockHandler(object sender, NewBlockArgs e);

        public event NewBlockHandler NewBlock;

        public delegate void AcceptedShareHandler(object sender, AcceptedShareArgs e);

        public event AcceptedShareHandler AcceptedShare;

        public delegate void StaleShareHandler(object sender, StaleShareArgs e);

        public event StaleShareHandler StaleShare;

        public delegate void ErrorHandler(object sender, ErrorArgs e);

        public event ErrorHandler Error;

        public delegate void StoppedHandler(object sender, MinerStoppedArgs e);

        public event StoppedHandler Stopped;

        #endregion

        public Miner(int Device, int framerate, bool vectors, int work, string Nickname, Pool p, MinerProgram mp, string PoolUser, string PoolPass, string host)
        {
            #region pools
            if (p == Pool.DeepBit)
            {
                pool = p;
                pURI = "pit.deepbit.net";
                pPort = 8332;
                pUser = PoolUser;
                pPass = PoolPass;
            }

            if (p == Pool.Slush)
            {
                pool = p;
                pURI = "mining.bitcoin.cz";
                pPort = 8332;
                pUser = PoolUser;
                pPass = PoolPass;
            }

            if (p == Pool.BTCMine)
            {
                pool = p;
                pURI = "btcmine.com";
                pPort = 8332;
                pUser = PoolUser;
                pPass = PoolPass;
            }
            if (p == Pool.Eligius)
            {
                pool = p;
                pURI = "mining.eligius.st";
                pPort = 8337;
                StreamReader sr = new StreamReader("settings.txt");
                string lol = sr.ReadLine();
                sr.Close();
                if (lol != string.Empty)
                {
                    pUser = lol;
                }
                pPass = "x";
            }
            if (p == Pool.BTCGuild)
            {
                pool = p;
                pURI = "btcguild.com";
                pPort = 8332;
                pUser = PoolUser;
                pPass = PoolPass;
            }
            if (p == Pool.Ozco)
            {
                pool = p;
                pURI = "ozco.in";
                pPort = 8332;
                pUser = PoolUser;
                pPass = PoolPass;
            }
            if (p == Pool.Custom)
            {
                pool = p;
                string[] s = host.Split(':');
                pURI = s[0];
                int t;
                bool b = int.TryParse(s[1], out t);
                pPort = t;
                pUser = PoolUser;
                pPass = PoolPass;
            }

            #endregion
            #region miners
            if (mp == MinerProgram.poclbm)
            {
                program = MinerProgram.poclbm;

                string pocstring = "-d" + DeviceID;

                pocstring += " --host=" + pURI;

                pocstring += " --port=" + pPort.ToString();

                pocstring += " --user=" + pUser;

                pocstring += " --pass=" + pPass;

                pocstring += " -f " + framerate.ToString();

                pocstring += " -w " + work.ToString();

                if (vectors)
                {
                    pocstring += " --vectors";
                }

                arguments = pocstring;
            }
            #endregion
            #region args
            nickname = Nickname;
            pool = p;
            program = mp;
            DeviceID = Device;
            f = framerate;
            d = Device;
            w = work;
            v = vectors;
            pu = PoolUser;
            pp = PoolPass;
            ph = host;
            #endregion
        }

        public void Initialize()
        {
            //TabPage page = newMinerPage(nickname);


            p.StartInfo.FileName = "poclbm/poclbm.exe";
            p.StartInfo.WorkingDirectory = "poclbm/";
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            p.ErrorDataReceived += new DataReceivedEventHandler(p_ErrorDataReceived);
        }

        public TabPage newMinerPage(string nick)
        {
            TabPage tp = new TabPage(nick);
            Label l1 = new Label(), l2 = new Label(), l3 = new Label(), l4 = new Label(), l5 = new Label(), l6 = new Label(), l7 = new Label(), l8 = new Label();
            Button b1 = new Button();

            l1.Text = "Current Block: ";
            l1.Location = new Point(23, 61 + 100);
            l1.Name = "l1";
            l2.Text = "Shares submitted: ";
            l2.Location = new Point(23, 89 + 100);
            l2.Name = "l2";
            l3.Text = "Stale shares: ";
            l3.Location = new Point(23, 115 + 100);
            l3.Name = "l3";
            l4.Text = "Current mh/s: ";
            l4.Location = new Point(23, 141 + 100);
            l4.Name = "l4";

            l5.Text = "0";
            l5.Location = new Point(l1.Location.X + 150, l1.Location.Y);
            l5.Name = "l5";
            l6.Text = "0";
            l6.Location = new Point(l2.Location.X + 150, l2.Location.Y);
            l6.Name = "l6";
            l7.Text = "0";
            l7.Location = new Point(l3.Location.X + 150, l3.Location.Y);
            l7.Name = "l7";
            l8.Text = "0";
            l8.Location = new Point(l4.Location.X + 150, l4.Location.Y);
            l8.Name = "l8";

            b1.Text = "Stop";
            b1.Name = "buttonstop";
            b1.Location = new Point(l8.Location.X + 350, l8.Location.Y - 2);
            b1.Click += new EventHandler(Stop);



            tp.Controls.Add(l1);
            tp.Controls.Add(l2);
            tp.Controls.Add(l3);
            tp.Controls.Add(l4);
            tp.Controls.Add(l5);
            tp.Controls.Add(l6);
            tp.Controls.Add(l7);
            tp.Controls.Add(l8);
            tp.Controls.Add(b1);

            return tp;
        }

        public void Start()
        {
            p.Start();
            id = p.Id;
            running = true;
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

        }

        public void Stop(object sender, EventArgs e)
        {
            if (running == true)
            {
                running = false;
                p.Kill();
            }
            else
            {
                throw new NotImplementedException("Miner already stopped");
            }
        }

        private void p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!running == false)
            {
                MessageBox.Show(e.Data, "ERROR!");
            }
        }

        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {

            StreamWriter sw = new StreamWriter("log.log", true); sw.WriteLine(e.Data); sw.Close();
            if (!running == false)
            {
                if (program == MinerProgram.poclbm)
                {
                    if (e.Data.Contains(" "))
                    {

                        string[] split = e.Data.Split(' ');
                        if (e.Data.Length != 0)
                        {
                            if (e.Data.Length != 60)
                            {
                                if (e.Data.Contains("khash"))
                                {
                                    SpeedChangedArgs a = new SpeedChangedArgs();
                                    a.CurrentSpeed = split[0];
                                    a.TabIndex = TabIndex;

                                    this.SpeedChanged(this, a);
                                }

                                else if (e.Data.Contains("new block"))
                                {
                                    System.Net.WebClient s = new System.Net.WebClient();
                                    Stream ss = s.OpenRead("http://blockexplorer.com/q/getblockcount");
                                    StreamReader sr = new StreamReader(ss);
                                    int t;
                                    bool b = int.TryParse(sr.ReadLine().ToString(), out t);
                                    if(b)
                                        BLOCKID = t;
                                    NewBlockArgs a = new NewBlockArgs();
                                    BLOCKID++;
                                    a.BlockID = BLOCKID;
                                    a.TabIndex = TabIndex;
                                    this.NewBlock(this, a);
                                }

                                else if (e.Data.Contains("accepted"))
                                {
                                    AcceptedShareArgs a = new AcceptedShareArgs();
                                    a.TabIndex = TabIndex;
                                    ASHARES++;
                                    a.Accepted = ASHARES;
                                    this.AcceptedShare(this, a);
                                }

                                else if (e.Data.Contains("invalid"))
                                {
                                    StaleShareArgs a = new StaleShareArgs();
                                    SSHARES++;
                                    a.Stale = SSHARES;
                                    a.TabIndex = TabIndex;
                                    this.StaleShare(this, a);
                                }
                                else if ((e.Data.ToLower().Contains("rpc")) && (e.Data.ToLower().Contains("problem")))
                                {
                                    FailOverArgs a = new FailOverArgs();
                                    a.nick = nickname;
                                    a.framerate = f;
                                    a.device = d;
                                    a.worksize = w;
                                    a.vectors = v;
                                    a.pooluser = pu;
                                    a.poolpass = pp;
                                    a.phost = ph;
                                    a.TabPage = TabIndex;
                                    a.pool = pool;
                                    a.mp = program;
                                    this.FailOver(this, a);
                                }

                                else
                                {
                                    ErrorArgs a = new ErrorArgs();
                                    this.Error(this, a);
                                }

                            }
                        }
                    }
                }
            }
        }
    }
}
       

   

        
