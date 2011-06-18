using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace BitMiner
{
    class StatsUpdater
    {


        private Timer t = new Timer();


        #region Handlers

        public class StatsUpdatedArgs : System.EventArgs
        {
            public string networkSpeed;
            public string networkBTC;
            public string exchangeRate;
            public string currentBlock;
            public string currentDifficulty;
        }

        public delegate void StatsChangedHandler(object sender, StatsUpdatedArgs e);

        public event StatsChangedHandler StatsChanged;

        #endregion

        public void TimerTick()
        {
            t_Tick(this, new EventArgs());
        }

        public StatsUpdater()
        {
            t.Tick += new EventHandler(t_Tick);
            t.Interval = 600000;
        }

        void t_Tick(object sender, EventArgs e)
        {
            StatsUpdatedArgs a = new StatsUpdatedArgs();
            a.currentBlock = getBlockCount().ToString();
            a.networkSpeed = getNetworkSpeed().ToString();
            a.currentDifficulty = getNetworkDifficulty().ToString();
            a.networkBTC = getCurrentBTC().ToString();
            a.exchangeRate = getExchangeRate();
                this.StatsChanged(this, a);
        }

        public void Start()
        {
            t.Start();
            t_Tick(this, new EventArgs());
        }

        public void Stop()
        {
            t.Stop();
        }

        int getBlockCount()
        {
            System.Net.WebClient s = new System.Net.WebClient();
            Stream ss = s.OpenRead("http://blockexplorer.com/q/getblockcount");
            StreamReader sr = new StreamReader(ss);
            string sc = sr.ReadLine();
            int o;
            bool b = int.TryParse(sc, out o);
            if(b)
                return o;
            return 1111111;
        }

        string getNetworkSpeed()
        {
            System.Net.WebClient s = new System.Net.WebClient();
            Stream ss = s.OpenRead("http://bitcoincharts.com/markets/");  //CHANGE TO BLOCKEXPLORER
            StreamReader sr = new StreamReader(ss);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("<tr><td class=\"label\">Network total</td><td>"))
                {
                    line = line.Remove(0, 48).Replace("</td></tr>", "");
                    return line;

                }
            }



            return "11111112"; //change

        }

        int getNetworkDifficulty()
        {
            System.Net.WebClient s = new System.Net.WebClient();
            Stream ss = s.OpenRead("http://blockexplorer.com/q/getdifficulty");
            StreamReader sr = new StreamReader(ss);

            string temp = sr.ReadLine();
            string[] temp2 = temp.Split('.');
            string difficulty = temp2[0];

            int i;
            bool b = int.TryParse(difficulty, out i);
            if (b)
                return i;



            return 1111113; //change

        }

        int getCurrentBTC()
        {

            System.Net.WebClient s = new System.Net.WebClient();
            Stream ss = s.OpenRead("http://blockexplorer.com/q/totalbc");
            StreamReader sr = new StreamReader(ss);
            string currentBTC = sr.ReadLine().Replace(".00000000", "");

            if (!(currentBTC == string.Empty))
            {
                int a;
                bool b = int.TryParse(currentBTC, out a);
                if (b)
                    return a;
            }
        


            return 1111114; //change

        }

        string getExchangeRate()
        {
            System.Net.WebClient s = new System.Net.WebClient();
            Stream ss = s.OpenRead("http://bitcoincharts.com/markets/mtgoxUSD.html");
            StreamReader sr = new StreamReader(ss);
            string exchange = string.Empty;
            string line;
            bool found = false;
            while ((line = sr.ReadLine()) != null)
            {
                if (found)
                    break;

                    if (line.Contains("<span class=\"sub\">USD (Liberty Reserve)</span>"))
                    {
                        sr.ReadLine();
                        exchange = sr.ReadLine();
                        string[] temp = exchange.Split('>');
                        exchange = "$" + temp[1];
                        found = true;
                    }
                
            }


            if (exchange != string.Empty)
                return exchange;

            return "1111115"; //change

        }



    }
}
