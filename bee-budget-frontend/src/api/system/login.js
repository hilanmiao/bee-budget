import request from '@/utils/request.js'

// 登录方法
export function login(username, password, captchaContent, captchaId) {
    const data = {
        username,
        password,
        captchaContent,
        captchaId
    }
    return request({
        url: '/auth/login',
        headers: {
            // 明确表示“不带 token”
            'X-With-Token': 'false',
            // 防重复提交
            'X-Allow-Repeat-Submit': 'false',
        },
        method: 'post',
        data: data
    })
}

// 注册方法
export function register(data) {
    return request({
        url: '/register',
        headers: {
            'X-With-Token': 'false'
        },
        method: 'post',
        data: data
    })
}

// 获取用户详细信息
export function getInfo() {
    return request({
        url: '/auth/info',
        method: 'get'
    })
}

// 退出方法
export function logout() {
    return request({
        headers: {
            // 告诉拦截器：这个请求即使 401 也不要弹窗
            'X-Skip-Relogin': 'true'
        },
        url: '/auth/logout',
        method: 'post'
    })
}

// 刷新令牌
export function refreshToken(userId, refreshTokenId) {
    const data = {
        userId,
        refreshTokenId
    }
    
    return request({
        headers: {
            // 告诉拦截器，如果是 refreshToken 请求自己 401，直接放行，不要拦截
            'X-Refresh-Token': 'true'
        },
        url: '/auth/refresh-token',
        method: 'post',
        data: data,
    })
}