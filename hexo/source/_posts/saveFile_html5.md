---
title: Save file in HTML5
tags: [html5, javascript, cytoscape.js]
date: 2016-8-28
---

> http://stackoverflow.com/questions/2897619/using-html5-javascript-to-generate-and-save-a-file

Simple solution for HTML5 ready browsers...

```javascript
function download(filename, text) {

    var pom = document.createElement('a');
    var contentType = 'data:text/plain;charset=utf-8,';

    pom.setAttribute('href', contentType + encodeURIComponent(text));
    pom.setAttribute('download', filename);

    if (document.createEvent) {
        var event = document.createEvent('MouseEvents');
        event.initEvent('click', true, true);
        pom.dispatchEvent(event);
    }
    else {
        pom.click();
    }
}
```

Usage:

```javascript
download('test.txt', 'Hello world!');
```
<!--more-->
Here is an example of export ``cytoscape.js`` svg canvas as PNG and download in browser:

```javascript
function download(data, filename) {
    var pom = document.createElement('a');
    pom.setAttribute('href', data);
    pom.setAttribute('download', filename);

    if (document.createEvent) {
        var event = document.createEvent('MouseEvents');
        event.initEvent('click', true, true);
        pom.dispatchEvent(event);
    }
    else {
        pom.click();
    }
}

var w = document.getElementById('png-width').value;
var h = document.getElementById('png-height').value;
var args = {
	bg: bg,
	maxWidth: parseInt(w),
	maxHeight: parseInt(h)
}
var png64 = cy.png(args);

// console.error(args);

download(png64, "networkImage_download.png");
```