using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace TestProject.Pages
{
    public class AmazonPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const int DefaultWaitSeconds = 20;

        public AmazonPage(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultWaitSeconds));
        }

        public AmazonPage NavigateToAmazonHomePage(string url = "https://www.amazon.com/")
        {
            _driver.Navigate().GoToUrl(url);
            System.Threading.Thread.Sleep(2000); // Wait for page to load
            return this;
        }

        public AmazonPage SearchForProduct(string productName)
        {
            try
            {
                var searchBox = _wait.Until(d => d.FindElement(By.Id("twotabsearchtextbox")));
                searchBox.Clear();
                searchBox.SendKeys(productName);
                
                var searchButton = _driver.FindElement(By.Id("nav-search-submit-button"));
                searchButton.Click();
                
                // Wait for search results to load
                _wait.Until(d => d.FindElements(By.XPath("//div[@data-component-type='s-search-result']")).Count > 0);
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during search: {ex.Message}");
                throw;
            }
            return this;
        }

        public AmazonPage SelectFirstIPhoneAndAddToCart()
        {
            try
            {
                Console.WriteLine("Looking for first iPhone product in search results...");
                
                // Try multiple selectors to find the first product result
                IWebElement firstProduct = null;
                
                // First try: h2 a tag within search results
                var productCandidates = _driver.FindElements(By.XPath("//h2/a[@href]"));
                if (productCandidates.Count > 0)
                {
                    firstProduct = productCandidates[0];
                    Console.WriteLine($"Found product using h2/a selector: {firstProduct.Text}");
                }
                
                // Fallback: Try a broader selector for product links
                if (firstProduct == null)
                {
                    productCandidates = _driver.FindElements(By.XPath("//a[@aria-label and contains(@href, '/dp/')]"));
                    if (productCandidates.Count > 0)
                    {
                        firstProduct = productCandidates[0];
                        Console.WriteLine($"Found product using /dp/ link selector");
                    }
                }
                
                if (firstProduct == null)
                {
                    throw new InvalidOperationException("Could not find any product in search results");
                }
                
                // Click the product
                firstProduct.Click();
                Console.WriteLine("Clicked on first product");
                
                // Wait for product page to load
                Thread.Sleep(3000);
                
                // Find and click the add to cart button
                var addToCartButton = FindAddToCartButton();
                if (addToCartButton == null)
                {
                    throw new InvalidOperationException("Could not find Add to Cart button on product page");
                }
                
                // Scroll to button if needed
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", addToCartButton);
                Thread.Sleep(500);
                
                addToCartButton.Click();
                Console.WriteLine("Product added to cart");
                
                // Wait for cart confirmation
                Thread.Sleep(2000);
                CloseOptionalPopups();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to cart: {ex.Message}");
                throw;
            }
            return this;
        }

        private IWebElement FindAddToCartButton()
        {
            try
            {
                // Try finding by ID
                var buttons = _driver.FindElements(By.Id("add-to-cart-button"));
                if (buttons.Count > 0 && buttons[0].Displayed)
                {
                    return buttons[0];
                }
                
                // Try finding by aria-label
                buttons = _driver.FindElements(By.XPath("//button[contains(@aria-label, 'Add to Shopping Cart') or contains(@aria-label, 'Add to Cart')]"));
                if (buttons.Count > 0 && buttons[0].Displayed)
                {
                    return buttons[0];
                }
                
                // Try finding any button with "cart" text
                buttons = _driver.FindElements(By.XPath("//button[contains(text(), 'Cart') or contains(text(), 'cart')]"));
                if (buttons.Count > 0 && buttons[0].Displayed)
                {
                    return buttons[0];
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
                var cartButtons = _driver.FindElements(By.XPath(
                    "//a[@href='/gp/cart/view.html'] | //a[contains(@href, '/gp/cart')] | //*[@id='nav-cart'] | //span[contains(text(), 'Cart')]/ancestor::a"
                ));
                
                if (cartButtons.Count == 0)
                {
                    throw new InvalidOperationException("Could not find Cart button");
                }
                
                cartButtons[0].Click();
                Console.WriteLine("Navigated to cart");
                
                // Wait for cart page to load
                Thread.Sleep(3000);
                _wait.Until(d => d.FindElements(By.XPath(
                    "//div[contains(@class, 'sc-item')] | //div[contains(@class, 'CartItem')] | //*[contains(@class, 'cart-item')]"
                )).Count > 0 
                || d.FindElements(By.XPath("//h2[contains(text(), 'Shopping Cart')]")).Count > 0);
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
                
                // Wait a bit for cart to fully load
                Thread.Sleep(1000);
                
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
                
                var allCartItems = _driver.FindElements(By.XPath(string.Join(" | ", cartItemSelectors)));
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
                _driver.Quit();
                Console.WriteLine("Browser closed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing browser: {ex.Message}");
            }
        }

        private void CloseOptionalPopups()
        {
            try
            {
                // Close add-on popup
                var noThanksButtons = _driver.FindElements(By.XPath(
                    "//input[@value='No thanks'] | //span[text()='No Thanks']/ancestor::button"
                ));
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
    }
}
