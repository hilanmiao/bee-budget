import router from './router'
import {ElMessage} from 'element-plus'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import {getToken, removeRefreshTokenId, removeToken, removeUserId} from '@/utils/auth'
import {isHttp, isPathMatch} from '@/utils/validate'
// import {isRelogin} from '@/utils/request'
import useUserStore from '@/store/modules/user'
import useSettingsStore from '@/store/modules/settings'
import usePermissionStore from '@/store/modules/permission'

NProgress.configure({showSpinner: false})

const whiteList = ['/login', '/register']

const isWhiteList = (path) => {
    return whiteList.some(pattern => isPathMatch(pattern, path))
}

router.beforeEach((to, from, next) => {
    NProgress.start()
    if (getToken()) {
        to.meta.title && useSettingsStore().setTitle(to.meta.title)
        /* has token*/
        if (to.path === '/login') {
            next({path: '/'})
            NProgress.done()
        } else if (isWhiteList(to.path)) {
            next()
        } else {
            if (useUserStore().roles.length === 0) {
                // 设置“正在重新登录”状态，防止重复弹窗（将在 getInfo 成功或失败后重置）
                // isRelogin.show = true
                // 判断当前用户是否已拉取完user_info信息
                useUserStore().getInfo().then(() => {
                    // isRelogin.show = false
                    usePermissionStore().generateRoutes().then(accessRoutes => {
                        // 根据roles权限生成可访问的路由表
                        accessRoutes.forEach(route => {
                            if (!isHttp(route.path)) {
                                router.addRoute(route) // 动态添加可访问路由表
                            }
                        })
                        next({...to, replace: true}) // hack方法 确保addRoutes已完成
                    })
                }).catch(err => {
                    console.log(err)
                    // 统一放拦截器里处理
                    // // 登出前重置“重新登录”锁状态，确保下次 401 能正常弹窗
                    // isRelogin.show = false
                    // useUserStore().logOut().then(() => {
                    //     ElMessage.error(err)
                    //     next({path: '/login'})
                    // })
                })
            } else {
                next()
            }
        }
    } else {
        // 没有token
        if (isWhiteList(to.path)) {
            // 在免登录白名单，直接进入
            next()
        } else {
            next(`/login?redirect=${to.fullPath}`) // 否则全部重定向到登录页
            NProgress.done()
        }
    }
})

router.afterEach(() => {
    NProgress.done()
})
