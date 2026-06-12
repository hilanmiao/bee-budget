<template>
  <view class="page">
    <view class="page__header">
      <view class="ledger">
        <view class="ledger__wrapper" @click="handleLedgerPickerClick">
          <view class="ledger__name">{{ selectedLedger.name }}</view>
          <view class="ledger__icon" v-if="ledgerList.length>1">
            <my-icon name="caret-down" size="36" color="#fff"></my-icon>
          </view>
        </view>
        <!--        <view class="date" @click="handleDateCalendarClick">-->
        <!--          <view class="date__text">{{ startDate }} ~ {{ endDate }}</view>-->
        <!--          <view class="date__icon">-->
        <!--            <MyIcon name="caret-down" size="36" color="#fff"></MyIcon>-->
        <!--          </view>-->
        <!--        </view>-->
      </view>
      <view class="statistic">
        <view class="statistic__item">
          <view class="statistic__label">本年支出</view>
          <view class="statistic__value">{{ ledgerSnapshot.yearExpense }}</view>
        </view>
        <view class="statistic__item">
          <view class="statistic__label">本年收入</view>
          <view class="statistic__value">{{ ledgerSnapshot.yearIncome }}</view>
        </view>
        <view class="statistic__item">
          <view class="statistic__label">本月支出</view>
          <view class="statistic__value">{{ ledgerSnapshot.monthExpense }}</view>
        </view>
        <view class="statistic__item">
          <view class="statistic__label">本月收入</view>
          <view class="statistic__value">{{ ledgerSnapshot.monthIncome }}</view>
        </view>
      </view>
    </view>

    <view class="page__main">
      <view class="box">
        <view class="box__header">收支趋势</view>
        <view class="box__body">
          <view class="chart">
            <qiun-data-charts
                type="line"
                :loadingType="1"
                :opts="opts"
                :chartData="chartData"
            />
          </view>
        </view>
      </view>
      <view class="box">
        <view class="box__header">支出分类构成</view>
        <view class="box__body">
          <view class="chart">
            <qiun-data-charts
                type="pie"
                :loadingType="1"
                :opts="optsPie"
                :chartData="chartDataPie"
            />
          </view>
        </view>
      </view>
      <view class="box">
        <view class="box__header">支出排名</view>
        <view class="box__body">
          <view class="transaction" v-for="(item,index) in monthlyExpenseTransactionTopN">
            <view class="transaction__number">{{ index + 1 }}</view>
            <view class="transaction__icon">
              <my-icon :name="item.transactionCategoryIcon" size="44" color="#fff"></my-icon>
            </view>
            <view class="transaction__body">
              <view class="left">
                <view class="transaction__category">{{ item.transactionCategoryName }}</view>
                <view class="transaction__desc">{{ item.description }}</view>
              </view>
              <view class="right">
                <view class="transaction__amount">- {{ item.amount }}</view>
                <view class="transaction__date">{{ formatDateHhmmss(item.date) }}</view>
              </view>
            </view>
          </view>
        </view>
      </view>
      <view class="box">
        <view class="box__header">支出分类排名</view>
        <view class="box__body">
          <view class="transaction" v-for="(item,index) in monthlyExpenseTransactionCategoryTopN">
            <view class="transaction__number">{{ index + 1 }}</view>
            <view class="transaction__icon">
              <my-icon :name="item.transactionCategoryIcon" size="44" color="#fff"></my-icon>
            </view>
            <view class="transaction__body">
              <view class="left">
                <view class="transaction__category">{{ item.transactionCategoryName }}</view>
              </view>
              <view class="right">
                <view class="transaction__amount">- {{ item.transactionTotalAmount }}</view>
              </view>
            </view>
          </view>
        </view>
      </view>
      <view class="box">
        <view class="box__header">收入分类排名</view>
        <view class="box__body">
          <view class="transaction" v-for="(item,index) in monthlyIncomeTransactionCategoryTopN">
            <view class="transaction__number">{{ index + 1 }}</view>
            <view class="transaction__icon">
              <my-icon :name="item.transactionCategoryIcon" size="44" color="#fff"></my-icon>
            </view>
            <view class="transaction__body">
              <view class="left">
                <view class="transaction__category">{{ item.transactionCategoryName }}</view>
              </view>
              <view class="right">
                <view class="transaction__amount">+ {{ item.transactionTotalAmount }}</view>
              </view>
            </view>
          </view>
        </view>
      </view>
    </view>

    <u-picker :show="showLedgerPicker" :columns="ledgerPickerColumns" key-name="name"
              @confirm="handleLedgerPickerConfirm" @cancel="showLedgerPicker = false"
              closeOnClickOverlay @close="showLedgerPicker = false"></u-picker>

    <my-tabbar></my-tabbar>
  </view>
</template>

<script>
import {
  mapState,
  mapMutations
} from 'vuex';
import MyTabbar from '@/components/MyTabbar';

