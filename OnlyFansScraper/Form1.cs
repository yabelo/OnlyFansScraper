using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.IO;
using System.Collections.Generic;


namespace OnlyFansScraper {
    public partial class OnlyFansScraper : Form {

        // HashSet to store unique links to avoid duplicates during scraping
        private static HashSet<string> uniqueLinks = new HashSet<string>();

        // Stores the user's search query
        public static string query = "";

        // Stores the number of pages to scrape
        public static int pages = 1;

        // Stores the user's user-agent
        public static string userAgent = "";

        private static CheckBox checkBox;

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

            // Store the queries in variables.
            query = queryTextBox.Text;
            query = query.ToLower().Trim();

            checkBox = saveVideosNameCheckBox;
            userAgent = userAgentTextBox.Text;
            pages = (int)numPagesNumericUpDown.Value;

            // Check if the query is empty
            if (String.IsNullOrEmpty(query)) {
                searchBtn.Enabled = true;
                return;
            }

            queryTextBox.Enabled = false;
            saveVideosNameCheckBox.Enabled = false;
            userAgentTextBox.Enabled = false;
            numPagesNumericUpDown.Enabled = false;

            // Show the save file dialog to the user
            string resultsFilePath = $"{Directory.GetCurrentDirectory()}\\output\\{query}.txt";

            int scrapedVids = 0;
            var standardOut = Console.Out;

            try {
                using (StreamWriter resultsWriter = new StreamWriter(resultsFilePath)) {
                    Console.SetOut(resultsWriter);

                    int totalScrapedLinks = 0;

                    // Show progress bar
                    progressBar.Visible = true;

                    Scrape scrape = new Scrape(query, pages, userAgent);

                    var scrapingTask = Task.Run(async () => {
                        // Start scraping from various sources
                        scrapedVids += await scrape.ScrapeErome();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeEroThots();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeViralPornHub();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeThotHub();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeMat6tube();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeHClips();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeCamWhorez();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeGotAnyNudes();
                        totalScrapedLinks++;

                        scrapedVids += await scrape.ScrapeInternetChicks();
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
                ResetUI();
            } finally {
                // Display a completion message and reset UI elements
                MessageBox.Show($"Done scraping.\nScraped {scrapedVids} links for you!", $"Done", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Invoke(new Action(() => {
                    progressBar.Value = 0;
                }));

                ResetUI();
            }
        }

        public static Task<int> SaveVideos(HtmlNodeCollection videoLinks, string query) {
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

                        if (videoName == null) {
                            videoName = linkNode.InnerHtml;
                        }

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
        private static bool CheckVideo(string link, string mainLink, string videoName, string query) {
            try {
                if (videoName.Contains(query)) {
                    if (checkBox.Checked) {
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
            return (100 / 9) * totalScrapedLinks;
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

        private void ResetUI() {
            searchBtn.Enabled = false;
            queryTextBox.Enabled = true;
            saveVideosNameCheckBox.Enabled = true;
            userAgentTextBox.Enabled = true;
            numPagesNumericUpDown.Enabled = true;
            progressBar.Visible = false;
            progressBar.Value = 0;
            uniqueLinks.Clear();
        }
    }
}
