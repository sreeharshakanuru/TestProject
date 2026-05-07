using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using TestProject.Shared;

namespace TestProject.Pages
{
    /// <summary>
    /// AmazonPage class contains Amazon-specific page actions.
    /// All Selenium code is abstracted through the BasePage class.
    /// </summary>
    public class AmazonPage : BasePage
    {
        // Locators
        private readonly By SearchBoxLocator = By.Id("twotabsearchtextbox");
        private readonly By SearchButtonLocator = By.Id("nav-search-submit-button");
        private readonly By SearchResultsLocator = By.XPath("//div[@data-component-type='s-search-result']");
        private readonly By ProductLinksLocator = By.XPath("//h2/a[@href]");
        private readonly By ProductLinksAlternateLocator = By.XPath("//a[@aria-label and contains(@href, '/dp/')]");
        private readonly By AddToCartButtonIdLocator = By.Id("add-to-cart-button");
        private readonly By AddToCartButtonXPathLocator = By.XPath("//button[contains(@id, 'add-to-cart')]");
        private readonly By AddToCartButtonAriaLocator = By.XPath("//button[contains(@aria-label, 'Add') and contains(@aria-label, 'Cart')]");
        private readonly By AddToCartButtonTextLocator = By.XPath("//button[contains(text(), 'Cart') or contains(text(), 'Add')]");
        private readonly By NoThanksButtonLocator = By.XPath("//input[@value='No thanks'] | //span[text()='No Thanks']/ancestor::button");
        private readonly By CartButtonLocator = By.XPath("//a[@href='/gp/cart/view.html'] | //a[contains(@href, '/gp/cart')] | //*[@id='nav-cart']");
        private readonly By CartItemsLocator = By.XPath("//div[contains(@class, 'sc-item')] | //div[contains(@class, 'CartItem')] | //*[contains(@class, 'cart-item')]");

        public AmazonPage(IWebDriver driver) : base(driver)
        {
        }

        public AmazonPage NavigateToAmazonHomePage(string url = "https://www.amazon.com/")
        {
            NavigateToUrl(url);
            Sleep(2000); // Wait for page to load
            return this;
        }

        public AmazonPage SearchForProduct(string productName)
        {
            try
            {
                SendText(SearchBoxLocator, productName);
                Click(SearchButtonLocator);
                
                // Wait for search results to load
                WaitForElementsCount(SearchResultsLocator, 1);
                Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during search: {ex.Message}");
                throw;
            }
            return this;
        }

        public AmazonPage SelectFirstProductAndAddToCart()
        {
            try
            {
                Console.WriteLine("Looking for first product in search results...");
                
                var firstProduct = FindFirstProduct();
                if (firstProduct == null)
                {
                    throw new InvalidOperationException("Could not find any product in search results");
                }
                
                firstProduct.Click();
                Console.WriteLine("Clicked on first product");
                
                Sleep(3000); // Wait for product page to load
                
                var addToCartButton = FindAddToCartButton();
                if (addToCartButton == null)
                {
                    throw new InvalidOperationException("Could not find Add to Cart button on product page");
                }
                
                ScrollIntoView(AddToCartButtonIdLocator);
                Sleep(1000);
                
                ClickWithFallback(AddToCartButtonIdLocator);
                Console.WriteLine("Product added to cart");
                
                Sleep(2000);
                CloseOptionalPopups();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to cart: {ex.Message}");
                throw;
            }
            return this;
        }

        public bool CartHasItems()
        {
            try
            {
                Console.WriteLine("Checking if cart has any items...");
                
                Sleep(1000);
                
                // Multiple selector strategies to find cart items
                var cartItemSelectors = new[]
                {
                    "//span[contains(@class, 'a-truncate')]",
                    "//div[contains(@class, 'sc-product')]",
                    "//a[@data-a-target='sc-product-link']",
                    "//h4//span",
                    "//*[contains(@class, 'product')]//span",
                    "//div[@data-name]",
                    "//div[contains(@class, 'CartItem')]"
                };
                
                var cartItems = FindElements(By.XPath(string.Join(" | ", cartItemSelectors)));
                Console.WriteLine($"Found {cartItems.Count} potential cart items");
                
                if (cartItems.Count > 0)
                {
                    Console.WriteLine("✓ Cart has items!");
                    return true;
                }
                
                Console.WriteLine("✗ Cart appears to be empty");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking cart: {ex.Message}");
                return false;
            }
        }