import {
  getLedgerAll,
  getTransactionMonthlyExpenseTransactionCategoryTopN, getTransactionMonthlyIncomeTransactionCategoryTopN,
  getTransactionMonthlyExpenseTransactionTopN, getTransactionMonthlyExpenseTransactionCategory,
  getTransactionYearlyStats, getTransactionLedgerSnapshot
} from '@/api/ledger';

export default {
  components: {
    MyTabbar
  },
  computed: {
    ...mapState(['PrimaryColor', 'userInfo']),
  },
  data() {
    return {
      scrollTop: 0,
      chartData: {},
      // chartData: {
      //   categories: ['01','02','03','04','05','06','07','08','09','10','11','12'],
      //   series: [{ name: '支出', data: [1,2,3,4,5,6,7,8,9.10,11,12]}]
      // },
      opts: {
        color: [
          '#EE6666','#91CB74',
        ],
        padding: [20, 0, 0, 0],
        // dataLabel: false,
        // dataPointShape: false,
        // enableScroll: false,
        legend: {
          // show: false,
          // float: 'center',
          // itemGap: 30,
          // fontSize: 16,
          // lineHeight: 30,
        },
        xAxis: {
          // rotateLabel: true,
          // rotateAngle: 30,
          // labelCount: 6,
          fontSize: 10,
        },
        yAxis: {
          // gridType: "dash",
          // dashLength: 2,
          // data: [
          //   {
          //     min: 0,
          //     // max: 150
          //   }
          // ]
        },
        extra: {
          line: {
            type: "curve",
            width: 4,
            activeType: "hollow",
            linearType: "custom",
            onShadow: true,
            animation: "horizontal"
          }
        }
      },
      chartDataPie: {},
      // chartDataPie: {
      //   series: [
      //     {
      //       data: [{"name":"一班","value":50},{"name":"二班","value":30},{"name":"三班","value":20},{"name":"四班","value":18,"labelText":"四班:18人"},{"name":"五班","value":8}]
      //     }
      //   ]
      // },
      optsPie: {
        color: ["#6495ed", "#91CB74", "#FAC858", "#EE6666", "#73C0DE", "#3CA272", "#FC8452", "#9A60B4", "#ea7ccc"],
        padding: [20, 10, 0, 10],
        enableScroll: false,
        legend: {show: false},
        extra: {
          pie: {
            activeOpacity: 0.5,
            activeRadius: 10,
            offsetAngle: 0,
            labelWidth: 15,
            border: true,
            borderWidth: 3,
            borderColor: "#FFFFFF"
          }
        }
      },

      // 基础
      ledgerList: [],

      // 账本picker相关
      selectedLedger: {},
      showLedgerPicker: false,
      ledgerPickerColumns: [],

      // 统计相关
      monthlyExpenseTransactionCategoryTopN: [],
      monthlyIncomeTransactionCategoryTopN: [],
      monthlyExpenseTransactionTopN: [],
      monthlyExpenseTransactionCategory: [],
      yearlyStats: [],
      ledgerSnapshot: {}

    }
  },
  watch: {
    async selectedLedger(newVal, oldVal) {
      await this._getTransactionLedgerSnapshot()
      await this._getTransactionYearlyStats()
      await this._getTransactionMonthlyExpenseTransactionTopN()
      await this._getTransactionMonthlyExpenseTransactionCategoryTopN()
      await this._getTransactionMonthlyIncomeTransactionCategoryTopN()
      await this._getTransactionMonthlyExpenseTransactionCategory()
    },
    yearlyStats(newVal, oldVal) {
      const categories = newVal.map(o => o.monthLabel)
      const series = [
        { name: '支出', data: newVal.map(o => o.expenseAmount )},
        { name: '收入', data: newVal.map(o => o.incomeAmount )},
      ]
      this.chartData = {
        categories,
        series
      }
    },
    monthlyExpenseTransactionCategory(newVal, oldVal) {
      const data = newVal.map(o => {
        return {
          name: o.transactionCategoryName,
          value: o.transactionTotalAmount,
          labelText: o.transactionCategoryName + o.transactionTotalAmount
        }
      })

      this.chartDataPie = {
        series: [{
          data
        }]
      }
    },
  },
  async onLoad() {
    // 隐藏原生的tabbar
    uni.hideTabBar();

    await this._getLedgerAll()
    if (this.ledgerList.length) {
      this.selectedLedger = this.ledgerList[0]
      this.ledgerPickerColumns = [this.ledgerList]

      await this._getTransactionLedgerSnapshot()
      await this._getTransactionYearlyStats()
      await this._getTransactionMonthlyExpenseTransactionTopN()
      await this._getTransactionMonthlyExpenseTransactionCategoryTopN()
      await this._getTransactionMonthlyIncomeTransactionCategoryTopN()
      await this._getTransactionMonthlyExpenseTransactionCategory()
    }
  },
  async onShow() {

  },
  methods: {
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    formatDateHhmmss(date) {
      // return this.$dayjs(date).format('HH:mm:ss')
      return this.$dayjs(date).format('MM-DD HH:mm')
    },
    handleLedgerPickerClick() {
      if (this.ledgerList.length > 1) {
        this.showLedgerPicker = true
      }
    },
    handleLedgerPickerConfirm(e) {
      console.log(e)
      this.selectedLedger = e.value[0]
      this.showLedgerPicker = false
    },
    async _getLedgerAll() {
      try {
        const response = await getLedgerAll()
        const {success, data, message} = response
        if (success) {
          this.ledgerList = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getTransactionLedgerSnapshot() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
        }
        const response = await getTransactionLedgerSnapshot({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.ledgerSnapshot = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getTransactionMonthlyExpenseTransactionCategoryTopN() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
          top: 10
        }
        const response = await getTransactionMonthlyExpenseTransactionCategoryTopN({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.monthlyExpenseTransactionCategoryTopN = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getTransactionMonthlyIncomeTransactionCategoryTopN() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
          top: 10
        }
        const response = await getTransactionMonthlyIncomeTransactionCategoryTopN({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.monthlyIncomeTransactionCategoryTopN = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getTransactionMonthlyExpenseTransactionTopN() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
          top: 10
        }
        const response = await getTransactionMonthlyExpenseTransactionTopN({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.monthlyExpenseTransactionTopN = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getTransactionMonthlyExpenseTransactionCategory() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
          top: 10
        }
        const response = await getTransactionMonthlyExpenseTransactionCategory({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.monthlyExpenseTransactionCategory = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getTransactionYearlyStats() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
          top: 10
        }
        const response = await getTransactionYearlyStats({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.yearlyStats = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },

  },
}
</script>

<style lang="scss" scoped>
.page {
  height: 100vh;
  //background-color: #fafafa;
  //padding: 40px 30px 30px;
  //overflow: hidden;
  display: flex;
  flex-direction: column;
  //background-size: contain;
  //background-repeat: no-repeat;
  //background-position: center;
  //background-image: url(/static/home-background.png);


  &__main {
    padding: 340rpx 24rpx;
  }

  &__header {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    position: fixed;
    top: 0;
    width: 100%;
    height: 320rpx;
    padding: 60rpx 40rpx 20rpx 40rpx;
    box-shadow: 0 2px 8px rgba(#a5a5a5, 0.3);
    border-radius: 0 0 24px 24px;
    background-color: $theme-color-primary-brand;
    color: #FFFFFF;
    z-index: 9;
    font-size: $uni-font-size-lg;

    .ledger {
      display: flex;
      align-items: center;
      justify-content: space-between;

      &__wrapper {
        display: flex;
        align-items: center;
        //background: rgba(255, 255, 255, .1);
        //border-radius: 12rpx;
        //padding: 12rpx 24rpx;
        font-size: 18px;

        &__icon {
          display: flex;
          align-items: center;
          justify-content: center;
          padding-left: 8rpx;
        }
      }
    }

    .date {
      display: flex;
      align-items: center;
      //background: rgba(255, 255, 255, .1);
      //border-radius: 12rpx;
      border: 1px solid #fff;
      border-radius: 24px;
      padding: 8px 12px;

      &__icon {
        display: flex;
        align-items: center;
        justify-content: center;
        padding-left: 8rpx;
      }
    }

    .statistic {
      flex: 1;
      display: flex;
      font-size: $uni-font-size-base;
      align-items: center;
      justify-content: space-around;

      &__item {
        display: flex;
        flex-direction: column;
        align-items: center;
      }

      &__label {
        font-size: 18px;
      }

      &__value {
        font-size: 20px;
        padding-top: 8px;
      }
    }
  }

  .box {
    background-color: #fff;
    border-radius: 12px;
    padding: 16px;
    margin-top: 16px;

    &:first-child {
      margin-top: 0;
    }

    &__header {
      font-size: 16px;
    }

    &__body {
      padding: 4px;
    }
  }

  .chart {
    width: 100%;
    height: 240px;
  }

  .transaction {
    display: flex;
    color: $uni-text-color;
    padding-top: 12px;

    &__number {
      width: 30px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: flex-start;
      font-size: 16px;
    }

    &__icon {
      width: 40px;
      height: 40px;
      border-radius: 10px;
      background: rgba(100, 147, 237, .2);
      color: $theme-color-primary-brand;
      display: flex;
      align-items: center;
      justify-content: center;

      ::v-deep .custom-icon {
        color: $theme-color-primary-brand !important;
      }
    }

    &__body {
      flex: 1;
      display: flex;
      margin-left: 12px;
      border-bottom: 1px dashed $u-border-color;
      padding-bottom: 8px;

      .left {
        flex: 1;
        display: flex;
        flex-direction: column;
        justify-content: space-around;
      }

      .right {
        display: flex;
        flex-direction: column;
        align-items: flex-end;
        justify-content: space-around;
      }
    }

    &__category {
      font-size: 18px;
      color: $uni-text-color;
    }

    &__desc {
      font-size: 16px;
      color: $uni-text-color-grey;
      padding-top: 4px;
    }

    &_amount {
      font-weight: bold;
    }

    &__date {
      font-size: 16px;
      color: $uni-text-color-grey;
      padding-top: 4px;
    }

    &:last-child {
      .transaction__body {
        border-bottom: none;
      }
    }
  }

}


</style>
