Feature: User Login

  Scenario: Successful login with valid credentials
    Given I am on the login page
    When I enter username "testuser" and password "password123"
    And I click the login button
    Then I should be redirected to the dashboard

  Scenario: Failed login with invalid credentials
    Given I am on the login page
    When I enter username "invalid" and password "wrong"
    And I click the login button
    Then I should see an error message "Invalid credentials"