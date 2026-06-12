<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="100px" @submit.prevent="onSearch">
            <el-form-item label="账本" prop="ledgerId">
              <el-select
                  v-model="searchParams.ledgerId"
                  placeholder="请选择账本"
                  style="width: 160px"
              >
                <el-option
                    v-for="item in ledgerList"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                    :disabled="item.status === 1"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="交易分类" prop="transactionCategoryId">
              <el-select
                  v-model="searchParams.transactionCategoryId"
                  placeholder="请选择账本"
                  style="width: 160px"
              >
                <el-option
                    v-for="item in transactionCategoryList"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                    :disabled="item.status === 1"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="交易描述" prop="description">
              <el-input
                  v-model="searchParams.description"
                  placeholder="请输入交易描述"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item label="状态" prop="status">
              <el-select
                  v-model="searchParams.status"
                  placeholder="状态"
                  clearable
                  style="width: 160px"
              >
                <el-option
                    v-for="dict in transaction_status"
                    :key="dict.value"
                    :label="dict.label"
                    :value="dict.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="日期范围" prop="dateRange">
              <el-date-picker
                  v-model="searchParams.dateRange"
                  type="datetimerange"
                  range-separator="-"
                  start-placeholder="开始日期"
                  end-placeholder="结束日期"
                  :shortcuts="shortcuts"
                  format="YYYY-MM-DD HH:mm"
                  time-format="HH:mm"
              ></el-date-picker>
            </el-form-item>
            <el-form-item>
              <el-button type="primary" icon="Search" native-type="submit">搜索</el-button>
              <el-button icon="Refresh" @click="onResetSearch">重置</el-button>
              <el-button
                  type="success"
                  icon="Download"
                  @click="onExport"
                  :loading="exporting"
              >导出
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>

        <el-card shadow="always" class="table">
          <div class="table-container">
            <div class="table-container__header">
              <el-row :gutter="12">
                <el-col :span="1.5">
                  <el-button type="primary" plain icon="Plus" @click="onAdd">新增
                  </el-button>
                </el-col>
                <el-col :span="1.5">
                  <el-button type="danger" plain icon="Delete" :disabled="hasNoSelection" @click="onDelete"
                  >批量删除
                  </el-button>
                </el-col>
              </el-row>
            </div>
            <div class="table-container__body" ref="tableRef">
              <el-table v-loading="tableLoading"
                        :data="list"
                        border
                        fit
                        style="width: 100%;"
                        :max-height="tableMaxHeight"
                        :default-sort="tableDefaultSort"
                        @selection-change="onSelectionChange"
                        @sort-change="onSortChange">
                <el-table-column type="selection" width="50" align="center"/>
                <el-table-column label="id" align="right" width="100" fixed="left" key="id" prop="id" sortable="custom"
                                 :sort-orders="['descending', 'ascending']"/>
                <el-table-column label="账本" key="ledgerName" prop="ledgerName"/>
                <el-table-column label="交易分类" key="transactionCategoryName" prop="transactionCategoryName"/>
                <el-table-column label="交易类型" align="center" key="type" prop="type">
                  <template #default="scope">
                    <el-tag type="success" v-if="scope.row.type === '收入'">收入</el-tag>
                    <el-tag type="danger" v-else-if="scope.row.type === '支出'">支出</el-tag>
                    <el-tag type="info" v-else>不计入收支</el-tag>
                  </template>
                </el-table-column>
                <el-table-column label="交易金额" key="amount" prop="amount"/>
                <el-table-column label="交易描述" key="description" prop="description"/>
                <el-table-column label="交易日期" align="center" key="date" prop="date" sortable="custom"
                                 :sort-orders="['descending', 'ascending']">
                  <template #default="scope">
                    <span>{{
                        scope.row.date && dayjs(scope.row.date).format('YYYY-MM-DD HH:mm:ss')
                      }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="状态" align="center" key="status" width="170">
                  <template #default="scope">
                    <el-switch
                        v-model="scope.row.status"
                        active-value="0"
                        inactive-value="1"
                        active-text="已完成"
                        inactive-text="已废弃"
                        @change="onStatusChange(scope.row)"
                    ></el-switch>
                  </template>
                </el-table-column>
                <el-table-column label="备注" align="center" key="remark" prop="remark"/>
                <el-table-column label="创建时间" align="center" key="createdAt" prop="createdAt" sortable="custom"
                                 :sort-orders="['descending', 'ascending']">
                  <template #default="scope">
                    <span>{{
                        scope.row.createdAt && dayjs(scope.row.createdAt).format('YYYY-MM-DD HH:mm:ss')
                      }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="更新时间" align="center" key="updatedAt" prop="updatedAt" sortable="custom"
                                 :sort-orders="['descending', 'ascending']">
                  <template #default="scope">
                    <span>{{
                        scope.row.updatedAt && dayjs(scope.row.updatedAt).format('YYYY-MM-DD HH:mm:ss')
                      }}</span>
                  </template>
                </el-table-column>
                <el-table-column label="操作" align="center" width="240" fixed="right">
                  <template #default="scope">
                    <el-button link type="primary" icon="Edit" @click="onUpdate(scope.row)"
                    >编辑
                    </el-button>
                    <el-button link type="primary" icon="Delete" @click="onDelete(scope.row)"
                    >删除
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
            </div>
            <div class="table-container__footer">
              <pagination
                  v-show="total > 0"
                  :total="total"
                  v-model:page="searchParams.pageNumber"
                  v-model:limit="searchParams.pageSize"
                  @pagination="loadList"
              />
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <el-dialog :title="dialogTitle" v-model="isDialogOpen" width="800px" append-to-body>
      <el-form :model="formData" :rules="formRules" ref="formRef" label-width="110">
        <el-form-item label="账本" prop="ledgerId">
          <el-select
              v-model="formData.ledgerId"
              placeholder="请选择账本"
              :disabled="formData.id"
          >
            <el-option
                v-for="item in ledgerList"
                :key="item.id"
                :label="item.name"
                :value="item.id"
                :disabled="item.status === 1"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="交易分类" prop="transactionCategoryId">
          <el-select
              v-model="formData.transactionCategoryId"
              placeholder="请选择交易分类"
          >
            <el-option
                v-for="item in transactionCategoryList"
                :key="item.id"
                :label="item.name"
                :value="item.id"
                :disabled="item.status === 1"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="交易类型" prop="type">
          <el-radio-group v-model="formData.type">
            <el-radio-button value="支出">支出</el-radio-button>
            <el-radio-button value="收入">收入</el-radio-button>
            <el-radio-button value="不计入收支">不计入收支</el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="交易描述" prop="description">
          <el-input v-model="formData.description" placeholder="请输入交易描述"/>
        </el-form-item>
        <el-form-item label="交易金额（元）" prop="amount" label-width="120">
          <el-input-number :step="0.01" v-model="formData.amount" placeholder="请输入交易金额"/>
        </el-form-item>
        <el-form-item label="交易时间" prop="date">
          <el-date-picker
              v-model="formData.date"
              format="YYYY-MM-DD HH:mm"
              time-format="HH:mm"
              type="datetime"
              placeholder="请选择交易时间"
          />
        </el-form-item>
        <el-form-item label="状态" prop="status">
          <el-radio-group v-model="formData.status">
            <el-radio
                v-for="dict in transaction_status"
                :key="dict.value"
                :value="dict.value"
            >{{ dict.label }}
            </el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input v-model="formData.remark" type="textarea" placeholder="请输入内容"></el-input>
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button type="primary" @click="onSubmitForm" :loading="isSubmitDisabled">确 定</el-button>
          <el-button @click="onCancel">取 消</el-button>
        </div>
      </template>
    </el-dialog>

  </div>
</template>

<script setup name="Transaction">
// --- 框架工具等相关 ---
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import {ElMessage, ElMessageBox} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc.js'

dayjs.extend(utc)
import XLSXS from "xlsx-js-style"

// --- api 相关 ---
import {
  getLedgerAll
} from "@/api/ledger/index.js"
import {
  getTransactionCategoryAll
} from "@/api/ledger/transaction-category.js"
import {
  getTransactionPaged,
  getTransaction,
  createTransaction,
  updateTransaction,
  deleteTransaction,
  batchDeleteTransaction,
  changeTransactionStatus,
} from "@/api/ledger/transaction.js"

// --- 表格相关 ---
const tableRef = ref(null)
const tableMaxHeight = ref(0)
const tableDefaultSort = ref({prop: "date", order: "descending"})
const tableLoading = ref(true)

// --- 表格数据相关 ---
const list = ref([])
const total = ref(0)
const selectedIds = ref([])
const hasNoSelection = ref(true)
const searchRef = ref(null)
const searchParams = ref({
  pageNumber: 1,
  pageSize: 20,
  orderByField: null,
  orderByType: null,
  ledgerId: null,
  transactionCategoryId: null,
  description: null,
  status: null,
  dateRange: null,
  startDate: null,
  endDate: null,
})
const shortcuts = [
  {
    text: '本周',
    value: () => {
      const start = dayjs().startOf('week').add(1, 'day'); // 周一（因为 startOf('week') 默认是周日）
      const end = start.add(6, 'day'); // 周日
      return [
        start.startOf('day').toDate(),
        end.endOf('day').toDate()
      ];
    },
  },
  {
    text: '本月',
    value: () => {
      const start = dayjs().startOf('month');
      const end = dayjs().endOf('month');
      return [
        start.toDate(),
        end.toDate()
      ];
    },
  },
  {
    text: '上月',
    value: () => {
      const start = dayjs().subtract(1, 'month').startOf('month');
      const end = dayjs().subtract(1, 'month').endOf('month');
      return [
        start.toDate(),
        end.toDate()
      ];
    },
  },
]

// --- 表单相关 ---
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  ledgerId: [{required: true, message: "账本不能为空", trigger: "change"}],
  transactionCategoryId: [{required: true, message: "交易分类不能为空", trigger: "change"}],
  type: [{required: true, message: "交易类型不能为空", trigger: "change"}],
  date: [{required: true, message: "交易日期不能为空", trigger: "blur"}],
  description: [{required: true, message: "交易描述不能为空", trigger: "blur"}],
  amount: [{required: true, message: "交易金额不能为空", trigger: "blur"}],
})
const formSubmitting = ref(false)
const isSubmitDisabled = computed(() => {
  // if (uploadCover.isUploading || formSubmitting.value) {
  //   return true
  // }
  // return false
  return formSubmitting.value
})

