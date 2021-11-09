# IS7024001Project
# KnowYourFavoriteNobelLaureate

---

Design Document  

## Introduction 

Do you want to know about your favorite Nobel Laureate?  Do you know about their research work? KnowYourFavoriteNobelLaureate can ease your curiosity:  

-	Search your favorite Nobel Laureates by countries.
-	Know about their research work.
-	Know the subjects in which they won prizes.
-	Know their affiliations.

## Requirements  
-	As a user, I want select select country so that I can gather information about Nobel Laureate based on the country I selected.

### Examples
1.1  
*Given* a list of countries 

*When* I select India

*Then* I should receive at least one result with these attributes:   

firstname: Sir Chandrasekhara Venkata  
surname: Raman  
born: 1888-11-07 
And I should receive at least one result with these attributes:  
prizes-year: 1930  
prizes-category: physics  
prizes-motivation:  work on the scattering of light and for the discovery of the effect named after him  

1.2 
*Given* No Nobel Prize won in the particular country
*When* I search with a country who has no Nobel Laureates
*Then* I should receive zero results (an empty list)

-	As a user I am able to play a quiz on Nobel Prizes.

### Examples
2.1  
*Given* a navigation to quiz section 

*When* I and my friend enter our name and group name, then click on join button

*Then* We should receive first question of quiz with four options like: 

Which of the following fields was not included in the Nobel Prize category at the time the Nobel Prizes were first established?
1. Medicine
2. Economics
3. Physics
4. Literature  

2.2 
*Given* The game has concluded 

*When* I and my friend click on play again button

*Then* We should receive first question of quiz with four options like:

When does the formal Nobel Prize ceremony take place every year?
1. 10 December
2. 10 October
3. 10 January
4. 10 November

2.3 
*Given* The game is running 

*When* I click on leave button

*Then* I will leave the room and my friend will win


2.4 
*Given* The game is running 

*When* I get disconnected

*Then* I will leave the room and my friend will win

2.5 
*Given* The game is running 

*When* I submit my answer to the quiz question

*Then* If I answered the quiz question correctly then I will get score of +1

2.6 
*Given* I am already in the room 

*When* I try to join the room again

*Then* I will receive a message that I am already in the room

2.7 
*Given* I have left  the room 

*When* I try to leave the room again

*Then* I will receive a message that I have already left the room

## Data sources  
-	https://pkgstore.datahub.io/core/country-list/data_json/data/8c458f2d15d9f2119654b29ede6e45b8/data_json.json
-	http://api.nobelprize.org/v1/laureate.json

## Team Composition
#### Team Members
-	Dhruv Agarwal
-	Abhishek Singh Pawar
-	Abhishek Jain
-	Shravan Kumar
-	Sriya Boyapally

#### Weekly meeting time and format
Monday at 9 AM EST
