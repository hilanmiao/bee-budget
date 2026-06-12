<template>
  <view class="page">

    <view class="header"
          :style="{background:'linear-gradient(to left top,'+primaryColor+','+freeSpecsButtonBackground+')',paddingTop:systemInfo.navBarH+'px'}">
      <view class="header__body">
        <block v-if="userInfo.user && userInfo.user.userName">
          <image class="header__avatar" mode="aspectFill"
                 :src="userInfo.user.avatar || '/static/logo.png'"></image>
          <view class="header__info" @click="onJump('/pages/user/set')">
            <view class="name">{{ userInfo.user.userName }}</view>
            <view class="description" v-if="userInfo.user.phoneNumber">手机号：{{ userInfo.user.phoneNumber }}</view>
            <view class="description" v-else>手机号:未绑定</view>
          </view>
        </block>
        <block v-else>
          <view class="header__avatar">
            <u-icon name="account-fill" color="#fff" size="30"></u-icon>
          </view>
          <view class="header__info" @click="openLogin">
            <view class="description">登录后享受更好的服务体验</view>
          </view>
        </block>
        <u-icon name="arrow-right" color="#fff" size="13"></u-icon>
      </view>
    </view>

    <view class="main">
      <view class="box">
        <view class="box__header">
          <view class="title">功能与服务</view>
          <!--              <view class="word">全部订单</view>-->
          <!--              <u-icon name="arrow-right" :size="13" color="#999"></u-icon>-->
        </view>
        <u-grid :col="5" :border="false">
          <u-grid-item @click="onJump('/pages/ledger/index')">
            <view class="menu">
              <my-icon name="book-fill" size="56" :color="primaryColor"></my-icon>
              <view class="menu__label">账本管理</view>
            </view>
          </u-grid-item>
          <u-grid-item @click="onJump('/pages/transaction-category/index')">
            <view class="menu">
              <my-icon name="appstoreadd" size="56" :color="primaryColor"></my-icon>
              <view class="menu__label">分类管理</view>
            </view>
          </u-grid-item>
        </u-grid>
      </view>

      <image class="illustration" mode="aspectFill" src="/static/line-chart.png"></image>
      <u--text type="info" align="center" text="更多功能正在开发中"></u--text>

    </view>

    <my-tabbar></my-tabbar>
  </view>
</template>

<script>
import {mapState} from 'vuex';
import myTabbar from '@/components/MyTabbar';
import {systemInfo} from "@/config/index";

export default {
  components: {
    myTabbar
  },
  computed: {
    ...mapState(['primaryColor', 'userInfo']),
    freeSpecsButtonBackground() {
      return this.$u.colorToRgba(this.$u.rgbToHex(this.primaryColor), 0.75)
    },
  },
  data() {
    return {
      systemInfo: systemInfo,
      scrollTop: 0,
    }
  },
  onLoad() {
    // 隐藏原生的tabbar
    uni.hideTabBar();
  },
  methods: {
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    openLogin() {
      uni.reLaunch({url: '/pages/user/login'})
    },
  },
  onPageScroll(e) {
    this.scrollTop = e.scrollTop;
  },
}
</script>

<style lang="scss" scoped>
.page {
  // 这里设置高度，上拉显示菜单栏---正式环境删除
  //min-height: 2000rpx;
}

.header {
  padding-top: 128rpx;
  background: linear-gradient(to left top, #f32735, #fc674d);
  border-radius: 50% / 0 0 5% 5%;
  overflow: hidden;

  &__body {
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 0 20rpx 40rpx 30rpx;
  }

  &__avatar {
    width: 60px;
    height: 60px;
    border-radius: 30px;
    background-color: #ccc;
    margin-right: 20rpx;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  &__info {
    flex: 1;
  }

  .name {
    color: #fff;
  }

  .description {
    color: #fff;
    font-size: 28rpx;
    padding-top: 6rpx;
  }
}

.main {
  padding: 0 24rpx;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
}

.box {
  width: 100%;
  background: #fff;
  padding: 0 24rpx;
  border-radius: 20rpx;
  overflow: hidden;
  margin-top: 24rpx;

  .box__header {
    padding: 32rpx 0;
    border-bottom: 1rpx solid #eee;

    .title {
      font-size: 28rpx;
      //font-weight: bold;
    }

    .word {
      font-size: 24rpx;
      color: #999;
    }
  }

  .menu {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 30rpx 0;
  }

  .menu__label {
    font-size: 24rpx;
    color: #333;
    padding-top: 10rpx;
  }

}
</style>
