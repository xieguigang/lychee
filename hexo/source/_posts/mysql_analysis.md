---
title: MySql data analysis notes
tags: [mysql, BI, data analysis]
date: 2016-8-7
---

本文随时笔记随时更新

## Statics by day

```sql
SELECT date_format(time, "%Y-%m-%d"), COUNT(*) 
FROM zika_news.news_entry 
GROUP BY date_format(time, "%Y-%m-%d");
```

Where ``time`` is the field in your data table, which its data type is ``DateTime``. ``%Y-%m-%d`` means **Year-Month-Day**.

###### View Output
|date_format(time, ""%Y-%m-%d"")|count(*)|
|-------------------------------|--------|
|2016-08-05|352|
|2016-08-06|179|
|2016-08-08|61|
