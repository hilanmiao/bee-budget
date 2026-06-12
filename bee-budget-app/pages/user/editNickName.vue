<template>
  <view class="page">
    <u-navbar
        title="编辑资料"
        @leftClick="navigateBack"
        safeAreaInsetTop
        fixed
        placeholder
    ></u-navbar>

    <view class="box-form">
      <u--form
          labelPosition="left"
          :model="formData"
          ref="formRef"
      >
        <u-form-item
            label="昵称"
            prop="nickName"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="formData.nickName"
              border="none"
              placeholder="请输入昵称"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="手机"
            prop="phoneNumber"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="formData.phoneNumber"
              border="none"
              placeholder="请输入手机号"
              type="number"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="邮箱"
            prop="email"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="formData.email"
              border="none"
              placeholder="请输入邮箱"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="性别"
            prop="sex"
            @click="showSex = true; hideKeyboard()"
            ref="item1"
        >
          <u--input
              v-model="formData.sex"
              placeholder="请选择性别"
              border="none"
          ></u--input>
          <u-icon
              slot="right"
              name="arrow-right"
          ></u-icon>
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

    <u-action-sheet
        :show="showSex"
        :actions="actions"
        title="请选择性别"
        @close="showSex = false"
        @select="sexSelect"
    >
    </u-action-sheet>

  </view>
</template>

<script>
import {mapState, mapMutations} from 'vuex';
import {updateUserInfo} from "@/api/auth";

export default {
  data() {
    return {
      formData: {
        nickName: '',
        sex: '',
        phoneNumber: '',
        email: ''
      },
      showSex: false,
      actions: [
        {
          name: '男',
          value: '0',
        },
        {
          name: '女',
          value: '1'
        },
      ],
      rules: {
        'nickName': [
          {
            type: 'string',
            required: true,
            message: '请填写昵称',
            trigger: ['blur', 'change']
          },
          // {
          //   // 此为同步验证，可以直接返回true或者false，如果是异步验证，稍微不同，见下方说明
          //   validator: (rule, value, callback) => {
          //     // 调用uView自带的js验证规则，详见：https://www.uviewui.com/js/test.html
          //     return uni.$u.test.chinese(value);
          //   },
          //   message: "昵称必须为中文",
          //   // 触发器可以同时用blur和change，二者之间用英文逗号隔开
          //   trigger: ["change", "blur"],
          // }
        ],
        'sex': {
          type: 'string',
          max: 1,
          required: true,
          message: '请选择男或女',
          trigger: ['blur', 'change']
        },
        'phoneNumber': [
          {
            type: 'string',
            required: true,
            message: '请填写手机号',
            trigger: ['blur', 'change']
          },
          {
            asyncValidator: (rule, value, callback) => {
              if (!uni.$u.test.mobile(value)) {
                // 如果验证不通过，需要在callback()抛出new Error('错误提示信息')
                callback(new Error('手机号码格式不正确'))
              } else {
                // 如果校验通过，也要执行callback()回调
                callback()
              }
            },
            // message: "邮箱格式不正确",
            // 触发器可以同时用blur和change，二者之间用英文逗号隔开
            trigger: ["blur"],
          }
        ],
        'email': [
          {
            type: 'string',
            required: true,
            message: '请填写邮箱',
            trigger: ['blur', 'change']
          },
          {
            asyncValidator: (rule, value, callback) => {
              if (!uni.$u.test.email(value)) {
                // 如果验证不通过，需要在callback()抛出new Error('错误提示信息')
                callback(new Error('邮箱格式不正确'))
              } else {
                // 如果校验通过，也要执行callback()回调
                callback()
              }
            },
            // message: "邮箱格式不正确",
            // 触发器可以同时用blur和change，二者之间用英文逗号隔开
            trigger: ["blur"],
          }
        ],
      }
    }
  },
  computed: {
    ...mapState(['userInfo'])
  },
  onReady() {
    // 如果需要兼容微信小程序，并且校验规则中含有方法等，只能通过setRules方法设置规则
    this.$refs.formRef.setRules(this.rules)

    const {nickName, phoneNumber, email, sex} = this.userInfo.user
    this.formData.nickName = nickName
    this.formData.phoneNumber = phoneNumber
    this.formData.email = email
    this.formData.sex = (this.actions.find(o => o.value === sex)).name
  },
  methods: {
    ...mapMutations(['setUserInfo']),
    navigateBack() {
      uni.navigateBack()
    },
    sexSelect(e) {
      this.formData.sex = e.name
      this.$refs.formRef.validateField('sex')
    },
    async submit() {
      // 如果有错误，会在catch中返回报错信息数组，校验通过则在then中返回true
      try {
        await this.$refs.formRef.validate()
        let httpData = {
          nickName: this.formData.nickName,
          phoneNumber: this.formData.phoneNumber,
          email: this.formData.email,
          sex: (this.actions.find(o => o.name === this.formData.sex)).value
        };
        const response = await updateUserInfo(httpData)
        const {success, data, message} = response
        if (success) {
          uni.$u.toast('提交成功')
          // 更新用户信息
          const userInfo = {
            ...this.userInfo,
            user: {
              ...this.userInfo.user,
              ...httpData
            }
          }
          this.setUserInfo(userInfo)
          console.log(9999, this.userInfo)
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
    hideKeyboard() {
      uni.hideKeyboard()
    }
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
