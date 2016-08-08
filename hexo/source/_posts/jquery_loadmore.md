---
title: Loading More Results From Database Using jQuery Ajax
tags: [javascript, vb.net, jquery]
date: 2016-8-9
---

You might have seen the implementation of ``Ajax`` based data loading system at Facebook, Twitter or some other sites. Instead of showing pagination links, we can load data dynamically whenever a load button click. In this article I’ll show you how we can easily adopt this technique to make similar loading page for our web projects using ``jQuery``.

![jQuery Load More Data](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/jquery-load-more-data.jpg)

> You may also want to checkout similar tutorial [Auto Load More Data On Page Scroll](https://www.sanwebe.com/2013/05/auto-load-records-on-page-scroll). In my earlier post [Simple Ajax Pagination](https://www.sanwebe.com/2013/02/ajax-pagination-jquery-php-simple), I have explained how to create Ajax Pagination using jQuery. Here we will be doing precisely the same thing.

![load more](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/ajax-loader.gif)

<!--more-->

### Configuration
We use this configuration file to store certain variables such as MySql username and password, which is required by our program..

```vbnet
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Oracle.LinuxCompatibility.MySQL
Imports SMRUCC.HTTPInternal.AppEngine.APIMethods
Imports SMRUCC.HTTPInternal.AppEngine.POSTParser
Imports SMRUCC.HTTPInternal.Platform

<[Namespace]("mysql")>
Public Class MySqlREST : Inherits SMRUCC.HTTPInternal.AppEngine.WebApp

    ReadOnly mysql As New MySQL

    Public Sub New(main As PlatformEngine)
        MyBase.New(main)

        If mysql <= New ConnectionUri With {
            .Database = "news_db",
            .IPAddress = "localhost",
            .Password = "1234",
            .ServicesPort = 3306,
            .User = "root"
        } = -1.0R Then
            Throw New Exception("No mysql connection!")
        End If
    End Sub

    ' ......
End Class
```

### Index Page

Here’s our HTML that contains a load button, a loading gif image and place to dump the results from the server. Loading image is hidden initially, when the load button is clicked, we show the loading image that indicates loading data.

```html
<html>
<head>
   <script src="http://www.w3school.com.cn/jquery/jquery-1.11.1.min.js"></script>
</head>
<body>
<div class="wrapper">
   <ul id="results"><!-- results appear here --></ul>
   <div align="center">

   <!-- load button -->
   <button id="load_more_button">
      <img src="ajax-loader.gif" width="16px" height="16px" class="animation_image" style="float:left;"> Load More
   </button>
   </div>
</div>

<script>
   // Javascript jquery load more script
</script>

</body>
</html>
```

### jQuery

There are various ways to do this, but here you can see I’ve added an increment to the button click, which we will use as page number to determine the records position in MySql query.

```javascript
var track_page = 1;        // track user click as page number, right now page number is 1
load_contents(track_page); // load content

// user clicks on button
$("#load_more_button").click(function (e) { 
   
   // page number increment everytime user clicks load button
   track_page++; 
   load_contents(track_page);    //load content
});

// Ajax load function
function load_contents(track_page){

   $('.animation_image').show(); //show loading image
    
   $.post( 'http://127.0.0.1/mysql/more.vb', {page: track_page, test_time: "2pm"}, 
   function(data){
        
      if(data.trim().length == 0){
         // display text and disable load button if nothing to load
         $("#load_more_button").text("No more records!").prop("disabled", true);
      }
      
      // append data into #results element	  
      $("#results").append(data); 
        
      // scroll page to button element
      $("html, body").animate({scrollTop: $("#load_more_button").offset().top}, 800);
    
      // hide loading image
      $('.animation_image').hide(); // hide loading image once data is received
   });
}
```

### Handle post on server

```vbnet
<POST(GetType(String))>
<ExportAPI("/mysql/more.vb")>
Public Function More(args As String, params As PostReader) As String
    Dim json As String = params.Form.ToDictionary.GetJson
    Return $"<li>{json}</li>"
End Function
```

And then running the test, yes, it success!

![httpd server console output for this test jquery post load more](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/httpd_console_on_post.png)
![Google Chrome output](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/chrome_jquery_loadmore.png)

### Congratulations!
That’s it! we should have a nifty ``jQuery`` based data loading page by now. Don’t forget to checkout the demo and download the whole thing in your computer, downloadable file includes loading image and jQuery file, required by the script. Good luck! If you want to make ``Ajax`` Pagination with Next-Previous buttons, you can checkout this updated article — [Ajax Pagination with jQuery &amp; PHP](https://www.sanwebe.com/2013/03/ajax-pagination-with-jquery-php).

> ![Loading More Results From Database Using jQuery Ajax](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/qrcode/jquery_loadmore.png)