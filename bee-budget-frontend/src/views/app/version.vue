<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="88px" @submit.prevent="onSearch">
            <el-form-item label="App应用" prop="appId">
              <el-select v-model="searchParams.appId" style="width: 160px">
                <el-option
                    v-for="item in appList"
                    :key="item.appId"
                    :label="item.name"
                    :value="item.appId"
                />
              </el-select>
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
                <el-table-column label="id" align="right" width="100" fixed="left" key="id" prop="id"
                                 sortable="custom" :sort-orders="['descending', 'ascending']"/>
                <el-table-column label="AppID" align="center" width="100" fixed="left" key="appId" prop="appId"/>
                <el-table-column label="应用名称" align="center" width="120" fixed="left" key="appName" prop="appName"/>
                <el-table-column label="版本标题" align="center" key="title" prop="title"/>
                <el-table-column label="更新内容" align="center" key="contents" prop="contents"/>
                <el-table-column label="平台" align="center" width="80" key="platform" prop="platform"/>
                <el-table-column label="版本名称" align="right" width="80" key="versionName" prop="versionName"/>
                <el-table-column label="版本号" align="right" width="100" key="versionCode" prop="versionCode"
                                 sortable="custom" :sort-orders="['descending', 'ascending']"/>
                <el-table-column label="下载地址" align="center" width="100" key="url" prop="url">
                  <template #default="scope">
                    <el-link type="primary" :href="`${BASE_URL}${scope.row.url}`"
                             target="_blank">
                      <span>打开链接</span>
                    </el-link>
                  </template>
                </el-table-column>
                <el-table-column label="发布状态" align="center" prop="IsStablePublish" width="100">
                  <template #default="scope">
                    <el-tag type="success" v-if="scope.row.isStablePublish">已发布</el-tag>
                    <span v-else>未发布</span>
                  </template>
                </el-table-column>
                <el-table-column label="静默更新" align="center" prop="IsSilently" width="100">
                  <template #default="scope">
                    <el-tag type="danger" v-if="scope.row.IsSilently">静默更新</el-tag>
                    <span v-else>否</span>
                  </template>
                </el-table-column>
                <el-table-column label="强制更新" align="center" prop="IsMandatory" width="100">
                  <template #default="scope">
                    <el-tag type="danger" v-if="scope.row.isMandatory">强制更新</el-tag>
                    <span v-else>否</span>
                  </template>
                </el-table-column>
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
                               v-if="!scope.row.isStablePublish"
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

    <el-dialog :title="dialogTitle" v-model="isDialogOpen" width="800px" append-to-body @close="closeDialog"
               @open="openDialog">
      <el-form :model="formData" :rules="formRules" ref="formRef" label-width="110">
        <el-form-item label="App应用">
          {{ appList.find(item => item.appId === searchParams.appId)?.name }}
        </el-form-item>
        <el-form-item label="更新标题" prop="title">
          <el-input v-model="formData.title" placeholder="请输入更新标题"/>
        </el-form-item>
        <el-form-item label="更新内容" prop="contents">
          <el-input v-model="formData.contents" placeholder="请输入更新内容"/>
        </el-form-item>
        <el-form-item label="平台" prop="platform">
          <el-radio-group v-model="formData.platform" disabled>
            <el-radio label="Android">Android</el-radio>
            <el-radio label="iOS">iOS</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="版本名称" prop="versionName">
          <span>最新版本名称：{{ appMaxVersion?.versionName }}</span>&nbsp;&nbsp;&nbsp;
          <el-input v-model="formData.versionName" style="width: 100px" placeholder="请输入版本名称"/>
        </el-form-item>
        <el-form-item label="版本号" prop="versionCode">
          <span>最新版本号：{{ appMaxVersion?.versionCode }}</span>&nbsp;&nbsp;&nbsp;
          <el-input-number v-model="formData.versionCode" :min="100" :step="1" placeholder="请输入版本号"/>
        </el-form-item>
        <el-form-item label="下载地址" prop="url">
          <el-input v-model="formData.url" placeholder="手动输入 http/https 地址，或使用上传地址">
            <template #prepend>
              <el-upload
                  class="list-uploader"
                  ref="uploadFilesRef"
                  accept=".apk,.wgt"
                  :limit="1"
                  :headers="uploadFiles.headers"
                  :action="uploadFiles.url"
                  :disabled="uploadFiles.isUploading"
                  :on-progress="onFilesUploadProgress"
                  :on-exceed="onFilesExceed"
                  :on-success="onFilesSuccess"
                  :file-list="formData.tempFiles"
                  :show-file-list="false"
              >
                <el-button icon="upload">上传文件</el-button>
              </el-upload>
            </template>
            <template #append>
              <el-link typ="info" icon="download" :href="`${BASE_URL}${formData.url}`" target="_blank">下载文件
              </el-link>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item label="已发布" prop="isStablePublish">
          <el-switch size="large" v-model="formData.isStablePublish" class="drawer-switch" :active-value="true"
                     :inactive-value="false"/>
        </el-form-item>
        <el-form-item label="静默更新" prop="isSilently">
          <el-switch size="large" v-model="formData.isSilently" class="drawer-switch" :active-value="true"
                     :inactive-value="false"/>
        </el-form-item>
        <el-form-item label="强制更新" prop="isMandatory">
          <el-switch size="large" v-model="formData.isMandatory" class="drawer-switch" :active-value="true"
                     :inactive-value="false"/>
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

<script setup name="AppVersion">
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'

const route = useRoute()
import _ from "lodash";
import {ElMessage, ElMessageBox, genFileId} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import dayjs from 'dayjs'

