<template>
  <view class="page">
    <u-navbar
        title="设置"
        @leftClick="navigateBack"
        safeAreaInsetTop
        fixed
        placeholder
    ></u-navbar>

    <u-cell-group :border="false">
      <u-cell title="头像" :isLink="true" :border="false" @click="upAvatar">
        <view slot="value">
          <u-avatar :src="user.avatar || '/static/logo.png'" shape="square"></u-avatar>
        </view>
      </u-cell>
    </u-cell-group>

    <u-cell-group :border="false">
      <u-cell title="密码" :isLink="true" @click="onJump('/pages/user/editPassword')">>
      </u-cell>
      <u-cell title="昵称" :isLink="true" :value="user.nickName" @click="onJump('/pages/user/editNickName')">
      </u-cell>
      <u-cell title="邮箱" :isLink="true" :value="user.email" @click="onJump('/pages/user/editNickName')">
      </u-cell>
      <u-cell title="手机号" :isLink="true" :border="false" :value="user.phoneNumber" @click="onJump('/pages/user/editNickName')">
      </u-cell>
    </u-cell-group>

    <!-- #ifdef APP-PLUS -->
    <u-cell-group :border="false">
      <u-cell title="检查更新" :isLink="true" @click="checkAppUpdate">
        <view slot="value" class="u-slot-value">
          <text v-if="versionInfo.versionName">{{ versionInfo.versionName }}</text>
        </view>
      </u-cell>
      <u-cell title="缓存大小" :isLink="true" :border="false" :value="fileSizeString" @click="appClearCache">
        <view slot="right-icon">
          <u-icon name="trash" :size="18" color="#909399"></u-icon>
        </view>
      </u-cell>
    </u-cell-group>
    <!-- #endif -->

    <u-cell-group :border="false">
      <u-cell title="" :border="false" @click="showLogout = true">
        <view slot="value" class="u-slot-value-center">
          退出登录
        </view>
      </u-cell>
    </u-cell-group>

    <u-modal :show="showLogout" title="确定要退出当前账户？" :closeOnClickOverlay="true" showCancelButton
             @close="showLogout = false" @cancel="showLogout = false" @confirm="handleLogout"></u-modal>

  </view>
</template>

<script>
import {mapState, mapActions} from 'vuex';

// #ifdef APP-PLUS
import {formatSize, clearCache} from '@/utils/index';
import APPUpdate from '@/uni_modules/zhouWei-APPUpdate/js_sdk/appUpdate';
// #endif

export default {
  computed: {
    ...mapState(['userInfo', 'token']),
    user() {
      return this.userInfo.user || {}
    }
  },
  data() {
    return {
      fileSizeString: '0B',//App缓存大小
      versionInfo: {},//版本信息
      // phoneNum: this.user.phonenumber || '',
      avatar: require('@//static/logo.png'),
      showLogout: false
    }
  },
  onLoad() {
    // #ifdef APP-PLUS
    this.appFormatSize() //计算app缓存大小
    this.getCurrentNo() //获取版本号
    // #endif
  },
  onShow() {
  },
  methods: {
    ...mapActions(['logout']),
    navigateBack() {
      uni.navigateBack()
    },
    onJump(url) {
      uni.navigateTo({
        url: url
      });
    },
    async handleLogout() {
      console.log('logout')
      await this.logout();
    },
    // 修改头像
    upAvatar() {
      var that = this
      uni.chooseImage({
        count: 1,
        success: (res) => {
          const tempFilePaths = res.tempFilePaths;
          // uni.$u.http.upload('api/upload/img', {
          //     filePath:tempFilePaths[0],
          //     name:'avatar',
          // }).then(res => {
          //     that.avatar = res
          // })
        }
      })
    },
    checkAppUpdate() {
      APPUpdate()
    },
    // App计算缓存
    appFormatSize() {
      let that = this;
      formatSize(res => {
        that.fileSizeString = res
      })
    },
    // App清理缓存
    appClearCache() {
      console.log('清除缓存--')
      let that = this;
      clearCache(this.fileSizeString).then(() => {
        that.appFormatSize()
      })
    },
    // 获取当前应用的版本号
    getCurrentNo(callback) {
      var that = this
      // 获取本地应用资源版本号
      plus.runtime.getProperty(plus.runtime.appid, function (inf) {
        that.versionInfo = {
          versionCode: inf.versionCode,
          versionName: inf.version
        }
      });
    }

  }
};
</script>

<style lang="scss">
.page {
  padding: 30rpx;
}

.u-slot-value {
  text-align: right;
  font-size: 28rpx;
  line-height: 48rpx;
  color: $u-content-color;
}

.u-cell-group {
  background-color: #ffffff;
  margin-bottom: 30rpx;
  border-radius: 20rpx;
   ::v-deep .u-cell {
     &__body {

     }
     .u-line {
       width: calc(100% - 60rpx) !important;
       margin-left: 15px !important;
       margin-right: 15px !important;
     }
   }
}

.u-slot-value-center {
  width: 100%;
  text-align: center;
}
</style>
