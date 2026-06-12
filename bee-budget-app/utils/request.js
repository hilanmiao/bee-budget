import qs from 'qs';
import store from '@/store';
import {baseApiUrl} from '@/config/index';
import {
    errorCode,
    WITH_TOKEN,
    ALLOW_REPEAT_SUBMIT,
    SKIP_RELOGIN_HEADER,
    REFRESH_TOKEN_HEADER,
    FILE_RESPONSE_HEADER,
} from '@/utils/constants'

// ==================== 全局状态管理 ====================
// 用于确保“重新登录”相关的用户提示（如 MessageBox）在同一时间只显示一次
export let isRelogining = false;
let isRefreshing = false;         // 🚦 设置“正在刷新”标志，锁定后续并发请求进入队列（而不是直接失败）
let requestQueue = [];    // 等待重发的请求队列
let isRedirectedToLogin = false; // 👈 新增：是否已触发跳转登录

// ==================== 初始化 uView HTTP 配置 ====================
uni.$u.http.setConfig((config) => {
    config.baseURL = baseApiUrl;
    config.timeout = 1000 * 60;
    config.header = {
        'Content-Type': 'application/json;charset=UTF-8'
    };
    return config;
});

// ==================== 请求拦截器 ====================
uni.$u.http.interceptors.request.use((config) => {
    // 1. 是否明确表示“不带 token”
    if (store.state.token && config.header[WITH_TOKEN] !== 'false') {
        config.header['Authorization'] = 'Bearer ' + store.state.token;
    }

    // 2. GET 参数序列化 (使用 qs)
    if (config.method?.toLowerCase() === 'get' && config.params) {
        const cleanParams = Object.fromEntries(
            Object.entries(config.params).filter(([_, value]) => value !== null && value !== '' && value !== undefined)
        );
        const queryString = qs.stringify(cleanParams, {arrayFormat: 'repeat'});
        if (queryString) {
            config.url += config.url.includes('?') ? '&' : '?';
            config.url += queryString;
        }
        config.params = {};
    }

    // 3. 防重复提交 (仅针对 POST/PUT)
    if (config.header[ALLOW_REPEAT_SUBMIT] === 'false' && (config.method === 'post' || config.method === 'put')) {
        const requestObj = {
            url: config.url,
            data: typeof config.data === 'object' ? JSON.stringify(config.data) : config.data,
            time: new Date().getTime()
        };

        const sessionObjStr = uni.getStorageSync('sessionObj');
        if (sessionObjStr) {
            const sessionObj = JSON.parse(sessionObjStr);
            const {url: s_url, data: s_data, time: s_time} = sessionObj;
            const interval = 1000; // 1秒防抖

            if (s_url === requestObj.url && s_data === requestObj.data && (requestObj.time - s_time) < interval) {
                return Promise.reject(new Error('数据正在处理，请勿重复提交'));
            }
        }
        uni.setStorageSync('sessionObj', JSON.stringify(requestObj));
    }

    return config;
}, (error) => {
    return Promise.reject(error);
});

// ==================== 响应拦截器 ====================
uni.$u.http.interceptors.response.use(
    // 成功回调 (statusCode 2xx)
    (response) => {
        // 如果是文件下载，直接返回原始响应
        // if (response.config?.responseType === 'arraybuffer' || response.config?.responseType === 'blob') {
        //     return response;
        // }
        if (response.config?.header[FILE_RESPONSE_HEADER] === 'true') {
            // 确认是文件流，直接放行 blob
            return response.data;
        }

        const data = response.data;
        const code = data.code || 200;
        const message = data.message || errorCode[code] || errorCode['default'];

        // 兼容模式：如果 data.code 是 401/403/404/500，但 HTTP 是 200
        if ([401, 403, 404, 500].includes(code)) {
            return Promise.reject({
                response: {
                    statusCode: code,
                    data: {code, message}
                },
                config: response.config
            });
        }

        // 处理其他业务错误（如 601 余额不足）
        if (code !== 200) {
            if (code === 601) {
                uni.$u.toast(message);
            } else {
                uni.$u.toast(message);
            }
            return Promise.reject(new Error(message));
        }

        return data;
    },
    // 失败回调 (网络错误或 statusCode 非 2xx)
    async (error) => {
        console.error('Request error:', error);
        const {data, errMsg} = error;

        // 有响应（HTTP 错误）
        if (data) {
            const {statusCode, config, data} = error || {};
            // 同步语义化 message 到 error 对象
            error.errMsg = data?.message || errorCode[statusCode] || errorCode['default'];

            // 👇 将错误交给统一的错误处理器
            return handleErrorResponse(statusCode, config, error)
        } else {
            // 网络错误（无响应）
            let errorMsg = '未知错误';
            if (errMsg.includes('request:fail')) {
                errorMsg = '请求失败';
            }
            uni.$u.toast(errorMsg);
        }

        return Promise.reject(error);
    }
);

