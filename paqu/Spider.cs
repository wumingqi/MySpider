using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Paqu
{
    class Spider
    {
        string[] useragents = new string[]
        {
            "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_8; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-us) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729; InfoPath.3; rv:11.0) like Gecko",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0;",//IE9.0
            "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)",//IE8.0
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)",//IE7.0
            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)",//IE6.0
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.6; rv:2.0.1) Gecko/20100101 Firefox/4.0.1",//firefox 4.0.1
            "Mozilla/5.0 (Windows NT 6.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1",
            "Opera/9.80 (Windows NT 6.1; U; en) Presto/2.8.131 Version/11.11",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_0) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.56 Safari/535.11",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Maxthon 2.0)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; TencentTraveler 4.0)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; The World)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; SE 2.X MetaSr 1.0; SE 2.X MetaSr 1.0; .NET CLR 2.0.50727; SE 2.X MetaSr 1.0)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; 360SE)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Avant Browser)",
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:53.0) Gecko/20100101 Firefox/53.0"//我的
        };

        public Spider(string CurrDir,int Page)
        {
            string dir = $"F:\\test\\{Page}";
            Directory.CreateDirectory(dir);
            this.CurrDir = dir;
            
            this.PageNum = Page;

            this.CurrTmpDir = $"{Page}.html";
        }

        private int PageNum;        //需要爬取的页码
        private string CurrDir;     //当前目录
        private string CurrTmpDir;

        //log信息
        void LogInfo(string info)
        {
            string fileName = $"F:\\login\\login{PageNum}.txt";
            var w = File.AppendText(fileName);
            w.WriteLine(info);
            w.Flush();
            w.Close();
        }

        public void Go()
        {
            Directory.SetCurrentDirectory(CurrDir);
            //for (int i = PageNum; i <= PageNum; i++)
            {
                string url = $"https://oa.jlu.edu.cn/defaultroot/PortalInformation!jldxList.action?1=1&channelId=179577&startPage={PageNum}";
                GetPage(url);      //获取校内通知网页源码

                string str = File.ReadAllText(CurrTmpDir);
                MatchTiaomu(str);
            }
        }

        //获取某个URL的源码
        string GetPage(string url)
        {
            string str = null;
            for (int i = 0; i < 50; i++) 
            {
                try
                {
                    var request = WebRequest.CreateHttp(url);
                    Random ran = new Random(DateTime.Now.Second);
                    request.UserAgent = useragents[ran.Next() % useragents.Length];
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.Method = "GET";
                    request.KeepAlive = false;
                    request.ProtocolVersion = HttpVersion.Version10;
                    var rep = request.GetResponse() as HttpWebResponse;
                    var stream = rep.GetResponseStream();
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    str = reader.ReadToEnd();

                    File.WriteAllText(CurrTmpDir, str);

                    reader.Close();
                    rep.Close();
                    break;
                }catch(Exception e)
                {
                    LogInfo($"GetPage异常 {e.Message}\t{url}\t再入队\n第{i+2}次尝试");
                    if (i == 49)
                        LogInfo($"{url}真的找不到了，可能是被管理员删掉了，继续下一个");
                    Thread.Sleep(1000);
                }
            }
            return str;
        }

        //匹配索引页
        void MatchTiaomu(string str)
        {
            string pattern = "href=\"(P[^\\?]*\\?title=([^&]*)&[^&]*&[^&]*&[^&]*&[^&]*&[^&]*&[^&]*orgname=([^\"]*))\"";
            Regex reg = new Regex(pattern);
            MatchCollection ms = reg.Matches(str);
            if(ms.Count<30)
            {
                LogInfo($"该页面仅匹配到 {ms.Count} 条通知");
            }
            foreach (Match m in ms)
            {
                try
                {
                    Item item = new Item();
                    item.Title = m.Groups[2].Captures[0].Value;
                    item.Bumen = m.Groups[3].Captures[0].Value;
                    MatchContent(m.Groups[1].Captures[0].Value, item);
                    SaveItem(item);
                }
                catch (Exception e)
                {
                    LogInfo($"MatchTiaomu异常 {e.Message} ms.count={ms.Count}");
                }
            }
        }

        //匹配时间、正文内容
        bool MatchContent(string url, Item item)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    string filename = item.Time + item.Bumen + "  " + item.Title;
                    filename = filename.Replace(':', '点');
                    filename = Regex.Replace(filename, "[\\\\\\/:*?\"<>|]", "^_^");
                    CurrTmpDir = $"{filename}.html";
                    if(GetPage("https://oa.jlu.edu.cn/defaultroot/" + url)==null)
                    {
                        LogInfo($"MatchContent异常，文件{filename}不存在");
                        return false;
                    }
                    string pageContent = File.ReadAllText(CurrTmpDir);
                    //匹配时间
                    Regex reg = new Regex("<div class=\"content_time\"[^>]*>([^&]*)");
                    Match m = reg.Match(pageContent);
                    if (m.Success)
                    {
                        item.Time = m.Groups[1].Captures[0].Value;
                    }

                    //匹配正文内容
                    reg = new Regex("<div class=\"content_font [\\s\\S]*<br>");     //先摘取div
                    string s = reg.Match(pageContent).Value;                        //获取匹配的文本
                    s = Regex.Replace(s, "</?p[^>]*>", "\n\r");                     //将段落标签替换为换行符
                    s = Regex.Replace(s, "<[^>]*>", "");                            //将所有标签去掉
                    s = Regex.Replace(s, "&nbsp;?", " ");                            //将&nbsp和换成空格

                    item.Content = s;
                    break;
                }
                catch(Exception e)
                {
                    LogInfo($"MatchContent异常 {e.Message} 进行第 {i + 1} 次尝试");
                    if (i == 9)
                        return false;
                    Thread.Sleep(1000);
                }
            }
            return true;
        }
        void SaveItem(Item item)
        {
            string filename = item.Time + item.Bumen + "  " + item.Title + ".txt";
            while (true)
            {
                try
                {
                    filename = filename.Replace(':', '点');
                    filename = Regex.Replace(filename, "[\\\\\\/:*?\"<>|]", "^_^");
                    File.WriteAllText(filename, item.Content);
                    break;
                }
                catch (Exception e)
                {
                    LogInfo($"SaveItem异常 {e.Message} item时间 {item.Time} 重新尝试");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
