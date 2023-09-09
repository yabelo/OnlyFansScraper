﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Collections.Generic;
using OpenQA.Selenium.DevTools.V114.Debugger;

namespace OnlyFansScraper {
    public partial class OnlyFansScraper : Form {

        // HashSet to store unique links to avoid duplicates during scraping
        private HashSet<string> uniqueLinks = new HashSet<string>();

        // Stores the user's search query
        private string query = "";

        // Stores the number of pages to scrape
        private int pages = 1;

        // Stores the user's user-agent
        private string userAgent = "";

        public OnlyFansScraper() {
            InitializeComponent();

            // Register the event handler for the queryTextBox's TextChanged event
            queryTextBox.TextChanged += queryTextBox_TextChanged;

            // Initialize progress bar settings
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Step = 1;
            progressBar.Visible = false;

            // Check if .NET Framework is installed; if not, prompt the user to install it
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

        // Event handler for the search button's click event
        private async void searchBtn_Click_1(object sender, EventArgs e) {
            searchBtn.Enabled = false;

            // Get the user's search query from the text box
            query = queryTextBox.Text;

            // Check if the query is empty
            if (String.IsNullOrEmpty(query)) {
                searchBtn.Enabled = true;
                return;
            }

            // Get the number of pages to scrape from the numeric up-down control
            pages = (int)numPagesNumericUpDown.Value;

            // Create a SaveFileDialog for the user to choose where to save the results
            using (SaveFileDialog saveFileDialog = new SaveFileDialog()) {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                // Show the save file dialog to the user
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    string resultsFilePath = saveFileDialog.FileName;

                    int scrapedVids = 0;
                    var standardOut = Console.Out;

                    try {
                        using (StreamWriter resultsWriter = new StreamWriter(resultsFilePath)) {
                            Console.SetOut(resultsWriter);

                            int totalScrapedLinks = 0;

                            // Show progress bar
                            progressBar.Visible = true;

                            var scrapingTask = Task.Run(async () => {
                                // Start scraping from various sources
                                scrapedVids += await ScrapeErome();
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeEroThots();
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeViralPornHub();
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeThotHub();
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeMat6tube();
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeHClips();
                                totalScrapedLinks++;

                                scrapedVids += await ScrapeCamWhores();
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
                        // Display an error message if an exception occurs during scraping
                        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } finally {
                        // Display a completion message and reset UI elements
                        MessageBox.Show($"Done scraping.\nScraped {scrapedVids} links for you!", $"Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Invoke(new Action(() => {
                            progressBar.Value = 0;
                        }));

                        progressBar.Visible = false;
                        searchBtn.Enabled = true;
                    }
                } else {
                    // Enable the search button if the user cancels the save file dialog
                    searchBtn.Enabled = true;
                }
            }
        }

        // Scrape videos from the CamWhorez website
        public async Task<int> ScrapeCamWhores() {
            // Modify the query for URL and paging
            string newQuery = query.Replace(" ", "-");
            string nextPage = query.Replace(" ", "%20");

            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                // Navigate to the CamWhorez search results page
                driver.Navigate().GoToUrl($"https://www.camwhorez.tv/search/{newQuery}/");

                for (int i = 2; i <= pages + 1; i++) {
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;

                    await Task.Delay(2000);

                    // Get the HTML content of the page
                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//div[contains(@class, 'item') and not(contains(@class, 'private'))]//a[contains(@href, 'https://www.camwhorez.tv/videos/')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);
                    scrapedVids += await SaveVideos(videoLinks, query);

                    // Construct data parameters for the next page
                    string dataParameterPattern = "q:" + nextPage + ";category_ids:;sort_by:post_date;from_videos+from_albums:{0}";
                    string dataParameters = string.Format(dataParameterPattern, i.ToString("D2"));

                    // Find and click the "Next" button for pagination
                    IWebElement nextButton = driver.FindElement(By.XPath($"//li[@class='page']/a[@data-parameters='{dataParameters}']"));
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

        // Scrape videos from the HClips website
        public async Task<int> ScrapeHClips() {
            // Modify the query for URL
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

                    // Get the HTML content of the page
                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//a[@href]";
                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                    // Save videos found on the current page
                    scrapedVids += await SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }

        // Scrape videos from the Mat6tube website
        public async Task<int> ScrapeMat6tube() {
            // Modify the query for URL
            string newQuery = query.Replace(" ", "%20");
            int scrapedVids = 0;

            userAgent = userAgentTextBox.Text;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            if (String.IsNullOrEmpty(userAgent)) return 0;
            chromeOptions.AddArgument($"--user-agent={userAgent}");

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

                // Get the HTML content of the page
                string htmlContent = driver.PageSource;

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlContent);

                // XPath to extract video links
                string xpath = "//a[contains(@class, 'item_link')]";
                var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                // Save videos found on the current page
                scrapedVids += await SaveVideos(videoLinks, query);
            }

            return scrapedVids;
        }


        // Scrape videos from the ThotHub website
        public async Task<int> ScrapeThotHub() {
            // Modify the query for URL
            string newQuery = query.Replace(" ", "-");

            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                // Navigate to the ThotHub search results page
                driver.Navigate().GoToUrl($"https://www.thothub.lol/search/{newQuery}/");

                for (int i = 0; i < pages; i++) {
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);

                    // Get the HTML content of the page
                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//div[contains(@class, 'item') and not(contains(@class, 'private'))]//a[contains(@href, 'https://thothub.lol/videos/')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                    // Save videos found on the current page
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

        // Scrape videos from the EroThots website
        public async Task<int> ScrapeEroThots() {
            // Modify the query for URL
            string newQuery = query.Replace(" ", "%20");
            int scrapedVids = 0;

            string baseUrl = "https://erothots.co/videos/" + newQuery;

            for (int i = 0; i < pages; i++) {
                string searchUrl = baseUrl + $"?p={i}";

                using (HttpClient httpClient = new HttpClient()) {
                    // Get the HTML content of the page
                    string htmlContent = httpClient.GetStringAsync(searchUrl).Result;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//a[contains(@class, 'video')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                    // Save videos found on the current page
                    scrapedVids += await SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }

        // Scrape videos from the ViralPornHub website
        public async Task<int> ScrapeViralPornHub() {
            // Modify the query for URL
            string newQuery = query.Replace(" ", "-");

            int scrapedVids = 0;

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");
            chromeOptions.AddArguments("--disable-gpu");
            chromeOptions.AddArguments("--log-level=3");

            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

            using (IWebDriver driver = new ChromeDriver(chromeDriverService, chromeOptions)) {
                // Navigate to the ViralPornHub search results page
                driver.Navigate().GoToUrl($"https://www.viralpornhub.com/search/{newQuery}/");

                for (int i = 0; i < pages; i++) {
                    IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
                    jsExecutor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);

                    // Get the HTML content of the page
                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//div[contains(@class, 'item')]//a[contains(@href, 'https://viralpornhub.com/videos/')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                    // Save videos found on the current page
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

        // Scrape videos from the Erome website
        public async Task<int> ScrapeErome() {
            // Modify the query for URL
            string newQuery = query.Replace(" ", "+");

            int scrapedVids = 0;

            string baseUrl = "https://erome.com/search?q=" + newQuery;

            for (int i = 0; i < pages; i++) {
                string searchUrl = baseUrl + $"&page={i + 1}";

                using (HttpClient httpClient = new HttpClient()) {
                    // Get the HTML content of the page
                    string htmlContent = httpClient.GetStringAsync(searchUrl).Result;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//a[contains(@class, 'album-link')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);

                    // Save videos found on the current page
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

                        // Check if the video link matches the query and save it
                        if (link.StartsWith("/watch/")) {
                            if (CheckVideo(link, "https://www.mat6tube.com", videoName, query)) {
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        } else if (link.StartsWith("/videos/")) {
                            if (CheckVideo(link, "https://www.hclips.com", videoName, query)) {
                                uniqueLinks.Add(link);
                                scrapedVids++;
                            }
                        } else if (CheckVideo(link, "", videoName, query)) {
                            uniqueLinks.Add(link);
                            scrapedVids++;
                        }

                        // Check for query variations with spaces, dashes, and underscores
                        if (query.Contains(" ")) {
                            string queryWithoutSpaces = query.Replace(" ", "");
                            if (CheckVideo(link, "", videoName, queryWithoutSpaces)) {
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

        // Method to check if a video link matches the query
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
            } catch (Exception ex) {
                return false;
            }
        }

        // Method to calculate the current progress for the progress bar
        private int CalculateCurrentProgress(int totalScrapedLinks) {
            return (100 / 7) * totalScrapedLinks;
        }

        // Event handler for the queryTextBox's TextChanged event
        private void queryTextBox_TextChanged(object sender, EventArgs e) {
            // Enable the search button if the query text is not empty
            searchBtn.Enabled = !string.IsNullOrWhiteSpace(queryTextBox.Text);
        }

        // Event handler for opening the GitHub page of the developer
        private void tag_Click(object sender, EventArgs e) {
            string url = "https://github.com/yabelo";
            System.Diagnostics.Process.Start(url);
        }

        // Method to check if .NET Framework is installed on the system
        private bool IsDotNetFrameworkInstalled() {
            try {
                _ = System.Drawing.Color.Red;
                return true;
            } catch {
                return false;
            }
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }
    }
}
