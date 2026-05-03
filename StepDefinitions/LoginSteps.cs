using NUnit.Framework;
using OpenQA.Selenium;
using Reqnroll;
using TestProject.Pages;

namespace TestProject.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private IWebDriver? _driver;
        private LoginPage? _loginPage;

        [BeforeScenario]
        public void Setup(ScenarioContext scenarioContext)
        {
            _driver = scenarioContext.Get<IWebDriver>("WebDriver");
            _loginPage = new LoginPage(_driver);
        }

        [Given("I am on the login page")]
        public void GivenIAmOnTheLoginPage()
        {
            _loginPage?.NavigateToLoginPage();
        }

        [When("I enter username \"(.*)\" and password \"(.*)\"")]
        public void WhenIEnterUsernameAndPassword(string username, string password)
        {
            _loginPage?.Login(username, password);
        }

        [When("I click the login button")]
        public void WhenIClickTheLoginButton()
        {
            _loginPage?.ClickLoginButton();
        }

        [Then("I should be redirected to the dashboard")]
        public void ThenIShouldBeRedirectedToTheDashboard()
        {
            var isDashboard = _loginPage?.IsOnDashboard() ?? false;
            Assert.That(isDashboard, Is.True, "User should be redirected to dashboard");
        }

        [Then("I should see an error message \"(.*)\"")]
        public void ThenIShouldSeeAnErrorMessage(string message)
        {
            var isErrorDisplayed = _loginPage?.IsErrorMessageDisplayed(message) ?? false;
            Assert.That(isErrorDisplayed, Is.True, $"Error message '{message}' should be displayed");
        }
    }
}
