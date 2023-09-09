using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using static System.Windows.Forms.LinkLabel;

namespace OnlyFansScraper {
    public partial class OnlyFansScraper : Form {

        private HashSet<string> uniqueLinks = new HashSet<string>();

        public OnlyFansScraper() {
            InitializeComponent();
            queryTextBox.TextChanged += queryTextBox_TextChanged;

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Visible = false;

            if (!IsDotNetFrameworkInstalled()) {
                MessageBox.Show(
                    "The .NET Framework is not installed on your computer. " +
                    "You need to install it to run this application. " +
                    "Click OK to open the .NET Framework download page.",
                    "Missing .NET Framework",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                System.Diagnostics.Process.Start("https://dotnet.microsoft.com/download/dotnet-framework");
                Environment.Exit(0);
            }
        }

        private async void searchBtn_Click_1(object sender, EventArgs e) {
            searchBtn.Enabled = false;

            string query = queryTextBox.Text;
            if (String.IsNullOrEmpty(query)) {
                searchBtn.Enabled = true;
                return;
            }
            int pages = (int)numPagesNumericUpDown.Value;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    string resultsFilePath = saveFileDialog.FileName;

                    int scrapedVids = 0;
                    var standardOut = Console.Out;


                    try {
                        using (StreamWriter resultsWriter = new StreamWriter(resultsFilePath)) {
                            Console.SetOut(resultsWriter);

                            int totalScrapedLinks = 0;

                            progressBar.Visible = true;

                            var scrapingTask = Task.Run(async () => {
                                scrapedVids += await ScrapeErome(query, pages);
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeEroThots(query, pages);
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeViralPornHub(query, pages);
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeThotHub(query, pages);
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeMat6tube(query, pages);
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeHClips(query, pages);
                                totalScrapedLinks++;


                            });

                            while (!scrapingTask.IsCompleted) {
                                int currentProgress = CalculateCurrentProgress(totalScrapedLinks);
                                Invoke(new Action(() => {
                                    progressBar.Value = currentProgress;
                                }));

                                await Task.Delay(100);
                            }

                            Invoke(new Action(() => {
                                progressBar.Value = 100;
                            }));

                            Console.SetOut(standardOut);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } finally {
                        MessageBox.Show($"Done scraping.\nScraped {scrapedVids} links for you!", $"Done", MessageBoxButtons.OK, MessageBoxIcon.Information);


                        Invoke(new Action(() => {
                            progressBar.Value = 0;
                        }));

                        progressBar.Visible = false;
                        searchBtn.Enabled = true;
                    }
                } else {
                    searchBtn.Enabled = true;
                }
            }
        }

        public async Task<int> ScrapeHClips(string query, int pages) {
            string newQuery = query.Replace(" ", "+");
            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                for (int i = 0; i < pages; i++) {
                    string url = $"https://hclips.com/search/{i + 1}/?s={newQuery}";
                    driver.Navigate().GoToUrl(url);

                    await Task.Delay(2000);

                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    string xpath = "//a[@href]";
                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);


                    scrapedVids += await SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }

        public async Task<int> ScrapeMat6tube(string query, int pages) {
            string newQuery = query.Replace(" ", "%20");
            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");
            chromeOptions.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                driver.Navigate().GoToUrl($"https://www.mat6tube.com/video/{newQuery}/");

                for (int i = 0; i < pages; i++) {

                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    jsExecutor.ExecuteScript("window.scrollBy(0, 200);");
                    await Task.Delay(2000);

                    IWebElement showMoreButton = null;
                    try {
                        showMoreButton = driver.FindElement(By.CssSelector($"div.more[data-page='{i}'][data-loading='0']"));
                    } catch (NoSuchElementException) {
                        break;
                    }

                    if (showMoreButton != null) {

                        if (showMoreButton.Displayed && showMoreButton.Enabled) {
                            jsExecutor.ExecuteScript("arguments[0].click();", showMoreButton);
                        } else {
                            break;
                        }
                    } else {
                        break;
                    }
                }

                await Task.Delay(2000);

                string htmlContent = driver.PageSource;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlContent);

                string xpath = "//a[contains(@class, 'item_link')]";

                var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                scrapedVids += await SaveVideos(videoLinks, query);

            }

