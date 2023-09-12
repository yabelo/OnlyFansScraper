using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OnlyFansScraper;

namespace OnlyFansScraper {
    public class Scrape {

        private static string query = OnlyFansScraper.query;

        private static int pages = OnlyFansScraper.pages;
        
        private static string userAgent = OnlyFansScraper.userAgent;

        // Scrape videos from the CamWhorez website
        public static async Task<int> ScrapeCamWhorez() {
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

                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;

                // Find the "Sort" element
                IWebElement sortElement = driver.FindElement(By.XPath("//div[@class='sort']"));

                // Hover over the "Sort" element using Actions class
                Actions action = new Actions(driver);
                action.MoveToElement(sortElement).Perform();

                await Task.Delay(2000);

                // Find and click the "Most Viewed" element
                IWebElement mostViewedSort = driver.FindElement(By.XPath("//a[contains(@data-action, 'ajax') and contains(@data-parameters, 'sort_by:video_viewed')]"));
                if (mostViewedSort != null) {
                    jsExecutor.ExecuteScript("arguments[0].click();", mostViewedSort);
                }

                for (int i = 2; i <= pages + 1; i++) {
                    jsExecutor = (IJavaScriptExecutor)driver;

                    await Task.Delay(2000);

                    // Get the HTML content of the page
                    string htmlContent = driver.PageSource;

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    // XPath to extract video links
                    string xpath = "//div[contains(@class, 'item') and not(contains(@class, 'private'))]//a[contains(@href, 'https://www.camwhorez.tv/videos/')]";

                    var videoLinks = doc.DocumentNode.SelectNodes(xpath);
                    scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);

                    // Construct data parameters for the next page
                    string dataParameterPattern = "q:" + nextPage + ";category_ids:;sort_by:video_viewed;from_videos+from_albums:{0}";
                    string dataParameters = string.Format(dataParameterPattern, i.ToString("D2"));


                    // Find and click the "Next" button for pagination
                    IWebElement nextButton = null;
                    int attempts = 0;

                    while (nextButton == null && attempts < 5) { // Adjust the number of attempts as needed
                        try {
                            nextButton = driver.FindElement(By.XPath($"//a[@data-parameters='{dataParameters}']"));
                            if (nextButton.Displayed && nextButton.Enabled) {
                                break; // Exit the loop if the button is found and clickable
                            }
                        } catch (NoSuchElementException) {
                            // Handle the case where the button is not found
                        }

                        // Wait for a while before attempting again
                        await Task.Delay(1000); // You can adjust the delay duration
                        attempts++;
                    }

                    if (nextButton != null) {
                        jsExecutor.ExecuteScript("window.scrollBy(0, 200);");
                        jsExecutor.ExecuteScript("arguments[0].click();", nextButton);
                    } else {
                        break;
                    }
                }
            }

            return scrapedVids;
        }

        // Scrape videos from the HClips website
        public static async Task<int> ScrapeHClips() {
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
                    scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }

        // Scrape videos from the Mat6tube website
        public static async Task<int> ScrapeMat6tube() {
            // Modify the query for URL
            string newQuery = query.Replace(" ", "%20");
            int scrapedVids = 0;

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
                scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);
            }

            return scrapedVids;
        }


        // Scrape videos from the ThotHub website
        public static async Task<int> ScrapeThotHub() {
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
                    scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);

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
        public static async Task<int> ScrapeEroThots() {
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
                    scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }

        // Scrape videos from the ViralPornHub website
        public static async Task<int> ScrapeViralPornHub() {
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
                    scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);

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
        public static async Task<int> ScrapeErome() {
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
                    scrapedVids += await OnlyFansScraper.SaveVideos(videoLinks, query);
                }
            }

            return scrapedVids;
        }
    }
}
