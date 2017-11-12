using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.XPath;
using HtmlAgilityPack;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Services;
using ShopifyFeed.Elements;

namespace ShopifyFeed.Drivers
{
    public class ShopifyFeedElementDriver : ElementDriver<Elements.ShopifyFeed> {
        
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public ShopifyFeedElementDriver(IEnumerable<IHtmlFilter> htmlFilters)
        {
            _htmlFilters = htmlFilters;
        }

        protected override EditorResult OnBuildEditor(Elements.ShopifyFeed element, ElementEditorContext context)
        {
            var editor = context.ShapeFactory.EditorTemplate(
                    TemplateName: "Parts/ShopifyFeed",
                    Model: element);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(element, context.Prefix, null, null);
            }
            
            return Editor(context, editor);
        }

        protected override void OnDisplaying(Elements.ShopifyFeed element, ElementDisplayContext context)
        {
            var request = (HttpWebRequest)WebRequest.Create(element.FeedUrl);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
            request.Timeout = 5000;

            using (var response = request.GetResponse()) {
                using (var reader = XmlReader.Create(response.GetResponseStream())) {
                    var feed = SyndicationFeed.Load(reader);
                    if (feed != null) {
                        element.Items = new List<ShopifyFeedItem>();
                        const string shopifyExtensionNamespaceUri = "http://jadedpixel.com/-/spec/shopify";
                        var items = feed.Items.ToList();
                        if (element.Randomise)
                        {
                            items.Shuffle();
                        }
                        if (element.NumberToShow > 0 && element.NumberToShow < items.Count)
                        {
                            items.RemoveRange(element.NumberToShow, items.Count - element.NumberToShow);
                        }
                        foreach (var item in items)
                        {
                            var shopifyItem = new ShopifyFeedItem {
                                Title = item.Title != null ? item.Title.Text : "",
                                Url = item.Links.Any() ? item.Links.First().Uri.AbsoluteUri : "",
                            };

                            // get price
                            var extension = item.ElementExtensions.FirstOrDefault(x => x.OuterNamespace == shopifyExtensionNamespaceUri);
                            if (extension != null) {
                                var dataNavigator = new XPathDocument(extension.GetReader()).CreateNavigator();
                                var resolver = new XmlNamespaceManager(dataNavigator.NameTable);
                                resolver.AddNamespace("s", shopifyExtensionNamespaceUri);
                                var variantNavigator = dataNavigator.SelectSingleNode("s:variant", resolver);
                                shopifyItem.Price = variantNavigator != null ? string.Format("${0}", variantNavigator.SelectSingleNode("s:price", resolver)) : "";
                            }

                            // get picture
                            var summaryText = WebUtility.HtmlDecode(item.Summary.Text);
                            var htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(summaryText);
                            var firstImg = htmlDoc.DocumentNode.Descendants("img").FirstOrDefault();
                            if (firstImg != null) {
                                shopifyItem.ImageUrl = firstImg.Attributes["src"].Value;
                            }

                            element.Items.Add(shopifyItem);
                        }
                    }
                }
            }
            context.ElementShape.ShopifyFeed = element;
        }

    }
}