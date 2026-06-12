import Cookies from 'js-cookie'

const TokenKey = 'My-Token'
const RefreshTokenIdKey = 'My-Refresh-Token-Id'
const UserIdKey = 'My-User-Id'

export function getToken() {
  // return Cookies.get(TokenKey)
  return localStorage.getItem(TokenKey)
}

export function setToken(token) {
  // return Cookies.set(TokenKey, token)
  return localStorage.setItem(TokenKey, token)
}

export function removeToken() {
  // return Cookies.remove(TokenKey)
  return localStorage.removeItem(TokenKey)
}

export function getRefreshTokenId() {
    return localStorage.getItem(RefreshTokenIdKey)
}

export function setRefreshTokenId(refreshTokenId) {
    return localStorage.setItem(RefreshTokenIdKey, refreshTokenId)
}

export function removeRefreshTokenId() {
    return localStorage.removeItem(RefreshTokenIdKey)
}

export function getUserId() {
    return localStorage.getItem(UserIdKey)
}

export function setUserId(userId) {
    return localStorage.setItem(UserIdKey, userId)
}

export function removeUserId() {
    return localStorage.removeItem(UserIdKey)
}

