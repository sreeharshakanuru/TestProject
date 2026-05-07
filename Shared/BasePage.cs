using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TestProject.Shared
{
    /// <summary>
    /// BasePage class contains all generic Selenium methods.
    /// All Selenium code is centralized here to maintain separation of concerns.
    /// Page classes should inherit from this and use these methods instead of direct Selenium calls.
    /// </summary>
    public class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected const int DefaultWaitSeconds = 20;

        public BasePage(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(DefaultWaitSeconds));
        }

        #region Navigation Methods

        /// <summary>
        /// Navigates to the specified URL.
        /// </summary>
        protected void NavigateToUrl(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Gets the current page URL.
        /// </summary>
        protected string GetCurrentUrl()
        {
            return Driver.Url;
        }

        #endregion

        #region Element Finding Methods

        /// <summary>
        /// Finds a single element by the specified locator. Waits for the element to be present.
        /// </summary>
        protected IWebElement FindElement(By locator)
        {
            try
            {
                return Wait.Until(d => d.FindElement(locator));
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new NoSuchElementException($"Element not found using locator: {locator}", ex);
            }
        }

        /// <summary>
        /// Finds multiple elements by the specified locator.
        /// </summary>
        protected IList<IWebElement> FindElements(By locator)
        {
            return Driver.FindElements(locator);
        }

        /// <summary>
        /// Finds an element without waiting (returns immediately if not found).
        /// </summary>
        protected IWebElement FindElementNoWait(By locator)
        {
            try
            {
                return Driver.FindElement(locator);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if an element exists on the page.
        /// </summary>
        protected bool IsElementPresent(By locator)
        {
            return Driver.FindElements(locator).Count > 0;
        }

        /// <summary>
        /// Waits for an element to be visible (displayed).
        /// </summary>
        protected IWebElement WaitForElementToBeVisible(By locator)
        {
            try
            {
                return Wait.Until(d => 
                {
                    var element = d.FindElement(locator);
                    if (element.Displayed)
                    {
                        return element;
                    }
                    return null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new InvalidOperationException($"Element not visible within timeout period: {locator}", ex);
            }
        }

        /// <summary>
        /// Waits for an element to be clickable.
        /// </summary>
        protected IWebElement WaitForElementToBeClickable(By locator)
        {
            try
            {
                return Wait.Until(d =>
                {
                    var element = d.FindElement(locator);
                    if (element.Displayed && element.Enabled)
                    {
                        return element;
                    }
                    return null;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new InvalidOperationException($"Element not clickable within timeout period: {locator}", ex);
            }
        }

        /// <summary>
        /// Waits for a specific count of elements to be present.
        /// </summary>
        protected void WaitForElementsCount(By locator, int expectedCount)
        {
            try
            {
                Wait.Until(d => d.FindElements(locator).Count >= expectedCount);
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new InvalidOperationException($"Expected {expectedCount} elements not found within timeout: {locator}", ex);
            }
        }

        #endregion

        #region Interaction Methods

        /// <summary>
        /// Sends text to an element. Clears the element first if it has existing text.
        /// </summary>
        protected void SendText(By locator, string text)
        {
            var element = WaitForElementToBeVisible(locator);
            element.Clear();
            element.SendKeys(text);
        }

        /// <summary>
        /// Clicks an element using standard click.
        /// </summary>
        protected void Click(By locator)
        {
            var element = WaitForElementToBeClickable(locator);
            element.Click();
        }

        /// <summary>
        /// Clicks an element using JavaScript execution (useful for overlays).
        /// </summary>
        protected void ClickUsingJavaScript(By locator)
        {
            var element = FindElement(locator);
            ExecuteScript("arguments[0].click();", element);
        }

        /// <summary>
        /// Clicks an element, with fallback to JavaScript if standard click fails.
        /// </summary>
        protected void ClickWithFallback(By locator)
        {
            try
            {
                Click(locator);
            }
            catch
            {
                ClickUsingJavaScript(locator);
            }
        }

        /// <summary>
        /// Gets the text content of an element.
        /// </summary>
        protected string GetText(By locator)
        {
            var element = FindElement(locator);
            return element.Text;
        }

        /// <summary>
        /// Gets the value attribute of an element.
        /// </summary>
        protected string GetValue(By locator)
        {
            var element = FindElement(locator);
            return element.GetAttribute("value");
        }

        /// <summary>
        /// Gets any attribute value of an element.
        /// </summary>
        protected string GetAttribute(By locator, string attributeName)
        {
            var element = FindElement(locator);
            return element.GetAttribute(attributeName);
        }

        /// <summary>
        /// Checks if an element is displayed (visible).
        /// </summary>
        protected bool IsElementDisplayed(By locator)
        {
            try
            {
                return FindElementNoWait(locator)?.Displayed ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an element is enabled.
        /// </summary>
        protected bool IsElementEnabled(By locator)
        {
            try
            {
                return FindElementNoWait(locator)?.Enabled ?? false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region JavaScript Methods

        /// <summary>
        /// Executes JavaScript code.
        /// </summary>
        protected object ExecuteScript(string script, params object[] args)
        {
            return ((IJavaScriptExecutor)Driver).ExecuteScript(script, args);
        }

        /// <summary>
        /// Scrolls an element into view.
        /// </summary>
        protected void ScrollIntoView(By locator)
        {
            var element = FindElement(locator);
            ExecuteScript("arguments[0].scrollIntoView(true);", element);
        }

        /// <summary>
        /// Scrolls to the top of the page.
        /// </summary>
        protected void ScrollToTop()
        {
            ExecuteScript("window.scrollTo(0, 0);");
        }

        /// <summary>
        /// Scrolls to the bottom of the page.
        /// </summary>
        protected void ScrollToBottom()
        {
            ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }

        #endregion

        #region Wait Methods

        /// <summary>
        /// Waits for a specified amount of time (in milliseconds).
        /// </summary>
        protected void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// Waits for an element to be present for a custom timeout period.
        /// </summary>
        protected IWebElement WaitForElement(By locator, int timeoutSeconds)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                return wait.Until(d => d.FindElement(locator));
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new NoSuchElementException($"Element not found within {timeoutSeconds} seconds: {locator}", ex);
            }
        }

        #endregion

        #region Page Properties

        /// <summary>
        /// Gets the page title.
        /// </summary>
        protected string GetPageTitle()
        {
            return Driver.Title;
        }

        /// <summary>
        /// Gets the page source.
        /// </summary>
        protected string GetPageSource()
        {
            return Driver.PageSource;
        }

        #endregion

        #region Cleanup Methods

        /// <summary>
        /// Closes the current browser window.
        /// </summary>
        protected void CloseBrowser()
        {
            Driver.Quit();
            Driver.Dispose();
        }

        #endregion
    }
}
