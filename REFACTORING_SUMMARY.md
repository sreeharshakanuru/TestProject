# Selenium Architecture - Quick Reference Guide

## What Changed?

### ✅ NEW: `Shared/` Folder
All Selenium code is now centralized in `Shared/BasePage.cs`

### ✅ REFACTORED: `Pages/LoginPage.cs`
- Now inherits from `BasePage`
- Uses generic Selenium methods instead of direct calls
- Contains only page-specific logic

### ✅ UNCHANGED: `StepDefinitions/` and `Features/`
- Step definitions remain the same - they call page methods
- Feature files remain the same

---

## Code Comparison

### BEFORE (Direct Selenium)
```csharp
public class AmazonPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    
    public void SearchForProduct(string productName)
    {
        var searchBox = _wait.Until(d => d.FindElement(By.Id("searchbox")));
        searchBox.Clear();
        searchBox.SendKeys(productName);
        var button = _driver.FindElement(By.Id("search-btn"));
        button.Click();
    }
}
```

### AFTER (Using BasePage)
```csharp
public class AmazonPage : BasePage
{
    private readonly By SearchBoxLocator = By.Id("searchbox");
    private readonly By SearchButtonLocator = By.Id("search-btn");
    
    public void SearchForProduct(string productName)
    {
        SendText(SearchBoxLocator, productName);
        Click(SearchButtonLocator);
    }
}
```

---

## File Structure

```
TestProject/
├── Shared/                      (NEW - All Selenium code)
│   └── BasePage.cs             (Generic Selenium methods)
├── Pages/
│   └── LoginPage.cs            (Refactored - Uses BasePage)
├── StepDefinitions/
│   └── LoginSteps.cs           (Unchanged)
├── Features/
│   └── Login.feature           (Unchanged)
├── Drivers/
│   ├── WebDriverSetup.cs       (Unchanged)
│   └── ScreenshotHelper.cs     (Unchanged)
├── Hooks/
│   └── TestHooks.cs            (Unchanged)
├── Common/
│   └── ResultTypes.cs          (Unchanged)
├── ARCHITECTURE.md             (NEW - Detailed guide)
└── README.md
```

---

## Key BasePage Methods

### Finding Elements
```csharp
FindElement(By locator)              // Wait for element presence
FindElements(By locator)             // Get all matching elements
FindElementNoWait(By locator)       // No wait
IsElementPresent(By locator)        // Check if element exists
```

### Waiting for Conditions
```csharp
WaitForElementToBeVisible(By locator)      // Wait for visibility
WaitForElementToBeClickable(By locator)    // Wait for clickability
WaitForElementsCount(By locator, count)    // Wait for specific count
WaitForElement(By locator, seconds)        // Custom timeout
```

### User Actions
```csharp
SendText(By locator, string text)          // Type text
Click(By locator)                          // Click element
ClickUsingJavaScript(By locator)           // JS click
ClickWithFallback(By locator)              // Try JS then regular
```

### Getting Information
```csharp
GetText(By locator)                 // Get element text
GetValue(By locator)                // Get value attribute
GetAttribute(By locator, "attr")    // Get any attribute
IsElementDisplayed(By locator)      // Check if visible
IsElementEnabled(By locator)        // Check if enabled
```

### Navigation & Utilities
```csharp
NavigateToUrl(string url)           // Go to URL
GetCurrentUrl()                     // Get current URL
ExecuteScript(string script)        // Run JavaScript
ScrollIntoView(By locator)          // Scroll to element
Wait(int milliseconds)              // Hard wait
CloseBrowser()                      // Close driver
```

---

## How to Add a New Page

1. Create a file in `Pages/` folder
2. Inherit from `BasePage`
3. Define locators
4. Implement page actions using BasePage methods

**Example:**
```csharp
using OpenQA.Selenium;
using TestProject.Shared;

namespace TestProject.Pages
{
    public class ProductPage : BasePage
    {
        private readonly By ProductNameLocator = By.Id("product-name");
        private readonly By PriceLocator = By.ClassName("price");
        private readonly By AddToCartLocator = By.Id("add-cart");
        
        public ProductPage(IWebDriver driver) : base(driver)
        {
        }
        
        public string GetProductName()
        {
            return GetText(ProductNameLocator);
        }
        
        public decimal GetPrice()
        {
            var priceText = GetText(PriceLocator);
            return decimal.Parse(priceText);
        }
        
        public ProductPage AddToCart()
        {
            Click(AddToCartLocator);
            return this;
        }
    }
}
```

---

## Alignment Across Layers

### Feature File
```gherkin
When I search for "laptop"
And I add the first product to cart
Then the cart should contain 1 item
```

### Step Definition
```csharp
[When("I search for \"(.*)\"")]
public void WhenISearchFor(string product)
{
    _amazonPage?.SearchForProduct(product);
}

[When("I add the first product to cart")]
public void WhenIAddFirstProductToCart()
{
    _amazonPage?.SelectFirstProductAndAddToCart();
}

[Then("the cart should contain (.*) item")]
public void ThenCartShouldContain(int count)
{
    Assert.That(_amazonPage?.CartHasItems(), Is.True);
}
```

### Page Class
```csharp
public AmazonPage SearchForProduct(string productName)
{
    SendText(SearchBoxLocator, productName);      // BasePage method
    Click(SearchButtonLocator);                   // BasePage method
    WaitForElementsCount(SearchResultsLocator, 1);
    return this;
}

public AmazonPage SelectFirstProductAndAddToCart()
{
    var product = FindFirstProduct();             // Helper method
    product.Click();
    ClickWithFallback(AddToCartButtonIdLocator);  // BasePage method
    CloseOptionalPopups();
    return this;
}

public bool CartHasItems()
{
    var items = FindElements(CartItemsLocator);   // BasePage method
    return items.Count > 0;
}
```

**Flow:**
1. Feature file defines test scenario
2. Step definition calls page method
3. Page method uses BasePage Selenium methods
4. **All Selenium code is in BasePage** ✅

---

## Benefits Summary

| Aspect | Before | After |
|--------|--------|-------|
| Selenium Code Location | Scattered across pages | Centralized in BasePage |
| Maintainability | Hard to update Selenium methods | Easy - update BasePage once |
| Reusability | Copy-paste code | Use inherited methods |
| Testing | Difficult to unit test | Easy to mock BasePage |
| Consistency | Inconsistent patterns | Standardized methods |
| New Pages | Boilerplate code | Inherit + define locators |
| Page Readability | Cluttered with Selenium | Clean, business logic focused |

---

## Next Steps

1. ✅ Created `Shared/BasePage.cs` with all generic Selenium methods
2. ✅ Refactored `Pages/LoginPage.cs` to use BasePage
3. ✅ Step definitions and features remain unchanged
4. ✅ No Selenium code visible outside BasePage
5. Ready to: Add more page classes using the same pattern

---

## Support

- **ARCHITECTURE.md** - Detailed architecture documentation
- **BasePage.cs** - Source of truth for all Selenium operations
- **LoginPage.cs** - Reference implementation for new pages
