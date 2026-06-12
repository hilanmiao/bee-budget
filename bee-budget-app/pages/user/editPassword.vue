<template>
  <view class="page">
    <u-navbar
        title="修改密码"
        @leftClick="navigateBack"
        safeAreaInsetTop
        fixed
        placeholder
    ></u-navbar>

    <view class="box-form">
      <u--form
          labelPosition="left"
          labelWidth="80"
          :model="formData"
          ref="formRef"
      >
        <u-form-item
            label="旧密码"
            prop="oldPassword"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="formData.oldPassword"
              border="none"
              placeholder="请输入旧密码"
              type="password"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="新密码"
            prop="newPassword"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="formData.newPassword"
              border="none"
              placeholder="请输入新密码"
              type="password"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="确认密码"
            prop="confirmPassword"
            ref="item1"
        >
          <u--input
              v-model="formData.confirmPassword"
              border="none"
              placeholder="请输入新密码"
              type="password"
          ></u--input>
        </u-form-item>
      </u--form>
    </view>

    <view class="box-button">
      <u-button
          shape="circle"
          type="primary"
          text="提交"
          @click="submit"
      ></u-button>
    </view>

  </view>
</template>

<script>
import { updateUserPassword } from '@/api/auth'

export default {
  data() {
    return {
      formData: {
        oldPassword: '',
        newPassword: '',
        confirmPassword: '',
      },
      rules: {
        'oldPassword': {
          type: 'string',
          required: true,
          message: '请输入旧密码',
          trigger: ['blur', 'change']
        },
        'newPassword': {
          type: 'string',
          required: true,
          message: '请输入新密码',
          trigger: ['blur', 'change']
        },
        'confirmPassword': [
          {
            type: 'string',
            required: true,
            message: '请输入新密码',
            trigger: ['blur', 'change']
          },
          {
            asyncValidator: (rule, value, callback) => {
              if (value !== this.formData.newPassword) {
                // 如果验证不通过，需要在callback()抛出new Error('错误提示信息')
                callback(new Error('两次输入的密码不一致'))
              } else {
                // 如果校验通过，也要执行callback()回调
                callback()
              }
            },
            // 触发器可以同时用blur和change，二者之间用英文逗号隔开
            trigger: ["blur"],
          }
        ],
      }
    }
  },
  computed: {
  },
  onReady() {
    // 如果需要兼容微信小程序，并且校验规则中含有方法等，只能通过setRules方法设置规则
    this.$refs.formRef.setRules(this.rules)
  },
  methods: {
    navigateBack() {
      uni.navigateBack()
    },
    async submit() {
      // 如果有错误，会在catch中返回报错信息数组，校验通过则在then中返回true
      try {
        await this.$refs.formRef.validate()

        let httpData = {
          oldPassword: this.formData.oldPassword,
          newPassword: this.formData.newPassword,
        }
        const response = await updateUserPassword(httpData)
        const {success, data, message} = response
        if (success) {
          uni.$u.toast('提交成功')
          setTimeout(() => {
            // uni.reLaunch({url: '/pages/my/myInfo'})
            this.navigateBack()
          }, 300)
        } else {
          uni.$u.toast(message)
        }
      } catch (e) {

      }
    },
  },
}
</script>

<style lang="scss">
.page {
  padding: 30rpx;
}

.box-form {
  background-color: #ffffff;
  margin-bottom: 15px;
  border-radius: 10px;
  padding: 8rpx 30rpx;
}

.box-button {
  //margin-top: 80rpx;

  .u-button {
    margin-top: 32rpx;
    background-color: $theme-color-primary-brand;
    border-color: $theme-color-primary-brand;
  }
}

</style>