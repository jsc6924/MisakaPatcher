<h1 align="center">
  MisakaTranslator 御坂翻译器
  <br>
</h1>

<p align="center">
  <b>Galgame/文字游戏/漫画多语种实时机翻工具</b>
  <br>
  <b>开源 | 高效 | 易用</b>
  <br>
  <img src="https://github.com/hanmin0822/MisakaTranslator/workflows/CI/badge.svg" alt="CI">
  <br>
  <br>
</p>

<p align="center">
  <a href="/README.md">中文</a> •
  <a href="/README_EN.md">English</a>
</p>

~~MisakaTranslator的名字由来是因为本软件连接到了一万多名御坂妹妹所组成的『御坂网络』，利用其强大的计算能力来提供实时可靠的翻译（误）~~

[MisakaTranslator官方网站](https://misaka.galeden.cn/)

[测试版演示视频](https://www.bilibili.com/video/av94082641)

[2.0版本教程](https://www.bilibili.com/video/BV1Qt4y11713)

## 软件功能及特点

* 兼容性强：支持`Hook+OCR`两种方式提取游戏文本，能适配绝大多数游戏
* 可离线：支持完全离线工作(`Hook模块离线、可选的Tesseract-OCR模块离线、三种离线翻译API`)
* 高度可拓展的文本修复：针对Hook提取到的重复文本提供多种去重方式
* 提供更好体验的在线API：提供多种在线API(`百度OCR+多种在线翻译API+多种公共接口翻译`)
* 更高的OCR精度：支持在提交OCR前对图片进行预处理（多种处理方法）
* 分词及字典：支持Mecab分词和字典功能，可针对单词进行查询
* 翻译优化系统：支持人名地名欲翻译，提高翻译质量，这个系统正在被不断完善
* 人工翻译系统：支持用户自己定义语句翻译并生成人工翻译文件以供分享
* TTS语音发音：支持朗读句子和单词
* 漫画翻译：更友好、准确的漫画翻译
* 高效：使用C#开发，程序效率较使用Python开发的VNR要高
* 易用：UI亲切，易上手，有详细教程
* 更多功能，正在开发

## 帮助开发者

如果您对这个项目感兴趣，想提供任何帮助，欢迎联系作者。

E-Mail/QQ:512240272@qq.com

## 本项目所使用到的其他开源项目

* [lgztx96/texthost](https://github.com/lgztx96/texthost)
* [Artikash/Textractor](https://github.com/Artikash/Textractor)
* 其他参见[Dependencies](https://github.com/hanmin0822/MisakaTranslator/network/dependencies)

## 本项目开发过程中使用到的重要参考文献

* [C#调用多种翻译API和使用Textractor提取文本](https://www.lgztx.com/) 
* [C#图像处理(二值化,灰阶)](https://blog.csdn.net/chaoguodong/article/details/7877312)
* [C#中使用全局键盘鼠标钩子](https://www.cnblogs.com/CJSTONE/p/4961865.html)

## 项目团队成员

* 画师-[洛奚](https://www.pixiv.net/users/13495987)
* 官网制作-[Disviel](https://github.com/Disviel)
* 人设&画师-[百川行](https://www.pixiv.net/users/17591894)
* 合作开发者-[unlsycn](https://github.com/HumphreyDotSln) 
* 合作开发者-[tpxxn](https://github.com/tpxxn)
* 合作开发者-[imba-tjd](https://github.com/imba-tjd)

## 相关功能及特点
* 智能模糊匹配：在hook/ocr提取到的文本与补丁中的原句不完全一致时，也能快速正确地匹配到对应的文本（`经测试，在提取到的文本仅保留原句约25%的信息时依然可以做到95%以上的正确匹配`）
* 为了与MisakaTranslator功能区分，本工具去除了所有机翻功能
* 保留所有OCR功能，并增加了对Tesseract 5的支持
* 增加了一种图像预处理方法（`提取白色文本，能将空心字转成实心字`）
* 修复了2.0版本图像预处理没有效果的问题
* 支持对汉化补丁的简单加密

## 补丁
#### 格式
补丁是一个文本文件（UTF-8无签名，仅支持单个文件，如有多个可以使用下一节介绍的工具合并，或者自行合并），格式如下：
```
<j>
原句1
<c>
翻译1
<j>    标签后面的内容不会被读取，可以写任何东西，如编号
原句2第一行
原句2第二行 （标签后面可以有很多行）
<c> 2
翻译2第一行
翻译2第二行（行数不一定要与原句匹配）
<j> 3

如果您有意将自己的相关类型项目加入到MisakaProject中，请联系作者。

# 如果一行的第一个字符为#，则这行被当做注释忽略
<c> 3 
（句子可以为空，但是原句和翻译句总数必须一致）
<j> 4
ああ
<c> 4
嗯
<j> 5
ああ
<c> 5
啊啊 （两句同样的原句可以有不同的翻译，程序会根据前一句匹配的位置自动识别）
```

#### 打包和加密
从1.2版开始在`tools/`目录下加入了打包工具`MisakaPatchPackager.exe`，可以对补丁进行打包和简单加密（加密很容易被破解，所以不保证任何安全性）
使用方法：
```
./MisakaPatchPackager.exe [option]... output
例：./MisakaPatchPackager.exe -f="input1.txt" -f="input2.txt" -e=xor -p output.msk
选项：
-f, --file      指定输入文件（可以指定多个，多个输入文件将被合并成一个文件输出）
-e, --enc       指定加密方法（可选择xor或aes），默认不加密，推荐xor
-p, --preview   预览前十行加密结果（只有在使用加密时才生效）
-h, --help      帮助
```
加密过的补丁可以和未加密的补丁一样使用，不需要再进行别的设置。

## 使用
打开软件，右下角设置 -> 翻译相关设置 -> 通用设置 -> 选择本地汉化补丁为翻译源
然后就可以开始游戏了，添加游戏向导中会提示选择汉化补丁，Hook只有第一次需要选择。Hook和OCR相关设置请参考[原版说明](/README_ORIGINAL.md)。

#### Tesseract OCR 5

去[这里](https://github.com/UB-Mannheim/tesseract/wiki)下载32位安装包，安装到`"C:\\Program Files (x86)\\Tesseract-OCR"`，记得选jpn语言包，然后在OCR设置中勾选Tesseract5，就可以使用了。

## 开发
`git clone`后，代码可以编译，但是运行会报错，因为运行库不全
#### 解决方法
下载最新版的release，与`MisakaTranslator-WPF\bin\Debug`之下的内容比较，把`Debug`中缺少的文件和文件夹全都复制过去。

## 版本更新
#### 1.3
- 支持每个游戏绑定一个补丁
- 支持调节匹配算法的灵敏度

#### 1.2 
- 支持补丁打包和简单加密

#### 1.1
- 支持禁用分词器

#### 1.0
- 初始版本

## 备注

* 提供软件的英语版本翻译-[Words](https://github.com/CPCer)
* 多次提供作者开发上的帮助-[lgztx96](https://github.com/lgztx96)
* [点此查看其他的贡献者名单](https://github.com/hanmin0822/MisakaTranslator/blob/master/THANKLIST.MD)

## 作出贡献

如果您想为本项目作出贡献，请参阅[贡献指南](https://github.com/hanmin0822/MisakaTranslator/blob/master/CONTRIBUTING.md)

## 其他注意

软件开发过程中使用到部分网络素材，如果侵犯到您的权益，请第一时间联系作者删除，谢谢！
