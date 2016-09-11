---
title: Explaining to laypeople why bootstrapping works
tags: [bootstrap, Monte Carlo]
date: 2016-9-11
---

You want to ask a question of a population but you can't. So you take a sample and ask the question of it instead. Now, how confident you should be that the sample answer is close to the population answer obviously depends on the structure of population. One way you might learn about this is to take samples from the population again and again, ask them the question, and see how variable the sample answers tended to be. Since this isn't possible you can either make some assumptions about the shape of the population, or you can use the information in the sample you actually have to learn about it.
<!--more-->
Imagine you decide to make assumptions, e.g. that it is Normal, or Bernoulli or some other convenient fiction. Following the previous strategy you could again learn about how much the answer to your question when asked of a sample might vary depending on which particular sample you happened to get by repeatedly generating samples of the same size as the one you have and asking them the same question. That would be straightforward to the extent that you chose computationally convenient assumptions. (Indeed particularly convenient assumptions plus non-trivial math may allow you to bypass the sampling part altogether, but we will deliberately ignore that here.)

This seems like a good idea provided you are happy to make the assumptions. Imagine you are not. An alternative is to take the sample you have and sample from it instead. You can do this because the sample you have is also a population, just a very small discrete one; it looks like the histogram of your data. Sampling 'with replacement' is just a convenient way to treat the sample like it's a population and to sample from it in a way that reflects its shape.

This is a reasonable thing to do because not only is the sample you have the best, indeed the only information you have about what the population actually looks like, but also because most samples will, if they're randomly chosen, look quite like the population they came from. Consequently it is likely that yours does too.

For intuition it is important to think about how you could learn about variability by aggregating sampled information that is generated in various ways and on various assumptions. Completely ignoring the possibility of closed form mathematical solutions is important to get clear about this.

> http://stats.stackexchange.com/questions/26088/explaining-to-laypeople-why-bootstrapping-works

蒙特卡洛方法是用抽样分布来近似随机变量的分布，以样本的数字特征来近似随机变量的数字特征。例如，用样本均值近似随机变量的期望，用样本方差近似随机变量的方差，等等。

bootstrap是一种非参数估计方法，它用到蒙特卡洛方法。bootstrap算法如下：
假设样本容量为N
1. 有放回的从样本中随机抽取N次(所以可能x1..xn中有的值会被抽取多次)，每次抽取一个元素。并将抽到的元素放到集合S中；
2. 重复**步骤1** B次（例如B=100），得到B个集合，记作S1,S2,...,SB;
3. 对每个S<sub>i</sub> （i=1,2,...,B），用蒙特卡洛方法估计随机变量的数字特征d，分别记作d1,d2,...,dB;
4. 用d1,d2,...dB来近似d的分布；

本质上，bootstrap算法是最大似然估计的一种实现，它和最大似然估计相比的优点在于，它不需要用参数来刻画总体分布。

作者：xmqv
链接：https://www.zhihu.com/question/22929263/answer/81333591
来源：知乎
著作权归作者所有，转载请联系作者获得授权。

作者：书牛
链接：https://www.zhihu.com/question/22929263/answer/86314894
来源：知乎
著作权归作者所有，转载请联系作者获得授权。

（1）首先，Bootstrap并不是和Monte Carlo方法完全无关。
The bootstrap procedure, developed by Bradley Efron(1979), is a Monte Carlo method that involves resampling -- that is, taking repeated samples of size n (with replacement) from the original sample data set.

-- <Statistics for Engineering and the Science>. Mendenhall

（2）然后解释一下，bootstrap的简单使用方法。
使用bootstrap 估测总体样本参数θ的 bootstrap confidence interval. 
样本参数θ可以是均值，方差等等。
对于一个样本x1,x2,...xn. 其总体的概率分布中参数θ是未知的，想要利用有限的这个样本来得到一个估测值θ‘。

Step1:选择一个resampling的次数i，1000或2000或3000等（取决于自己的需要）；
Step2:然后每一次的重复计算程序是：从x1,x2,...xn中有放回的，抽取n次（所以可能x1..xn中有的值会被抽取多次）；
Step3:重复上述抽取程序，一共i次（step1中已经选定的i值），利用每次抽取后的结果都计算出一个θ‘值 （一共i个θ‘值）；
Step4：利用得到的θ‘1，θ‘2，θ‘3... ... θ‘i作为一个sample，计算其（α／2）100% 和（1-α）100％的百分位值，得到的就是近似出的，估测总体参数θ的，置信区间的最低和最高点［ x, y].

作者：赵卿元
链接：https://www.zhihu.com/question/22929263/answer/31090959
来源：知乎
著作权归作者所有，转载请联系作者获得授权。

Monte Carlo是一个更基础的想法。在很多数学、物理或者工程问题种有很多无法写出closed form的表达式，为了能得到数值上的一个解，需要通过随机采样的方法去估计。

Bootstrap是重新改变统计学的一个想法。统计推断的主体总是一个的随机变量分布。在这个分布很复杂无法假设合理的参数模型时，bootstrap提供了一种非参数的推断方法，依靠的是对观测到的样本的重新抽样（resampling），其实是用empirical distribution去近似真正的distribution。

这两种方法从目的到用法都完全不同，有联系的话就是都涉及到计算机抽样。

==============================================================

@豆豆叶 觉得“bootstrap是对empirical distribution的monte carlo”的说法更合理，我保留意见。我认为monte carlo和sampling还是不能互为替换的。我认为Monte Carlo和Bootstrap更多的是两种思想，都是基于random sampling去近似某一目标。Monte Carlo的目标一般是一个难以计算的积分，bootstrap的目标一般是统计推断。

> https://www.zhihu.com/question/22929263