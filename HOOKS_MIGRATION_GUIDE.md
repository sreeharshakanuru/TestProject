# HOOKS Folder Migration Guide

## Current State

You now have **two hook systems** in your project:

### 1. Old System (Hooks folder - lowercase)
- **Location**: `Hooks/TestHooks.cs`
- **Purpose**: Original test lifecycle management
- **Status**: Legacy (can be deprecated)

### 2. New System (HOOKS folder - uppercase)
- **Location**: `HOOKS/`
- **Files**: `TestSetupPage.cs`, `HooksConfiguration.cs`
- **Purpose**: Centralized test setup with reusable helpers
- **Status**: Production-ready (recommended)

---

## Folder Structure

```
TestProject/
├── Hooks/                          (OLD - lowercase)
│   ├── TestHooks.cs               (Original hooks - legacy)
│
├── HOOKS/                          (NEW - uppercase)
│   ├── TestSetupPage.cs           (Test setup page class)
│   ├── HooksConfiguration.cs       (SpecFlow hooks bindings)
│   └── TestHooks.cs               (Duplicate - can delete)
│
└── Pages/
    └── LoginPage.cs               (Page objects)
```

---

## Migration Path

### Phase 1: Use New System (Current State) ✅
The new `HOOKS` folder system is ready to use immediately with your existing tests.

**Step 1**: Update Step Definitions
```csharp
// OLD
[BeforeScenario]
public void Setup(ScenarioContext scenarioContext)
{
    _driver = scenarioContext.Get<IWebDriver>("WebDriver");
    _amazonPage = new AmazonPage(_driver);
}

// NEW
[BeforeScenario]
public void Setup(ScenarioContext scenarioContext)
{
    _testSetupPage = scenarioContext.Get<TestSetupPage>("TestSetupPage");
    _amazonPage = _testSetupPage.GetAmazonPage();
}
```

**Step 2**: Use Helper Methods in Steps
```csharp
// OLD
[When("I search for \"(.*)\"")]
public void WhenISearchFor(string productName)
{
    _amazonPage?.SearchForProduct(productName);
}

// NEW
[When("I search for \"(.*)\"")]
public void WhenISearchFor(string productName)
{
    _testSetupPage?.ExecuteAction(
        () => _amazonPage!.SearchForProduct(productName),
        $"Search for {productName}"
    );
}
```

### Phase 2: Remove Old Hooks (Optional)
Once all tests are updated to use the new system:

1. Delete `Hooks/TestHooks.cs`
2. Delete `Hooks` folder (if empty)
3. Rename `HOOKS` to `Hooks` (optional, for consistency)

### Phase 3: Full Adoption
- Use TestSetupPage for all setup/teardown
- Use ExecuteAction for better logging
- Use AssertCondition for assertions
- Use SetScenarioData for shared state

---

## Side-by-Side Comparison

### Scenario Setup

**OLD** (TestHooks.cs)
```csharp
[BeforeScenario(Order = 0)]
public void BeforeScenario(ScenarioContext scenarioContext)
{
    _driver = WebDriverSetup.CreateChromeDriver();
    scenarioContext.Set(_driver, "WebDriver");

    Console.WriteLine($"\n📝 Scenario: {scenarioContext.ScenarioInfo.Title}");
    Console.WriteLine("🌐 Chrome browser launched for scenario");
}
```

**NEW** (HooksConfiguration.cs + TestSetupPage.cs)
```csharp
[BeforeScenario(Order = 0)]
public void BeforeScenario(ScenarioContext scenarioContext)
{
    _driver = WebDriverSetup.CreateChromeDriver();
    scenarioContext.Set(_driver, "WebDriver");
    
    _testSetupPage = new TestSetupPage(_driver, scenarioContext);
    scenarioContext.Set(_testSetupPage, "TestSetupPage");
    
    _testSetupPage.InitializeScenario();  // Handles all initialization
}
```

### Scenario Cleanup

**OLD** (TestHooks.cs)
```csharp
[AfterScenario(Order = 100)]
public void AfterScenario(ScenarioContext scenarioContext)
{
    HandleScenarioCompletion(scenarioContext);
    CleanupWebDriver();
}
```

