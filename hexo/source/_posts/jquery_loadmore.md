---
title: Loading More Results From Database Using jQuery Ajax
tags: [javascript, vb.net, jquery]
date: 2016-8-9
---

You might have seen the implementation of Ajax based data loading system at Facebook, Twitter or some other sites. Instead of showing pagination links, we can load data dynamically whenever a load button click. In this article I’ll show you how we can easily adopt this technique to make similar loading page for our web projects using jQuery.

jQuery Load More Data
You may also want to checkout similar tutorial Auto Load More Data On Page Scroll. In my earlier post Simple Ajax Pagination, I have explained how to create Ajax Pagination using jQuery. Here we will be doing precisely the same thing.
Configuration
We use this configuration file to store certain variables such as MySql username and password, which is required by our program..

PHP
1
2
3
4
5
6
7
8
9
10
11
12
13
<?php
$db_username = 'root';
$db_password = '';
$db_name = 'demo';
$db_host = 'localhost';
$item_per_page = 5;

//Try to connect to database or display error
$mysqli = new mysqli($db_host, $db_username, $db_password, $db_name);
if ($mysqli->connect_error) { //Output any connection error
    die('Error : ('. $mysqli->connect_errno .') '. $mysqli->connect_error);
}
?>
Index Page
Here’s our HTML that contains a load button, a loading gif image and place to dump the results from the server. Loading image is hidden initially, when the load button is clicked, we show the loading image that indicates loading data.

HTML
1
2
3
4
5
6
<div class="wrapper">
    <ul id="results"><!-- results appear here --></ul>
    <div align="center">
        <button id="load_more_button"><img src="ajax-loader.gif"  class="animation_image" style="float:left;"> Load More</button> <!-- load button -->
    </div>
</div>
jQuery

There are various ways to do this, but here you can see I’ve added an increment to the button click, which we will use as page number to determine the records position in MySql query.

JQUERY
1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
var track_page = 1; //track user click as page number, right now page number is 1
load_contents(track_page); //load content

$("#load_more_button").click(function (e) { //user clicks on button
    track_page++; //page number increment everytime user clicks load button
    load_contents(track_page); //load content
});

//Ajax load function
function load_contents(track_page){
    $('.animation_image').show(); //show loading image
    
    $.post( 'fetch_pages.php', {'page': track_page}, function(data){
        
        if(data.trim().length == 0){
            //display text and disable load button if nothing to load
            $("#load_more_button").text("No more records!").prop("disabled", true);
        }
        
        $("#results").append(data); //append data into #results element
        
        //scroll page to button element
        $("html, body").animate({scrollTop: $("#load_more_button").offset().top}, 800);
    
        //hide loading image
        $('.animation_image').hide(); //hide loading image once data is received
    });
}
Fetching Pages
As I discussed earlier, we need to pass the page number to the PHP script below, it then will connect to database and fetch the records using page number and “item_per_page” offset. The PHP code uses MySqli Prepared Statements to fetch the records, which is very useful against SQL injections and the performance is highly efficient. You can learn more about MySqli Prepared Statements here.

PHP
1
2
3
4
5
6
7
8
9
10
11
12
13
14
15
16
17
18
19
20
21
22
23
24
25
26
27
28
<?php
include("config.inc.php"); //include config file
//sanitize post value
$page_number = filter_var($_POST["page"], FILTER_SANITIZE_NUMBER_INT, FILTER_FLAG_STRIP_HIGH);

//throw HTTP error if page number is not valid
if(!is_numeric($page_number)){
    header('HTTP/1.1 500 Invalid page number!');
    exit();
}

//get current starting point of records
$position = (($page_number-1) * $item_per_page);

//fetch records using page position and item per page. 
$results = $mysqli->prepare("SELECT id, name, message FROM paginate ORDER BY id DESC LIMIT ?, ?");

//bind parameters for markers, where (s = string, i = integer, d = double,  b = blob)
//for more info https://www.sanwebe.com/2013/03/basic-php-mysqli-usage
$results->bind_param("dd", $position, $item_per_page); 
$results->execute(); //Execute prepared Query
$results->bind_result($id, $name, $message); //bind variables to prepared statement

//output results from database

while($results->fetch()){ //fetch values
    echo '<li>'.$id.') <strong>'.$name.'</strong> : '.$message.'</li>'; 
}
Conclusion
That’s it! we should have a nifty jQuery based data loading page by now. Don’t forget to checkout the demo and download the whole thing in your computer, downloadable file includes loading image and jQuery file, required by the script. Good luck! If you want to make Ajax Pagination with Next-Previous buttons, you can checkout this updated article — Ajax Pagination with jQuery & PHP.