<template>
  <el-container>
    <el-main>
<!--      <el-row :gutter="16" class="menu">-->
<!--        <el-col :span="4" class="menu__item">-->
<!--          <div class="menu__wrapper">-->
<!--            <div class="menu__icon">-->
<!--              <svg-icon icon-class="dashboard" class=""/>-->
<!--            </div>-->
<!--            <div class="menu__name">记一笔</div>-->
<!--          </div>-->
<!--        </el-col>-->
<!--        <el-col :span="4" class="menu__item">-->
<!--          <div class="menu__wrapper">-->
<!--            <div class="menu__icon">-->
<!--              <svg-icon icon-class="dashboard" class=""/>-->
<!--            </div>-->
<!--            <div class="menu__name">交易明细</div>-->
<!--          </div>-->
<!--        </el-col>-->
<!--        <el-col :span="4" class="menu__item">-->
<!--          <div class="menu__wrapper">-->
<!--            <div class="menu__icon">-->
<!--              <svg-icon icon-class="dashboard" class=""/>-->
<!--            </div>-->
<!--            <div class="menu__name">账本管理</div>-->
<!--          </div>-->
<!--        </el-col>-->
<!--        <el-col :span="4" class="menu__item">-->
<!--          <div class="menu__wrapper">-->
<!--            <div class="menu__icon">-->
<!--              <svg-icon icon-class="dashboard" class=""/>-->
<!--            </div>-->
<!--            <div class="menu__name">分类管理</div>-->
<!--          </div>-->
<!--        </el-col>-->
<!--        <el-col :span="4" class="menu__item">-->
<!--          <div class="menu__wrapper">-->
<!--            <div class="menu__icon">-->
<!--              <svg-icon icon-class="dashboard" class=""/>-->
<!--            </div>-->
<!--            <div class="menu__name">财务报表</div>-->
<!--          </div>-->
<!--        </el-col>-->
<!--      </el-row>-->
      
      <el-row :gutter="16">
        <el-col :span="24">
          <div class="current-data">
            <el-card>
              <template #header>
                <div class="card-header">
                  <span>实时数据</span>
                  <el-select
                      v-model="ledgerId"
                      placeholder="请选择账本"
                      style="width: 140px"
                  >
                    <el-option
                        v-for="item in ledgerList"
                        :key="item.id"
                        :label="item.name"
                        :value="item.id"
                    />
                  </el-select>
                </div>
              </template>
              <el-row :gutter="24">
                <el-col :span="6">
                  <div class="data">
                    <div class="data__desc">本日</div>
                    <div class="data__value data__value1">
                      <span><small>支出：</small>{{ financialSummaries.todayExpense }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <top/>-->
                      <!--                      </el-icon>-->
                      <!--                        1%-->
                      <!--                      </span>-->
                    </div>
                    <div class="data__value data__value2">
                      <span><small>收入：</small>{{ financialSummaries.todayIncome }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <bottom/>-->
                      <!--                      </el-icon>-->
                      <!--                        0%-->
                      <!--                      </span>-->
                    </div>
                  </div>
                </el-col>
                <el-col :span="6">
                  <div class="data">
                    <div class="data__desc">本周</div>
                    <div class="data__value data__value1">
                      <span><small>支出：</small>{{ financialSummaries.weekExpense }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <top/>-->
                      <!--                      </el-icon>-->
                      <!--                        20%-->
                      <!--                      </span>-->
                    </div>
                    <div class="data__value data__value2">
                      <span><small>收入：</small>{{ financialSummaries.weekIncome }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <bottom/>-->
                      <!--                      </el-icon>-->
                      <!--                        0%-->
                      <!--                      </span>-->
                    </div>
                  </div>
                </el-col>
                <el-col :span="6">
                  <div class="data">
                    <div class="data__desc">本月</div>
                    <div class="data__value data__value1">
                      <span><small>支出：</small>{{ financialSummaries.monthExpense }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <top/>-->
                      <!--                      </el-icon>-->
                      <!--                        4%-->
                      <!--                      </span>-->
                    </div>
                    <div class="data__value data__value2">
                      <span><small>收入：</small>{{ financialSummaries.monthIncome }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <bottom/>-->
                      <!--                      </el-icon>-->
                      <!--                        10%-->
                      <!--                      </span>-->
                    </div>
                  </div>
                </el-col>
                <el-col :span="6">
                  <div class="data">
                    <div class="data__desc">本年</div>
                    <div class="data__value data__value1">
                      <span><small>支出：</small>{{ financialSummaries.yearExpense }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <top/>-->
                      <!--                      </el-icon>-->
                      <!--                        23%-->
                      <!--                      </span>-->
                    </div>
                    <div class="data__value data__value2">
                      <span><small>收入：</small>{{ financialSummaries.yearIncome }}</span>
                      <!--                      <span class="data__percent">-->
                      <!--                        <el-icon>-->
                      <!--                        <bottom/>-->
                      <!--                      </el-icon>-->
                      <!--                        10%-->
                      <!--                      </span>-->
                    </div>
                  </div>
                </el-col>

              </el-row>
            </el-card>
          </div>
        </el-col>
        <!--        <el-col :span="12">-->
        <!--          <div class="budget">-->
        <!--            <el-card>-->
        <!--              <template #header>-->
        <!--                <div class="card-header">-->
        <!--                  <span>实时预算</span>-->
        <!--                </div>-->
        <!--              </template>-->
        <!--              <div class="budget__wrapper">-->
        <!--                <div class="budget__info">-->
        <!--                  <div>预算金额：￥0</div>-->
        <!--                  <div>已用金额：￥0</div>-->
        <!--                  <div>开始时间：2025-01-01</div>-->
        <!--                  <div>结束时间：2025-12-31</div>-->
        <!--                </div>-->
        <!--                <div class="budget__progress">-->
        <!--                  <el-progress type="circle" :stroke-width="10" :width="120" :color="colors" :percentage="99">-->
        <!--                    <template #default="{ percentage }">-->
        <!--                      <div class="percentage-value">0%</div>-->
        <!--                      <div class="percentage-label">已用</div>-->
        <!--                    </template>-->
        <!--                  </el-progress>-->
        <!--                </div>-->
        <!--                <div class="budget__chart">-->
        <!--                  <div ref="chart1" style="width: 100%; height: 100%;"/>-->
        <!--                </div>-->
        <!--              </div>-->
        <!--            </el-card>-->
        <!--          </div>-->
        <!--        </el-col>-->
      </el-row>
      <el-row :gutter="16" style="margin-top: 16px;">
        <el-col :span="24">
          <el-card class="forecast">
            <template #header>
              <div class="card-header">
                <span>年度收支对比分析</span>
              </div>
            </template>
            <div class="chart">
              <div ref="yearlyStatsRef" style="width: 100%; height: 100%;"/>
            </div>
          </el-card>
        </el-col>
      </el-row>
      <el-row :gutter="16" style="margin-top: 16px;" class="rank">
        <el-col :span="6" class="rank__item">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>本月支出分类排名Top10</span>
              </div>
            </template>
            <div class="rank__content">
              <div class="content" v-for="item in monthlyExpenseTransactionCategoryTopNComputed">
                <div class="rank__progress">
                  <el-progress :stroke-width="12" :color="colors" :percentage="item.percentage" :show-text="false"/>
                </div>
                <div class="rank__desc">
                  <div class="desc__label">{{ item.transactionCategoryName }}</div>
                  <div class="desc__value">总金额：{{ item.transactionTotalAmount }}</div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="6" class="rank__item">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>本月收入分类排名Top10</span>
              </div>
            </template>
            <div class="rank__content">
              <div class="content" v-for="item in monthlyIncomeTransactionCategoryTopNComputed">
                <div class="rank__progress">
                  <el-progress :stroke-width="12" :color="colors" :percentage="item.percentage" :show-text="false"/>
                </div>
                <div class="rank__desc">
                  <div class="desc__label">{{ item.transactionCategoryName }}</div>
                  <div class="desc__value">总金额：{{ item.transactionTotalAmount }}</div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="6" class="rank__item">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>本月支出分类构成</span>
              </div>
            </template>
            <div class="rank__content">
              <div class="content compose">
                <div ref="monthlyExpenseTransactionCategoryChartRef" style="width: 100%; height: 100%;"/>
              </div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="6" class="rank__item">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>本月支出排名Top10</span>
              </div>
            </template>
            <div class="rank__content">
              <div class="content transaction" v-for="item in monthlyExpenseTransactionTopN">
                <div class="transaction__icon">
                  <svg-icon :icon-class="item.transactionCategoryIcon" class=""/>
                </div>
                <div class="transaction__body">
                  <div class="transaction__category">{{ item.transactionCategoryName }}</div>
                  <div class="transaction__desc">{{ item.description }}</div>
                </div>
                <div class="transaction__footer">
                  <div class="transaction__amount">- {{ item.amount }}</div>
                  <div class="transaction__date">{{ dayjs(item.date).format('MM月DD日 HH:mm') }}</div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>

      </el-row>
    </el-main>
  </el-container>
</template>

<script setup name="Index">
// --- 框架工具等相关 ---
import _ from "lodash";
import * as echarts from 'echarts';
import {computed, onMounted} from "vue";
import dayjs from "dayjs";

// --- api 相关 ----
import {
  getTransactionLedgerSnapshot,
  getTransactionMonthlyExpenseTransactionCategoryTopN,
  getTransactionMonthlyIncomeTransactionCategoryTopN,
  getTransactionMonthlyExpenseTransactionCategory,
  getTransactionMonthlyExpenseTransactionTopN,
  getTransactionYearlyStats
} from "@/api/ledger/transaction";
import {getLedgerAll} from "@/api/ledger";

// --- 统计相关 ----
const ledgerList = ref([])
const ledgerId = ref()

const expenseMonthlyTotals = ref([])

const financialSummaries = ref({})

const yearlyStats = ref([])
const yearlyStatsRef = ref(null)

const monthlyExpenseTransactionCategoryTopN = ref([])
const monthlyExpenseTransactionCategoryTopNComputed = computed(() => {
  const sum = _.sumBy(monthlyExpenseTransactionCategoryTopN.value, 'transactionTotalAmount')
  return monthlyExpenseTransactionCategoryTopN.value.map(o => {
    return {
      ...o,
      percentage: parseFloat((o.transactionTotalAmount / sum * 100).toFixed(2))
    }
  })
})

const monthlyIncomeTransactionCategoryTopN = ref([])
const monthlyIncomeTransactionCategoryTopNComputed = computed(() => {
  const sum = _.sumBy(monthlyIncomeTransactionCategoryTopN.value, 'transactionTotalAmount')
  return monthlyIncomeTransactionCategoryTopN.value.map(o => {
    return {
      ...o,
      percentage: parseFloat((o.transactionTotalAmount / sum * 100).toFixed(2))
    }
  })
})

const monthlyExpenseTransactionCategory = ref([])
const monthlyExpenseTransactionCategoryChartRef = ref(null)

const monthlyExpenseTransactionTopN = ref([])

const chart1 = ref(null);

const colors = [
  {color: '#e6a23c', percentage: 10},
  {color: '#f87e27', percentage: 50},
]

watch(ledgerId, (val) => {
  _getFinancialSummaries()
  _getTransactionYearlyStats()
  _getTransactionMonthlyExpenseTransactionCategoryTopN()
  _getTransactionMonthlyIncomeTransactionCategoryTopN()
  _getTransactionMonthlyExpenseTransactionCategory()
  _getTransactionMonthlyExpenseTransactionTopN()
})

// watch(incomeMonthlyTotals, (val) => {
//   if (val.length) {
//     getChart2()
//   }
// })

async function getChart1() {
  const xAxis = ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月']
  const series = [{
    categoryType: '支出',
    data: expenseMonthlyTotals.value.map(o => o.totalAmount)
  }]
  const chartIntance = echarts.init(chart1.value, "macarons");
  chartIntance.setOption({
    title: {
      text: '月度支出趋势',
      textStyle: {
        fontWeight: 'normal',
        fontSize: 14,
        color: '#666666'
      },
      left: 'center',
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross',
        label: {
          backgroundColor: '#6a7985'
        }
      },
    },
    grid: {
      top: '30',
      left: '20',
      right: '30',
      bottom: '0',
      containLabel: true
    },
    legend: {
      show: false,
      data: series.map(o => o.categoryType),
      // icon: 'roundRect',
      itemGap: 16,
      // right: 80,
    },
    xAxis: [
      {
        type: 'category',
        boundaryGap: false,
        data: xAxis,
        axisTick: {
          show: false
        },
        axisLine: {
          show: false,
        }
      }
    ],
    yAxis: [
      {
        type: 'value',
        // name: '支出￥',
        nameTextStyle: {
          color: "#909399",
        },
        axisLabel: {
          show: false,
          margin: 20,
        },
        splitLine: {
          show: false,
          lineStyle: {
            type: 'dashed'
          }
        }
      },
    ],
    series: [
      {
        name: series[0].categoryType,
        type: 'line',
        // barWidth: '12',
        itemStyle: {
          borderRadius: [4, 4, 0, 0],
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1,
              [
                {
                  offset: 0,
                  color: 'rgb(212,37,77)',
                },
                {
                  offset: 1,
                  color: 'rgb(198,3,24)',
                },
              ], false)
        },
        smooth: true,
        showSymbol: false,
        data: series[0].data,
        tooltip: {
          valueFormatter: function (value) {
            return '￥' + value
          }
        }
      },
    ]
  });
}

const _getLedgerAll = async () => {
  const response = await getLedgerAll()
  ledgerList.value = response.data
}

const _getFinancialSummaries = async () => {
  const params = {ledgerId: ledgerId.value}
  const response = await getTransactionLedgerSnapshot(params)
  financialSummaries.value = response.data
}

const _getTransactionYearlyStats = async () => {
  const params = {ledgerId: ledgerId.value}
  const response = await getTransactionYearlyStats(params)
  yearlyStats.value = response.data
  await initYearlyStatsChart()
}

const _getTransactionMonthlyExpenseTransactionCategoryTopN = async () => {
  const params = {ledgerId: ledgerId.value, top: 10}
  const response = await getTransactionMonthlyExpenseTransactionCategoryTopN(params)
  monthlyExpenseTransactionCategoryTopN.value = response.data
}

const _getTransactionMonthlyIncomeTransactionCategoryTopN = async () => {
  const params = {ledgerId: ledgerId.value, top: 10}
  const response = await getTransactionMonthlyIncomeTransactionCategoryTopN(params)
  monthlyIncomeTransactionCategoryTopN.value = response.data
}

const _getTransactionMonthlyExpenseTransactionCategory = async () => {
  const params = {ledgerId: ledgerId.value}
  const response = await getTransactionMonthlyExpenseTransactionCategory(params)
  monthlyExpenseTransactionCategory.value = response.data
  await initMonthlyExpenseTransactionCategoryChart()
}

const _getTransactionMonthlyExpenseTransactionTopN = async () => {
  const params = {ledgerId: ledgerId.value, top: 10}
  const response = await getTransactionMonthlyExpenseTransactionTopN(params)
  monthlyExpenseTransactionTopN.value = response.data
}

async function initYearlyStatsChart() {
  const xAxis = yearlyStats.value.map(o => o.monthLabel)
  const series = [
    {
      categoryType: '支出',
      data: yearlyStats.value.map(o => o.expenseAmount)
    },
    {
      categoryType: '收入',
      data: yearlyStats.value.map(o => o.incomeAmount)
    }
  ]
  const chartIntance = echarts.init(yearlyStatsRef.value, "macarons");
  chartIntance.setOption({
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross',
        label: {
          backgroundColor: '#6a7985'
        }
      },
    },
    grid: {
      top: '30',
      left: '20',
      right: '30',
      bottom: '0',
      containLabel: true
    },
    legend: {
      data: series.map(o => o.categoryType),
      // icon: 'roundRect',
      itemGap: 16,
      // right: 80,
    },
    xAxis: [
      {
        type: 'category',
        boundaryGap: false,
        data: xAxis
      },
    ],
    yAxis: [
      {
        type: 'value',
        name: '金额',
        nameTextStyle: {
          color: "#909399",
        },
        axisLabel: {
          margin: 20,
        },
        splitLine: {
          lineStyle: {
            type: 'dashed'
          }
        }
      },
    ],
    series: [
      {
        name: series[0].categoryType,
        type: 'line',
        barWidth: '12',
        itemStyle: {
          borderRadius: [10, 10, 0, 0],
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1,
              [
                {
                  offset: 0,
                  color: 'rgb(212,37,77)',
                },
                {
                  offset: 1,
                  color: 'rgb(198,3,24)',
                },
              ], false)
        },
        smooth: true,
        showSymbol: false,
        data: series[0].data,
        tooltip: {
          valueFormatter: function (value) {
            return value
          }
        }
      },
      {
        name: series[1].categoryType,
        type: 'line',
        barWidth: '12',
        itemStyle: {
          borderRadius: [10, 10, 0, 0],
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1,
              [
                {
                  offset: 0,
                  color: 'rgb(0,194,69)',
                },
                {
                  offset: 1,
                  color: 'rgb(10,151,0)',
                },
              ], false)
        },
        smooth: true,
        showSymbol: false,
        emphasis: {
          focus: 'series'
        },
        data: series[1].data,
        tooltip: {
          valueFormatter: function (value) {
            return value
          }
        }
      },
    ]
  });
}

async function initMonthlyExpenseTransactionCategoryChart() {
  const data = monthlyExpenseTransactionCategory.value.map(o => {
    return {
      name: o.transactionCategoryName,
      value: o.transactionTotalAmount,
    }
  })

  const chartIntance = echarts.init(monthlyExpenseTransactionCategoryChartRef.value, "macarons")
  chartIntance.setOption({
    tooltip: {
      trigger: 'item'
    },
    series: [
      {
        name: '',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: true,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        data: data
      }
    ]
  })
}

onMounted(async () => {
  await _getLedgerAll()
  if (ledgerList.value.length) {
    ledgerId.value = ledgerList.value[0].id

    await _getFinancialSummaries()
    await _getTransactionYearlyStats()
    await _getTransactionMonthlyExpenseTransactionCategoryTopN()
    await _getTransactionMonthlyIncomeTransactionCategoryTopN()
    await _getTransactionMonthlyExpenseTransactionCategory()
    await _getTransactionMonthlyExpenseTransactionTopN()
  }

})

</script>

<style scoped lang="scss">
::v-deep .el-card {
  height: 100%;
  display: flex;
  flex-direction: column;

  .el-card__body {
    flex: 1;
    display: flex;
  }

  .el-card__footer {
    padding-top: 12px;
    padding-bottom: 12px;
  }

  .card-header {
    font-weight: bold;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .card-footer {
    text-align: right;
    font-size: var(--el-font-size-base);
    color: var(--el-text-color-secondary);

  }
}

.menu {
  display: flex;

  &__item {
    &:first-child {
      .menu__wrapper {
        background: linear-gradient(to right, #ffac80, #F99C4A);
      }
    }

    &:nth-child(2) {
      .menu__wrapper {
        background: linear-gradient(to right, #50d57b, #0AB1A7);
      }
    }

    &:nth-child(3) {
      .menu__wrapper {
        background: linear-gradient(to right, #59b5c9, #3AC2FC);
      }
    }

    &:nth-child(4) {
      .menu__wrapper {
        background: linear-gradient(to right, #fdae7f, #ff6464);
      }
    }

    &:nth-child(5) {
      .menu__wrapper {
        background: linear-gradient(to right, #09C8BD, #0AB1A7);
      }
    }

    &:last-child {
      .menu__wrapper {
        background: linear-gradient(to right, #62D0FF, #3AC2FC);
      }
    }
  }

  &__wrapper {
    display: flex;
    align-items: center;
    justify-content: space-around;
    padding: 16px;
    border-radius: 12px;
    color: #FFFFFF;
  }

  &__icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 56px;
    height: 56px;
    border-radius: 12px;

    .svg-icon {
      width: 32px;
      height: 32px;
    }
  }

  &__name {
    font-weight: bold;
  }
}

:deep(.current-data) {
  height: 220px;

  .el-card {
    &__body {
      //padding-bottom: 0 !important;
    }
  }

  .el-row {
    height: 100%;
    width: 100%;
  }

  .data {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;

    &__value {
      //font-weight: bold;
      font-size: 18px;
      display: flex;
      align-items: center;
      padding-top: 8px;

      small {
        color: var(--el-text-color-secondary);
      }
    }

    &__percent {
      display: flex;
      align-items: center;
      font-weight: bold;
      font-size: 14px;
      padding-left: 8px;
    }

    &__value1 {
      .data__percent {
        color: var(--el-color-danger);
      }
    }

    &__value2 {
      .data__percent {
        color: var(--el-color-success);
      }
    }

    &__desc {
      font-size: 18px;
      color: #606266;
      font-weight: bold;
      padding-bottom: 8px;
    }
  }
}

.budget {

  &__wrapper {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex: 1;
  }

  &__info {
    display: flex;
    flex-direction: column;
    justify-content: space-evenly;
    height: 100%;
    color: var(--el-text-color-primary);
  }

  &__progress {
    padding-left: 32px;
    padding-right: 32px;
  }

  &__chart {
    flex: 1;
  }
}

.weather {
  height: 220px;

  .chart {
    flex: 1;
  }
}

.forecast {
  height: 320px;

  .chart {
    flex: 1;
  }
}

.rank {
  color: var(--el-text-color-regular);

  ::v-deep .el-card {
    &__body {
      display: block;
      overflow: auto;
    }
  }

  &__item {
    &:first-child {
      :deep(.el-progress-bar__inner) {
        background: linear-gradient(to left, #ffac80, #F99C4A);
      }
    }

    &:nth-child(2) {
      :deep(.el-progress-bar__inner) {
        background: linear-gradient(to left, #80ffa9, #0AB1A7);
      }
    }

    &:nth-child(3) {
      :deep(.el-progress-bar__inner) {
        background: linear-gradient(to left, #80e8ff, #3AC2FC);
      }
    }

    &:nth-child(4) {
      :deep(.el-progress-bar__inner) {
        background: linear-gradient(to left, #fdae7f, #ff6464);
      }
    }
  }

  &__content {
    height: 260px;
  }

  .content {
    padding-bottom: 12px;

    &:last-child {
      padding-bottom: 0;
    }
  }

  &__desc {
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-size: 12px;
    padding-top: 4px;
  }

  .desc__label {
    flex: 1;
    padding-right: 16px;
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;
  }
}

.el-progress {
  :deep(.el-progress-circle) {
    svg > path:nth-child(2) {
      stroke: url(#progress1);
    }
  }

  ::v-deep &__text {
    font-size: var(--el-font-size-base) !important;
    color: var(--el-text-color-secondary);
  }

  .percentage-value {
    font-size: 22px;
    color: #f87e27;
  }

  .percentage-label {
    line-height: 24px;
  }
}

.transaction {
  display: flex;
  color: var(--el-text-color-primary);

  &__icon {
    width: 48px;
    height: 48px;
    border-radius: 24px;
    background: linear-gradient(to right, #ffac80, #F99C4A);
    display: flex;
    align-items: center;
    justify-content: center;
    color: #FFFFFF;
  }

  &__body {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    padding-left: 12px;
  }

  &__desc {
    font-size: var(--el-font-size-base);
    color: var(--el-text-color-secondary);
  }

  &_amount {
    font-weight: bold;
  }

  &__date {
    font-size: var(--el-font-size-base);
    color: var(--el-text-color-secondary);
  }

  &__footer {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    justify-content: space-around;
  }
}

.compose {
  width: 100%;
  height: 100%;
}

</style>

