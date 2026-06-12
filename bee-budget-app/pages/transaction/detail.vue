<template>
  <view class="page">
    <u-navbar
        title="交易详情"
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
            label="交易分类"
            prop="userInfo.nickName"
            borderBottom
            ref="item1"
            @click="showTransactionCategoryPopup = true"
        >
          <my-icon :name="form.transactionCategoryIcon" size="48" color="#999"
                  @click="showTransactionCategoryPopup = true"></my-icon>
          &nbsp;&nbsp;&nbsp;{{ form.transactionCategoryName }}
        </u-form-item>
        <u-form-item
            label="交易类型"
            prop="userInfo.name"
            borderBottom
            ref="item1"
        >
          <u-radio-group
              v-model="form.type"
              placement="row">
            <u-radio :activeColor="primaryColor" name="支出" label="支出"></u-radio>
            <u-radio :activeColor="primaryColor" name="收入" label="收入" style="margin-left: 12px;"></u-radio>
            <u-radio :activeColor="primaryColor" name="不计入收支" label="不计入收支"
                     style="margin-left: 12px;"></u-radio>
          </u-radio-group>
        </u-form-item>
        <u-form-item
            label="交易描述"
            prop="userInfo.name"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="form.description"
              placeholder="请输入交易描述"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="交易金额"
            prop="userInfo.name"
            borderBottom
            ref="item1"
        >
          <u--input
              v-model="form.amount"
              placeholder="请输入金额"
              type="number"
          ></u--input>
        </u-form-item>
        <u-form-item
            label="交易时间"
            prop="userInfo.name"
            borderBottom
            ref="item1"
            @click="showLogCalendar = true;"
        >
          <view>
            {{ $dayjs(form.date).format('YYYY-MM-DD HH:mm') }}
          </view>
          <u-icon
              slot="right"
              name="arrow-right"
          ></u-icon>
        </u-form-item>
        <u-form-item
            label="状态"
            prop="userInfo.name"
            borderBottom
            ref="item1"
        >
          作废&nbsp;&nbsp;&nbsp;
          <u-switch v-model="form.status" :active-color="primaryColor" active-value="0" inactive-value="1"></u-switch>
          &nbsp;&nbsp;&nbsp;正常
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

    <u-modal class="delete-modal" :show="showDelete" title="确定删除当前交易？" :closeOnClickOverlay="true"
             showCancelButton
             @close="showDelete = false" @cancel="showDelete = false" @confirm="handleDelete">
      <view class="slot-content">
      </view>
    </u-modal>

    <u-calendar :show="showLogCalendar" closeOnClickOverlay @close="showLogCalendar = false"
                @confirm="handleConfirmLogCalendar"
                :min-date="calendarMinDate" :max-date="calendarMaxDate" :monthNum="16"></u-calendar>

    <u-popup class="transaction-category-popup" :show="showTransactionCategoryPopup" mode="bottom"
             @close="showTransactionCategoryPopup = false">
      <view class="header">
        <view class="tip">选择交易分类</view>
        <view class="close" @click="showTransactionCategoryPopup= false">
          <my-icon name="close" size="48" color="#333"></my-icon>
        </view>
      </view>
      <u-row gutter="10" class="transaction-category-popup__body">
        <u-col span="2" class="transaction-category-popup__item" v-for="item in transactionCategoryList"
               @click="onSelectTransactionCategory(item)">
          <view class="transaction-category-popup__icon">
            <my-icon :name="item.icon" size="48" color="#999"></my-icon>
          </view>
          <view class="transaction-category-popup__label">
            {{ item.name }}
          </view>
        </u-col>
      </u-row>
    </u-popup>

  </view>
</template>

<script>
import {mapState} from 'vuex';
import {getTransactionCategoryAll, updateTransaction, deleteTransaction} from '@/api/ledger';

