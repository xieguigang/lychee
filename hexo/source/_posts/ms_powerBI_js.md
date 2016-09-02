---
title: 微软发布全新Power BI JavaScript API：智能双向交互
tags: [javascript, BI, github]
time: 2016-9-3
---

微软发布了全新Power BI JavaScript API，能提供在Power BI报告和你的应用之间的双向联系。它可以让你轻松地嵌入到你的应用程序中，并且以编程方式与报告进行交互，从而实现应用和报告集成度更高。你的应用可以正确的语境为你的用户打开报告，就算报告已经打开，也可以改变语境内容。例如您可以向一个特定的客户打开一个报表，用户与应用的其他部分进行交互后，用户信息将自动更新。

![Power BI - Sample - Client - Javascript]()

<!--more-->

### Include the library via import or manually

Ideally you would use module loader or compilation step to import using ES6 modules as:

```javascript
import * as pbi from 'powerbi-client';
```

However, the library is exported as a Universal Module and the powerbi.js script can be included before your apps closing ``</body>`` tag as:

```html
<script src="/bower_components/powerbi-client/dist/powerbi.js"></script>
```

When included directly the library is exposd as a global named ``powerbi-client``. There is also another global powerbi which is an instance of the service.

Power BI reports can now be embedded into your own applications. There are two different authentication methods for embedding depending on which Power BI Service that you use. Both use access tokens, but when using Power BI Embedded the tokens are issued by your own service and are specific to a report, while the tokens used for PowerBI.com are issued by Azure Active Directory (AAD) and are specific to a user.

#### Power BI Embedded (Azure)

When using Power BI Embedded, the tokens issued are for a specific report, and the token should be associated with the embed URL on the same element to ensure each has a unique token. This allows embedding multiple reports using the same service instance.

Provide an embed configuration using attributes:

```html
<div
    powerbi-type="report"
    powerbi-access-token="eyJ0eXAiO...Qron7qYpY9MI"
    powerbi-report-id="5dac7a4a-4452-46b3-99f6-a25915e0fe55"
    powerbi-embed-url="https://embedded.powerbi.com/appTokenReportEmbed"
></div>
```

Embed using javascript:

```html
<div id="reportContainer"></div>
```

```javascript
var embedConfiguration = {
    type: 'report',
    accessToken: 'eyJ0eXAiO...Qron7qYpY9MI',
    id: '5dac7a4a-4452-46b3-99f6-a25915e0fe55',
    embedUrl: 'https://embedded.powerbi.com/appTokenReportEmbed'
};
var $reportContainer = $('#reportContainer');
var report = powerbi.embed($reportContainer.get(0), embedConfiguration);
```

> Notice how the attributes and embed configuration hold the same data, just provided to the service in different ways.

#### PowerBI.com

When using PowerBI.com the tokens issued are for a specific user who can view many different reports. This means you can add this as a global token reused for all embedded visuals as shown below.

It is not required, but you can assign a global access token on an instance of the Power BI service which will be used as a fallback if a local token isn't provided.

```html
<script>
    powerbi.accessToken = '{{AccessToken}}';
</script>
```

Provide embed configuration using attributes (notice that the access token does not need to be supplied because it will fall back to using the global token):

```html
<div
    powerbi-type="report"
    powerbi-report-id="5dac7a4a-4452-46b3-99f6-a25915e0fe55"
    powerbi-embed-url="https://app.powerbi.com/reportEmbed"
></div>
```

Embed using javascript:

```javascript
var embedConfiguration = {
    type: 'report',
    id: '5dac7a4a-4452-46b3-99f6-a25915e0fe55',
    embedUrl: 'https://app.powerbi.com/reportEmbed'
};
var $reportContainer = $('#reportContainer');
var report = powerbi.embed($reportContainer.get(0), embedConfiguration);
```

> Note: You can still choose to supply a AAD access token (instead of one issued from your own service) in the embed configuration. This allows you to have apps that embed reports using both authentication methods.
>
> Also, notice how the embed experience across both services is nearly identical except for how you specify the access token.

#### Set the size of embedded components

The report will automatically be embedded based on the size of its container. To override the default size of the embeds simply add a CSS class attribute or inline styles for width & height.

#### Launch an embedded report in fullscreen mode

The code below assumes element with id myReport already contains an embedded report:

```javascript
var element = document.getElementById('#myReport');
var report = powerbi.get(element);

report.fullscreen();
```