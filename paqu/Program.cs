using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paqu
{
    class Item
    {
        public string Time { get; set; }
        public string Title { get; set; }
        public string Bumen { get; set; }
        public string Content { get; set; }
    }
    class Param
    {
        public int ID;
        public Param(int id)
        {
            ID = id;
        }
    } 
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            int page;
            if(!int.TryParse(args[0], out page))
                return;
            new Spider(args[0], page).Go();
        }
    }
}