**NEW** (HooksConfiguration.cs + TestSetupPage.cs)
```csharp
[AfterScenario(Order = 100)]
public void AfterScenario(ScenarioContext scenarioContext)
{
    if (scenarioContext.TryGetValue("TestSetupPage", out TestSetupPage? testSetupPage))
    {
        testSetupPage.FinalizeScenario();  // Handles all cleanup
        testSetupPage.PrintSessionSummary();
    }
}
```

### Step Execution

**OLD** (Step definitions)
```csharp
[When("I search for \"(.*)\"")]
public void WhenISearchFor(string productName)
{
    try
    {
        _amazonPage?.SearchForProduct(productName);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        throw;
    }
}
```

**NEW** (Step definitions with TestSetupPage)
```csharp
[When("I search for \"(.*)\"")]
public void WhenISearchFor(string productName)
{
    _testSetupPage?.ExecuteAction(
        () => _amazonPage!.SearchForProduct(productName),
        $"Search for {productName}"
    );
}
```

---

## Benefits of New System

| Feature | OLD | NEW |
|---------|-----|-----|
| Logging | Manual | Automatic |
| Error Handling | Manual try-catch | Built-in |
| Execution Tracking | Console.WriteLine | Structured logging |
| Page Object Management | Manual | Centralized |
| Scenario Data Sharing | Direct context | SetScenarioData() |
| Retry Logic | Not built-in | RetryAction() |
| Assertions | NUnit Assert | AssertCondition() |
| Failure Screenshots | Manual | Automatic |
| Session Summary | Manual | PrintSessionSummary() |

---

## How to Transition Your Steps

### Example: LoginSteps.cs Transition

**BEFORE** (Using OLD hooks)
```csharp
using Reqnroll;
using NUnit.Framework;
using OpenQA.Selenium;
using TestProject.Pages;

[Binding]
public class LoginSteps
{
    private IWebDriver? _driver;
    private AmazonPage? _amazonPage;

    [BeforeScenario]
    public void Setup(ScenarioContext scenarioContext)
    {
        _driver = scenarioContext.Get<IWebDriver>("WebDriver");
        _amazonPage = new AmazonPage(_driver);
    }

    [Given("I open the Amazon home page")]
    public void GivenIOpenTheAmazonHomePage()
    {
        Console.WriteLine("Opening Amazon home page...");
        try
        {
            _amazonPage?.NavigateToAmazonHomePage();
            Console.WriteLine("✓ Opened successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            throw;
        }
    }

    [Then("the cart should contain at least one item")]
    public void ThenTheCartShouldContainAtLeastOneItem()
    {
        var hasItems = _amazonPage?.CartHasItems() ?? false;
        Assert.That(hasItems, Is.True, "Expected cart to have items");
    }
}
```

**AFTER** (Using NEW hooks with TestSetupPage)
```csharp
using Reqnroll;
using TestProject.Pages;
using TestProject.HOOKS;

[Binding]
public class LoginSteps
{
    private AmazonPage? _amazonPage;
    private TestSetupPage? _testSetupPage;

    [BeforeScenario]
    public void Setup(ScenarioContext scenarioContext)
    {
        _testSetupPage = scenarioContext.Get<TestSetupPage>("TestSetupPage");
        _amazonPage = _testSetupPage.GetAmazonPage();
    }

    [Given("I open the Amazon home page")]
    public void GivenIOpenTheAmazonHomePage()
    {
        _testSetupPage?.ExecuteAction(
            () => _amazonPage!.NavigateToAmazonHomePage(),
            "Navigate to Amazon home page"
        );
    }

    [Then("the cart should contain at least one item")]
    public void ThenTheCartShouldContainAtLeastOneItem()
    {
        var hasItems = _amazonPage!.CartHasItems();
        _testSetupPage?.AssertCondition(hasItems, "Cart contains at least one item");
    }
}
```

---

## Recommended Transition Steps

### 1. Start Using New HOOKS Folder (NOW)
- Keep old Hooks folder as-is
- Both systems can coexist
- Gradually update step definitions

