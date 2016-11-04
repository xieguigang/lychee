# Hexo-Theme-Primer

![image](http://o7k7yxkn2.bkt.clouddn.com/2016-08-24_Primer.png)

## 感谢原作者

本主题是基于jekyll的一款主题移植过来的，首先在这里感谢原作者:[overtrue](https://github.com/overtrue/overtrue.github.io)

## 为什么叫Primer?

使用了Github的`primer`，主题弥漫着一股Git风，由于个人是个强迫症患者，喜欢简洁 去掉了 `搜索` `分类` 只保留了tag

## 安装
`git clone https://github.com/yumemor/hexo-theme-primer.git`

放在你的Hexo／theme下面

修改`_config.yml`:

```
theme: primer
```

## 开源项目页面

![开源项目](http://oct8d1mqf.bkt.clouddn.com/2016-09-27-30AD2169-8D9E-4181-9B5F-73337B1C1120.png)

```shell
hexo new page 'open-source'
```

```md
title: open-source
layout: open
---

```

## 分类页面

![分类页面](http://oct8d1mqf.bkt.clouddn.com/2016-09-27-21%3A25%3A04.jpg)

```shell
hexo new page 'blog'
```

```md
title: 文章
isShowDate: false
layout: category
---
```

## 导航

```yml
menu:
	Home: /
	Life: 
		href: subentry/Life/
		target: true
	Open-Source: open-source/
	Blog: blog/
	Guestbook: guestbook/
```
target代表新开启一个页面进行打开

## 个人信息

```yml
profile:
	location: ChengDu, China
	github: yumemor
	#stackoverflow: 
		#title: yumemor
		#href: http://stackoverflow.com/users/5662132/yumemor
	#organization: 组织/公司
```
## 布局

```yml
sidebar: true
navfixed: true
```

* 开启右侧菜单栏
* 开启导航fixed布局

## 配置多说
```yml
comments:
	duoshuo_username: 你的账号
```
> 注意⚠️ 如果用disqus评论请 注释掉多说
Install
```html
#comments:
	#doshuo_username: 你的账号
```

## 配置Disuaz
找到primer/_widget/disqus-comments.ejs

```js

<div class="comments">
    <div id="disqus_thread"></div>

    <script>
    /**
    * RECOMMENDED CONFIGURATION VARIABLES: EDIT AND UNCOMMENT THE SECTION BELOW TO INSERT DYNAMIC VALUES FROM YOUR PLATFORM OR CMS.
    * LEARN WHY DEFINING THESE VARIABLES IS IMPORTANT: https://disqus.com/admin/universalcode/#configuration-variables
    */
    /*
    var disqus_config = function () {
    this.page.url = PAGE_URL; // Replace PAGE_URL with your page's canonical URL variable
    this.page.identifier = PAGE_IDENTIFIER; // Replace PAGE_IDENTIFIER with your page's unique identifier variable
    };
    */
    (function() { // DON'T EDIT BELOW THIS LINE
    var d = document, s = d.createElement('script');

    s.src = '//yumemor.disqus.com/embed.js';

    s.setAttribute('data-timestamp', +new Date());
    (d.head || d.body).appendChild(s);
    })();
    </script>
    <noscript>Please enable JavaScript to view the <a href="https://disqus.com/?ref_noscript" rel="nofollow">comments powered by Disqus.</a></noscript>
</div>
```

修改script标签和noscript的标签为`disqus`提供的代码，点击生成代码：[disqus](https://disqus.com/)

## Git项目
主要是在博客首页显示 以及开源项目页面使用。

```yml
github:
  popular_repos: ['cordova-plugin-alipay','hexo-theme-primer']
  contribute_repos: ['yumemor/1','yumemor/2']
```
> 注意⚠️ 配置git项目先检查`profile->github` 有无配置 这是前置条件。


欢迎大家 fork push。
