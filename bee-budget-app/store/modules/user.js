import {login, getUserInfo as getUserInfoApi, logout, refreshToken} from '@/api/auth'
import {
    getToken, setToken, removeToken,
    getRefreshTokenId, setRefreshTokenId, removeRefreshTokenId,
    getUserInfo, setUserInfo, removeUserInfo,
} from "@/utils/auth"

export const state = {
    token: getToken(),
    refreshTokenId: getRefreshTokenId(),
    userInfo: getUserInfo(),
};
export const mutations = {
    setToken(state, token) {
        state.token = token
        setToken(token)
    },
    setRefreshTokenId(state, refreshTokenId) {
        state.refreshTokenId = refreshTokenId
        setRefreshTokenId(refreshTokenId)
    },
    setUserInfo(state, userInfo) {
        state.userInfo = userInfo
        setUserInfo(state.userInfo);
    },
    clearAuthInfo(state) {
        state.token = ''
        state.refreshTokenId = ''
        state.userInfo = {}
        removeToken()
        removeRefreshTokenId()
        removeUserInfo()
    },
};
export const actions = {
    async login({commit, state}, payload) {
        try {
            const response = await login(payload)
            const {success, data, message} = response
            if (success) {
                const {accessToken, refreshTokenId} = data
                commit('setToken', accessToken)
                commit('setRefreshTokenId', refreshTokenId)
                return response
            } else {
                throw new Error(message)
            }
        } catch (e) {
            console.error(e)
            throw e
        }
    },
    async getUserInfo({commit, state}) {
        try {
            const response = await getUserInfoApi()
            const {success, data, message} = response
            if (success) {
                const userInfo = data // 包含 user、roles、permissions
                commit('setUserInfo', userInfo)
                return response
            } else {
                throw new Error(message)
            }
        } catch (e) {
            throw e
        }
    },
    async logout({commit, state}) {
        console.log(999)
        try {
            await logout()
        } catch (e) {
            throw e
        } finally {
            commit('clearAuthInfo')
            uni.reLaunch({url: '/pages/user/login'});
        }
    },
    async refreshTokenAction({commit, state}) {
        try {
            const payload = {
                userId: state.userInfo.user.id,
                refreshTokenId: state.refreshTokenId,
            }
            const response = await refreshToken(payload)
            const {success, data, message} = response
            if (success) {
                const {accessToken, refreshTokenId} = data
                commit('setToken', accessToken);
                commit('setRefreshTokenId', refreshTokenId);
                return response
            } else {
                throw new Error(message)
            }
        } catch (e) {
            throw e
        }
    },
    async clearAuthInfo({commit, state}) {
        console.log('clearAuthInfo')
        commit('clearAuthInfo')
    }
};
