using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace TestProject.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private const int DefaultWaitSeconds = 10;

        public LoginPage(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultWaitSeconds));
        }

        // Locators using lambda functions for lazy evaluation
        private IWebElement UsernameField => _wait.Until(d => d.FindElement(By.Id("username")));
        private IWebElement PasswordField => _wait.Until(d => d.FindElement(By.Id("password")));
        private IWebElement LoginButton => _wait.Until(d => d.FindElement(By.Id("loginButton")));
        private IWebElement ErrorMessage => _driver.FindElement(By.ClassName("error"));
        
        public string CurrentUrl => _driver.Url;
        public string ErrorMessageText => ErrorMessage.Text;

        public LoginPage NavigateToLoginPage(string url = "https://example.com/login")
        {
            _driver.Navigate().GoToUrl(url);
            return this;
        }

        public LoginPage EnterUsername(string username)
        {
            UsernameField.Clear();
            UsernameField.SendKeys(username);
            return this;
        }

        public LoginPage EnterPassword(string password)
        {
            PasswordField.Clear();
            PasswordField.SendKeys(password);
            return this;
        }

        public LoginPage ClickLoginButton()
        {
            LoginButton.Click();
            return this;
        }

        public LoginPage Login(string username, string password)
        {
            return EnterUsername(username)
                .EnterPassword(password)
                .ClickLoginButton();
        }

        public bool IsOnDashboard(string expectedDashboardUrl = "https://example.com/dashboard")
        {
            var waitForUrl = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultWaitSeconds));
            return waitForUrl.Until(d => d.Url.Equals(expectedDashboardUrl));
        }

        public bool IsErrorMessageDisplayed(string expectedMessage)
        {
            var waitForError = new WebDriverWait(_driver, TimeSpan.FromSeconds(DefaultWaitSeconds));
            return waitForError.Until(d => 
            {
                try 
                { 
                    return ErrorMessageText.Equals(expectedMessage);
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
        }
    }
}
