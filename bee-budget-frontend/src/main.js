import {createApp} from 'vue'

import Cookies from 'js-cookie'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import locale from 'element-plus/es/locale/lang/zh-cn'

import '@/assets/styles/index.scss' // global css

import App from './App'
import store from './store'
import router from './router'
import directive from './directive' // directive

// 注册指令
import plugins from './plugins' // plugins

// svg图标
import 'virtual:svg-icons-register'
import SvgIcon from '@/components/SvgIcon'
import elementIcons from '@/components/SvgIcon/svgicon'

import './permission' // permission control

// import { useDict } from '@/utils/dict'

// 分页组件
import Pagination from '@/components/Pagination'
// 字典标签组件
import DictTag from '@/components/DictTag'

const app = createApp(App)

// 全局方法挂载
// app.config.globalProperties.useDict = useDict

// 全局组件挂载
app.component('DictTag', DictTag)
app.component('Pagination', Pagination)

app.use(router)
app.use(store)
app.use(plugins)
app.use(elementIcons)
app.component('svg-icon', SvgIcon)

directive(app)

// 使用element-plus 并且设置全局的大小
app.use(ElementPlus, {
    locale: locale,
    // 支持 large、default、small
    size: Cookies.get('size') || 'default'
})

app.mount('#app')