import {getAppAll, getAppPaged} from "@/api/app"
import {
  getAppVersionPaged,
  getAppVersion,
  createAppVersion,
  updateAppVersion,
  deleteAppVersion,
  batchDeleteAppVersion, getAppMaxVersion,
} from "@/api/app/version.js"

// 表格相关
const tableRef = ref(null)
const tableMaxHeight = ref(0)
const tableDefaultSort = ref({prop: "createdAt", order: "descending"})
const tableLoading = ref(true)

// 表格数据相关
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
  appId: null,
})

// 表单相关
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  title: [{required: true, message: "更新标题不能为空", trigger: "blur"}],
  contents: [{required: true, message: "更新内容不能为空", trigger: "blur"}],
  versionName: [{required: true, message: "版本名称不能为空", trigger: "blur"}],
  versionCode: [{required: true, message: "版本号不能为空", trigger: "blur"}],
  url: [{required: true, message: "下载地址不能为空", trigger: "blur"}],
})
const formSubmitting = ref(false)
const isSubmitDisabled = computed(() => {
  // if (uploadCover.isUploading || formSubmitting.value) {
  //   return true
  // }
  // return false
  return formSubmitting.value
})

// 字典相关
const {sys_normal_disable} = useDict('sys_normal_disable')

const appMaxVersion = ref({})
const defaultApp = ref("")
const appList = ref([])

// 上传相关
import {BASE_URL, BASE_API_URL} from '@/utils/constants.js'

const uploadPrefix = ref(BASE_API_URL)
const uploadFilesRef = ref(null)
const uploadFiles = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-file"
})

watch(() => formData.value.tempFiles, () => {
  if (formData.value.tempFiles.length > 0) {
    formData.value.url = formData.value.tempFiles[0].url
  }
}, {deep: true})

/* 加载列表 */
async function loadList() {
  tableLoading.value = true
  try {
    const response = await getAppVersionPaged(searchParams.value)
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
  searchParams.value.appId = defaultApp.value
  onSearch()
}

/* 删除 */
async function onDelete(row) {
  const delIds = row.id ? [row.id] : selectedIds.value
  try {
    await ElMessageBox.confirm('是否确认删除编号为 ' + delIds.join(', ') + ' 的数据项？')

    let response
    if (delIds.length > 1) {
      response = await batchDeleteAppVersion(delIds)
    } else {
      response = await deleteAppVersion(delIds[0])
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
    appId: null,
    title: null,
    contents: null,
    platform: 'Android',
    versionName: '1.0.0',
    versionCode: 100,
    url: null,
    tempFiles: [],
    isStablePublish: false,
    isSilently: false,
    isMandatory: false,
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
  formData.value.appId = searchParams.value.appId
  isDialogOpen.value = true
  dialogTitle.value = '添加'
}

/* 修改 */
async function onUpdate(row) {
  if (row.isStablePublish) {
    ElMessage.error('该版本已发布，不允许修改')
    return
  }

  reset()
  const dictItemId = row.id
  try {
    const response = await getAppVersion(dictItemId)
    const {success, data, message} = response
    if (success) {
      formData.value = data
      formData.value.tempFiles = data.url?.length ? data.url.split(',').map(o => {
        return {name: o, url: o}
      }) : []
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
        const formDataClone = _.cloneDeep(formData.value)
        formDataClone.appId = searchParams.value.appId
        formDataClone.url = formData.value.tempFiles?.length ? formData.value.tempFiles.map(o => o.url).join(',') : null
        const response = await (formDataClone.id !== null
            ? updateAppVersion(formDataClone)
            : createAppVersion(formDataClone))
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

/*打开模态框*/
const openDialog = () => {
  loadAppMaxVersion()
}

/*关闭模态框*/
const closeDialog = () => {
  uploadFilesRef.value.clearFiles()
}

/*覆盖前一个*/
const onFilesExceed = (files) => {
  uploadFilesRef.value.clearFiles()
  const file = files[0]
  file.uid = genFileId()
  uploadFilesRef.value.handleStart(file)
  uploadFilesRef.value.submit()
}

/*文件上传中处理 */
const onFilesUploadProgress = (event, file, fileList) => {
  uploadFiles.isUploading = true
}

/* 文件上传成功处理 */
const onFilesSuccess = (res, file, fileList) => {
  console.log(file, fileList)
  uploadFiles.isUploading = false
  const {code, success, data, message} = res
  if (code === 200 && success) {
    // formData.value.file_path = data.file_path
    file.url = data
    // 筛选匹配的文件
    formData.value.tempFiles = formData.value.tempFiles.filter(o => {
      if (fileList.some(t => t.uid === o.uid)) {
        return true
      }
    })
    formData.value.tempFiles.push({uid: file.uid, name: file.name, url: data})
    // ElMessageBox.alert("上传文件成功！")
  } else {
    ElMessageBox.alert(message)
  }
}

/* 加载App列表 */
async function loadAppList() {
  try {
    const response = await getAppAll()
    const {success, data, message} = response
    if (success) {
      appList.value = data
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

async function loadAppMaxVersion() {
  try {
    const response = await getAppMaxVersion(searchParams.value.appId)
    const {success, data, message} = response
    if (success) {
      appMaxVersion.value = data
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

/* 设置表格最大高度 */
function updateTableMaxHeight() {
  tableMaxHeight.value = tableRef.value.clientHeight
}

/* 初始化 */
async function init() {
  await loadAppList()
  if (appList.value.length > 0) {
    if (route.params && route.params.appId) {
      const findObj = appList.value.find(item => item.appId === route.params.appId)
      searchParams.value.appId = findObj.appId
    } else {
      searchParams.value.appId = appList.value[0].appId
    }
    defaultApp.value = searchParams.value.appId
    loadList()
  }
  await loadAppMaxVersion()
}

onMounted(async () => {
  await init()
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

