// 1.导入vue和vue插件
import Vue from 'vue'

// 2.导入组件和库
import uView from '@/uni_modules/uview-ui'
import store from '@/store'
import _ from '@/plugins/lodash.min.js'
import dayjs from 'dayjs'
import isBetween from 'dayjs/plugin/isBetween'
import utc from 'dayjs/plugin/utc'
dayjs.extend(isBetween)
dayjs.extend(utc)

// ==============3.导入自定义组件=============
import App from './App'
import MyIcon from '@/components/MyIcon/index'
Vue.component("my-icon", MyIcon)

// ==============4.使用插件=============
Vue.use(uView)

// ==============5.配置全局=============
Vue.prototype.$_ = _
Vue.prototype.$dayjs = dayjs
Vue.prototype.$store = store

// ==============6.创建实例=============
Vue.config.productionTip = false
App.mpType = 'app'
const app = new Vue({
    store,
    ...App
})
app.$mount()
