#BestStories.API

# Compilation Steps:
1. Open the project in Visual Studio
2. Check your .net Target framework and update as required.
3. Compile the solution, select profile BestStories.API and run the application
4. Launch swagger url - http://localhost:5000/swagger/index.html


#Implementation & Assumptions
1. External Uri and details are config driven.
1. Restrict the API to a Max ceiling to avoid hacker-news abuse.
2. Maintain a cache and refresh after 24hrs - only hit the story detail API for ids which are unavailable in the cache. 

#Scope of Improvement 
1. Decoupling of cache implementation from GetBestStories method
2. Correct Error code and logging as per the exception type
