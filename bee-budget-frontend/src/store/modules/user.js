import {login, logout, getInfo, refreshToken as refreshTokenApi} from '@/api/system/login.js'
import {
    getToken, setToken, removeToken, getRefreshTokenId, setRefreshTokenId, removeRefreshTokenId,
    getUserId, setUserId, removeUserId
} from '@/utils/auth'
import {isHttp, isEmpty} from "@/utils/validate"
import defAva from '@/assets/logo/logo2.png'

const useUserStore = defineStore(
    'user',
    {
        state: () => ({
            token: getToken(),
            refreshTokenId: getRefreshTokenId(),
            id: getUserId(),
            name: '',
            avatar: '',
            roles: [],
            permissions: []
        }),
        actions: {
            // 登录
            async login(userInfo) {
                const username = userInfo.username.trim()
                const password = userInfo.password.trim()
                const captchaContent = userInfo.captchaContent.trim()
                const captchaId = userInfo.captchaId
                try {
                    const response = await login(username, password, captchaContent, captchaId)
                    const { success, data, message } = response
                    if (!success) {
                        throw new Error(message)
                    }
                    const {accessToken, refreshTokenId} = data
                    setToken(accessToken)
                    setRefreshTokenId(refreshTokenId)
                    this.token = accessToken
                    this.refreshTokenId = refreshTokenId

                    return response
                } catch (e) {
                    throw e
                }
            },
            // 获取用户信息
            async getInfo() {
                try {
                    const response = await getInfo()
                    const { success, data, message } = response
                    if (!success) {
                        throw new Error(message)
                    }
                    const user = data.user
                    let avatar = user.avatar || ""
                    if (!isHttp(avatar)) {
                        avatar = (isEmpty(avatar)) ? defAva : avatar
                    }
                    if (data.roles && data.roles.length > 0) { // 验证返回的roles是否是一个非空数组
                        this.roles = data.roles
                        this.permissions = data.permissions
                    } else {
                        this.roles = ['ROLE_DEFAULT']
                    }
                    setUserId(user.id)
                    this.id = user.id
                    this.name = user.userName
                    this.avatar = avatar

                    return response
                } catch (e) {
                    throw e
                }
            },
            // 退出系统
            async logOut() {
                try {
                    await logout()
                } catch (e) {
                    throw e
                } finally {
                    await this.cleanAuthAction()
                }
            },
            // 刷新访问令牌
            async refreshTokenAction() {
                const oldRefreshTokenId = this.refreshTokenId;
                if (!oldRefreshTokenId) {
                    console.warn('无可用刷新令牌ID');
                    throw new Error('无可用刷新令牌ID')
                }

                try {
                    const response = await refreshTokenApi(this.id, oldRefreshTokenId);
                    const { success, data, message } = response
                    if (!success) {
                        throw new Error(message)
                    }
                    const {accessToken, refreshTokenId} = data;

                    // 更新本地存储
                    setToken(accessToken);
                    setRefreshTokenId(refreshTokenId);

                    // 更新 Store 状态
                    this.token = accessToken;
                    this.refreshTokenId = refreshTokenId;

                    console.log('令牌刷新成功');
                    return response
                } catch (e) {
                    console.error('令牌刷新失败', e);

                    // 清理状态 + 跳转登录
                    this.token = '';
                    this.refreshTokenId = '';
                    this.userId = '';
                    removeToken();
                    removeRefreshTokenId();
                    removeUserId()

                    // 可选：触发全局登出事件（如通知 router 或 UI）
                    // 但不要在这里跳转页面（由调用方或拦截器处理）
                    throw e
                }
            },
            async cleanAuthAction() {
                this.token = ''
                this.refreshTokenId = ''
                this.roles = []
                this.permissions = []
                removeToken()
                removeRefreshTokenId()
                removeUserId()
            }
        }
    })

export default useUserStore
