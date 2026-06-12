import { getTabbarHeight } from '@/utils';
let baseUrl = '/h5';
let baseApiUrl = '';
if (process.env.NODE_ENV === 'development') {
	// 开发环境
    
    // #ifdef H5
    baseApiUrl = baseUrl + '/dev-api'
    // #endif
    
    // #ifdef APP
    baseApiUrl = 'http://localhost:50003/api'
    // #endif
} else if (process.env.NODE_ENV === 'production') {
	// 生产环境
    
    // #ifdef H5
    // baseApiUrl = '/dev-api'
    baseApiUrl = baseUrl + '/prod-api'
    // #endif
    
    // #ifdef APP
    baseApiUrl = 'http://localhost:50003/api'
    // #endif
}

let systemInfo = {
    tabbarH: 50, // tabbar高度--单位px
    navBarH: uni.getSystemInfoSync().statusBarHeight + 44, // 菜单栏总高度--单位px
    titleBarHeight: 44, // 标题栏高度--单位px
};

// 平台
// #ifdef MP-WEIXIN
systemInfo.platform = 'weixin'
// #endif
// #ifdef MP-ALIPAY
systemInfo.platform = 'alipay'
// #endif
// #ifdef MP-TOUTIAO
systemInfo.platform = 'toutiao'
// #endif
// #ifdef APP-PLUS
systemInfo.platform = 'plus'
// #endif
console.log(systemInfo,'systemInfo')

export
{
    baseUrl, 
    baseApiUrl, 
    systemInfo
}