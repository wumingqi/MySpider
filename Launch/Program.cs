using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Launch
{
    class Program
    {
        static void Main(string[] args)
        {
            int beg,end;
            if (args.Length < 2 || !int.TryParse(args[0], out beg) || !int.TryParse(args[1], out end))
            {
                Console.WriteLine("参数错误");
                return;
            }
            if (beg < 1 || end > 1309)
            {
                Console.WriteLine("页码范围应该在1~1309");
                return;
            }
            for (int i = beg; i <= end; i++)
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo("paqu.exe", $"{i}");
                p.Start();
            }
        }
    }
}
