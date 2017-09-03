using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WanwanChecker
{
    public static class TimerScrape
    {
        [FunctionName("TimerScrape")]
        //public static void Run([TimerTrigger("0 0 */6 * * *")]TimerInfo myTimer, TraceWriter log)
        public static async Task Run([TimerTrigger("5 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                var document = await Scrape(GetAddress());
                var rows = document.QuerySelectorAll(GetRowSelector());
                //var row = rows.LastOrDefault();
                var row = rows.ElementAt(7);
                var imgElement = row?.LastElementChild?.LastElementChild?.LastElementChild as IHtmlImageElement;

                if (imgElement?.AlternativeText != "ご案内中")
                {
                    log.Info($"受付が始まってないため終了");
                    return;
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
            }

            // Slackへの通知
            var payload = GetSlackPayload();
            var endpoint = GetSlackEndpoint();
            var httpClient = new HttpClient();
            await httpClient.PostAsync(endpoint, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("payload", payload)
            }));
        }

        /// <summary>
        /// スクレイピングを行います。
        /// </summary>
        /// <param name="address">スクレイピングを行うアドレス</param>
        /// <returns>スクレイピングを行った IDocument</returns>
        private static async Task<IDocument> Scrape(string address)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(address);
            return document;
        }

        /// <summary>
        /// アドレスを取得します。
        /// </summary>
        /// <returns>アドレス</returns>
        private static string GetAddress()
        {
            return ConfigurationManager.AppSettings["Address"];
        }

        /// <summary>
        /// セレクタを取得します。
        /// </summary>
        /// <returns>セレクタ</returns>
        private static string GetRowSelector()
        {
            return ConfigurationManager.AppSettings["RowSelector"];
        }

        /// <summary>
        /// Slackの通知情報を取得します。
        /// </summary>
        /// <returns>通知情報</returns>
        private static string GetSlackPayload()
        {
            var payload = new
            {
                attachments = new []
                {
                    new
                    {
                        title = "ワンワンわんだーらんどの受付が始まったよ",
                        title_link = GetAddress(),
                    }
                }
            };
            return JsonConvert.SerializeObject(payload);
        }

        /// <summary>
        /// Slackのエンドポイントを取得します。
        /// </summary>
        /// <returns>エンドポイント</returns>
        private static string GetSlackEndpoint()
        {
            return ConfigurationManager.AppSettings["SlackEndpoint"];
        }
    }
}
