Feature: TwitterAppDemo
	 Run the twitter app and figure out the results based on the inputs given

	 		 
 Background:
  Given alteryx running at" http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"
  #And I publish the application "twitter tracker"
  #And I check if the application is "Valid"


Scenario Outline: publish and run Twitter App
When I run the App "<app>" with the "<description>" use a twitter handle to search "<Search term>"
Then I see the searched phase "<Search term>" has atleast has a tweet <Count>

Examples: 
| app             | description                                                   | Search term | Count |
| Twitter Tracker | "Find out what others are saying about your brand on Twitter" | alteryx       |   239 |
| Twitter Tracker | "Find out what others are saying about your brand on Twitter" |pitchinvasion       |   10 |