// --- 字典相关 ---
const {transaction_status} = useDict('transaction_status')

// --- 账本相关 ---
const ledgerList = ref([])

// --- 交易分类相关 ---
const transactionCategoryList = ref([])

// --- 导出相关 ---
const exporting = ref(false)

/* 加载账本列表 */
async function loadLedgerList() {
  try {
    const response = await getLedgerAll()
    const {success, data, message} = response
    if (success) {
      ledgerList.value = data
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 加载交易分类列表 */
async function loadTransactionCategoryList() {
  try {
    const response = await getTransactionCategoryAll()
    const {success, data, message} = response
    if (success) {
      transactionCategoryList.value = data
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 加载列表 */
async function loadList() {
  tableLoading.value = true
  try {
    console.log(searchParams.value.dateRange)
    if (searchParams.value.dateRange?.length > 0) {
      searchParams.value.startDate = dayjs(searchParams.value.dateRange[0]).utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]')
      searchParams.value.endDate = dayjs(searchParams.value.dateRange[1]).utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]')
    } else {
      searchParams.value.startDate = null
      searchParams.value.endDate = null
    }
    const response = await getTransactionPaged(searchParams.value)
    const {success, data, message} = response
    if (success) {
      list.value = data.items
      total.value = data.totalItems
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
    tableLoading.value = false
  }
}

/* 搜索 */
function onSearch() {
  searchParams.value.pageNumber = 1
  loadList()
}

/* 重置搜索 */
function onResetSearch() {
  searchRef.value?.resetFields()
  onSearch()
}

/* 删除 */
async function onDelete(row) {
  const delIds = row.id ? [row.id] : selectedIds.value
  try {
    await ElMessageBox.confirm('是否确认删除编号为 ' + delIds.join(', ') + ' 的数据项？')

    let response
    if (delIds.length > 1) {
      response = await batchDeleteTransaction(delIds)
    } else {
      response = await deleteTransaction(delIds[0])
    }

    // 接口成功但业务失败
    const {success, data, message} = response
    if (!success) {
      ElMessage.error(message)
      return
    }

    // 删除成功
    await loadList()
    ElMessage.success('删除成功')
  } catch (error) {
    // 用户点击“取消”或校验失败：静默处理
    if (error === 'cancel' || error === undefined) {
      return
    }

    // 其他异常（如网络错误、接口异常等）
    console.error('删除失败:', error)
    // ElMessage.error('删除失败，请重试')
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 修改状态  */
async function onStatusChange(row) {
  const originalStatus = row.status === '0' ? '1' : '0'
  const text = row.status === '0' ? '完成' : '废弃'

  try {
    await ElMessageBox.confirm(`确认要 ${text} ${row.description} 吗?`)
    const response = await changeTransactionStatus(row.id, row.status)

    // 接口成功但业务失败
    const {success, data, message} = response
    if (!success) {
      ElMessage.error(message)
      row.status = originalStatus // 直接回滚，不依赖 catch
    }

    ElMessage.success('操作成功')
  } catch (error) {
    // 无论是用户取消，还是接口失败，都回滚状态
    row.status = originalStatus

    // 用户点击“取消”或校验失败：静默处理
    if (error === 'cancel' || error === undefined) {
      return
    }

    // 其他异常（如网络错误、接口异常等）
    console.error('状态切换异常:', error)
    // ElMessage.error('删除失败，请重试')
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 多选框选中数据  */
function onSelectionChange(selection) {
  selectedIds.value = selection.map(item => item.id)
  hasNoSelection.value = !selection.length
}

/* 排序 */
function onSortChange({column, prop, order}) {
  searchParams.value.orderByField = prop
  searchParams.value.orderByType = order === 'ascending' ? 'ASC' : 'DESC'
  loadList()
}

/* 重置表单  */
function reset() {
  formData.value = {
    id: null,
    ledgerId: null,
    transactionCategoryId: null,
    type: '支出',
    amount: null,
    description: null,
    date: null,
    status: '0',
    remark: null
  };
  formRef.value?.resetFields()
}

/* 取消表单 */
function onCancel() {
  isDialogOpen.value = false
  reset()
}

/* 新增 */
function onAdd() {
  reset()
  isDialogOpen.value = true
  dialogTitle.value = '添加'
}

/* 修改 */
async function onUpdate(row) {
  reset()
  try {
    const response = await getTransaction(row.id)
    const {success, data, message} = response
    if (success) {
      formData.value = data
      dialogTitle.value = '修改'
      isDialogOpen.value = true
    } else {
      ElMessage.error(message)
    }
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
  }
}

/* 提交 */
function onSubmitForm() {
  formRef.value?.validate(async valid => {
    if (valid) {
      formSubmitting.value = true
      try {
        const response = await (formData.value.id !== null
            ? updateTransaction(formData.value)
            : createTransaction(formData.value))
        const {success, data, message} = response
        if (success) {
          ElMessage.success('操作成功')
          isDialogOpen.value = false
          await loadList()
        } else {
          ElMessage.error(message)
        }
      } catch (error) {
        // 可选：处理错误，比如提示用户
        console.error('提交失败:', error)
      } finally {
        // 无论成功或失败都会执行，如 loading 必须结束
        formSubmitting.value = false
      }
    }
  })
}

/* 设置表格最大高度 */
function updateTableMaxHeight() {
  tableMaxHeight.value = tableRef.value.clientHeight
}

/* 导出Excel */
function onExport() {
  exporting.value = true; // 开始导出，设置加载状态为 true

  // 必要数据
  const sheetName = '交易记录'
  const startDate = searchParams.value.dateRange?.length ? dayjs(searchParams.value.dateRange[0]).format('YYYY年MM月DD日HH时mm分') : ''
  const endDate = searchParams.value.dateRange?.length ? dayjs(searchParams.value.dateRange[1]).format('YYYY年MM月DD日HH时mm分') : ''

  // 定义颜色
  const grayColor = 'D3D3D3'; // 浅灰色（柔和背景，非刺眼）
  const yellowColor = 'FFF2CC'; // 柔和黄色（类似 Excel 默认高亮）
  const redColor = 'F8CBAD'; // 柔和红色/珊瑚色（避免纯红刺眼）

  // 需要导出的数据源
  const data = list.value;

  // 将数据源转换为二维数组格式，用于生成 Excel 表格
  const body = []

  // 定义标题
  const title = [
    {
      v: `${sheetName}`,
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
  ];
  body.push(title)

  // 定义副标题
  const subTitle = [
    {
      v: `时间范围：${startDate} - ${endDate}`,
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'right'}, // 居右对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
  ];
  body.push(subTitle)

  /* 定义表头
     每个元素代表一个单元格。
     如果需要样式，使用对象形式定义单元格内容及样式；
     v: 单元格显示值
     t: 数据类型（'s' 表示字符串）
     s: 样式对象，包含字体、对齐方式、填充色等
  */
  const header = [
    {
      v: 'Id',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '账本',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '交易分类',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '交易类型',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '交易金额',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '交易描述',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '交易日期',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '状态',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '备注',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '创建时间',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },
    {
      v: '更新时间',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      },
    },

  ]
  body.push(header)

  // 定义表身
  for (const item of data) {
    body.push([
      {
        v: item.id,
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.ledgerName,
        t: 's',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.transactionCategoryName,
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.type,
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.amount,
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.description,
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: dayjs(item.date).format('YYYY-MM-DD HH:mm:ss'),
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.status === '0' ? '已完成' : '已作废',
        t: 's',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.remark,
        t: 'n',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: dayjs(item.createdAt).format('YYYY-MM-DD HH:mm:ss'),
        t: 's',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },
      {
        v: item.updatedAt ? dayjs(item.updatedAt).format('YYYY-MM-DD HH:mm:ss') : '',
        t: 's',
        s: {
          alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
          fill: {fgColor: {rgb: yellowColor}}, // 背景色
        },
      },

    ])
  }

  // 定义表底
  const footer = [
    {
      v: '合计',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
    {
      v: '',
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'center'}, // 居中对齐
        fill: {fgColor: {rgb: grayColor}}, // 背景色
      }
    },
  ]
  body.push(footer);

  const footer2 = [
    {
      v: `备注：pageNumber:${searchParams.value.pageNumber}、pageSize:${searchParams.value.pageSize}、total:${total.value}`,
      t: 's',
      s: {
        font: {bold: true}, // 加粗
        alignment: {vertical: 'center', horizontal: 'right'}, // 居中对齐
        fill: {fgColor: {rgb: redColor}}, // 背景色
      }
    },
  ]
  body.push(footer2);

  // 使用 xlsx-js-style 的 aoa_to_sheet 方法将二维数组转换为 sheet 对象
  const sheet = XLSXS.utils.aoa_to_sheet(body);

  // 解析当前 sheet 的范围（如 A1:F20）
  const range = XLSXS.utils.decode_range(sheet['!ref']);

  // 获取最大列号和行号，用于遍历所有单元格
  const maxCol = range.e.c; // 结束列索引（c 是 column）
  const maxRow = range.e.r; // 结束行索引（r 是 row）

  // 统一设置单元格样式：边框、对齐方式等
  for (let r = 0; r <= maxRow; r++) {
    for (let c = 0; c <= maxCol; c++) {
      const cellAddress = XLSXS.utils.encode_cell({r: r, c: c});

      // 如果单元格为空，则设为空字符串，避免样式不生效
      if (!sheet[cellAddress]) {
        sheet[cellAddress] = {v: '', t: 's'};
      }

      // 统一添加边框样式
      sheet[cellAddress].s = {
        ...sheet[cellAddress].s,
        border: {
          top: {style: 'thin', color: {rgb: '000000'}},
          bottom: {style: 'thin', color: {rgb: '000000'}},
          left: {style: 'thin', color: {rgb: '000000'}},
          right: {style: 'thin', color: {rgb: '000000'}}
        }
      };

      // 可以使用如下方式单独设置行和列
      // if (r === 0) {
      //   sheet[cellAddress].s = {
      //     ...sheet[cellAddress].s,
      //     alignment: {vertical: 'center', horizontal: 'center'},
      //   };
      // } else if (r === 1) {
      //   sheet[cellAddress].s = {
      //     ...sheet[cellAddress].s,
      //     alignment: {vertical: 'center', horizontal: 'left'},
      //   };
      // } else if (r === 2) {
      //   sheet[cellAddress].s = {
      //     ...sheet[cellAddress].s,
      //     alignment: {vertical: 'center', horizontal: 'center'},
      //   };
      // } else if (r === maxRow) {
      //   sheet[cellAddress].s = {
      //     ...sheet[cellAddress].s,
      //     alignment: {vertical: 'center', horizontal: 'right'},
      //   };
      // } else {
      //   // 从第四行开始，设置第一列为居中，其他列为右对齐
      //   // if (c > 0) {
      //   //   sheet[cellAddress].s = {
      //   //     ...sheet[cellAddress].s,
      //   //     alignment: {vertical: 'center', horizontal: 'right'},
      //   //   };
      //   // } else {
      //   sheet[cellAddress].s = {
      //     ...sheet[cellAddress].s,
      //     alignment: {vertical: 'center', horizontal: 'right'},
      //   };
      //   // }
      // }
    }
  }

  // 行合并的数据
  const getSpanArr = (data, spanKey, columnIndex, mergeKey, extraMergeKeys) => {
    let spanArr = [];
    let pos = 0;

    for (let i = 0; i < data.length; i++) {
      if (i === 0) {
        spanArr.push(1);
        pos = 0;
      } else {
        let currentCompareValue, previousCompareValue;

        if (columnIndex === 0) {
          // 第一列只比较 spanKey
          currentCompareValue = data[i][spanKey];
          previousCompareValue = data[i - 1][spanKey];
        } else {
          // 如果有额外合并标记，则使用复杂标记 [合并标记字段的值 + 额外合并标记字段的值 + 当前列的值]
          if (extraMergeKeys && extraMergeKeys.length) {
            let currentComparePrefix = `${data[i][mergeKey]}__`;
            currentComparePrefix += extraMergeKeys.map(key => data[i][key]).join('__');
            currentCompareValue = currentComparePrefix + '__' + data[i][spanKey];
            let previousComparePrefix = `${data[i - 1][mergeKey]}__`;
            previousComparePrefix += extraMergeKeys.map(key => data[i - 1][key]).join('__');
            previousCompareValue = previousComparePrefix + '__' + data[i - 1][spanKey];
          } else {
            // 否则使用简单标记 [合并标记字段的值 + 当前列的值]
            currentCompareValue = `${data[i][mergeKey]}__${data[i][spanKey]}`;
            previousCompareValue = `${data[i - 1][mergeKey]}__${data[i - 1][spanKey]}`;
          }
        }

        if (currentCompareValue === previousCompareValue) {
          spanArr[pos] += 1;
          spanArr.push(0);
        } else {
          spanArr.push(1);
          pos = i;
        }
      }
    }

    console.log('span', spanKey, spanArr)
    return spanArr;
  };

  // 准备合并信息
  function prepareSpans(data) {
    // 合并的行
    const merges = [
      {
        // 第1行
        s: {r: 0, c: 0},
        e: {r: 0, c: maxCol}
      },
      {
        // 第2行
        s: {r: 1, c: 0},
        e: {r: 1, c: maxCol}
      },
      // {
      //   // 第3行第1列
      //   s: {r: 2, c: 0},
      //   e: {r: 3, c: 0}
      // },
      // {
      //   // 第3行第2列
      //   s: {r: 2, c: 1},
      //   e: {r: 3, c: 1}
      // },
      // {
      //   // 第3行第5列
      //   s: {r: 2, c: 4},
      //   e: {r: 3, c: 4}
      // },
      // {
      //   // 第3行第6列
      //   s: {r: 2, c: 5},
      //   e: {r: 3, c: 5}
      // },
      // {
      //   // 第3行第7列
      //   s: {r: 2, c: 6},
      //   e: {r: 3, c: 6}
      // },
      {
        // 最后一行
        s: {r: maxRow, c: 0},
        e: {r: maxRow, c: maxCol}
      }
    ]

    // 直接指定需要合并的列及其字段名和索引
    const mergeColumns = [
      // {key: 'client_name', columnIndex: 0},
      // {key: 'unit_price', columnIndex: 2},
      // {key: 'total_price_energy_usage', columnIndex: 3, extraMergeKeys: ['unit_price']},
      // {key: 'total_price_bills', columnIndex: 4, extraMergeKeys: ['unit_price']},
      // {key: 'total_energy_usage', columnIndex: 26},
      // {key: 'total_bills', columnIndex: 27},
    ];

    const mergeKey = 'id'; // 合并标记

    mergeColumns.forEach(({key, columnIndex, extraMergeKeys}) => {
      const spanArr = getSpanArr(data, key, columnIndex, mergeKey, extraMergeKeys);

      // 添加到 merges 数组供 Excel 使用
      for (let i = 0; i < spanArr.length; i++) {
        if (spanArr[i] > 0) {
          merges.push({
            s: {r: i + 3, c: columnIndex},                // 数据起始行是 i+3（标题、副标题、表头占3行）,表示第4行开始
            e: {r: i + 3 + spanArr[i] - 1, c: columnIndex}         // 结束行是 i+3+合并行（合并数-1）
          });
        }
      }
    });

    return merges;
  }

  sheet['!merges'] = prepareSpans(data); // 应用合并规则

  // 设置列宽
  const cols = [
    {wch: 15}, // Id
    {wch: 15}, // 账本
    {wch: 15}, // 交易分类
    {wch: 15}, // 交易类型
    {wch: 15}, // 交易金额
    {wch: 20}, // 交易描述
    {wch: 20}, // 交易日期
    {wch: 15}, // 状态
    {wch: 15}, // 备注
    {wch: 20}, // 创建时间
    {wch: 20}, // 更新时间
  ];
  sheet['!cols'] = cols; // 应用列宽设置

  // 设置行高
  // const rows = [{hpx: 40}];
  const rows = [];
  for (let i = 0; i <= maxRow; i++) {
    if (i > 2 && i < maxRow) {
      rows.push({hpx: 30}); // 其他行
    } else {
      rows.push({hpx: 40}); // 前三行和最后一行
    }
  }
  sheet['!rows'] = rows; // 应用行高设置

  // 创建虚拟 workbook
  const workbook = XLSXS.utils.book_new();

  // 向 workbook 中添加 sheet
  XLSXS.utils.book_append_sheet(workbook, sheet, `${sheetName}`);

  // 导出 Excel 文件
  XLSXS.writeFile(workbook, `${sheetName}（${startDate}-${endDate}）.xlsx`);

  exporting.value = false; // 导出完成，关闭加载状态
}

loadLedgerList()
loadTransactionCategoryList()
loadList()

onMounted(async () => {
  await nextTick(() => {
    updateTableMaxHeight()
  })
  window.addEventListener('resize', () => {
    updateTableMaxHeight()
  })
})
</script>

<style lang='scss' scoped>
.app-container {
  height: 100%;

  .layout {
    height: 100%;
    display: flex;
  }

}

.layout-left {
  width: 300px;
  height: 100%;

  .el-card {
    height: 100%;
  }
}

.layout-center {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  padding: 0 12px;

}

.layout-right {
  width: 70px;

  :deep(.el-card__body) {
    padding: 0 !important;
  }

  .menu {
    margin: 0;
    padding: 0;
    list-style: none;
    font-size: 12px;

    &__item {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      height: 70px;
      border-bottom: 1px solid var(--el-border-color-light);
      color: var(--el-text-color-regular);

      &:hover {
        background-color: var(--el-color-primary);
        color: #ffffff;
      }
    }

    &__name {
      padding-top: 6px;
    }

    &__item--active {
      background-color: var(--el-color-primary);
      color: #ffffff;
    }
  }
}

.search {
  margin-bottom: 12px;

  :deep(.el-card__body) {
    padding-bottom: 0 !important;
  }

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  &__footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-top: 1px solid var(--el-border-color-light);
    margin-top: 10px;
    padding-top: 8px;
  }

  .el-link {
    margin-left: 24px;
  }

  .el-button {
    margin-left: 24px;
  }
}

.table {
  flex: 1;
  overflow: auto;
  display: flex;
  flex-direction: column;

  :deep(.el-card__body) {
    flex: 1;
    display: flex;
    flex-direction: column;
    //overflow: hidden;
    //flex-wrap: nowrap;
  }

  .el-row {
    //width: 100%;
  }
}

.table-container {
  flex: 1;
  overflow: hidden;
  //height: 100%;
  display: flex;
  flex-direction: column;

  &__body {
    flex: 1;
    overflow: hidden;
    margin-top: 8px;
  }

  &__footer {
    height: 44px;

    .pagination-container {
      display: flex;
      justify-content: flex-end;
      margin-top: 12px;
      margin-bottom: 0;
      padding: 0 !important;
      height: auto;

      :deep(.el-pagination) {
        position: relative;
      }
    }
  }
}

</style>
