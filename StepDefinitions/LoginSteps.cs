using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;

namespace TestProject.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private IWebDriver _driver;

        [Given("I am on the login page")]
        public void GivenIAmOnTheLoginPage()
        {
            _driver = new ChromeDriver();
            _driver.Navigate().GoToUrl("https://example.com/login"); // Replace with actual URL
        }

        [When("I enter username \"(.*)\" and password \"(.*)\"")]
        public void WhenIEnterUsernameAndPassword(string username, string password)
        {
            _driver.FindElement(By.Id("username")).SendKeys(username);
            _driver.FindElement(By.Id("password")).SendKeys(password);
        }

        [When("I click the login button")]
        public void WhenIClickTheLoginButton()
        {
            _driver.FindElement(By.Id("loginButton")).Click();
        }

        [Then("I should be redirected to the dashboard")]
        public void ThenIShouldBeRedirectedToTheDashboard()
        {
            Assert.AreEqual("https://example.com/dashboard", _driver.Url); // Replace with actual URL
            _driver.Quit();
        }

        [Then("I should see an error message \"(.*)\"")]
        public void ThenIShouldSeeAnErrorMessage(string message)
        {
            var errorElement = _driver.FindElement(By.ClassName("error"));
            Assert.AreEqual(message, errorElement.Text);
            _driver.Quit();
        }
    }
}