using HtmlAgilityPack;
using System.Text;

namespace ChatApp.Utils
{
    public class Helper
    {
        public static string GenOtp()
        {
            return new Random().Next(0, 1000000).ToString("D6");
        }
        public static string GenRandomPass()
        {
            return new Random().Next(0, 1000000).ToString("D6");
        }
        public static async Task<string> GetThumbnailFromUrl(string url)
        {
            using HttpClient client = new HttpClient();

            // Add headers to simulate a browser request
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");

            try
            {
                var response = await client.GetStringAsync(url);

                // Load the HTML document
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(response);

                // Look for <meta> tags with property "og:image" or "twitter:image"
                var thumbnailNode = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']")
                                    ?? htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");

                // Return the content attribute value if found
                return thumbnailNode?.GetAttributeValue("content", "Thumbnail not found") ?? "Thumbnail not found";
            }
            catch (HttpRequestException ex)
            {
                return $"Error: Unable to fetch content from the URL. {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Unexpected Error: {ex.Message}";
            }
        }

        public static async Task<object> GetWebpageInfo(string url)
        {
            using HttpClient client = new HttpClient();

            // Add headers to simulate a browser request
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36");
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.5");

            try
            {
                var response = await client.GetStringAsync(url);

                // Load the HTML document
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(response);

                // Retrieve the title
                var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//title");
                string title = titleNode?.InnerText.Trim() ?? "Title not found";

                // Retrieve the description
                var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='description']");
                string description = descriptionNode?.GetAttributeValue("content", "Description not found") ?? "Description not found";

                // Retrieve the thumbnail URL
                var thumbnailNode = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']")
                                    ?? htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']");
                string thumbnailUrl = thumbnailNode?.GetAttributeValue("content", "Thumbnail not found") ?? "Thumbnail not found";

                // Return all gathered information
                return new 
                {
                    Title = title,
                    Description = description,
                    ThumbnailUrl = thumbnailUrl
                };
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Unable to fetch content from the URL. " + ex.Message);
            }
        }
    }
}
