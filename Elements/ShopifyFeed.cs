using System.Collections.Generic;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Helpers;

namespace ShopifyFeed.Elements
{
    public class ShopifyFeed : Element
    {
        public override string Category {
            get { return "Content"; }
        }

        public string FeedUrl
        {
            get { return this.Retrieve(x => x.FeedUrl); }
            set { this.Store(x => x.FeedUrl, value); }
        }

        public bool Randomise
        {
            get { return this.Retrieve(x => x.Randomise); }
            set { this.Store(x => x.Randomise, value); }
        }

        public int NumberToShow
        {
            get { return this.Retrieve(x => x.NumberToShow); }
            set { this.Store(x => x.NumberToShow, value); }
        }

        public List<ShopifyFeedItem> Items { get; set; }
    }

    public class ShopifyFeedItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Price { get; set; }
        public string ImageUrl { get; set; }
    }
}