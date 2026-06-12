<template>
  <view class="page">
    <u-navbar
        title="账本详情"
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
            prop="userInfo.name"
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
          text="修改"
          @click="submit"
      ></u-button>
      <u-button
          shape="circle"
          type="error"
          text="删除"
          @click="showDelete = true"
      ></u-button>
    </view>

    <u-modal class="delete-modal" :show="showDelete" title="确定删除当前账本？" :closeOnClickOverlay="true"
             showCancelButton
             @close="showDelete = false" @cancel="showDelete = false" @confirm="handleDelete">
      <view class="slot-content">
      </view>
    </u-modal>

  </view>
</template>

<script>
import {mapState} from 'vuex';
import {updateLedger, deleteLedger } from '@/api/ledger';

export default {
  components: {},
  data() {
    return {
      // 分段器
      form: {
      },
      detail: {},
      showDelete: false,
    }
  },
  computed: {
    ...mapState(['primaryColor', 'userInfo']),
  },
  async onLoad(e) {
    //获取传递过来的参数
    this.detail = JSON.parse(e.detail)
    this.form = { ...this.detail }
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

      const response = await updateLedger(httpData)
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
    async handleDelete() {
      let httpData = {
        id: this.form.id
      }

      const response = await deleteLedger(httpData)
      const {success, data, message} = response
      if (success) {
        uni.$u.toast('删除成功')
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
  display: flex;
  align-items: center;
  justify-content: space-between;
  .u-button {
    margin-top: 32rpx;
    &:first-child {
      background-color: $theme-color-primary-brand;
      border-color: $theme-color-primary-brand;
      //width: 60%;
      margin-right: 32rpx;
    }
    &:last-child {
      //width: 40%;
      //margin-left: 16px;
    }
  }
}

.title {
  padding: 4px 0;
  color: $theme-color-primary-brand;
}

</style>
