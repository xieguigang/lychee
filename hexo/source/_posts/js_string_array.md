---
title: Javascript 中的String对象、Array对象、Dictionary
tags: [javascript, String, Array, Dictionary]
date: 2016-9-2
---

## String对象

|属性或者方法 | 说明|
|-------------------------------------------|-------------------------------------------------------------------|
|``length``属性  |获取字符串的字符个数。（无论中文字符还是英文字符都算1个字符。）|
|``charAt(index)``方法|获取指定索引位置的字符。（索引从0开始）|
|``indexOf(‘字符串’,startIndex)``方法 |获取指定字符串第一次出现的位置。``startIndex``表示从第几个开始搜索。|
|``split(‘分隔符’,limit);``  |根据分隔符将一个字符串返回为一个数组。``limit``表示要返回的数组的最大长度（可自定义）。|
|``substr(startIndex,len)`` |从``startIndex``开始，截取``len``个字符。|
|``substring(startIndex,stopIndex)``|从``startIndex``开始，截取到``stopIndex``位置（不包括``stopIndex``所在的字符）。|
|``match()``、``replace()``、``search()``方法|正则表达式相关。|

<!--more-->

例子：

```javascript
var uname = new String('a我不h是美女haha10086');

/*
alert(uname.length);
alert(uname.charAt(6)); //返回a，不是h,索引从0开始。
alert(uname.indexOf('h', 3)); //从索引为3的字符开始搜索h,如果在索引为3的字符前有h,也不予理会。
*/

/*
var reg = /\w+/g;
//当采用全局模式g,的时候直接把所有匹配的元素返回到一个数组当中
var result = uname.match(/\w+/g);

alert(result.length);

for (var i = 0; i < result.length; i++) {
	alert(result[i]);
}
*/

/*
var reg = /^\w$/;
var reg1 = new RegExp('\\w');
*/

/*
//不要忽略这里的全局模式g,否则只能匹配第一个符合的元素
//加上g以后，可以吧所有匹配正则表达式的元素都替换
var result = uname.replace(/a/g, 'X');
alert(result);
*/

//search()方法与indexOf()方法都是返回找到字符串的第一次出现的索引，
//只是search()方法可以用正则表达式，而indexOf()只能写字符串。
var result = uname.search(/\d{5}/);
var resultIndexof = uname.indexOf('10086');

alert(result);
alert(resultIndexof);

/*
//spilt第一个参数是分隔符，第二个参数是要返回的最大元素个数。
var result = uname.split(',', 3);

alert(result.length);

for (var i = 0; i < result.length; i++) {
	alert(result[i]);
}
*/

/*
//substr从索引为2的字符开始，截取3个字符，索引是从零开始的。
var result1 = uname.substr(2, 3);
alert(result1);
*/

//substring从第2个索引开始截取，截取到第3个索引之前。（不包括第3个索引的字符）
var result2 = uname.substring(2, 3);
alert(result2);
```

## Array对象

JavaScript中的``Array``对象就是数组，首先是一个动态数组，而且是一个像C#中数组、``ArrayList``、``Hashtable``等的超强综合体。
数组的使用方式：

```javascript
var names = new Array();//无需初始化长度，动态

names[0] = "tom";
names[1] = "jerry";
names[2] = "lily";
```

循环遍历数组（for循环）：

```javascript
for (var i = 0; i < names.length; i++) {
	alert(names[i]);
}
```

循环遍历数组（for in循环）：类似于c#中的``foreach``

```javascript
for (var i in names) {
	alert(names[i]); //如果直接输出i是什么？
}
```
使用for in循环可以遍历对象的所有属性。
for in循环其实遍历的还是key。
数组的其他几种声明方式：

```javascript
new Array(); 
new Array(size); 
new Array(element0, element0, ..., elementn);

var arr = ['China', 2008, true, 'Hello'];

arr[0]?'China'
arr[1]?2008
```

## JS中的Dictionary
JS中的Array是一个宝贝，不仅是一个数组，还是一个``Dictionary``，还是一个``Stack``（栈集合，不能是``Dictionary``）。

