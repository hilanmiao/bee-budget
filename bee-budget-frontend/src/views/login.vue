<template>
  <div class="login">
    <div class="left">
      <img class="img-background" src="@/assets/images/login-mesh@2x.png" alt="">
      <!--      <img class="img-logo" src="@/assets/images/logo-white.png" alt="">-->
      <!--      <img class="img-logo-text" src="@/assets/images/logo-text-white.png" alt="">-->
      <!--      <img class="img-slogan-1" src="@/assets/images/slogan-1.png" alt="">-->
      <!--      <img class="img-slogan-1-en" src="@/assets/images/slogan-1-en.png" alt="">-->
      <!--      <img class="img-slogan-2" src="@/assets/images/slogan-2.png" alt="">-->
      <!--      <img class="img-slogan-2-en" src="@/assets/images/slogan-2-en.png" alt="">-->
    </div>
    <div class="right">
      <div class="download">
        <el-button plain class="el-button--primary" icon="iphone" round>
          下载App
        </el-button>
      </div>

      <img class="power-plant" src="@/assets/logo/logo2.png" alt="">
      <!--      <img class="system-name" src="@/assets/images/system-name.png" alt="">-->
      <!--      <img class="system-name-en" src="@/assets/images/system-name-en.png" alt="">-->
      <h2>蜜蜂记账</h2>

      <div class="tabs">
        <div class="tabs__item tabs__item--active">密码登录</div>
        <div class="tabs__item">短信登录</div>
        <div class="tabs__item">扫码登录</div>
      </div>

      <el-form ref="loginRef" :model="loginForm" :rules="loginRules" class="login-form">
        <!--        <h3 class="title">后台管理系统</h3>-->
        <el-form-item prop="username">
          <el-input
              v-model="loginForm.username"
              type="text"
              size="large"
              auto-complete="off"
              placeholder="账号"
          >
            <template #prefix>
              <svg-icon icon-class="user" class="el-input__icon input-icon"/>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item prop="password">
          <el-input
              v-model="loginForm.password"
              type="password"
              size="large"
              auto-complete="off"
              placeholder="密码"
              @keyup.enter="onLogin"
          >
            <template #prefix>
              <svg-icon icon-class="password" class="el-input__icon input-icon"/>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item prop="captchaContent" v-if="captchaEnabled">
          <el-input
              v-model="loginForm.captchaContent"
              size="large"
              auto-complete="off"
              placeholder="验证码"
              @keyup.enter="onLogin"
          >
            <template #prefix>
              <svg-icon icon-class="validCode" class="el-input__icon input-icon"/>
            </template>
          </el-input>
          <div class="login-captcha">
            <img :src="captchaContentUrl" @click="getCaptchaImg" class="login-captcha-img"/>
          </div>
        </el-form-item>
        <!--        <el-checkbox v-model="loginForm.rememberMe" style="margin:0px 0px 25px 0px;">记住密码</el-checkbox>-->
        <!--        <div class="forget">-->
        <!--          <el-text type="primary">忘记密码？</el-text>-->
        <!--        </div>-->

        <el-form-item style="width:100%;">
          <el-button
              :loading="loading"
              size="large"
              type="primary"
              icon="right"
              round
              style="width:100%;"
              @click.prevent="onLogin"
          >
            <span v-if="!loading">登 录</span>
            <span v-else>登 录 中...</span>
          </el-button>
          <div style="float: right;" v-if="register">
            <router-link class="link-type" :to="'/register'">立即注册</router-link>
          </div>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script setup>
// --- 框架工具等相关 ---
import Cookies from "js-cookie";
import {encrypt, decrypt} from "@/utils/jsencrypt";
import {ElMessage} from 'element-plus'
import useUserStore from '@/store/modules/user'

const userStore = useUserStore()
const route = useRoute();
const router = useRouter();

// --- api 相关 ----
import {getCaptcha} from "@/api/common.js";

// --- 表单相关 ---
const activeName = ref('first')
const loginRef = ref(null)
const loginForm = ref({
  username: "",
  password: "",
  rememberMe: false,
  captchaContent: "",
  captchaId: ""
});
const loginRules = {
  username: [{required: true, trigger: "blur", message: "请输入您的账号"}],
  password: [{required: true, trigger: "blur", message: "请输入您的密码"}],
  captchaContent: [{required: true, trigger: "change", message: "请输入验证码"}]
};
const captchaContentUrl = ref("");
const loading = ref(false);
const captchaEnabled = ref(true);
const register = ref(false);
const redirect = ref(undefined);

watch(route, (newRoute) => {
  redirect.value = newRoute.query && newRoute.query.redirect;
}, {immediate: true});

