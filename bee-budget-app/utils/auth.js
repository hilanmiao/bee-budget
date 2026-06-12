const UserInfoKey = 'My-User-Info'
const LoginInfoKey = 'My-Login-Info'
const TokenKey = 'My-Token'
const RefreshTokenIdKey = 'My-Refresh-Token-Id'
const UserIdKey = 'My-User-Id'

export function getLoginInfo() {
    return uni.getStorageSync(LoginInfoKey)
}

export function setLoginInfo(data) {
    return uni.setStorageSync(LoginInfoKey, data)
}

export function getToken() {
    return uni.getStorageSync(TokenKey)
}

export function setToken(token) {
    return uni.setStorageSync(TokenKey, token)
}

export function removeToken() {
    return uni.removeStorageSync(TokenKey)
}

export function getUserInfo() {
    return uni.getStorageSync(UserInfoKey)
}

export function setUserInfo(userInfo) {
    return uni.setStorageSync(UserInfoKey, userInfo)
}

export function removeUserInfo() {
    return uni.removeStorageSync(UserInfoKey)
}

export function getRefreshTokenId() {
    return uni.getStorageSync(RefreshTokenIdKey)
}

export function setRefreshTokenId(refreshTokenId) {
    return uni.setStorageSync(RefreshTokenIdKey, refreshTokenId)
}

export function removeRefreshTokenId() {
    return uni.removeStorageSync(RefreshTokenIdKey)
}
