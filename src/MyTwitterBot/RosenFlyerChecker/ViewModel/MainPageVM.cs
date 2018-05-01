using RosenFlyerChecker.Core;
using RosenFlyerChecker.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace RosenFlyerChecker.ViewModel
{
    public class MainPageVM : BindableBase
    {
        // member
        private HtmlCheck htmlCheck = new HtmlCheck();
        private FileCheck fileCheck = new FileCheck();
        private List<string> currentFiles = new List<string>();
        private List<string> previousFiles = new List<string>();

        // Binding properties

        /// <summary>
        /// チラシのURLを持つWebページのURL
        /// </summary>
        private string _url = "https://www.sotetsu.rosen.co.jp/shopdetail?scode=0061";
        public string Url
        {
            get { return this._url; }
            set { SetProperty(ref this._url, value); }
        }

        /// <summary>
        /// チラシのURLのリスト
        /// </summary>
        private List<string> _flyerUrl = new List<string>();
        public List<string> FlyerUrl
        {
            get { return this._flyerUrl; }
            set { SetProperty(ref this._flyerUrl, value); }
        }

        /// <summary>
        /// ファイルを保存するディレクトリパス
        /// </summary>
        private string _saveDirPath = Path.Combine(
            ApplicationData.Current.LocalFolder.Path,
            typeof(MainPageVM).GetTypeInfo().Assembly.GetName().Name,
            "PDF");
        public string SaveDirPath
        {
            get { return this._saveDirPath; }
            set { SetProperty(ref this._saveDirPath, value); }
        }

        // Binding methods

        /// <summary>
        /// チラシのURLのリストを取得する
        /// </summary>
        public async void GetFlyerUrlClickAsync()
        {
            this.FlyerUrl = await htmlCheck.GetFileUrlAsync(Url);
            if (this.FlyerUrl == null)
            {
                MessageDialog md = new MessageDialog("Flyer is not found.", "information");
                await md.ShowAsync();
            }
        }

        /// <summary>
        /// 保存するディレクトリを選択する
        /// </summary>
        public async void ChooseSaveDirectoryClickAsync()
        {
            FolderPicker fp = new FolderPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.Desktop;
            StorageFolder folder = await fp.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            this.SaveDirPath = folder.Path.ToString();
        }

        /// <summary>
        /// チラシのダウンロードを実行する
        /// </summary>
        public async void DownloadFlyerClickAsync()
        {
            try
            {
                if (!Directory.Exists(this.SaveDirPath))
                {
                    Directory.CreateDirectory(this.SaveDirPath);
                }
                htmlCheck.DownloadFile(this.FlyerUrl, this.SaveDirPath);
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.StackTrace, ex.Message);
                await md.ShowAsync();
            }
        }

        public async void SaveFlyerUrlAsync()
        {
            try
            {
                if (!Directory.Exists(this.SaveDirPath))
                {
                    Directory.CreateDirectory(this.SaveDirPath);
                }
                htmlCheck.WriteFlyerUrl(this.FlyerUrl, this.SaveDirPath);
            }
            catch (Exception ex)
            {
                MessageDialog md = new MessageDialog(ex.StackTrace, ex.Message);
                await md.ShowAsync();
            }
        }
        
        public void CheckFileEquals()
        {
            this.fileCheck.EqualsFiles(this.FlyerUrl[0], this.FlyerUrl[1]);
        }
    }
}
