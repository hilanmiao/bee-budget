<template>
  <view class="page">
    <view class="page__header">
      <view class="ledger" @click="handleLedgerPickerClick">
        <view class="ledger__wrapper">
          <view class="ledger__name">{{ selectedLedger.name }}</view>
          <view class="ledger__icon" v-if="ledgerList.length>1">
            <my-icon name="caret-down" size="36" color="#fff"></my-icon>
          </view>
        </view>
      </view>
      <view class="category" @click="showTransactionCategoryPopup = true">
        <view class="category__wrapper">
          <view class="category__name">{{
              selectedTransactionCategory ? selectedTransactionCategory.name : '全部类型'
            }}
          </view>
          <view class="category__icon">
            <my-icon name="appstoreadd" size="36" color="#fff"></my-icon>
          </view>
        </view>
      </view>
      <view class="filter" @click="showDatePicker = true">
        <view class="filter__wrapper">
          <view class="date">
            <view class="date__text">{{ $dayjs(maxDate).format('YYYY年MM月') }}</view>
            <view class="date__icon">
              <my-icon name="caret-down" size="36" color="#fff"></my-icon>
            </view>
          </view>
          <view class="statistic">
            <view class="statistic__out">支出 {{ monthStats.totalExpense }}</view>
            <view class="statistic__in">收入 {{ monthStats.totalIncome }}</view>
          </view>
        </view>
      </view>
    </view>

    <mescroll-body ref="mescrollRef" top="176px" @init="mescrollInit" :down="downOption"
                   @up="upCallback" @down="downCallback">
      <view class="page__main">
        <view class="group" v-for="(group, index) in groupedList" :key="index">
          <view class="group__header">
            <view class="group__date">
              {{ formatDate(group.date) }}
            </view>
            <view class="group__statistic">
              <view class="statistic">支 {{ getExpenseByDate(group.date) }}
              </view>
              <view class="statistic">收 {{ getIncomeByDate(group.date) }}
              </view>
            </view>
          </view>
          <view class="group__body">
            <view class="transaction" v-for="item in group.transactions" :key="item.id" @click="goToDetail(item)">
              <view class="transaction__icon">
                <my-icon :name="item.transactionCategoryIcon" size="44" color="#fff"></my-icon>
              </view>
              <view class="transaction__body">
                <view class="left">
                  <view class="transaction__category">{{ item.transactionCategoryName }}</view>
                  <view class="transaction__desc">
                    {{ item.description }} 
                    <u--text type="error" text="（已作废）" v-if="item.status === '1'"></u--text>
                  </view>
                </view>
                <view class="right">
                  <view class="transaction__amount">{{ item.type === '支出' ? '-' : '+' }} {{
                      item.amount
                    }}
                  </view>
                  <view class="transaction__date">
                    {{ formatDateHhmmss(item.date) }}
                  </view>
                </view>
              </view>
            </view>
          </view>
        </view>
      </view>
    </mescroll-body>

    <u-picker :show="showLedgerPicker" :columns="ledgerPickerColumns" key-name="name"
              @confirm="handleLedgerPickerConfirm" @cancel="showLedgerPicker = false"
              closeOnClickOverlay @close="showLedgerPicker = false"></u-picker>

    <u-popup class="category-popup" :show="showTransactionCategoryPopup" mode="bottom"
             @close="showTransactionCategoryPopup = false">
      <view class="category-popup-wrapper">
        <view class="header">
          <view class="ledger">
            <view class="ledger__name">请选择类型</view>
          </view>
          <view class="close" @click="showTransactionCategoryPopup= false">
            <my-icon name="close" size="48" color="#333"></my-icon>
          </view>
        </view>
        <view class="content">
          <view class="group">
            <!--          <view class="group__header">支出</view>-->
            <u-row gutter="12" class="category">
              <u-col span="4" class="category__item" :class="{'category__item--active' : !selectedTransactionCategory }"
                     @click="handleClickTransactionCategory(null)">
                <view class="category__label">
                  全部类型
                </view>
              </u-col>
            </u-row>
          </view>
          <view class="group">
            <!--            <view class="group__header">{{ group.type }}</view>-->
            <u-row gutter="20" class="category">
              <u-col span="4" class="category__item"
                     :class="{'category__item--active' : selectedTransactionCategory && selectedTransactionCategory.id === item.id}"
                     v-for="item in transactionCategoryList" :key="item.id"
                     @click="handleClickTransactionCategory(item)">
                <view class="category__label">
                  {{ item.name }}
                </view>
              </u-col>
            </u-row>
          </view>

        </view>
      </view>
    </u-popup>

    <u-popup class="add-popup" :show="showLogPopup" mode="bottom" @close="showLogPopup = false">
      <view class="app-popup-wrapper">
        <view class="header">
          <view class="ledger">
            <view class="ledger__name">{{ selectedLedger.name }}</view>
            <!--          <view class="ledger__icon">-->
            <!--            <MyIcon name="caret-down" size="36" color="#333"></MyIcon>-->
            <!--          </view>-->
          </view>
          <view class="close" @click="showLogPopup= false">
            <my-icon name="close" size="48" color="#333"></my-icon>
          </view>
        </view>
        <view class="action">
          <view class="category-type">
            <view class="category-type__item"
                  :class="{'category-type__item--active' : logType=== '支出'}" @click="logType = '支出'">支出
            </view>
            <view class="category-type__item"
                  :class="{'category-type__item--active' : logType=== '收入'}" @click="logType = '收入'">收入
            </view>
            <view class="category-type__item"
                  :class="{'category-type__item--active' : logType=== '不计入收支'}" @click="logType = '不计入收支'">
              不计入收支
            </view>
          </view>
          <view class="date" @click="showLogCalendar = true">
            <view class="date__text">{{ $dayjs(logDate).format('YYYY-MM-DD') }}</view>
            <view class="date__icon">
              <my-icon name="caret-down" size="36" color="#333"></my-icon>
            </view>
          </view>
        </view>
        <view class="money">
          <view class="money__label">￥</view>
          <view class="value-wrapper">
            <view class="money__value">{{ keyboard && keyboard.valueToLocaleString }}</view>
            <view class="money__cursor"></view>
          </view>
        </view>
        <view class="description">
          <u--input
              placeholder="请输入交易描述（必填）"
              border="surround"
              v-model="logDescription"
          ></u--input>
        </view>
        <!--        <view class="link">-->
        <!--          分类管理-->
        <!--          <MyIcon name="right" size="24" color="#333"></MyIcon>-->
        <!--        </view>-->
        <u-row justify="flex-start" gutter="10" class="category">
          <u-col span="2" class="category__item"
                 :class="{'category__item--active' : logTransactionCategory && logTransactionCategory.id === item.id}"
                 v-for="item in transactionCategoryList" :key="item.id"
                 @click="handleClickLogTransactionCategory(item)">
            <view class="category__icon">
              <my-icon :name="item.icon" size="48" color="#999"></my-icon>
            </view>
            <view class="category__label">
              {{ item.name }}
            </view>
          </u-col>
        </u-row>
        <view class="keyboard">
          <pan-keyboard ref="panKeyboardRef" :maxValue="1000000000" :isCheck='checkValue' @onSubmit='onSubmit'
                        @onChange="onChange"
                        @onError="onErrorx"></pan-keyboard>
        </view>
      </view>
    </u-popup>

    <u-calendar :show="showLogCalendar" closeOnClickOverlay @close="showLogCalendar = false"
                @confirm="handleConfirmLogCalendar" 
                :min-date="calendarMinDate" :max-date="calendarMaxDate" :monthNum="16"></u-calendar>

    <u-picker :show="showDatePicker" ref="datePicker" :columns="datePickerColumns" :default-index="defaultIndex"
              @confirm="handleDatePickerConfirm"
              @change="handleDatePickerChange" @cancel="showDatePicker = false"
              closeOnClickOverlay @close="showDatePicker = false"></u-picker>

    <view class="quick-add" @click="showLogPopup = true">
      <my-icon name="edit-fill" size="36" color="#fff"></my-icon>
      记一笔
    </view>

    <my-tabbar></my-tabbar>
  </view>
