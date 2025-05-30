1 svn 目录结构
设定的 svn 路径下应包含
	assets: 资源根目录
	config: 配置文件目录
		conf.txt: 配置文件
		tags.txt: 版本标记 (由工具生成)
		history.txt: 历史版本记录 (由工具生成)

2 conf.txt 格式
基本形式为：<key> = <value>
例：
	project = wok-android
	prefix = wok
	extension = wkzp
	include = 3rd arts configs data lua maps models scenes shaders sound surfaces
	no-compress-extension = u3dext bank
	step = 3

2.1 project
更新包中的 project
例：project = wok-android

2.2 prefix
更新包文件名前缀
例：prefix = wok

2.3 extension
更新包文件扩展名
例：extension = wkzp

2.4 include
处理的资源目录，列表外的目录不处理
例：include = 3rd arts configs data lua maps models scenes shaders sound surfaces

2.5 no-compress-extension
不压缩的文件扩展名
例：no-compress-extension = u3dext bank

2.6 step
内服打包时，自动生成更新包的版本间隔
例：step = 3
