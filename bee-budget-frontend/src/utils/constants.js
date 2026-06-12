export const errorCode = {
  '401': '认证失败，无法访问系统资源',
  '403': '当前操作没有权限',
  '404': '访问资源不存在',
  'default': '系统未知错误，请反馈给管理员'
}

export const WITH_TOKEN = 'X-With-Token';
export const ALLOW_REPEAT_SUBMIT = 'X-Allow-Repeat-Submit';
export const SKIP_RELOGIN_HEADER = 'X-Skip-Relogin';
export const REFRESH_TOKEN_HEADER = 'X-Refresh-Token';
export const FILE_RESPONSE_HEADER = 'X-File-Response';

export const BASE_URL = import.meta.env.BASE_URL.slice(0, -1)
export const BASE_API_URL = import.meta.env.BASE_URL.slice(0, -1) + import.meta.env.VITE_APP_BASE_API;