export default {
  components: {},
  data() {
    return {
      form: {},
      detail: {},
      showDelete: false,
      showTransactionCategoryPopup: false,
      showLogCalendar: false,
      calendarMinDate: this.$dayjs().subtract(1, 'year').format('YYYY-MM-DD'), // 往前一年
      calendarMaxDate: this.$dayjs().add(3, 'month').format('YYYY-MM-DD'), // 往后3个月，12+1+3=16越多越卡
      transactionCategoryList: []
    }
  },
  computed: {
    ...mapState(['primaryColor', 'userInfo']),
  },
  async onLoad(e) {
    //获取传递过来的参数
    this.detail = JSON.parse(e.detail)
    this.form = {...this.detail}
    console.log(this.form)

    await this._getTransactionCategoryAll()
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
    onSelectTransactionCategory(obj) {
      this.form.transactionCategoryId = obj.id
      this.form.transactionCategoryName = obj.name
      this.form.transactionCategoryIcon = obj.icon
      this.showTransactionCategoryPopup = false
    },
    handleConfirmLogCalendar(e) {
      console.log(e)
      // 1. 获取用户选择的日期部分（只有年月日）
      const selectedDate = this.$dayjs(e[0]); // e.g., "2026-03-31"
      // 2. 获取当前的本地时间（用于提取时分秒）
      const now = this.$dayjs();
      // 3. 将选中的日期 和 当前的时分秒 合并成一个新的本地时间
      const combinedLocalTime = selectedDate
          .hour(now.hour())
          .minute(now.minute())
          .second(now.second())
          .millisecond(now.millisecond());
      // 4. 格式化为带 'Z' 的 UTC 字符串（这才是关键！）
      //    然后转换成 UTC，再格式化为 ISO 字符串（带 Z）
      const utcStringForBackend = combinedLocalTime.utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]');
      this.form.date = utcStringForBackend
      this.showLogCalendar = false
    },
    async submit() {
      if (!this.form.transactionCategoryId) {
        uni.showToast({
          title: '请选择交易分类',
          icon: 'none'
        });
        return;
      }
      if (!this.form.type) {
        uni.showToast({
          title: '请选择交易类型',
          icon: 'none'
        });
        return;
      }
      if (!this.form.description) {
        uni.showToast({
          title: '请填写交易描述',
          icon: 'none'
        });
        return;
      }
      if (this.form.amount == null || this.form.amount == '') {
        uni.showToast({
          title: '请填写交易金额',
          icon: 'none'
        });
        return;
      }
      if (!this.form.date) {
        uni.showToast({
          title: '请选择交易时间',
          icon: 'none'
        });
        return;
      }
      if (!this.form.status) {
        uni.showToast({
          title: '请选择状态',
          icon: 'none'
        });
        return;
      }
      let httpData = {
        ...this.form,
      }

      const response = await updateTransaction(httpData)

      uni.$u.toast('提交成功')
      setTimeout(() => {
        // this.onJump('/pages/home/index')
        uni.reLaunch({
          url: '/pages/home/index'
        })
      }, 300)
    },
    async handleDelete() {
      let httpData = {
        id: this.form.id
      }

      const response = await deleteTransaction(httpData)

      uni.$u.toast('删除成功')
      setTimeout(() => {
        // this.onJump('/pages/home/index')
        uni.reLaunch({
          url: '/pages/home/index'
        })
      }, 300)
    },
    async _getTransactionCategoryAll() {
      try {
        const res = await getTransactionCategoryAll()
        console.log(res)
        this.transactionCategoryList = res
      } catch (err) {
        console.log(err, 'catch')
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

.transaction-category-popup {
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

  &__icon {
    width: 40px;
    height: 40px;
    border-radius: 12px;
    //background: #f9f9f9;
    display: flex;
    align-items: center;
    justify-content: center;
    color: #333;
  }

  &__label {
    padding-top: 2px;
    color: #666;
    font-size: 12px;
    text-align: center;
  }

}

</style>
