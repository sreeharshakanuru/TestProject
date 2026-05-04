Feature: Amazon Shopping Flow

  Scenario: Search for iPhone and verify cart contains the added item
    Given I open the Amazon home page
    When I search for "iphone17pro"
    And I select the first iPhone from search results and add it to the cart
    And I return to the Amazon home page
    And I go to the cart
    Then the cart should contain an item with text "iPhone"
    And I close the browser