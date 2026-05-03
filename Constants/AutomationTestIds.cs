namespace TestProject.Constants
{
    public static class AutomationTestIds
    {
        // URLs
        public const string AmazonUrl = "https://www.amazon.com";

        // Search Elements
        public const string SearchBox = "twotabsearchtextbox";
        public const string SearchButton = "nav-search-submit-button";

        // Product Elements
        public const string FirstProductLink = "a[data-component-type='s-search-result']";
        public const string AddToCartButton = "add-to-cart-button";
        public const string AddToCartButtonSecondary = "a-button-input";

        // Cart Elements
        public const string CartLink = "nav-cart";
        public const string CartCount = "nav-cart-count";
        public const string CartItemName = "sc-product-link";
        public const string CartItemQuantity = "a-declarative";
        public const string DeleteFromCart = "sc-action-delete";

        // Wait Elements
        public const string ProductContainer = "s-result-list";
        public const string CartContainer = "sc-cart-header";

        // Locator Types
        public const string IdLocator = "id";
        public const string CssLocator = "css";
        public const string XPathLocator = "xpath";
        public const string ClassLocator = "class";
    }
}