function onLogin() {
  loginRef.value.validate(valid => {
    if (valid) {
      loading.value = true;
      // 勾选了需要记住密码设置在 cookie 中设置记住用户名和密码
      if (loginForm.value.rememberMe) {
        Cookies.set("username", loginForm.value.username, {expires: 30});
        Cookies.set("password", encrypt(loginForm.value.password), {expires: 30});
        Cookies.set("rememberMe", loginForm.value.rememberMe, {expires: 30});
      } else {
        // 否则移除
        Cookies.remove("username");
        Cookies.remove("password");
        Cookies.remove("rememberMe");
      }
      // 调用action的登录方法
      userStore.login(loginForm.value).then(() => {
        const query = route.query;
        const otherQueryParams = Object.keys(query).reduce((acc, cur) => {
          if (cur !== "redirect") {
            acc[cur] = query[cur];
          }
          return acc;
        }, {});
        router.push({path: redirect.value || "/", query: otherQueryParams});
      }).catch(e => {
        // ElMessage.error(e.message)
        loading.value = false;
        // 重新获取验证码
        if (captchaEnabled.value) {
          getCaptchaImg()
        }
      });
    }
  });
}

function getCaptchaImg() {
  getCaptcha().then(response => {
    const {code, success, data, message} = response
    if (code === 200 && success) {
      const {captchaId, imageBase64} = data
      captchaContentUrl.value = imageBase64
      loginForm.value.captchaId = captchaId
      // ElMessage.success(message)
    } else {
      ElMessage.error(message)
    }
  })
}

function getCookie() {
  const username = Cookies.get("username");
  const password = Cookies.get("password");
  const rememberMe = Cookies.get("rememberMe");
  loginForm.value = {
    username: username === undefined ? loginForm.value.username : username,
    password: password === undefined ? loginForm.value.password : decrypt(password),
    rememberMe: rememberMe === undefined ? false : Boolean(rememberMe)
  };
}

getCaptchaImg()
getCookie()
</script>

<style lang='scss' scoped>
.login {
  width: 100%;
  height: 100%;
  display: flex;
}

.title {
  margin: 0px auto 30px auto;
  text-align: center;
  color: #707070;
}

.login-form {
  border-radius: 6px;
  background: #ffffff;
  width: 400px;
  padding: 25px 25px 5px 25px;

  .el-input {
    flex: 1;
    height: 40px;

    input {
      height: 40px;
    }
  }

  .input-icon {
    height: 39px;
    width: 14px;
    margin-left: 0px;
  }

  .forget {
    text-align: right;
    margin-bottom: 18px;
  }

  ::v-deep {
    .el-input {
      &__wrapper {
        border-radius: var(--el-border-radius-round);
      }
    }
  }

  .el-button {
    background-color: #1977f0;
    border-color: #1977f0;
  }

}

.login-tip {
  font-size: 13px;
  text-align: center;
  color: #bfbfbf;
}

.login-captcha {
  //width: 40%;
  height: 40px;
  float: right;

  img {
    cursor: pointer;
    vertical-align: middle;
  }
}

.el-login-footer {
  height: 40px;
  line-height: 40px;
  position: fixed;
  bottom: 0;
  width: 100%;
  text-align: center;
  color: #fff;
  font-family: Arial;
  font-size: 12px;
  letter-spacing: 1px;
}

.login-captcha-img {
  height: 40px;
  padding-left: 12px;
}

.left {
  width: 50%;
  height: 100%;
  //background-image: url("../assets/images/login-mesh@2x.png");
  //background-size: 100% 100%;
  .img-background {
    //height: 100%;
    width: 100%;
  }

  .img-logo {
    position: absolute;
    top: 24px;
    left: 24px;
    height: 60px;
  }

  .img-logo-text {
    position: absolute;
    top: 32px;
    left: 134px;
    height: 18px;
  }

  .img-slogan-1 {
    position: absolute;
    top: 50%;
    left: 140px;
    height: 80px;
    transform: translateY(-80px);
  }

  .img-slogan-1-en {
    position: absolute;
    top: 50%;
    left: 140px;
    height: 12px;
    transform: translateY(4px);
  }

  .img-slogan-2 {
    position: absolute;
    top: 50%;
    left: 240px;
    height: 80px;
    transform: translateY(60px);
  }

  .img-slogan-2-en {
    position: absolute;
    top: 50%;
    left: 240px;
    height: 12px;
    transform: translateY(80+60+4px);
  }

}

.right {
  width: 50%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;

  .download {
    position: absolute;
    top: 24px;
    right: 24px;

    .el-button {
      background-color: transparent;

      &:hover {
        color: var(--el-color-primary);
      }
    }
  }

  .power-plant {
    //width: 440px;
    height: 180px;
  }

  .tabs {
    width: 350px;
    height: 40px;
    margin-top: 48px;
    display: flex;
    align-items: center;
    background-color: var(--el-fill-color);
    border-radius: 22px;

    &__item {
      flex: 1;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--el-text-color-secondary);
      font-size: var(--el-font-size-base);
      border-radius: 22px;
      cursor: pointer;

      &:hover {
        color: #ffffff;
        background-color: #1977f0;
      }
    }

    &__item--active {
      color: #ffffff;
      background-color: #1977f0;
    }
  }

  .system-name {
    height: 24px;
    margin-top: 48px;
  }

  .system-name-en {
    height: 6px;
    margin-top: 8px;
  }
}

</style>
