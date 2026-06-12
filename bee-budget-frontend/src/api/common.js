import request from '@/utils/request'

// 获取验证码
export function getCaptcha() {
  return request({
    url: '/captcha',
    headers: {
      withToken: false
    },
    method: 'get',
    timeout: 20000
  })
}