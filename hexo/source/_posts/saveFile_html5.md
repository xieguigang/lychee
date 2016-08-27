---
title: Save file in HTML5
tags: [html5, javascript]
date: 2016-8-28
---

> http://stackoverflow.com/questions/2897619/using-html5-javascript-to-generate-and-save-a-file

Simple solution for HTML5 ready browsers...

```javascript
function download(filename, text) {
    var pom = document.createElement('a');
    pom.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
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

Usage

```javascript
download('test.txt', 'Hello world!');
```

