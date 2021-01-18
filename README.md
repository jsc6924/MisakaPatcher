<h1 align="center">
  MisakaPatcher · 文字游戏外挂汉化补丁工具
  <br>
</h1>


MisakaPatcher去除了原版[MisakaTranslator 2.0](/README_ORIGINAL.md)的所有机器翻译功能，转而添加了对外挂汉化补丁的支持，因此本工具更适合喜欢人工翻译的玩家，也为解包封包遇到困难的汉化人员提供了另一种发布汉化的途径。本文介绍了补丁的格式以及加载的方法。

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

原句3 (原句前后可以空行，空行在读取时会被自动忽略）

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

感谢原作者[hanmin0822](https://github.com/hanmin0822/MisakaTranslator)及项目团队提供这个良好的平台
[MisakaTranslator](https://github.com/hanmin0822/MisakaTranslator)

使用本工具制作、加载使用汉化补丁，产生的一切责任均由使用者自行承担。



