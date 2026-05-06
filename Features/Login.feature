Feature: Amazon Shopping Flow

  Scenario: Search for a phone and verify cart contains the added item
    Given I open the Amazon home page
    When I search for "phone"
    And I select the first product from search results and add it to the cart
    And I return to the Amazon home page
    And I go to the cart
    Then the cart should contain at least one item
    And I close the browser