### 2. Update Step Definitions (This Sprint)
- Add TestSetupPage to each step file
- Replace manual logging with ExecuteAction()
- Replace Assert with AssertCondition()

### 3. Test Everything (Before Cleanup)
- Run all tests
- Verify they pass
- Check logs for correct output

### 4. Deprecate Old System (Next Sprint)
- Delete Hooks/TestHooks.cs
- Delete Hooks folder
- Clean up old imports

### 5. Cleanup (After Verification)
- Remove old namespaces
- Update documentation
- Archive old code

---

## Adding New Page Objects

With the new system, adding pages is easier:

### Step 1: Create Page Class
```csharp
public class CheckoutPage : BasePage
{
    private readonly By EmailInput = By.Id("email");
    
    public CheckoutPage(IWebDriver driver) : base(driver) { }
    
    public void FillEmail(string email)
    {
        SendText(EmailInput, email);
    }
}
```

### Step 2: Register in TestSetupPage
```csharp
private CheckoutPage? _checkoutPage;

private void InitializePageObjects()
{
    // ... existing pages ...
    
    _checkoutPage = new CheckoutPage(Driver);
    RegisterPageObject("CheckoutPage", _checkoutPage);
}
```

### Step 3: Use in Step Definitions
```csharp
[When("I fill email")]
public void WhenIFillEmail(string email)
{
    var checkoutPage = _testSetupPage?.GetPageObject<CheckoutPage>("CheckoutPage");
    _testSetupPage?.ExecuteAction(
        () => checkoutPage!.FillEmail(email),
        "Fill email address"
    );
}
```

---

## Troubleshooting

### Problem: "TestSetupPage not found in context"
**Solution**: Ensure HooksConfiguration.BeforeScenario is running
- Check that HooksConfiguration.cs is in HOOKS folder
- Verify namespace is correct (TestProject.HOOKS)

### Problem: Duplicate hooks running
**Solution**: If both old and new hooks run:
1. Delete Hooks/TestHooks.cs (or disable it)
2. Keep only HooksConfiguration.cs
3. Restart test runner

### Problem: Page object not initialized
**Solution**: 
1. Verify page is registered in InitializePageObjects()
2. Use GetPageObject<T>() method
3. Check for null before using

### Problem: Scenario data not persisting
**Solution**: Use SetScenarioData() instead of direct context.Set()
```csharp
_testSetupPage?.SetScenarioData("key", value);
// Later
var value = _testSetupPage?.GetScenarioData<T>("key");
```

---

## File Locations Reference

```
HOOKS/TestSetupPage.cs
├── InitializeTestRun()
├── FinalizeTestRun()
├── InitializeScenario()
├── FinalizeScenario()
├── Page Object Methods
├── Scenario Data Methods
├── Test Execution Helpers
├── Validation Methods
└── Session Management

HOOKS/HooksConfiguration.cs
├── [BeforeTestRun]
├── [AfterTestRun]
├── [BeforeScenario(Order=0)]
├── [AfterScenario(Order=100)]
├── [BeforeStep]
├── [AfterStep]
├── [BeforeScenarioBlock]
└── [AfterScenarioBlock]

HOOKS_USAGE.md
└── Complete usage documentation
```

---

## Summary

✅ **New HOOKS System is Ready**
- Centralized test lifecycle management
- Reusable helper methods
- Better logging and error handling
- Scalable page object management

📋 **Migration is Simple**
- Update step definitions one-by-one
- Run tests to verify
- Gradually deprecate old system

🎯 **End Goal**
- All tests use new HOOKS system
- Consistent logging throughout
- Easier debugging and maintenance
- Better test reports

---

## Next Actions

1. ✅ Read HOOKS_USAGE.md for complete documentation
2. 📋 Update first step definition to use TestSetupPage
3. 📋 Run tests and verify they pass
4. 📋 Update remaining step definitions
5. 📋 Delete old Hooks/TestHooks.cs when ready
6. 📋 Rename HOOKS to Hooks (optional)

**You're all set!** Start using the new HOOKS system today! 🚀
