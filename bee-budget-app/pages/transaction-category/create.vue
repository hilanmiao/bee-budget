<template>
  <view class="page">
    <u-navbar
        title="交易分类创建"
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
              placeholder="请输入名称（最长6个字符）"
              maxlength="6"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="图标"
            prop="userInfo.nickName"
            borderBottom
            ref="item1"
        >
          <my-icon :name="form.icon" size="48" color="#999" @click="showIconPopup = true"></my-icon>
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

    <u-popup class="icon-popup" :show="showIconPopup" mode="bottom" @close="showIconPopup = false">
      <view class="header">
        <view class="tip">选择图标</view>
        <view class="close" @click="showIconPopup= false">
          <my-icon name="close" size="48" color="#333"></my-icon>
        </view>
      </view>
      <u-row justify="flex-start" gutter="10" class="icon-popup__body">
        <u-col span="2" class="icon-popup__item" v-for="item in iconList">
          <my-icon :name="item" size="64" color="#999" @click="onSelectIcon(item)"></my-icon>
        </u-col>
      </u-row>
    </u-popup>

  </view>
</template>

<script>
import iconList from '@/style/icon-list'; 

import {mapState} from 'vuex';
import {createTransactionCategory} from '@/api/ledger';

export default {
  components: {},
  data() {
    return {
      form: {
        name: '',
        remark: '',
        icon: 'block'
      },
      showIconPopup: false,
      iconList
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
    onSelectIcon(item) {  
      this.form.icon = item
      this.showIconPopup = false
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

      const response = await createTransactionCategory(httpData)
      const {success, data, message} = response
      if (success) {
        uni.$u.toast('提交成功')
        setTimeout(() => {
          this.onJump('/pages/transaction-category/index')
        }, 300)    
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

.icon-popup {
  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 16px 16px 16px;
  }

  &__body {
    padding: 0 16px 0 16px;
    flex-wrap: wrap;
    max-height: 480px;
    overflow: auto;
  }

  &__item {
    display: flex;
    flex-direction: column;
    align-items: center !important;
    justify-content: center !important;
    padding-bottom: 16px;
  }

}

</style>
