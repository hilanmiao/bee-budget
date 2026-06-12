const platform = uni.getSystemInfoSync().platform;
export default {
    // 发起ajax请求获取服务端版本号
    getServerNo: (version, isPrompt = false, callback) => {
        let httpData = {
            appid: version.appid,
            // appid: '__UNI__ADABFF0',
            version: version.versionCode,
            // 版本名称
            versionName: version.versionName,
            // setupPage参数说明（判断用户是不是从设置页面点击的更新，如果是设置页面点击的更新，有不要用静默更新了，不然用户点击没反应很奇怪的）
            setupPage: isPrompt
        };
        if (platform == "android") {
            httpData.type = 1101;
        } else {
            httpData.type = 1102;
        }
        /* 接口入参说明
         * version: 应用当前版本号（已自动获取）
         * versionName: 应用当前版本名称（已自动获取）
         * type：平台（1101是安卓，1102是IOS）
         */
        /****************以下是示例*******************/
        // 可以用自己项目的请求方法（接口自己找后台要，插件不提供）
        // uni.$u.http.post("api/common/v1/app_version", httpData,{
        //     isPrompt: isPrompt
        // }).then(res => {
        /* res的数据说明
         * | 参数名称        | 一定返回     | 类型        | 描述
         * | -------------|--------- | --------- | ------------- |
         * | versionCode     | y        | int       | 版本号        |
         * | versionName     | y        | String    | 版本名称      |
         * | versionInfo     | y        | String    | 版本信息      |
         * | updateType      | y        | String    | forcibly = 强制更新, solicit = 弹窗确认更新, silent = 静默更新 |
         * | downloadUrl     | y        | String    | 版本下载链接（IOS安装包更新请放跳转store应用商店链接,安卓apk和wgt文件放文件下载链接）  |
         */
        // 参数示例
        // var res = {
        //     downloadUrl:'https://api.yichengshi.cn//minImage/h5/__UNI__2B3105B.wgt',
        //     updateType:'solicit',
        //     versionCode:'100',
        //     versionName:'1.0.1', //版本号必须高于上一个版本----打包wgt时manifest.json的版本名称必须增加
        //     versionInfo:'测试升级'
        // }
        // if (res && res.downloadUrl) {
        //     // 兼容之前的版本（updateType是新版才有的参数）
        //     if(res.updateType){
        //         callback && callback(res);
        //     } else {
        //         if(res.forceUpdate){
        //             res.updateType = "forcibly";
        //         } else {
        //             res.updateType = "solicit";
        //         }
        //         callback && callback(res);
        //     }
        // } else if (isPrompt) {
        //     uni.showToast({
        //         title: "暂无新版本",
        //         icon: "none"
        //     });
        // }
        // });
        /****************以上是示例*******************/
        const baseUrl = 'http://localhost:9004'
		// 测试代码
		// httpData.appid = '__UNI__9B0E754';
        // 检测app是否可用
        uni.request({
            url: baseUrl + '/api/app/by-app-id/' + httpData.appid,
            method: 'GET',
            success: (res) => {
                console.log('检测是否可用：', res)
                if (res.data.code !== 200 || !res.data.success || !res.data.data) return
                const data = res.data.data
                if(!data.isEnabled) {
                    uni.showModal({
                        title: '应用已经禁用',
                        showCancel: false,
                        success: res => {
                            if (res.confirm) {
                                // 退出
                                plus.runtime.quit();
                            }
                        }
                    });

                    return
                }

                getMaxVersion()
            },
            fail: (err) => {
				console.log('检测是否可用失败：', err)
            }
        })

        function getMaxVersion () {
            // 检查app版本
            uni.request({
                url: baseUrl + '/api/app-version/max-version/' + httpData.appid,
                method: 'GET',
                success: (res) => {
                    console.log('检测版本：', res)
                    if (res.data.code !== 200 || !res.data.success || !res.data.data) return
                    const data = res.data.data
                    if (!(data.versionCode > parseInt(httpData.version) && data.isStablePublish)) return
                    var callbackData = {
                        downloadUrl: baseUrl + '/' + data.url,
                        updateType: 'solicit',
                        versionCode: data.versionCode,
                        versionName: data.versionName,
                        versionInfo: data.contents
                    }
                    if (data.isMandatory) {
                        callbackData.updateType = 'forcibly'
                    }
                    if (data.isSilently) {
                        callbackData.updateType = 'silent'
                    }

                    if (callbackData && callbackData.downloadUrl) {
                        // 兼容之前的版本（updateType是新版才有的参数）
                        if (callbackData.updateType) {
                            callback && callback(callbackData);
                        } else {
                            if (callbackData.forceUpdate) {
                                callbackData.updateType = "forcibly";
                            } else {
                                callbackData.updateType = "solicit";
                            }
                            callback && callback(callbackData);
                        }
                    } else if (isPrompt) {
                        uni.showToast({
                            title: "暂无新版本",
                            icon: "none"
                        });
                    }
                },
                fail: (err) => {
					console.log('检测版本失败：', err)
                }
            })
        }
    },
    // 弹窗主颜色（不填默认粉色）
    appUpdateColor: "6495ed",
    // 弹窗图标（不填显示默认图标，链接配置示例如： '/static/demo/ic_attention.png'）
    appUpdateIcon: ''
}
