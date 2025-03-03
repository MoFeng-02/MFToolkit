跳转到说明页面：[跳转](./docs/Index.md)  

Go to the description page:[skip](./docs/Index.md)

# MFToolkit - .Net9_OR_GREATER 版本工具库

## 1. 说明

1.1 介绍  
    1.1.1本库为.Net9_OR_GREATER（部分支持net8.0） 版本工具库，包含一些常用的工具类，如：  
        1.1.1 文件操作类  
        1.1.2 字符串操作类  
        1.1.3 日期时间操作类  
            ...  
1.1.2 关于和如何使用  
            本库大部分都可以进行DI注入然后使用，也提供了一些全局单例方便在非服务端的时候也可以很方便的调用，但是需要进行一些简单的初始化注册。  
            在服务器（API）中可以通过入口类的Builder进行基本注入，也可以通过本库提供的方法进行简单注入（或许你可以根据自身来选择，手动处理这些更方便你来管理）。  
            在App端（桌面/移动），也可以使用，因为提供了基本的App端注入方法，但是需要进行一些简单的初始化注册，提供了更加简洁的注入方式，你甚至可以直接使用了。  
            当然，也有一些特殊的，比如Download，Upload，Assets等等这些组件，是需要手动注入的，不过也提供了相应的注入函数，也可以很方便的注入使用。  
            
1.1.3 具体有哪些方面的？  
            目前已经拓展了一部分的，比如通用的MFToolkit，这个库是全部通用了，处于Src/Common文件夹下，在Common文件夹下还包括了一些基于MFToolkit的额外库，其中就包括了微信小程序/公众号等，也有支持全部这些额外库的集成类库，更加方便的让自己拓展各个小程序的功能。
            在App端，有拓展AvaloniaUI、MAUI两个UI库的，但是目前样式库已经暂时停更，目前推荐您使用：[Ursa.Avalonia](https://github.com/irihitech/Ursa.Avalonia.git)，[Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia.git)，[UraniumUI](https://github.com/enisn/UraniumUI.git)，这些UI拓展样式库也是很棒的！

## 更新日志
    2025/3/3 提供第一版自动生成器，用于依赖注入
