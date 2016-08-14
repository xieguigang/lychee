---
title: D3.js制作日期折线图
tags: [javascript, d3js]
date: 2016-8-13
---

### 一、引入文件

```html
<script src="http://d3js.org/d3.v3.min.js"></script>
```

### 二、创建容器

```javascript
var margin = {top: 20, right: 20, bottom: 30, left: 50},
    width = 590 - margin.left - margin.right,
    height = 300 - margin.top - margin.bottom;

var svg = d3.select("body").append("svg")
    .attr("width", width + margin.left + margin.right)
    .attr("height", height + margin.top + margin.bottom)
    .append("g")
    .attr("transform", "translate(" + margin.left + "," + margin.top + ")");
```

需要注意的几个尺寸：svg画布的尺寸、chart图片的尺寸、margin值，另外svg的坐标原点是左上角，向右为正，向下为正。这对之后的各种元素的``transition``很重要。

可以参考一下这篇文章：[Margin Convention](http://bl.ocks.org/mbostock/3019563)
<!--more-->

### 三、定义X轴与Y轴：

```javascript
var parseDate = d3.time.format("%Y-%m-%d").parse;
var x = d3.time.scale()
    .range([0, width]);

var y = d3.scale.linear()
    .range([height, 0]);

var xAxis = d3.svg.axis()
    .scale(x)
    .orient("bottom")
    .tickFormat(d3.time.format("%m.%d"));

var yAxis = d3.svg.axis()
    .scale(y)
    .orient("left");
```

1. ``parseDate``是个函数，类似于公式。 ``d3.time.format("%Y-%m-%d").parse``的作用是将``%Y-%m-%d``格式的字符串转成真正的日期对象（Date）。
2. 规定xy轴的比例尺：``d3.time.scale()``表示时间比例尺，``d3.scale.linear()``表示线性比例尺，domain(输入)和range(输出)分别定义比例尺的定义域和值域。
3. 定义坐标轴：``d3.svg.axis()``生成坐标轴，.scale()表示坐标轴应用的比例尺，orient表示方位，``tickFormat(d3.time.format("%m.%d"))``表示刻度格式化，也就是输出日期格式为12.14的刻度；``axis().ticks()``可以用来设置刻度的数量。

### 四、读取文件：

```javascript
d3.tsv("data.tsv", function(error, data) {
    if (error) throw error;

    data.forEach(function(d) {
        d.date = parseDate(d.date);
        d.close = +d.close;
    });

    //在这里对读取的数据进行绘图处理
});
```

1. ``d3.tsv()``用来读取文件。forEach()来遍历数据的每项内容。
2. 用之前定义好的日期函数parseDate对数据进行格式化，把日期转成真正的日期对象。
3. ``+``用来将字符串的数值转化成真正的Number数值。

接下来的内容都需要在此读取函数内部进行处理。

### 五、定义X轴和Y轴的输入域：

```javascript
x.domain(d3.extent(data, function(d) { return d.date; }));
y.domain(d3.extent(data, function(d) { return d.close; }));
```

1. 通常情况下.domain()和.range()都是链式操作一起写的，当然也可以像我们一样先设置.range()再来设置.domain()；
2. d3.extent() 返回给定数组自然排序的最小值和最大值。data指定要处理的数据，d表示data的每一项，然后extent返回data每一项date的最大值和最小值。

### 六、绘制坐标轴：

```javascript
var line = d3.svg.line()
    .x(function(d) { return x(d.date); })
    .y(function(d) { return y(d.close); });

svg.append("g")
    .attr("class", "x axis")
    .attr("transform", "translate(0," + height + ")")
    .call(xAxis);

svg.append("g")
    .attr("class", "y axis")
    .call(yAxis)
    .append("text")
    .attr("transform", "rotate(0)")
    .attr("y", 25)
    .attr("x", 75)
    .attr("dy", ".71em")
    .style("text-anchor", "end")
    .text("我的数据");
```

1. 定义线条：``d3.svg.line()``的功能也类似于一个公式，传入对象后返回对象的x坐标和y坐标。
2. ``.call(xAxis)``把选择的数据应用到定义的坐标轴。

### 七、绘制网格线：

```javascript
//定义纵轴网格线
var yInner = d3.svg.axis()
    .scale(y)
    .tickSize(-width,0,0)
    .tickFormat("")
    .orient("left")
    .ticks(5);

//添加纵轴网格线
var yInnerBar=svg.append("g")
    .attr("class", "inner_line")
    .attr("transform", "translate(0,-25)")
    .call(yInner);
```

``.tickSize()``设定了刻度线的长度和位置。

### 八、绘制折线路径：

```javascript
svg.append("path")
   .datum(data)
   .attr("class", "line")
   .attr("d", line)
   .attr("opacity", 0)
   .transition()
   .duration(2000)
   .attr("opacity", 1);
```

### 九、添加数据散点：

```javascript
// 散点
var points = svg.selectAll(".MyCircle")
    .data(data)
    .enter()
    .append("circle")
    .attr("class","MyCircle")
    .attr("transform","translate(0,0)")
    .attr("r", 3)
    .attr("opacity", 0)
    .transition()
    .duration(2000)
    .attr("cx", function(d){ return x(d.date); })
    .attr("opacity", 1)
    .attr("cy", function(d){ return y(d.close); });
```

附1：测试数据 ``data.tsv``

```
date    close
2015-11-21    620.13
2015-11-22    605.23
2015-11-23    622.77
2015-11-24    626.2
2015-11-25    628.44
2015-11-26    636.23
2015-11-27    633.68
2015-11-28    624.31
2015-11-29    629.32
2015-11-30    618.63
2015-12-1    599.55
2015-12-2    609.86
2015-12-3    617.62
2015-12-4    614.48
2015-12-5    606.98
2015-12-6    596.05
2015-12-7    599.34
2015-12-8    602.5
2015-12-9    605.96
2015-12-10    601.1
2015-12-11    585.57
```

附2、图表样式：

```css
.axis path,
.axis line {
    fill: none;
    stroke: #F2E8DE;
    shape-rendering: crispEdges;
}
.line {
    fill: none;
    stroke: #FCAD62;
    stroke-width: 1.5px;
}
text{
    fill:#999;
}
.inner_line line {
    fill: none;
    stroke:#E7E7E7;
    shape-rendering: crispEdges;
}
.MyCircle {
    fill: #FCAD62;
}
```

![最终效果](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/719779-20151106183107617-140974873.png)

> ![D3.js制作日期折线图](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/qrcode/d3js_datelines.png)