const handleErrorResponse = async (statusCode, config, error) => {
    // 👇 新增：如果已经跳转登录，直接 reject，不再处理
    if (isRedirectedToLogin) {
        return Promise.reject(error);
    }

    // 👇 核心：401 错误处理逻辑
    if (statusCode === 401) {
        // ✅ 关键修复：如果是刷新 token 的请求自己 401，直接放行，不要拦截！
        if (config.header[REFRESH_TOKEN_HEADER] === 'true') {
            console.log('刷新令牌失败，跳转登录页');
            uni.$u.toast('登录已过期，请重新登录');
            await redirectToLogin();
            return Promise.reject(error);
        }

        // 🚫 如果是主动登出（如点击“退出登录”），跳过刷新逻辑
        if (config?.header?.[SKIP_RELOGIN_HEADER] === 'true') {
            error.errMsg = '主动登出';
            await redirectToLogin();
            return Promise.reject(error);
        }

        // 🔁 如果正在刷新 token，当前请求不立即 reject，而是加入队列，等待刷新完成后重发
        if (isRefreshing) {
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

        // 🚦 设置“正在刷新”标志
        isRefreshing = true;
        try {
            // 1. 刷新 token
            await store.dispatch('refreshTokenAction');
            const newToken = store.state.token;

            // 2. 更新当前请求的 token 并重发
            config.header['Authorization'] = 'Bearer ' + newToken;
            const result = await uni.$u.http.request(config); // 👈 直接 await 当前请求结果

            // 3. 并行重发队列中的请求（不 await，避免阻塞）
            requestQueue.forEach(({config, resolve, reject}) => {
                config.header['Authorization'] = 'Bearer ' + newToken;
                uni.$u.http.request(config).then(resolve).catch(reject); // 成功 resolve，失败 reject
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
            await promptRelogin();

            // 👇 关键：直接 throw error！
            // 外层的 .catch() 会捕获它，并最终 Promise.reject(error)
            throw error;
        } finally {
            // 4. 清理
            requestQueue = [];
            isRefreshing = false;
        }
    } else if (statusCode === 400) {
        uni.$u.toast(error.errMsg);
    } else if (statusCode === 403) {
        uni.$u.toast(error.errMsg);
    } else if (statusCode === 404) {
        uni.$u.toast(error.errMsg);
    } else if (statusCode === 500) {
        uni.$u.toast(error.errMsg);
    } else {
        uni.$u.toast(error.errMsg);
    }

    return Promise.reject(error);
}

// ==================== 辅助函数：安全弹出重新登录提示 ====================
async function promptRelogin() {
    console.log('[promptRelogin] 开始');
    // 👇 新增防护
    if (isRedirectedToLogin) {
        throw new Error('already redirected to login');
    }

    if (isRelogining) {
        // 已有弹窗正在处理，直接拒绝
        throw new Error('relogin dialog already showing');
    }

    isRelogining = true;
    try {
        const result = await new Promise((resolve, reject) => {
            uni.showModal({
                title: '系统提示',
                content: '登录状态已过期，您可以继续留在该页面，或者重新登录',
                confirmText: '重新登录',
                cancelText: '取消',
                success: resolve,
                fail: reject
            });
        });

        if (result.confirm) {
            await redirectToLogin();
        }
    } finally {
        // 无论确认、取消、失败，都释放锁
        isRelogining = false;
    }
}

// 跳转到登录页
async function redirectToLogin() {
    isRedirectedToLogin = true;

    await store.dispatch('clearAuthInfo');

    return new Promise((resolve) => {
        uni.reLaunch({
            url: '/pages/user/login',
            success: () => resolve(), // 跳转成功（注意：不代表页面已加载完成）
            fail: (err) => {
                console.error('跳转失败', err);
                resolve(); // 或 reject(err)
            }
        });
    });
}

export default uni.$u.http;