# CVS_MinuteClinic_Crawler
A Two-Step process that crawls and scrapes CVS pharmacies data out of http://www.cvs.com/minuteclinic/clinic-locator

# Project Overview

There are two runnable processes on this solution:

* CVS_MinuteClinicCrawler: This will navigate through the site and fetch a list of all pharmacy urls to be parsed, and save it to a txt file as it's output

* CVS_MinuteClinicWaitTimeScraper: This process uses the output of the first one (to speed things up, since it won't have to navigate through the site to find the pharmacies everytime it runs), and once it reaches each pharmacy page it parses data out of it, and uses CVS's API endpoint to compliment the missing data.

# How to run it ?

* Change variables on the App.config files

* Run the CVS_MinuteClinicCrawler code first

* Run the CVS_MinuteClinicWaitTimeScraper, making sure to point it's config to the output file of the first project

# About me
My name is Marcello Lins, i am a 25 y/o developer from Brazil who works with BigData and DataMining techniques at the moment.

http://about.me/marcellolins