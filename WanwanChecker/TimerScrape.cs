using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WanwanChecker
{
    public static class TimerScrape
    {
        /// <summary>
        /// スクレイピングを行うアドレス
        /// </summary>
        private const string address = "https://www.nhk.or.jp/event/wanwan/detail_29.html";

        [FunctionName("TimerScrape")]
        //public static void Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, TraceWriter log)
        public static async Task Run([TimerTrigger("5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                var document = await Scrape(address);
                var rowSelector = "div.SP table.indent tr";
                var rows = document.QuerySelectorAll(rowSelector);
                //var row = rows.LastOrDefault();
                var row = rows.ElementAt(7);
                var imgElement = row?.LastElementChild?.LastElementChild?.LastElementChild as IHtmlImageElement;

                if (imgElement?.AlternativeText == "ご案内中")
                {
                    log.Info($"受付中");
                }
                else
                {
                    log.Info($"受付開始待ち");
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
            }
        }

        /// <summary>
        /// スクレイピングを行います。
        /// </summary>
        /// <param name="address">スクレイピングを行うアドレス</param>
        /// <returns>スクレイピングを行った IDocument</returns>
        private static async Task<IDocument> Scrape(string address)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(address);
            return document;
        }
    }
}
