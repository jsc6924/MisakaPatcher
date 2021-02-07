<h1 align="center">
  MisakaPatcher · 文字游戏外挂汉化补丁工具
  <br>
</h1>


MisakaPatcher去除了原版[MisakaTranslator 2.0](/README_ORIGINAL.md)的所有机器翻译功能，转而添加了对外挂汉化补丁的支持，因此本工具更适合喜欢人工翻译的玩家，也为解包封包遇到困难的汉化人员提供了另一种发布汉化的途径。本文介绍了补丁的格式以及加载的方法。

[Gitee镜像](https://gitee.com/jsc723/MisakaPatcher)

## 相关功能及特点
* 智能模糊匹配：在hook/ocr提取到的文本与补丁中的原句不完全一致时，也能快速正确地匹配到对应的文本（`经测试，在提取到的文本仅保留原句约25%的信息时依然可以做到95%以上的正确匹配`）
* 为了与MisakaTranslator功能区分，本工具去除了所有机翻功能
* 包含了补丁的打包工具MisakaPatchPackager，可以合并文本，检查语法
* 保留所有OCR功能，并增强了对Tesseract 5的支持
* 增加了一种图像预处理方法（`提取白色文本，能将空心字转成实心字`）
* 支持对汉化补丁的简单加密，被加密的补丁将不会被导入数据库，也不能被修改

## 补丁
#### 格式
补丁是一个文本文件（UTF-8无签名，仅支持单个文件，如有多个可以使用下一节介绍的工具MisakaPatchPackager合并，或者自行合并），格式如下：
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
-f, --file [FILE]       指定输入文件（可以使用多个-f指定多个）
-e, --enc  [xor|aes]    指定加密方法（可选择xor或aes），默认不加密
-c, --check             检查语法
-p, --preview           预览前十行加密结果（只有在使用加密时才生效）
-v, --version           显示版本
-h, --help              帮助
```
加密过的补丁可以和未加密的补丁一样使用，不需要再进行别的设置。

#### 有关补丁发布
- 不加密的补丁可以发布到MisakaTranslator的[公共网盘](http://mskt.ys168.com/)
- 加密的补丁可以发布到https://gitee.com/jsc723/misaka-patches-headers，目前只支持通过提交pull request进行发布，之后会为Patcher添加联网查询补丁和自动发布功能

## 使用
打开软件，右下角设置 -> 翻译相关设置 -> 通用设置 -> 选择本地汉化补丁为翻译源。
然后就可以开始游戏了，添加游戏向导中会提示选择汉化补丁，Hook只有第一次需要选择。Hook和OCR相关设置请参考[原版说明](/README_ORIGINAL.md)。

#### Tesseract OCR 5

去[这里](https://github.com/UB-Mannheim/tesseract/wiki)下载安装包，记得选jpn语言包，然后在OCR设置中勾选Tesseract5，再去Tesseract5设置界面中选择安装路径，就可以使用了。

## 开发
`git clone`后，代码可以编译，但是运行会报错，因为运行库不全
#### 解决方法
下载最新版的release，与`MisakaTranslator-WPF\bin\Debug`之下的内容比较，把`Debug`中缺少的文件和文件夹全都复制过去。

## 版本更新
#### 1.4
- 基于MisakaTranslator2.7重写
- 增强Tesseract 5的支持（日语横向、纵向、自定义命令行参数）
- 优化匹配算法：概率分布函数变得相对更平滑
- 增强人工翻译功能：
  - 可以在翻译窗口控制是否自动记录到数据库
  - 记录到数据库的原句可以重复
  - 只能将非加密的补丁导入数据库（补丁被加密说明作者不希望补丁内容被修改，所以我们就不要去修改它）
- packager支持语法检查

从1.4开始，会发布两个版本
- lite版：轻量级的版本，去除了mecab分词功能和自带的tesseract4 OCR，解压后的大小比完全版要小100Mb
- 完全版：即包含mecab分词和tesseract4（英语、日语）的版本

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



