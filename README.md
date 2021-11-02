# IS7024001Project
# KnowYourFavoriteNobelLaureate

---

Design Document  

## Introduction 

Do you want to know about your favorite nobel laureate?  Do you know about their research work? KnowYourFavoriteNobelLaureate can ease your curiosity:  

-	Search your favorite nobel laureates by countries.
-	Know about their research work.
-	Know the subjects in which they won prizes.
-	Know their affiliations.

## Requirements  
-	As a user, I want select select country so that I can gather information about nobel laureate based on the country I selected.
-	As a user I am able to play a quiz on nobel prizes.

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
*Given* No nobel prize won in the particular country
*When* I search with a country who has no nobel laureates
*Then* I should receive zero results (an empty list)

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
Tuesday at 6 PM EST
