---
title: MySql data analysis notes
tags: [mysql, BI, data analysis, 数据视图, vb.net]
date: 2016-8-7
---

本文随时笔记随时更新

### Statics by day

```sql
SELECT date_format(time, "%Y-%m-%d"), COUNT(*)
FROM zika_news.news_entry
GROUP BY date_format(time, "%Y-%m-%d");
```

Where ``time`` is the field in your data table, which its data type is ``DateTime``. ``%Y-%m-%d`` means **Year-Month-Day**.

###### View Output
|date_format(time, "%Y-%m-%d")|count(*)|
|-----------------------------|--------|
|2016-08-05|352|
|2016-08-06|179|
|2016-08-08|61|

###### Server Code

```vbnet
Dim dt As DataTable = mysql.Fetch(SQL).Tables(0)
Dim data As date_count() <=

    From row As DataRow
    In dt.Rows
    Select dd = New date_count With {
        .date = CStr(row(MySqlREST.[date])),
        .count = CInt(row(MySqlREST.count))
    }
    Order By dd.date Ascending

Call response.WriteJSON(data)
```

###### JSON output Example

```json
[
  {
    "count": 84,
    "date": "2016-07-08"
  },
  {
    "count": 141,
    "date": "2016-07-09"
  },
  {
    "count": 33,
    "date": "2016-07-10"
  },
  {
    "count": 41,
    "date": "2016-07-11"
  }
]
```

<!--more-->
### Multiple fileds Grouping

```sql
SELECT date_format(time, "%Y-%m-%d"), COUNT(`key`), `key`
FROM zika_news.news_entry
GROUP BY date_format(time, "%Y-%m-%d"),`key`;
```

###### View Output
|date_format(time, "%Y-%m-%d")|COUNT(`key`)|key|
|-----------------------------|------------|---|
|2016-07-18|46|1|
|2016-07-18|1|2|
|2016-07-18|1|3|
|2016-07-18|9|5|
|2016-07-19|127|1|
|2016-07-19|4|2|
|2016-07-19|2|3|
|2016-07-19|2|4|
|2016-07-19|4|5|
|2016-07-20|135|1|
|2016-07-20|1|2|
|2016-07-20|4|3|
|2016-07-20|2|4|
|2016-07-20|3|5|

###### Server Code

```vbnet
Dim dt As DataTable = mysql.Fetch(VirusDateCount).Tables(0)
Dim data = From row As DataRow
           In dt.Rows
           Select key = CLng(row("key")),
               x = New date_count With {
                   .date = CStr(row(MySqlREST.[date])),
                   .count = CInt(row("COUNT(`key`)"))
               }
           Group By key Into Group

Dim out As New List(Of virus)

For Each x In data
    out += New virus With {
        .name = __keys(x.key).key,
        .data = x.Group.ToArray(Function(o) o.x)
    }
Next

Call response.WriteJSON(out)
```

###### JSON output example

```json
[
  {
    "data": [
      {
        "count": 84,
        "date": "2016-07-08"
      },
      {
        "count": 141,
        "date": "2016-07-09"
      }
    ],
    "name": "zika"
  }
  {
    "data": [

      {
        "count": 2,
        "date": "2016-08-15"
      },
      {
        "count": 1,
        "date": "2016-08-16"
      }
    ],
    "name": "Ebola"
  },
  {
    "data": [
      {
        "count": 4,
        "date": "2016-08-10"
      },
      {
        "count": 2,
        "date": "2016-08-16"
      }
    ],
    "name": "Influenza"
  }
]
```