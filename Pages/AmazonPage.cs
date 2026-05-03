using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TestProject.Constants;
using SeleniumExtras.WaitHelpers;

namespace TestProject.Pages
{
    public class AmazonPage
    {
        private IWebDriver _driver;
        private WebDriverWait _wait;

        public AmazonPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        public void NavigateToAmazon()
        {
            _driver.Navigate().GoToUrl(AutomationTestIds.AmazonUrl);
        }

        public void SearchForProduct(string productName)
        {
            try
            {
                // Wait for search box to be visible
                var searchBox = _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(AutomationTestIds.SearchBox)));
                searchBox.Clear();
                searchBox.SendKeys(productName);

                // Click search button
                var searchButton = _driver.FindElement(By.Id(AutomationTestIds.SearchButton));
                searchButton.Click();

                // Wait for search results to load
                _wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(AutomationTestIds.FirstProductLink)));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching for product '{productName}': {ex.Message}", ex);
            }
        }

        public void AddFirstProductToCart()
        {
            try
            {
                // Wait for product results to load
                _wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(AutomationTestIds.FirstProductLink)));

                // Click on the first product
                var firstProduct = _driver.FindElement(By.CssSelector(AutomationTestIds.FirstProductLink));
                firstProduct.Click();

                // Wait for product page to load and find add to cart button
                var addToCartButton = _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(AutomationTestIds.AddToCartButton)));
                addToCartButton.Click();

                // Wait for confirmation
                System.Threading.Thread.Sleep(2000); // Wait for add to cart animation
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding product to cart: {ex.Message}", ex);
            }
        }

        public void GoToCart()
        {
            try
            {
                var cartLink = _wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(AutomationTestIds.CartLink)));
                cartLink.Click();

                // Wait for cart page to load
                _wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.ClassName("sc-list-item")));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error navigating to cart: {ex.Message}", ex);
            }
        }

        public bool IsProductInCart(string productName)
        {
            try
            {
                // Navigate to cart
                GoToCart();

                // Wait for cart items to be visible
                var cartItems = _wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.ClassName("sc-list-item")));

                // Check if product name exists in cart
                foreach (var item in cartItems)
                {
                    var itemText = item.Text.ToLower();
                    if (itemText.Contains(productName.ToLower()))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error verifying product in cart: {ex.Message}", ex);
            }
        }

        public int GetCartItemCount()
        {
            try
            {
                var cartCountElement = _driver.FindElement(By.Id(AutomationTestIds.CartCount));
                string countText = cartCountElement.Text;

                if (int.TryParse(countText, out int count))
                {
                    return count;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public void CloseDriver()
        {
            if (_driver != null)
            {
                _driver.Quit();
            }
        }
    }
}
