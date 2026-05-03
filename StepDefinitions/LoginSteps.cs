using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using TestProject.Pages;

namespace TestProject.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private IWebDriver _driver;
        private AmazonPage _amazonPage;
        private string _searchedProduct;

        [Given("I have an Amazon web page")]
        public void GivenIHaveAnAmazonWebPage()
        {
            _driver = new ChromeDriver();
            _amazonPage = new AmazonPage(_driver);
        }

        [When("I go to the Amazon URL")]
        public void WhenIGoToTheAmazonUrl()
        {
            _amazonPage.NavigateToAmazon();
            System.Threading.Thread.Sleep(2000); // Wait for page to load
        }

        [When("I search for \"(.*)\"")]
        public void WhenISearchFor(string productName)
        {
            _searchedProduct = productName;
            _amazonPage.SearchForProduct(productName);
            System.Threading.Thread.Sleep(3000); // Wait for search results
        }

        [When("I add the first product to the cart")]
        public void WhenIAddTheFirstProductToTheCart()
        {
            _amazonPage.AddFirstProductToCart();
        }

        [Then("I should verify the product is added in the cart")]
        public void ThenIShouldVerifyTheProductIsAddedInTheCart()
        {
            bool isProductInCart = _amazonPage.IsProductInCart(_searchedProduct);
            Assert.IsTrue(isProductInCart, $"Product '{_searchedProduct}' was not found in the cart");
            _amazonPage.CloseDriver();
        }
    }
}