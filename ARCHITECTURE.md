# Selenium Architecture Refactoring

## Overview
This solution has been refactored to follow the Page Object Model (POM) pattern with a centralized generic Selenium layer. All Selenium code has been moved to a dedicated `Shared` folder to maintain separation of concerns.

## Folder Structure

### `Shared/` Folder (NEW)
Contains all generic Selenium operations that can be reused across the project:
- **BasePage.cs** - Base class with all generic Selenium methods

### `Pages/` Folder
Contains page-specific implementations that inherit from BasePage:
- **LoginPage.cs** (AmazonPage) - Amazon-specific page actions

### `StepDefinitions/` Folder
Contains SpecFlow step definitions that use page classes:
- **LoginSteps.cs** (AmazonSteps) - BDD step implementations

### `Features/` Folder
Contains BDD feature files:
- **Login.feature** - Feature scenarios

---

## Architecture

### 1. BasePage (Shared/BasePage.cs)
The foundation of the refactored architecture. Contains ALL Selenium-specific code:

**Key Methods:**
- **Element Finding**: `FindElement()`, `FindElements()`, `FindElementNoWait()`, `IsElementPresent()`
- **Waiting**: `WaitForElementToBeVisible()`, `WaitForElementToBeClickable()`, `WaitForElementsCount()`
- **Interactions**: `SendText()`, `Click()`, `ClickUsingJavaScript()`, `ClickWithFallback()`, `GetText()`, `GetValue()`, `GetAttribute()`
- **Navigation**: `NavigateToUrl()`, `GetCurrentUrl()`
- **JavaScript**: `ExecuteScript()`, `ScrollIntoView()`, `ScrollToTop()`, `ScrollToBottom()`
- **Utilities**: `Wait()`, `GetPageTitle()`, `GetPageSource()`, `CloseBrowser()`

### 2. Page Classes (Pages/LoginPage.cs)
Contains page-specific logic using BasePage methods:
- Defines page locators as readonly `By` objects
- Implements page-specific actions
- No direct Selenium code - all operations use BasePage methods
- Business logic is separated from Selenium implementation

**Example:**
```csharp
// OLD - Direct Selenium
var searchBox = _driver.FindElement(By.Id("searchbox"));
searchBox.Clear();
searchBox.SendKeys("keyword");

// NEW - Using BasePage
SendText(SearchBoxLocator, "keyword");
```

### 3. Step Definitions (StepDefinitions/LoginSteps.cs)
Uses page classes to implement BDD steps:
- No Selenium code
- Simple delegation to page class methods
- Clean, readable step implementations

---

## Usage Examples

### Example 1: Searching for a Product
**Page Class:**
```csharp
public AmazonPage SearchForProduct(string productName)
{
    SendText(SearchBoxLocator, productName);  // BasePage method
    Click(SearchButtonLocator);               // BasePage method
    WaitForElementsCount(SearchResultsLocator, 1);
    Wait(2000);
    return this;
}
```

**Step Definition:**
```csharp
[When("I search for \"(.*)\"")]
public void WhenISearchFor(string productName)
{
    _amazonPage?.SearchForProduct(productName);
}
```

**Feature File:**
```gherkin
When I search for "phone"
```

### Example 2: Finding Elements
Instead of:
```csharp
_driver.FindElements(By.Id("add-to-cart-button"))
```

Use:
```csharp
FindElements(AddToCartButtonIdLocator)  // Defined in page class
```

---

## Benefits

1. **Maintainability**: All Selenium code in one place (BasePage)
2. **Reusability**: Generic methods usable by all page classes
3. **Consistency**: Standardized way to interact with elements
4. **Separation of Concerns**: 
   - Selenium logic in `Shared/BasePage.cs`
   - Page logic in `Pages/`
   - Test logic in `StepDefinitions/`
5. **Easy Testing**: Page classes can be tested independently
6. **Scalability**: Easy to add new page classes for new features

---

## Adding a New Page Class

1. Create a new class in `Pages/` folder
2. Inherit from `BasePage`
3. Define locators as readonly `By` objects
4. Implement page-specific methods using BasePage methods
5. No direct Selenium code needed

**Template:**
```csharp
public class NewPage : BasePage
{
    // Locators
    private readonly By ElementLocator = By.Id("element-id");
    
    public NewPage(IWebDriver driver) : base(driver)
    {
    }
    
    public NewPage PerformAction()
    {
        Click(ElementLocator);  // Uses BasePage method
        return this;
    }
}
```

---

## Important Notes

- **Locators**: Always define as readonly fields in page classes for reusability
- **Waits**: Use appropriate wait methods (WaitForElementToBeVisible, WaitForElementToBeClickable, etc.)
- **Error Handling**: Selenium exceptions are caught and re-thrown with meaningful messages in BasePage
- **Thread Safety**: All methods are thread-safe as they operate on the IWebDriver instance
- **Performance**: Implement waits wisely to avoid test timeouts

---

## Testing the Refactored Code

Run your SpecFlow scenarios as normal:
```bash
dotnet test
# or
nunit-console TestProject.dll
```

The behavior remains the same, but the code is now more maintainable and follows best practices.
