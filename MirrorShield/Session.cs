using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrorShield
{
    class Session
    {
        private string protocol;
        private string dp;
        private string dip;
        private string sip;
        private string sp;
        private string desc;

        public Session(string protocol, string sip, string dip, string sp, string dp)
        {
            this.protocol = protocol;
            this.sip = sip;
            this.dip = dip;
            this.sp = sp;
            this.dp = dp;
            desc = protocol + "" + sip + "" + dip + "" + sp + "" + dp;
        }

        public string SourceIP()
        {
            return sip;
        }

        public string DestinationIP()
        {
            return dip;
        }

        public string SoucePort()
        {
            return sp;
        }

        public string DestinationPort()
        {
            return dp;
        }

        public override string ToString()
        {
            return desc;
        }
    }
}