            return scrapedVids;
        }


        public async Task<int> ScrapeThotHub(string query, int pages) {
            string newQuery = query.Replace(" ", "-");

            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                driver.Navigate().GoToUrl($"https://www.thothub.lol/search/{newQuery}/");

                for (int i = 0; i < pages; i++) {
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);

                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    string xpath = "//div[contains(@class, 'item') and not(contains(@class, 'private'))]//a[contains(@href, 'https://thothub.lol/videos/')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);
                    scrapedVids += await SaveVideos(videoLinks, query);

                    IWebElement nextButton = driver.FindElement(By.CssSelector("a[data-action='ajax']"));
                    if (nextButton == null) {
                        break;
                    }
                    if (nextButton.Displayed && nextButton.Enabled) {
                        jsExecutor.ExecuteScript("arguments[0].click();", nextButton);
                    }
                }
            }

            return scrapedVids;
        }

        public async Task<int> ScrapeEroThots(string query, int pages) {
            string newQuery = query.Replace(" ", "%20");
            int scrapedVids = 0;


            string baseUrl = "https://erothots.co/videos/" + newQuery;

            for (int i = 0; i < pages; i++) {

                string searchUrl = baseUrl + $"?p={i}";

                using (HttpClient httpClient = new HttpClient()) {

                    string htmlContent = httpClient.GetStringAsync(searchUrl).Result;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    string xpath = "//a[contains(@class, 'video')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);
                    scrapedVids += await SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }


        public async Task<int> ScrapeViralPornHub(string query, int pages) {
            string newQuery = query.Replace(" ", "-");

            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                driver.Navigate().GoToUrl($"https://www.viralpornhub.com/search/{newQuery}/");

                for (int i = 0; i < pages; i++) {
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);

                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    string xpath = "//div[contains(@class, 'item')]//a[contains(@href, 'https://viralpornhub.com/videos/')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);
                    scrapedVids += await SaveVideos(videoLinks, query);

                    IWebElement nextButton = driver.FindElement(By.CssSelector("a[data-action='ajax']"));
                    if (nextButton == null) {
                        break;
                    }
                    if (nextButton.Displayed && nextButton.Enabled) {
                        jsExecutor.ExecuteScript("arguments[0].click();", nextButton);
                    }
                }
            }

            return scrapedVids;
        }

        public async Task<int> ScrapeErome(string query, int pages) {
            string newQuery = query.Replace(" ", "+");

            int scrapedVids = 0;

            string baseUrl = "https://erome.com/search?q=" + newQuery;

            for (int i = 0; i < pages; i++) {

                string searchUrl = baseUrl + $"&page={i + 1}";

                using (HttpClient httpClient = new HttpClient()) {
                    string htmlContent = httpClient.GetStringAsync(searchUrl).Result;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    string xpath = "//a[contains(@class, 'album-link')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);
                    scrapedVids += await SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }

        private Task<int> SaveVideos(HtmlNodeCollection videoLinks, string query) {
            int scrapedVids = 0;

            if (videoLinks != null) {
                foreach (var linkNode in videoLinks) {
                    try {
                        if (linkNode == null) continue;
                        string link = linkNode.GetAttributeValue("href", "");

                        if (uniqueLinks.Contains(link)) continue;

                        string videoName = linkNode.Descendants("img")
                                .FirstOrDefault()
                                ?.GetAttributeValue("alt", "No Name");

                        if (videoName == null) continue;

                        int hashIndex = videoName.IndexOf('#');
                        if (hashIndex != -1) {
                            videoName = videoName.Substring(0, hashIndex);
                        }

                        videoName = videoName.ToLower();
                        query = query.ToLower();

                        if (link.StartsWith("/watch/")) {
                            if(CheckVideo(link, "https://www.mat6tube.com", videoName, query)){
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        }
                        else if (link.StartsWith("/videos/")) {
                            if (CheckVideo(link, "https://www.hclips.com", videoName, query)) {
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        }
                        else if (CheckVideo(link, "", videoName, query)) {
                            uniqueLinks.Add(link);
                            scrapedVids++;
                        }

                        if (query.Contains(" ")) {
                            string queryWithoutSpaces = query.Replace(" ", "");
                            if(CheckVideo(link, "", videoName, queryWithoutSpaces)) {
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        }
                        if (query.Contains(" ")) {
                            string queryWithoutSpaces = query.Replace(" ", "-");
                            if (CheckVideo(link, "", videoName, queryWithoutSpaces)) {
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        }
                        if (query.Contains(" ")) {
                            string queryWithoutSpaces = query.Replace(" ", "_");
                            if (CheckVideo(link, "", videoName, queryWithoutSpaces)) {
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"An exception occurred: {ex.Message}");
                    }
                }
            }

            return Task.FromResult(scrapedVids);
        }

        private bool CheckVideo(string link, string mainLink, string videoName, string query) {

            try {
                if (videoName.Contains(query)) {
                    if (saveVideosNameCheckBox.Checked) {
                        Console.WriteLine($"{videoName}: {mainLink}{link}");
                    } else {
                        Console.WriteLine($"{mainLink}{link}");
                    }
                    return true;
                }
                return false;

            }
            catch (Exception ex) { return false; }
            
        }

        private int CalculateCurrentProgress(int totalScrapedLinks) {
            return (100 / 6) * totalScrapedLinks;
        }


        private void queryTextBox_TextChanged(object sender, EventArgs e) {
            searchBtn.Enabled = !string.IsNullOrWhiteSpace(queryTextBox.Text);
        }

        private void tag_Click(object sender, EventArgs e) {
            string url = "https://github.com/yabelo";
            System.Diagnostics.Process.Start(url);
        }

        private bool IsDotNetFrameworkInstalled() {
            try {
                _ = System.Drawing.Color.Red;
                return true;
            } catch {
                return false;
            }
        }

    }
}
