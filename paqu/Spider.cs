using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace paqu
{
    class Spider
    {

        public void TF(Object obj)
        {
            Directory.SetCurrentDirectory("F:\\test");
            int id = (obj as Param).ID;
            for (int i = id + 1001; i <= 1309; i += 8)
            {
                try
                {

                    string url = $"https://oa.jlu.edu.cn/defaultroot/PortalInformation!jldxList.action?1=1&channelId=179577&startPage={i}";

                    string str = GetPage(url);      //获取校内通知网页源码

                    MatchTiaomu(str);
                }
                catch (Exception e)
                {
                    string fileName = $"F:\\login{id}.txt";
                    var w = File.AppendText(fileName);
                    w.WriteLine($"i={i} 错误信息{e.Message}");
                    w.Flush();
                    w.Close();
                    continue;
                }
            }
        }

        //获取某个URL的源码
        string GetPage(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.2) AppleWebKit/525.13 (KHTML, like Gecko) Chrome/0.2.149.27 Safari/525.13";
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            var rep = request.GetResponse() as HttpWebResponse;
            var stream = rep.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            string str = reader.ReadToEnd();

            reader.Close();
            rep.Close();
            return str;
        }

        //匹配索引页
        List<Item> MatchTiaomu(string str)
        {
            List<Item> list = new List<Item>();

            string pattern = "href=\"(PortalInformation!getInformation\\.action\\?title=([^&]*)&[^&]*&[^&]*&[^&]*&[^&]*&[^&]*&[^&]*orgname=([^\"]*))\"";
            Regex reg = new Regex(pattern);

            MatchCollection ms = reg.Matches(str);
            foreach (Match m in ms)
            {
                Item item = new Item();
                item.Title = m.Groups[2].Captures[0].Value;
                item.Bumen = m.Groups[3].Captures[0].Value;

                MatchContent(m.Groups[1].Captures[0].Value, item);

                SaveItem(item);
            }

            return list;
        }

        //匹配时间、正文内容
        void MatchContent(string url, Item item)
        {
            string pageContent = GetPage("https://oa.jlu.edu.cn/defaultroot/" + url);

            //匹配时间
            Regex reg = new Regex("<div class=\"content_time\"[^>]*>([^&]*)");
            Match m = reg.Match(pageContent);
            if (m.Success)
            {
                item.Time = m.Groups[1].Captures[0].Value;
            }

            //匹配正文内容
            reg = new Regex("<div class=\"content_font [\\s\\S]*<strong>");//先摘取div

            string s = reg.Match(pageContent).Value;    //获取匹配的文本
            s = Regex.Replace(s, "</?p[^>]*>", "\n\r");   //将段落标签替换为换行符
            s = Regex.Replace(s, "<[^>]*>", "");        //将所有标签去掉
            s = Regex.Replace(s, "&nbsp;", " ");        //将&nbsp和换成空格

            item.Content = s;
        }
        void SaveItem(Item item)
        {
            string filename = item.Time + item.Bumen + "  " + item.Title + ".txt";
            filename = filename.Replace(':', '点');
            filename = Regex.Replace(filename, "[\\/:*?\"<>|]", "^_^");
            //File.Create(filename).Close();
            File.WriteAllText(filename, item.Content);
        }
    }
}
