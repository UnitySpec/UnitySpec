Feature: WASD moves player
    As a user
    I want to be able to move using wasd-keys
    
    Scenario Outline: Basic movement
        Given I load the level "MoveTest"
        And I have a position
        When I press <key> for 1 second
        Then I have moved <direction>

        Scenarios:
            | key   | direction |
            | w     | forward   |
            | a     | left      |
            | s     | backward  |
            | d     | right     |
            