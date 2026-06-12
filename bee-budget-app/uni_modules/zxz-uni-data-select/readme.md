## DataSelect 下拉框选择器  <strong style="color:orangered;"><em>先导入示例项目</em></strong>  看看是否满足需求，然后再下载插件，有问题可以加微<strong style="color:orangered;"><em>weiyila520</em></strong>
> **组件名：zxz-uni-data-select**
> 代码块： `zxz-uni-data-select`

本插件基于官方插件 [uni-data-select](https://ext.dcloud.net.cn/plugin?id=7993) 进行二次开发拓展功能，支持uni-data-select本身功能不变（表单验证等）

<h1>拓展功能</h1>
<ol>
<li>支持多选功能</li>
<li>支持选项禁用</li>
<li>支持自定义显示值</li>
<li>支持搜索</li>
<li>支持多选时将选中值按文字形式展示</li>
<li>支持下拉菜单溢出屏幕底部时自动改为向上弹出</li>
<li>监听搜索输入事件</li>
</ol>
 
<h2>API</h2>
### zxz-uni-data-select Props

|  属性名		|    类型				| 默认值		| 说明																									|
| -				| -						| -				| -																										|
| v-model		| String、Array、Number	|-				| 选中项绑定值																							|
| multiple		| Boolean				| false			| 是否多选																								|
| disabled		| Boolean				|false			| 是否禁用																								|
| dataKey		| String				|"key"			| 作为 key 唯一标识的键名																				|
| dataValue		| String				| "value"		| 作为 value 唯一标识的键名																				|
| filterable	| Boolean				| false			|  是否开启搜索																							|
| collapseTags	| Boolean				| false			| 多选时是否将选中值按文字的形式展示																	|
|collapseTagsNum|Number					| 1				| 多选时选中值按文字的形式展示的数量																	|
| localdata		| Array					|-				| 下拉列表本地数据																						|
|label			| String				| -				| 左侧标题																								|
|placeholder	| String				| "请选择"		| 输入框的提示文字																						|
|emptyTips		| String				|"无选项"		| 无选项提示																							|
|clear			| Boolean				| true			| 是否清空																								|
|format			| String				| -				| 格式化输出 用法 field="_id as value, version as text, uni_platform as label" format="{label} - {text}"|
|@inputChange	| event					| event(String)	| 搜索输入事件																							|