<template>
  <view class="page">
    <u-navbar
        title="账本创建"
        @leftClick="navigateBack"
        safeAreaInsetTop
        fixed
        placeholder
        :bgColor="primaryColor"
        leftIconColor="#fff"
        :titleStyle="{color: '#fff'}"
    ></u-navbar>

    <view class="box-form">
      <u--form
          labelPosition="left"
          :model="form"
          ref="form"
          labelWidth="80"
      >
        <u-form-item
            label="名称"
            prop="userInfo.nickName"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="form.name"
              placeholder="请输入名称（最长8个字符）"
              maxlength="8"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="备注"
            prop="userInfo.name"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="form.remark"
              placeholder="请输入备注"
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
import {mapState} from 'vuex';
import {createLedger } from '@/api/ledger';

export default {
  components: {},
  data() {
    return {
      form: {
        name: '',
        remark: '',
      },
    }
  },
  computed: {
    ...mapState(['primaryColor']),
  },
  async onLoad(e) {
   
  },
  methods: {
    navigateBack() {
      uni.navigateBack()
    },
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    async submit() {
      if (!this.form.name) {
        uni.showToast({
          title: '请填写名称',
          icon: 'none'
        });
        return;
      }
      let httpData = {
        ...this.form,
      }

      const response = await createLedger(httpData)
      const {success, data, message} = response
      if (success) {
        uni.$u.toast('提交成功')
        setTimeout(()=>{
          this.onJump('/pages/ledger/index')
        },300)
      } else {
        uni.$u.toast(message)
      }
    },
    
  },
}
</script>

<style scoped lang="scss">
.page {
  padding: 36rpx;
  height: 100vh;
  background-color: #fff;
}


.box-button {
  margin-top: 80rpx;

  .u-button {
    margin-top: 32rpx;
    background-color: $theme-color-primary-brand;
    border-color: $theme-color-primary-brand;
  }
}

.title {
  padding: 4px 0;
  color: $theme-color-primary-brand;
}

</style>