```javascript
var pinyins = new Array();

pinyins["人"] = "ren";
pinyins["口"] = "kou";
pinyins["手"] = "shou";

alert(pinyins["人"]);
alert(pinyins.人);
```

字典风格的简化创建方式：

```javascript
var arr = {“人”:”ren”,“口”:”kou”}; //json格式。
```

像``Hashtable``、``Dictionary``那样用，而且像它们一样效率高。
不能直接用for循环遍历，需要用for in循环。
只要有了``Array``你就同时拥有了数组、``List``、``Hashtable``，它就是JavaScript中的要你命三千，价格只要998.

```javascript
var arr = new Array();

arr[0] = "tom";
arr[1] = "jim";

for (var i in arr) { //打印出来的是0tom、1jim证明了数组用法是Dictionary用法的一个特例而已。
	alert(i+arr[i]);
}
```

### Array的简化声明

JSON格式：

```javascript
var arr={“name”:”tom”,”age”:18,”email”:’tom@itcast.cn’};
```

Json格式可以当做一个对象。

```javascript
arr.name、arr.age、arr.email
var arr=[{“name”:”steve”,”age”:18},{“name”:”steve”,”age”:18];//json对象数组。
```

在Ajax中使用JSON格式传输数据非常方便。

JSON的一些其他写法：

1. 

```javascript
var personalInfo = { "name": "周杰伦", "age": 20, "address": { "city": "beijing", "country": "China"} };

alert(personalInfo.name);
alert(personalInfo.age);
alert(personalInfo.address.city);
alert(personalInfo.address.country);
```

2.

```javascript
var students = [{ "id": "1001", "name": "james" }, { "id": "1002", "name": "bob"}];

for (var i = 0; i < students.length; i++) {
	alert(students[i].id + "===>" + students[i].name);
}
```

Array的其他方法：

```javascript
Array.join(“连接字符串”)//将数组中的元素通过指定的“连接字符串”连接起来，返回一个字符串。
Array.reverse();//将数组中的元素反转。
Array.sort();//排序
```

练习
====================
1.求一个整数组中的最大值。定义成函数。
2.将一个字符串数组输出为|分割的形式，比如“梅西|卡卡|郑大世”。不要使用JavaScript中的Join函数。arr1.join(“|”)将数组用分隔符连接成一个字符串。//自定义一个myJoin函数。
3. 将一个字符串数组的元素的顺序进行反转。``{"3","a","8","haha"} {"haha","8","a","3"}``。不要使用JavaScript中的反转函数。提示：第i个和第length-i-1个进行交换。定义成函数。 myreverse数组同样是传递引用，js出错很麻烦。交换两个变量。（测试一下是否会交换）(作业)


===========练习1====================
```javascript
var num = [1, 2, 344, 5, 6, 7];
var res = GetMax(num);
alert(res);

function GetMax(nums) {
	var maxVal = nums[0];
	
	for (var i = 0; i < nums.length; i++) {
		if (nums[i] > maxVal) {
			maxVal = nums[i];
		}
	}
	return maxVal;
}
```

=============练习2==================

```javascript
var strs = ['梅西', '卡卡', '郑大世'];
//var str = strs.join('|');
var str = myJoin('|', strs);

alert(str);

function myJoin(sep, strArray) {
	var result = '';
	
	for (var i = 0; i < strArray.length - 1; i++) {
		result += strArray[i] + sep;
	}
	result += strArray[strArray.length - 1];
	return result;
}
```

===========练习3=====================
```javascript
var strArr = ["3", "a", "8", "haha"];
//strArr.reverse();
myReverse(strArr);

for (var i = 0; i < strArr.length; i++) {
	document.write(strArr[i] + '<br/>');
}

function myReverse(strArray) {
	for (var i = 0; i < strArray.length / 2; i++) {
		var tmp = strArray[i];
		
		strArray[i] = strArray[strArray.length - 1 - i];
		strArray[strArray.length - 1 - i] = tmp;
	}
}
```

> ![js中的String对象、Array对象、Dictionary](https://raw.githubusercontent.com/xieguigang/xieguigang.github.io-hexo/master/images/qrcode/js_string_array.png)