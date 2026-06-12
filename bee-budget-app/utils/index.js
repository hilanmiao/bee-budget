// App计算缓存
function formatSize(callback){
    let fileSizeString = "0B";
    plus.cache.calculate(function(size) {
        let sizeCache = parseInt(size);
        if (sizeCache === 0) {
            fileSizeString = "0B";
        } else if (sizeCache < 1024) {
            fileSizeString = sizeCache + "B";
        } else if (sizeCache < 1048576) {
            fileSizeString = (sizeCache / 1024).toFixed(2) + "KB";
        } else if (sizeCache < 1073741824) {
            fileSizeString = (sizeCache / 1048576).toFixed(2) + "MB";
        } else {
            fileSizeString = (sizeCache / 1073741824).toFixed(2) + "GB";
        }
        callback && callback(fileSizeString)
    });
}
/*
 *  App清理缓存
 *  fileSizeString:当前缓存
 */
function clearCache(fileSizeString){
    if(fileSizeString === '0B'){
        // uni.showToast({
        //     title: '暂无缓存',
        //     duration: 2000,
        //     icon: 'none'
        // });
        return
    }
    return new Promise((resolve, reject) => {
        let os = plus.os.name;
        if (os === 'Android') {
            let main = plus.android.runtimeMainActivity();
            let sdRoot = main.getCacheDir();
            let files = plus.android.invoke(sdRoot, "listFiles");
            let len = files.length;
            for (let i = 0; i < len; i++) {
                let filePath = '' + files[i]; // 没有找到合适的方法获取路径，这样写可以转成文件路径
                plus.io.resolveLocalFileSystemURL(filePath, function(entry) {
                    if (entry.isDirectory) {
                        entry.removeRecursively(function(entry) { //递归删除其下的所有文件及子目录
                            uni.showToast({
                                title: '缓存清理完成',
                                duration: 2000
                            });
                            // 回调
                            resolve()
                        }, function(e) {
                            console.log(e.message)
                        });
                    } else {
                        entry.remove();
                    }
                }, function(e) {
                    console.log('文件路径读取失败')
                });
            }
        } else { // ios
            plus.cache.clear(function() {
                uni.showToast({
                    title: '缓存清理完成',
                    duration: 2000
                });
                // 回调
                resolve()
            });
        }
    });
}

export {
    formatSize,       
    clearCache
}
