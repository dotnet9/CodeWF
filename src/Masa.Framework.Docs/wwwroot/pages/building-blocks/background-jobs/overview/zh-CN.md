# BackgroundJobs（后台任务）- 概述

提供一个简单版本的后台任务，将耗时操作交由后台任务来执行有利于快速响应用户操作，更复杂的后台任务推荐使用 [MASA Scheduler](/stack/scheduler/introduce)

## 最佳实践

* [Memory](/framework/building-blocks/background-jobs/memory)：基于内存实现的后台任务
* [Hangfire](/framework/building-blocks/background-jobs/hangfire)：基于[Hangfire](https://www.hangfire.io/)实现的后台任务

## 能力介绍

| 最佳实践                                              | 一次性任务 | 周期性任务 |
|:--------------------------------------------------|:-----:|:-----:|
| [Memory](/framework/building-blocks/background-jobs/memory) |   ✅   |   ❌   |
| [Hangfire](/framework/building-blocks/background-jobs/hangfire) |   ✅   |   ✅   |