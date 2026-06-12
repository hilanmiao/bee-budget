import axios from 'axios'
import qs from 'qs'
import {ElMessage, ElMessageBox, ElNotification} from 'element-plus'
import {
    errorCode,
    ALLOW_REPEAT_SUBMIT,
    WITH_TOKEN,
    SKIP_RELOGIN_HEADER,
    REFRESH_TOKEN_HEADER,
    FILE_RESPONSE_HEADER,
} from '@/utils/constants'
import cache from '@/plugins/cache'
import useUserStore from '@/store/modules/user'

// 全局互斥锁标志位，用于确保“重新登录”相关的用户提示（如 MessageBox）在同一时间只显示一次，避免 UI 重复弹窗、提升用户体验和系统稳定性
export let isRelogining = false;
let isRefreshing = false; // 🚦 设置“正在刷新”标志，锁定后续并发请求进入队列（而不是直接失败）
let requestQueue = [];   // 等待重发的请求队列

// axios.defaults.headers['Content-Type'] = 'application/json;charset=utf-8'
// 创建axios实例
const service = axios.create({
    // axios中请求配置有baseURL选项，表示请求URL公共部分
    // baseURL: import.meta.env.VITE_APP_BASE_API,
    baseURL: import.meta.env.BASE_URL.slice(0, -1) + import.meta.env.VITE_APP_BASE_API,
    // 超时
    // timeout: 10000
    timeout: 1000 * 60
})

// 请求拦截器
service.interceptors.request.use(config => {
    const userStore = useUserStore();

    // 1. 是否明确表示“不带 token”
    if (userStore.token && config.headers[WITH_TOKEN] !== 'false') {
        config.headers['Authorization'] = 'Bearer ' + userStore.token;
    }

    // 2. GET 请求参数处理
    if (config.method === 'get' && config.params) {
        // 过滤掉空值 (null, '', undefined)，防止后端报错
        const cleanParams = Object.fromEntries(
            Object.entries(config.params).filter(([_, value]) => value !== null && value !== '' && value !== undefined)
        )

        // 使用 qs 序列化，arrayFormat: 'repeat' 适合大多数 Java 后端 (如 ?ids=1&ids=2)
        // 如果你的后端需要 ?ids=1,2，请改为 arrayFormat: 'comma'
        config.paramsSerializer = (params) => {
            return qs.stringify(cleanParams, {arrayFormat: 'repeat'})
        }
    }

    // 3. 防重复提交 (仅针对 POST/PUT)
    if (config.headers[ALLOW_REPEAT_SUBMIT] === 'false' && (config.method === 'post' || config.method === 'put')) {
        const requestObj = {
            url: config.url,
            data: typeof config.data === 'object' ? JSON.stringify(config.data) : config.data,
            time: new Date().getTime()
        }

        const sessionObj = cache.session.getJSON('sessionObj')

        if (sessionObj) {
            const {url: s_url, data: s_data, time: s_time} = sessionObj
            const interval = 1000 // 1秒防抖

            // 核心比对
            if (s_url === requestObj.url && s_data === requestObj.data && (requestObj.time - s_time) < interval) {
                return Promise.reject(new Error('数据正在处理，请勿重复提交'))
            }
        }

        // 更新缓存
        cache.session.setJSON('sessionObj', requestObj)
    }

    return config
}, error => {
    console.log(error)
    return Promise.reject(error)
})

// 响应拦截器
service.interceptors.response.use(response => {
        // 成功响应（HTTP 2xx）

        // // 如果是 blob/arraybuffer，直接返回
        // const responseType = response.request?.responseType;
        // if (responseType === 'blob' || responseType === 'arraybuffer') {
        //     return response.data;
        // }
        if (response.headers[FILE_RESPONSE_HEADER] === 'true') {
            // 确认是文件流，直接放行 blob
            return response.data;
        }

        const data = response.data;
        const code = data.code || 200;
        const message = data.message || errorCode[code] || errorCode['default'];

        // 兼容模式：如果 data.code 是 401/403/404/500，但 HTTP 是 200
        // 我们主动抛出一个“标准格式”的错误，让它进入 .catch()
        if ([401, 403, 404, 500].includes(code)) {
            return Promise.reject({
                response: {
                    status: code,
                    data: {code, message}
                }
            });
        }

        // 处理其他业务错误（如 601 余额不足）
        if (code !== 200) {
            if (code === 601) {
                ElMessage({message, type: 'warning'});
            } else {
                ElNotification.error({title: message});
            }
            return Promise.reject(new Error(message));
        }

        // 成功
        return data;
    },
    async error => {
        console.log('err' + error);
        const {response, message} = error;

        // 默认：最终一定会 reject 原 error，保持调用方 catch 能捕获完整上下文

        // 有响应（HTTP 错误）
        if (response) {
            const {status, data, config} = response;
            // 同步语义化 message 到 error 对象
            error.message = data?.message || errorCode[status] || errorCode['default'];

            // 👇 将错误交给统一的错误处理器
            return handleErrorResponse(status, config, error)
        } else {
            // 网络错误（无响应）
            let errorMsg = '未知错误';
            if (message.includes('Network Error')) {
                errorMsg = '后端接口连接异常';
            } else if (message.includes('timeout')) {
                errorMsg = '系统接口请求超时';
            } else if (message.includes('Request failed with status code')) {
                errorMsg = '系统接口' + message.substr(message.length - 3) + '异常';
            } else {
                errorMsg = message;
            }
            ElMessage({message: errorMsg, type: 'error', duration: 5 * 1000});
        }

        // 统一出口：永远 reject 原 error，但可以增强 message
        // 调用方 .catch(err) 能拿到完整 error 对象 + message
        return Promise.reject(error);
    }
)

