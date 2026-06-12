import request from '@/utils/request'

// 查询用户个人信息
export function getProfile() {
  return request({
    url: '/sys-user/profile',
    method: 'get'
  })
}

// 修改用户个人信息
export function updateProfile(data) {
  return request({
    url: '/sys-user/profile',
    method: 'put',
    data: data
  })
}

// 用户密码重置
export function updateProfilePwd(data) {
  return request({
    url: '/sys-user/profile/password',
    method: 'put',
    data: data
  })
}

// 用户头像上传
export function uploadProfileAvatar(data) {
  return request({
    url: '/sys-user/profile/avatar',
    method: 'post',
    data: data
  })
}