</template>

<script>
import {
  mapState,
  mapMutations
} from 'vuex'
import MescrollMixin from "@/uni_modules/mescroll-uni/components/mescroll-uni/mescroll-mixins.js"
import MyTabbar from '@/components/MyTabbar'
import {
  getLedgerAll, getTransactionCategoryAll, getTransactionPaged, getTransactionRangeStats,
  getTransactionEarliest, createTransaction
} from '@/api/ledger'

export default {
  mixins: [MescrollMixin], // 使用mixin (在main.js注册全局组件)
  components: {
    MyTabbar
  },
  computed: {
    ...mapState(['primaryColor', 'userInfo']),
    checkValue() {
      let valueNumber = this.keyboard?.valueNumber

      // 可以在实时这里校验输入的值
      // if (valueNumber < 100) return false;
      // if (valueNumber % 10) return false;
      return true;
    },
  },
  data() {
    return {
      scrollTop: 0,

      // 基础
      ledgerList: [],
      transactionCategoryList: [],

      // 列表相关
      downOption: {
        auto: false //是否在初始化后,自动执行downCallback; 默认true
      },
      list: [],
      groupedList: [],
      statsSomeDayCache: [],

      // 分类popup相关
      showTransactionCategoryPopup: false,
      selectedTransactionCategory: null,

      // 账本picker相关
      selectedLedger: {},
      showLedgerPicker: false,
      ledgerPickerColumns: [],

      // 记一笔popup相关
      showLogPopup: false,
      logType: '支出',
      logTransactionCategory: null,
      logDate: this.$dayjs().format(),
      logDescription: null,
      showLogCalendar: false,
      keyboard: null,
      calendarMinDate: this.$dayjs().subtract(1, 'year').format('YYYY-MM-DD'), // 往前一年
      calendarMaxDate: this.$dayjs().add(3, 'month').format('YYYY-MM-DD'), // 往后3个月，12+1+3=16越多越卡

      // 日期datepicker相关
      showDatePicker: false,
      minDate: '',
      maxDate: this.$dayjs().format(),
      datePickerColumns: [],
      yearColumns: [],
      monthColumns: [],
      minYearMonthColumns: [],
      currentYearMonthColumns: [],
      transactionEarliest: {},
      defaultIndex: [],
      monthStats: {
        totalExpense: 0,
        totalIncome: 0
      }

    }
  },
  watch: {
    selectedLedger(newVal, oldVal) {
      this.onSearch()
    },
    selectedTransactionCategory(newVal, oldVal) {
      this.onSearch()
    },
    maxDate(newVal, oldVal) {
      this.onSearch()
      this._getMonthStats()
    },
    showLogPopup(newVal, oldVal) {
      console.log(this.keyboard)
      // this.keyboard.enterAmount = ''
      // this.keyboard.valueNumber = 0
      // this.keyboard.valueToLocaleString = ''
      this.keyboard = null
      if (!this.logTransactionCategory) {
        this.logTransactionCategory = this.transactionCategoryList[0]
      }
    },
  },
  async onLoad() {
    // 隐藏原生的tabbar
    uni.hideTabBar();
    // // 用这个方式设置初始值；不设置默认为空
    // setTimeout(()=>{
    //   this.$refs.panKeyboardRef.setKeyboard(100)
    // },100)

    await this._getLedgerAll()
    if (this.ledgerList.length) {
      this.selectedLedger = this.ledgerList[0]
      this.ledgerPickerColumns = [this.ledgerList]
    }
    await this._getTransactionCategoryAll()

    // 设置默认日期
    await this._getTransactionEarliest()
    this.setDefaultDate()

    await this._getMonthStats()
  },
  async onShow() {

  },
  methods: {
    onJump(url) {
      uni.navigateTo({
        url: url
      })
    },
    onRelaunch(url) {
      uni.reLaunch({
        url: url
      })
    },
    /*上拉加载的回调: 其中page.num:当前页 从1开始, page.size:每页数据条数,默认10 */
    upCallback(page) {
      this.loadData(page.num)
    },
    //搜索框搜索
    onSearch() {
      // if(!this.keyword){
      //   this.$u.toast('搜索的内容不能为空~')
      //   return
      // }
      this.groupedList = []
      this.list = []; // 先清空列表,显示加载进度
      this.$nextTick(() => {
        this.mescroll.scrollTo(0, 0);
        this.mescroll.resetUpScroll();
      })
    },
    // 金额完成/打开验证码
    async onSubmit(e) {
      this.keyboard = e
      console.log(e)

      if (!this.selectedLedger?.id || !this.logTransactionCategory?.id || !this.logDate || !this.logDescription) {
        return;
      }
      // 1. 获取用户选择的日期部分（只有年月日）
      const selectedDate = this.$dayjs(this.logDate); // e.g., "2026-03-31"
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

      let httpData = {
        ledgerId: this.selectedLedger.id,
        transactionCategoryId: this.logTransactionCategory.id,
        type: this.logType,
        amount: e.valueNumber,
        date: utcStringForBackend,
        description: this.logDescription,
      }

      const response = await createTransaction(httpData)
      const {success, data, message} = response
      if (success) {
        uni.$u.toast('提交成功')
        this.onSearch()
        this.showLogPopup = false
      } else {
        uni.$u.toast(message)
      }
    },
    // 金额输入中
    onChange(e) {
      this.keyboard = e
    },
    onErrorx() {
      console.log('超过限制')
    },
    formatDateHhmmss(date) {
      // return this.$dayjs(date).format('HH:mm:ss')
      return this.$dayjs(date).format('HH:mm')
    },
    // 格式化日期
    formatDate(date) {
      const today = this.$dayjs(); // 当前日期
      const targetDate = this.$dayjs(date); // 目标日期

      // 判断是否为同一年
      if (today.year() === targetDate.year()) {
        // 如果是同一年，格式化为 "x月x日 星期x"
        return `${targetDate.format('M月D日')} ${this.getWeekday(targetDate)}`;
      } else {
        // 如果不是同一年，格式化为 "xxxx年xx月xx日"
        return targetDate.format('YYYY年M月D日');
      }
    },
    // 获取星期几的中文表示
    getWeekday(date) {
      const weekdayMap = ['日', '一', '二', '三', '四', '五', '六'];
      return `星期${weekdayMap[date.day()]}`;
    },
    getIncomeByDate(date) {
      const stat = this.getStatByDate(date);
      return stat && stat.income ? stat.income.toFixed(2) : '0.00';
    },
    getExpenseByDate(date) {
      const stat = this.getStatByDate(date);
      return stat && stat.expense ? stat.expense.toFixed(2) : '0.00';
    },
    getStatByDate(date) {
      return this.statsSomeDayCache.find(stat => stat.date === date) || {income: 0, expense: 0};
    },
    setDefaultDate() {
      if (this.transactionEarliest?.id) {
        this.minDate = this.transactionEarliest.date
      } else {
        this.minDate = this.$dayjs()
      }
      const minYear = this.$dayjs(this.minDate).year()
      const minMonth = this.$dayjs(this.minDate).month() + 1
      const currentYear = this.$dayjs().year()
      const currentMonth = this.$dayjs().month() + 1
      for (let i = minYear; i <= currentYear; i++) {
        this.yearColumns.push(i + '年')
      }
      for (let i = minMonth; i <= 12; i++) {
        this.minYearMonthColumns.push(i + '月')
      }
      for (let i = 1; i <= currentMonth; i++) {
        this.currentYearMonthColumns.push(i + '月')
      }
      for (let i = 1; i <= 12; i++) {
        this.monthColumns.push(i + '月')
      }
      this.datePickerColumns = [this.yearColumns, this.currentYearMonthColumns]
      // 默认选中最新月份
      this.defaultIndex = [this.yearColumns.length - 1, this.currentYearMonthColumns.length - 1]
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
    handleClickTransactionCategory(category) {
      this.selectedTransactionCategory = category
      this.showTransactionCategoryPopup = false
    },
    handleDatePickerConfirm(e) {
      console.log('confirm', e)
      const year = parseInt(e.value[0].replace('年', ''));
      const month = parseInt(e.value[1].replace('月', ''));
      const lastDay = this.$dayjs().year(year).month(month - 1).endOf('month');
      this.maxDate = lastDay.format('YYYY-MM-DD 23:59:59.999');
      this._getMonthStats()
      this.showDatePicker = false
    },
    handleDatePickerChange(e) {
      const {
        columnIndex,
        value,
        values, // values为当前变化列的数组内容
        index,
        // 微信小程序无法将picker实例传出来，只能通过ref操作
        picker = this.$refs.datePicker
      } = e
      // 当第一列值发生变化时，变化第二列(后一列)对应的选项
      if (columnIndex === 0) {
        // picker为选择器this实例，变化第二列对应的选项
        const selectedYear = value[0]
        let secondColumns
        if (selectedYear === this.$dayjs(this.minDate).year() + '年') {
          secondColumns = this.minYearMonthColumns
        } else if (selectedYear === this.$dayjs().year() + '年') {
          secondColumns = this.currentYearMonthColumns
        } else {
          secondColumns = this.monthColumns
        }
        picker.setColumnValues(1, secondColumns)
      }
    },
    handleClickLogTransactionCategory(category) {
      this.logTransactionCategory = category
    },
    handleConfirmLogCalendar(e) {
      console.log(e)
      this.logDate = e[0]
      this.showLogCalendar = false
    },
    goToDetail(obj) {
      const params = `detail=${JSON.stringify(obj)}`
      const url = '/pages/transaction/detail?' + params

      this.onJump(url)
    },
    // 按日分组交易记录
    async groupList(pageData) {
      // this.groupedList = [];
      // this.statsSomeDayCache = [];

      for (const item of pageData) {
        const date = this.$dayjs(item.date).format('YYYY-MM-DD');

        // 检查 groupedList 中是否已有该的分组
        let groupIndex = this.groupedList.findIndex(group => group.date === date);
        if (groupIndex === -1) {
          // 如果没有找到，则新增一个分组
          this.groupedList.push({date, transactions: [item]});
        } else {
          // 如果已存在，则直接添加到现有分组
          this.groupedList[groupIndex].transactions.push(item);
        }

        // 检查 statsSomeDayCache 中是否已有该日的统计信息
        let statsIndex = this.statsSomeDayCache.findIndex(stats => stats.date === date);
        if (statsIndex === -1) {
          try {
            const httpData = {
              ledgerId: this.selectedLedger.id,
              startDate: this.$dayjs(date).startOf('day').utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]'),
              endDate: this.$dayjs(date).add(1, 'day').startOf('day').utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]'),
            }
            const statsResponse = await getTransactionRangeStats({params: httpData});

            const {totalIncome, totalExpense} = statsResponse
            this.statsSomeDayCache.push({
              date,
              income: parseFloat(totalIncome || 0),
              expense: parseFloat(totalExpense || 0),
            });
          } catch (error) {
            console.error(`获取 ${date} 的统计信息失败`, error);
            this.statsSomeDayCache.push({date, income: 0, expense: 0}); // 默认值
          }
        }
      }

    },
    async loadData(pageNo) {
      if (!this.selectedLedger.id) return
      let httpData = {
        pageNumber: pageNo,
        pageSize: 20,
        ledgerId: this.selectedLedger.id,
        transactionCategoryId: this.selectedTransactionCategory?.id,
        endDate: this.$dayjs(this.maxDate).add(1, 'day').startOf('day').utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]')
      }

      try {
        const response = await getTransactionPaged({params: httpData})
        const {success, data, message} = response
        if (success) {
          let {items, totalItems} = data
          const pageCount = parseInt(Math.ceil(totalItems / httpData.pageSize))

          uni.stopPullDownRefresh();
          this.mescroll.endByPage(items.length, pageCount); //必传参数(当前页的数据个数, 总页数)
          if (pageNo == 1) {
            this.groupedList = []
            this.list = items
          } else {
            this.list = this.list.concat(items);
          }
          await this.groupList(items)
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
        //联网失败, 结束加载
        this.mescroll.endErr();
      }
    },
    async _getTransactionCategoryAll() {
      try {
        const response = await getTransactionCategoryAll()
        const {success, data, message} = response
        if (success) {
          this.transactionCategoryList = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
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
    async _getTransactionEarliest() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
        }
        const response = await getTransactionEarliest({params: httpData})
        const {success, data, message} = response
        if (success) {
          this.transactionEarliest = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
    async _getMonthStats() {
      try {
        const httpData = {
          ledgerId: this.selectedLedger.id,
          startDate: this.$dayjs(this.maxDate).startOf('month').utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]'),
          endDate: this.$dayjs(this.maxDate).add(1, 'month').startOf('month').utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]'),
        }
        const response = await getTransactionRangeStats({params: httpData});
        const {success, data, message} = response
        if (success) {
          this.monthStats = data
        } else {
          uni.$u.toast(message)
        }
      } catch (err) {
        console.log(err, 'catch')
      }
    },
  },
  onPageScroll(e) {
    this.scrollTop = e.scrollTop;
  },
}
</script>

