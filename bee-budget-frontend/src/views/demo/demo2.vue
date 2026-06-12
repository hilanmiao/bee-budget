<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="100px" @submit.prevent="onSearch">
            <el-form-item label="样例1" prop="demo1Id">
              <el-select
                  v-model="searchParams.demo1Id"
                  placeholder="请选择样例1"
                  style="width: 160px"
              >
                <el-option
                    v-for="item in demo1List"
                    :key="item.id"
                    :label="item.name"
                    :value="item.id"
                    :disabled="item.status === 1"
                />
              </el-select>
            </el-form-item>
            <el-form-item label="名称" prop="name">
              <el-input
                  v-model="searchParams.name"
                  placeholder="请输入名称"
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
                    v-for="dict in sys_normal_disable"
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
                <el-table-column label="样例1名称" key="demo1Name" prop="demo1Name"/>
                <el-table-column label="名称" key="name" prop="name"/>
                <el-table-column label="别名" key="aliasName" prop="aliasName"/>
                <el-table-column label="编码" key="code" prop="code"/>
                <el-table-column label="是否可见" align="center" key="isVisible" prop="isVisible">
                  <template #default="scope">
                    <el-tag type="success" v-if="scope.row.isVisible">是</el-tag>
                    <el-tag type="info" v-else>是</el-tag>
                  </template>
                </el-table-column>
                <el-table-column label="显示顺序" key="sort" prop="sort" sortable="custom"
                                 :sort-orders="['descending', 'ascending']"/>
                <el-table-column label="状态" align="center" key="status" width="160">
                  <template #default="scope">
                    <el-switch
                        v-model="scope.row.status"
                        active-value="0"
                        inactive-value="1"
                        active-text="正常"
                        inactive-text="停用"
                        @change="onChangeStatus(scope.row)"
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
        <el-form-item label="样例1" prop="demo1Id">
          <el-select
              v-model="formData.demo1Id"
              placeholder="请选择样例1"
          >
            <el-option
                v-for="item in demo1List"
                :key="item.id"
                :label="item.name"
                :value="item.id"
                :disabled="item.status === 1"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="formData.name" placeholder="请输入名称"/>
        </el-form-item>
        <el-form-item label="别名" prop="aliasName">
          <el-input v-model="formData.aliasName" placeholder="请输入别名"/>
        </el-form-item>
        <el-form-item label="编码" prop="code">
          <el-input-number v-model="formData.code" controls-position="right" :min="0"/>
        </el-form-item>
        <el-form-item label="是否可见" prop="isVisible">
          <el-switch size="large" v-model="formData.isVisible" class="drawer-switch" :active-value="true"
                     :inactive-value="false" active-text="是" inactive-text="否"/>
        </el-form-item>
        <el-form-item label="显示顺序" prop="sort">
          <el-input-number v-model="formData.sort" controls-position="right" :min="0"/>
        </el-form-item>
        <el-form-item label="状态" prop="status">
          <el-radio-group v-model="formData.status">
            <el-radio
                v-for="dict in sys_normal_disable"
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

<script setup name="Demo2">
// --- 框架工具等相关 ---
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import {ElMessage, ElMessageBox} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc.js'

dayjs.extend(utc)

// --- api 相关 ---
import {
  getDemo1All
} from "@/api/demo/demo1.js"
import {
  getDemo2Paged,
  getDemo2,
  createDemo2,
  updateDemo2,
  deleteDemo2,
  batchDeleteDemo2,
  changeDemo2Status,
} from "@/api/demo/demo2.js"

// --- 表格相关 ---
const tableRef = ref(null)
const tableMaxHeight = ref(0)
// const tableDefaultSort = ref({prop: "createdAt", order: "descending"})
const tableDefaultSort = ref({prop: "sort", order: "ascending"})
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
  demo1Id: null,
  name: null,
  status: null,
  dateRange: null,
  startDate: null,
  endDate: null,
})
const shortcuts = [
  {
    text: '过去7日',
    value: () => {
      const end = dayjs();
      const start = end.subtract(7, 'day');
      return [start.toDate(), end.toDate()];
    },
  },
  {
    text: '过去30日',
    value: () => {
      const end = dayjs();
      const start = end.subtract(30, 'day');
      return [start.toDate(), end.toDate()];
    },
  },
  {
    text: '过去90日',
    value: () => {
      const end = dayjs();
      const start = end.subtract(90, 'day');
      return [start.toDate(), end.toDate()];
    },
  },
]

// --- 表单相关 ---
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  demo1Id: [{required: true, message: "样例1不能为空", trigger: "change"}],
  name: [{required: true, message: "名称不能为空", trigger: "blur"}],
  sort: [{required: true, message: "显示顺序不能为空", trigger: "blur"}],
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
const {sys_normal_disable} = useDict('sys_normal_disable')

// --- 样例1相关 ---
const demo1List = ref([])

/* 加载样例1列表 */
async function loadDemo1List() {
  try {
    const response = await getDemo1All()
    const {success, data, message} = response
    if (success) {
      demo1List.value = data
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
    if (searchParams.value.dateRange?.length > 0) {
      searchParams.value.startDate = dayjs(searchParams.value.dateRange[0]).utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]')
      searchParams.value.endDate = dayjs(searchParams.value.dateRange[1]).utc().format('YYYY-MM-DDTHH:mm:ss.SSS[Z]')
    } else {
      searchParams.value.startDate = null
      searchParams.value.endDate = null
    }
    const response = await getDemo2Paged(searchParams.value)
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
      response = await batchDeleteDemo2(delIds)
    } else {
      response = await deleteDemo2(delIds[0])
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
async function onChangeStatus(row) {
  const originalStatus = row.status === '0' ? '1' : '0'
  const text = row.status === '0' ? '启用' : '停用'

  try {
    await ElMessageBox.confirm(`确认要 ${text} ${row.name} 吗?`)
    const response = await changeDemo2Status(row.id, row.status)

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
    demo1Id: null,
    name: null,
    aliasName: null,
    code: null,
    isVisible: true,
    sort: 1,
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
    const response = await getDemo2(row.id)
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
            ? updateDemo2(formData.value)
            : createDemo2(formData.value))
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

loadDemo1List()
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