        private IWebElement FindAddToCartButton()
        {
            try
            {
                Console.WriteLine("Searching for Add to Cart button...");
                
                var pageSource = GetPageSource();
                Console.WriteLine($"Page source contains 'add-to-cart': {pageSource.Contains("add-to-cart")}");
                Console.WriteLine($"Page source contains 'Add to Cart': {pageSource.Contains("Add to Cart")}");
                
                // Try finding by ID - most direct
                if (IsElementPresent(AddToCartButtonIdLocator))
                {
                    Console.WriteLine("Found button by ID: add-to-cart-button");
                    return FindElementNoWait(AddToCartButtonIdLocator);
                }
                
                // Try finding by different ID patterns
                var buttons = FindElements(AddToCartButtonXPathLocator);
                if (buttons.Count > 0)
                {
                    Console.WriteLine($"Found {buttons.Count} buttons with 'add-to-cart' in ID");
                    return buttons[0];
                }
                
                // Try finding by aria-label
                buttons = FindElements(AddToCartButtonAriaLocator);
                if (buttons.Count > 0)
                {
                    Console.WriteLine($"Found {buttons.Count} buttons with aria-label");
                    return buttons[0];
                }
                
                // Try finding by text
                buttons = FindElements(AddToCartButtonTextLocator);
                if (buttons.Count > 0)
                {
                    Console.WriteLine($"Found {buttons.Count} buttons with Cart/Add text");
                    return buttons[0];
                }
                
                // Print all buttons on the page for debugging
                var allButtons = FindElements(By.XPath("//button[@type='button' or not(@type)]"));
                Console.WriteLine($"Total buttons on page: {allButtons.Count}");
                for (int i = 0; i < Math.Min(5, allButtons.Count); i++)
                {
                    Console.WriteLine($"  Button {i}: id='{allButtons[i].GetAttribute("id")}', text='{allButtons[i].Text}', aria-label='{allButtons[i].GetAttribute("aria-label")}'");
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding add to cart button: {ex.Message}");
                return null;
            }
        }

        public AmazonPage ReturnToHomePage()
        {
            NavigateToAmazonHomePage();
            return this;
        }

        public AmazonPage GoToCart()
        {
            try
            {
                var cartButtons = FindElements(CartButtonLocator);
                
                if (cartButtons.Count == 0)
                {
                    throw new InvalidOperationException("Could not find Cart button");
                }
                
                Console.WriteLine("Found cart button, navigating to cart...");
                ClickWithFallback(CartButtonLocator);
                
                Console.WriteLine("Navigated to cart");
                
                Sleep(3000);
                WaitForElementsCount(CartItemsLocator, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to cart: {ex.Message}");
                throw;
            }
            return this;
        }

        public bool IsProductInCart(string expectedProductText)
        {
            try
            {
                Console.WriteLine($"Checking if '{expectedProductText}' is in cart...");
                
                Sleep(1000);
                
                // Multiple selector strategies to find cart items
                var cartItemSelectors = new[]
                {
                    "//span[contains(@class, 'a-truncate')]",
                    "//div[contains(@class, 'sc-product')]",
                    "//a[@data-a-target='sc-product-link']",
                    "//h4//span",
                    "//*[contains(@class, 'product')]//span",
                    "//div[@data-name]"
                };
                
                var allCartItems = FindElements(By.XPath(string.Join(" | ", cartItemSelectors)));
                Console.WriteLine($"Found {allCartItems.Count} potential cart items");
                
                var foundItems = new List<string>();
                foreach (var item in allCartItems)
                {
                    try
                    {
                        var itemText = item.Text;
                        if (!string.IsNullOrWhiteSpace(itemText))
                        {
                            foundItems.Add(itemText);
                            Console.WriteLine($"  Item: {itemText.Substring(0, Math.Min(50, itemText.Length))}");
                            
                            if (itemText.Contains(expectedProductText, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"✓ Found '{expectedProductText}' in cart!");
                                return true;
                            }
                        }
                    }
                    catch { }
                }
                
                Console.WriteLine($"✗ '{expectedProductText}' not found in cart items");
                Console.WriteLine($"Total items checked: {foundItems.Count}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking cart: {ex.Message}");
                return false;
            }
        }

        public void CloseBrowser()
        {
            try
            {
                Driver.Quit();
                Console.WriteLine("Browser closed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing browser: {ex.Message}");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Finds the first product in search results using multiple selector strategies.
        /// </summary>
        private IWebElement FindFirstProduct()
        {
            try
            {
                var productCandidates = FindElements(ProductLinksLocator);
                if (productCandidates.Count > 0)
                {
                    Console.WriteLine($"Found product using primary selector: {productCandidates[0].Text}");
                    return productCandidates[0];
                }
                
                productCandidates = FindElements(ProductLinksAlternateLocator);
                if (productCandidates.Count > 0)
                {
                    Console.WriteLine($"Found product using alternate selector");
                    return productCandidates[0];
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding first product: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Closes any optional popups that may appear after adding to cart.
        /// </summary>
        private void CloseOptionalPopups()
        {
            try
            {
                var noThanksButtons = FindElements(NoThanksButtonLocator);
                foreach (var btn in noThanksButtons)
                {
                    if (btn.Displayed)
                    {
                        btn.Click();
                        break;
                    }
                }
            }
            catch
            {
                // Popups might not exist, that's fine
            }
        }

        #endregion
    }
}
