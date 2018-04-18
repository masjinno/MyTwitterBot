using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;

namespace RosenFlyerChecker.Model
{
    public class HtmlCheck
    {
        public static readonly string NO_FLYER = "■現在チラシはございません。";
        public static readonly string FLYER_URL_START = "http://sotetsu.digimu.jp/data/";
        private static readonly string HTML_TAG_A = "//a";
        private static readonly string HTML_ATTR_HREF = "href";
        private static readonly string TARGET_FILE_END = ".pdf";
        
        /// <summary>
        /// <paramref name="url"/>先にある<a href="">のPDFファイルをリストを取得する
        /// </summary>
        /// <param name="url">PDFのリンクを持つWebサイトのURL</param>
        /// <returns>PDFリンクのURLのリスト</returns>
        public async Task<List<string>> GetFileUrlAsync(string url)
        {
            // HTML取得
            HttpClient client = new HttpClient();
            string html = await client.GetStringAsync(url);

            // HTML解析
            if (html.Contains(HtmlCheck.NO_FLYER))
            {
                return new List<string> { HtmlCheck.NO_FLYER };
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(HtmlCheck.HTML_TAG_A);
            if (nodes == null)
            {
                return null;
            }
            List<string> ret = new List<string>();
            foreach (HtmlNode node in nodes)
            {
                string defaultValue = string.Empty;
                string href = node.GetAttributeValue(HtmlCheck.HTML_ATTR_HREF, defaultValue);
                if (string.IsNullOrEmpty(href) || !href.EndsWith(HtmlCheck.TARGET_FILE_END) || !href.StartsWith(HtmlCheck.FLYER_URL_START))
                {
                    continue;
                }
                ret.Add(href);
            }

            return ret;
        }

        /// <summary>
        /// <paramref name="dirPath"/>に、<paramref name="fileUrl"/>をダウンロードする
        /// </summary>
        /// <param name="fileUrl">ダウンロード対象ファイルのURLのリスト</param>
        /// <param name="dirPath">ダウンロードするディレクトリパス</param>
        public void DownloadFile(List<string> fileUrl, string dirPath)
        {
            string downloadLogPath = Path.Combine(dirPath, "download.log");
            FileStream fs = new FileStream(downloadLogPath, FileMode.Create);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (string url in fileUrl)
                {
                    this.DownloadFileAsync(url, dirPath);
                    sw.WriteLine(url);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">ダウンロード対象ファイルのURL</param>
        /// <param name="dirPath">ダウンロードするディレクトリパス</param>
        private async void DownloadFileAsync(string url, string dirPath)
        {
            try
            {
                string fileName = Path.GetFileName(url);

                Uri source = new Uri(url);

                StorageFolder dstFolder = await StorageFolder.GetFolderFromPathAsync(dirPath).AsTask();
                StorageFile dstFile = await dstFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).AsTask();
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, dstFile);
                await download.StartAsync().AsTask();
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.StackTrace, ex.Message);
                await md.ShowAsync();
            }
        }
    }
}