const handleErrorResponse = async (status, config, error) => {
    const userStore = useUserStore();

    if (status === 401) {
        // ✅ 关键修复：如果是 refreshToken 请求自己 401，直接放行，不要拦截！
        if (config.headers[REFRESH_TOKEN_HEADER] === 'true') {
            console.log('刷新令牌失败，跳转登录页');
            // 可选：显示一个轻量提示（避免 MessageBox 弹窗，因为可能正在刷新中）
            ElMessage.error('登录已过期，请重新登录');
            await redirectToLogin(userStore);
            return Promise.reject(error); // 立即终止，不走后续逻辑
        }

        // 🚫 如果是主动登出（如点击“退出登录”），跳过刷新逻辑，直接跳转登录页
        if (config?.headers?.[SKIP_RELOGIN_HEADER] === 'true') {
            error.message = '主动登出';
            await redirectToLogin(userStore);
            return Promise.reject(error); // 立即终止，不走后续逻辑
        }

        // 🔁 如果正在刷新 token，当前请求不立即 reject，而是加入队列，等待刷新完成后重发
        if (isRefreshing) {
            // 返回一个 pending 状态的 Promise，让当前请求“等待”
            return new Promise((resolve, reject) => {
                // 60秒超时，如果 Token 刷新接口永久挂起（如服务器宕机），requestQueue 中的请求会永久 pending，导致页面假死
                const timeout = setTimeout(() => {
                    reject(new Error('Token refresh timeout'));
                }, 60000);

                // 将当前请求的 config 和 resolve/reject 回调存入队列
                requestQueue.push({
                    config,
                    resolve: (v) => {
                        clearTimeout(timeout);
                        resolve(v);
                    },
                    reject: (e) => {
                        clearTimeout(timeout);
                        reject(e);
                    }
                });
            });
        }

        // 🔄 尝试用 refreshToken 刷新 accessToken
        isRefreshing = true;

        try {
            // 1. 刷新 token
            await userStore.refreshTokenAction();
            const newToken = userStore.token;

            // 2. 更新当前请求的 token 并重发
            config.headers['Authorization'] = 'Bearer ' + newToken;
            const result = await service(config); // 👈 直接 await 当前请求结果

            // 3. 并行重发队列中的请求（不 await，避免阻塞）
            requestQueue.forEach(({config, resolve, reject}) => {
                config.headers['Authorization'] = 'Bearer ' + newToken;
                service(config).then(resolve, reject); // 成功 resolve，失败 reject
            });

            // 5. 直接 return 结果！axios 会把它当作正常响应
            return result;
        } catch (refreshError) {
            console.error('RefreshError:', refreshError);

            // 刷新失败
            isRefreshing = false;

            // 拒绝队列中所有等待的请求
            requestQueue.forEach(({reject}) => reject(refreshError));

            // 弹窗让用户重新登录
            await promptRelogin(userStore);

            // 👇 关键：直接 throw error！
            // 外层的 .catch() 会捕获它，并最终 Promise.reject(error)
            throw error;
        } finally {
            // 4. 清理
            requestQueue = [];
            isRefreshing = false;
        }
    } else if (status === 400) {
        ElMessage({message: error.message, type: 'error'});
    } else if (status === 403) {
        ElMessage({message: error.message, type: 'error'});
    } else if (status === 404) {
        ElMessage({message: error.message, type: 'error'});
    } else if (status === 500) {
        ElMessage({message: error.message, type: 'error'});
    } else {
        ElMessage({message: error.message, type: 'error', duration: 5 * 1000});
    }

    return Promise.reject(error);
}

// ==================== 辅助函数：安全弹出重新登录提示 ====================
const promptRelogin = async (userStore) => {
    console.log('[promptRelogin] 开始');

    if (isRelogining) {
        // 已有弹窗正在处理，直接拒绝
        throw new Error('relogin dialog already showing');
    }

    isRelogining = true;
    try {
        await ElMessageBox.confirm(
            '登录状态已过期，您可以继续留在该页面，或者重新登录',
            '系统提示',
            {confirmButtonText: '重新登录', cancelButtonText: '取消', type: 'warning'}
        )
        await redirectToLogin(userStore);
    } catch (error) {
        // 👇 关键修改：只在用户明确点击“取消”时才静默处理
        if (error === 'cancel') {
            // 用户点击了“取消”按钮 → 允许留在当前页，不跳转
            console.log('用户取消了重新登录');
            // 注意：这里不要 reject！让调用方决定如何处理
        } else {
            // 其他情况（如点击遮罩、ESC、程序错误）→ 视为需要重新登录
            console.warn('非取消操作关闭弹窗，强制跳转登录:', error);
            await redirectToLogin(userStore);
        }
    } finally {
        // 无论确认、取消、失败，都释放锁
        isRelogining = false;
    }
}

// 跳转到登录页
async function redirectToLogin(userStore) {
    await userStore.cleanAuthAction()
    location.href = import.meta.env.BASE_URL.slice(0, -1) + '/login';
}

export default service
