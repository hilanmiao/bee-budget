<template>
  <view class="page">
    <view class="header">
      <image class="img" mode="aspectFit" src="/static/logo3.png"></image>
    </view>
    <view class="title">蜜蜂记账</view>
    <view class="body">
      <u--input
          placeholder="请输入用户名"
          shape="circle"
          border="surround"
          v-model="username"
      ></u--input>
      <u--input
          placeholder="请输入密码"
          type="password"
          shape="circle"
          border="surround"
          v-model="password"
      ></u--input>
    </view>
    <view class="actions">
      <u-button type="primary" shape="circle" icon="arrow-rightward" size="large" @click="onSubmit"></u-button>
    </view>
    <view class="footer">张国栋 hilanmiao@126.com</view>
  </view>
</template>

<script>
import {mapState, mapActions} from 'vuex';
import { getLoginInfo, setLoginInfo } from "@/utils/auth"

export default {
  data() {
    return {
      username: '', //账户名
      password: '', //验证码
      isAgree: true, //是否同意协议
      showPassword: false,
    };
  },
  computed: {
    ...mapState(['primaryColor', 'userInfo'])
  },
  //第一次加载
  onLoad(e) {
    const loginInfo = getLoginInfo()
    if(loginInfo) {
      this.username = loginInfo.username
      this.password = loginInfo.password
    }
  },
  //页面显示
  onShow() {
  },
  //方法
  methods: {
    ...mapActions(['login', 'getUserInfo']),
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    async onSubmit() {
      if (this.username === '') {
        uni.showToast({
          title: '请输入用户名',
          icon: 'none'
        });
        return;
      }
      if (this.password === '') {
        uni.showToast({
          title: '请输入密码',
          icon: 'none'
        });
        return;
      }
      try {
        const data = {username: this.username, password: this.password, captchaId: 'fromApp', captchaContent: 'none'}
        await this.login(data)
        await this.getUserInfo()
        setLoginInfo(data)

        uni.switchTab({url: '/pages/home/index'})
       } catch (e) {
        console.log(e);
      }
    },

  }
};
</script>
<style lang="scss" scoped>
.page {
  min-height: 100vh;
  padding: 40px 40px 40px;
  background-color: $theme-color-primary-brand;
  background-size: 100% 100%;
  background-repeat: no-repeat;
  background-position: center;
}

.header {
  display: flex;
  align-items: center;
  justify-content: center;
  padding-top: 60px;
  padding-bottom: 20px;

  .img {
    width: 160px;
    height: 160px;
  }
}

.title {
  color: #FFFFFF;
  //font-weight: bold;
  font-size: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.body {
  margin-top: 40px;

  .u-input {
    height: 56px;
    box-shadow: 0 2px 8px rgba(#ffffff, 0.3);
    border: none;
    background-color: #ffffff;

    ::v-deep .u-input__content__field-wrapper__field {
      //color: $theme-color-primary !important;
      font-size: 18px !important;
      text-align: center !important;
      //font-weight: bold;
    }

    ::v-deep .uni-input-input {
      font-size: 26px;
      //font-weight: bold;
    }

    &:last-child {
      margin-top: 20px;
    }
  }
}

.actions {
  margin-top: 50px;
  display: flex;
  align-items: center;
  justify-content: center;

  .u-button {
    width: 80px;
    height: 80px;
    box-shadow: 0 5px 10px rgba(#ffffff, 0.3);
    //background-color: $theme-color-primary;
    //border-color: $theme-color-primary;
    background-color: #FFFFFF;
    border-color: #FFFFFF;

    ::v-deep .u-icon__icon {
      font-size: 32px !important;
      color: $theme-color-primary-brand !important;
    }
  }
}

.footer {
  text-align: center;
  width: 100%;
  position: fixed;
  bottom: 16px;
  left: 0;
  color: #fff;
  font-size: 14px;
}
</style>