<style lang="scss" scoped>
.page {
  //height: 100vh;
  //background-color: #fafafa;
  //padding: 40px 30px 30px;
  //overflow: hidden;
  //display: flex;
  //flex-direction: column;
  //background-size: contain;
  //background-repeat: no-repeat;
  //background-position: center;
  //background-image: url(/static/home-background.png);

  &__main {
    //padding: 340rpx 24rpx;
    padding: 0 24rpx;

    .group {
      background-color: #fff;
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 16px;

      &__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        color: $uni-text-color-grey;
        font-size: 16px;
        padding-bottom: 8px;
      }

      &__date {

      }

      &__statistic {
        display: flex;

        .statistic {
          padding-right: 8px;
        }
      }

      &__action {
        position: absolute;
        width: 100px;
        height: 32px;
        right: 16px;
        bottom: 4px;
        font-size: 16px;
        border-radius: 16px;
        padding: 4px 12px;
        background-color: $theme-color-primary-brand;
        color: #fff;
        display: flex;
        align-items: center;
        justify-content: center;
      }
    }

    .transaction {
      display: flex;
      color: $uni-text-color;
      padding-top: 12px;

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
        display: flex;
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

  &__header {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    position: fixed;
    top: 0;
    width: 100%;
    height: 160px;
    padding: 30px 20px 10px 20px;
    box-shadow: 0 2px 8px rgba(#a5a5a5, 0.3);
    border-radius: 0 0 24px 24px;
    background-color: $theme-color-primary-brand;
    color: #FFFFFF;
    z-index: 9;
    font-size: $uni-font-size-lg;

    .ledger {
      display: flex;
      align-items: center;
      justify-content: center;

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

    .category {
      display: flex;
      align-items: center;
      justify-content: flex-start;

      &__wrapper {
        display: flex;
        align-items: center;
        background: rgba(255, 255, 255, .1);
        border-radius: 12rpx;
        padding: 12rpx 24rpx;

        &:hover {
          background: rgba(255, 255, 255, .05);
        }
      }

      &__icon {
        display: flex;
        align-items: center;
        justify-content: center;
        padding-left: 8rpx;
      }
    }

    .filter {
      display: flex;
      align-items: center;
      justify-content: flex-start;

      &__wrapper {
        display: flex;
      }
    }

    .date {
      display: flex;
      align-items: center;
      //background: rgba(255, 255, 255, .1);
      //border-radius: 12rpx;
      padding: 12rpx 24rpx;

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
      padding-left: 8px;

      &__in {
        padding-left: 16px;
      }
    }
  }
}

.quick-add {
  position: fixed;
  right: 48px;
  bottom: 80px;
  padding: 8px 16px;
  box-shadow: 0 2px 8px rgba(#a5a5a5, 0.3);
  border-radius: 24px;
  background-color: #fff;
  color: $theme-color-primary-brand;
  z-index: 9;
  font-size: 20px;

  ::v-deep .custom-icon {
    color: $theme-color-primary-brand !important;
    margin-right: 4px;
  }
}

.add-popup {
  //border-radius: 24px 24px 0 0;
  //padding: 16px;

  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 16px 16px 16px;

    .ledger {
      display: flex;
      align-items: center;
      justify-content: center;
    }
  }

  .action {
    display: flex;
    align-items: center;
    justify-content: space-between;
    color: $uni-text-color;
    font-size: 14px;
    padding: 0px 16px 0px 16px;

    .category-type {
      display: flex;
      align-items: center;

      &__item {
        margin-right: 8px;
        display: flex;
        align-items: center;
        background: $u-bg-color;
        border-radius: 12rpx;
        padding: 12rpx 24rpx;
      }

      &__item--active {
        background: rgba(100, 147, 237, .2);
        color: $theme-color-primary-brand;
      }
    }

    .date {
      display: flex;
      align-items: center;
      background: $u-bg-color;
      border-radius: 12rpx;
      padding: 12rpx 24rpx;

      &__icon {
        display: flex;
        align-items: center;
        justify-content: center;
        padding-left: 8rpx;
      }
    }
  }

  .money {
    display: flex;
    align-items: center;
    font-size: 32px;
    border-bottom: 1px solid $u-border-color;
    margin: 8px 16px 12px 16px;
    padding: 6px 0;

    &__label {
      padding-right: 8px;
    }

    .value-wrapper {
      flex: 1;
      display: flex;
      align-items: center;
      position: relative;
    }

    &__value {
      padding: 0 8px;
    }

    &__cursor {
      position: relative;
      width: 2px;
      height: 40px;
      animation-name: cursor;
      animation-duration: 1s;
      animation-iteration-count: infinite;
      z-index: 1;
      background-color: $theme-color-primary-brand;
    }

    @keyframes cursor {
      0% {
        opacity: 1;
      }

      100% {
        opacity: 0;
      }
    }
  }

  .description {
    margin: 0 16px 12px 16px;
  }

  .link {
    color: $uni-text-color-grey;
    padding: 16px 16px 16px 16px;
    font-size: 14px;
    text-align: right;
  }

  .category {
    padding: 0 16px 0 16px;
    flex-wrap: wrap;
    max-height: 160px;
    overflow: auto;

    &__item {
      display: flex;
      flex-direction: column;
      align-items: center !important;
      justify-content: center !important;
      padding-bottom: 16px;
    }

    &__item--active {
      .category__icon {
        background: rgba(100, 147, 237, .2);
        color: $theme-color-primary-brand;

        ::v-deep .custom-icon {
          color: $theme-color-primary-brand !important;
        }
      }
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
      padding-top: 10px;
      color: #666;
      font-size: 12px;
      text-align: center;
    }
  }

  .keyboard {
    padding: 16px 16px 32px 16px;
    background-color: #f7f7f7;
  }
}

.category-popup {
  //border-radius: 24px 24px 0 0;
  //padding: 16px;
  //font-size: 16px;
  .category-popup-wrapper {

  }

  ::v-deep .u-popup__content {
    background-color: #f8f8f8;
  }

  .header {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16px 16px 16px 16px;
    border-bottom: 0.5px solid $u-border-color;

    .ledger {
      display: flex;
      align-items: center;
      justify-content: center;
      color: $uni-text-color;
    }

    .close {
      position: absolute;
      right: 16px;
    }
  }

  .content {
    padding: 16px;
    height: 640px;
    overflow: auto;

    .group {
      &__header {
        font-size: 14px;
        color: $uni-text-color-grey;
        padding-bottom: 12px;
      }

      .category {
        flex-wrap: wrap;

        &__item {
          padding-bottom: 12px;
        }

        &__item--active {
          .category__label {
            background: $theme-color-primary-brand;
            color: #FFFFFF;
          }
        }

        &__label {
          background-color: #fff;
          color: $uni-text-color;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 14px;
          height: 40px;
          border-radius: 4px;
        }
      }
    }
  }
}

</style>
<style lang="scss">
.add-popup {
  .category {
    &::-webkit-scrollbar {
      display: block;
      width: 10px !important; /* 设置滚动条宽度 */
    }

    &::-webkit-scrollbar-track {
      background: #fff !important; /* 滚动条轨道背景色 */
    }

    &::-webkit-scrollbar-thumb {
      background: #ccc !important; /* 滚动条滑块颜色 */
      border-radius: 5px;
    }
  }

}
</style>
