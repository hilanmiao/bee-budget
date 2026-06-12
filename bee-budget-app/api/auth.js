import request from '@/utils/request'

// 登录
export const login = (data) => request.post('/auth/login', data, {
        header: {
            // 告诉拦截器：这个请求即使 401 也不要弹窗
            'X-Skip-Relogin': 'true'
        },
    }
)

// 刷新令牌
export const refreshToken = (data) => request.post('/auth/refresh-token', data, {
        header: {
            // 告诉拦截器，如果是 refreshToken 请求自己 401，直接放行，不要拦截
            'X-Refresh-Token': 'true'
        },
    }
)

// 退出
export const logout = (data) => request.post('/auth/logout', data, {
    header: {
        // 告诉拦截器：这个请求即使 401 也不要弹窗
        'X-Skip-Relogin': 'true'
    },
})

// 获取用户信息
export const getUserInfo = (config) => request.get('/auth/info', config)

// 修改用户信息
export const updateUserInfo = (data) => request.put('/sys-user/profile', data)

// 修改密码
export const updateUserPassword = (data) => request.put('/sys-user/profile/password', data)

// 获取字典
export const getDicts = (dictType) => request.get('/sys-dict-category/by-code/' + dictType)

