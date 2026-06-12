<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="100px" @submit.prevent="onSearch">
            <el-form-item label="菜单名称" prop="name">
              <el-input
                  v-model="searchParams.name"
                  placeholder="请输入菜单名称"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item label="菜单状态" prop="status">
              <el-select
                  v-model="searchParams.status"
                  placeholder="菜单状态"
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
                  <el-button
                      type="info"
                      plain
                      icon="Sort"
                      @click="onToggleExpandAll"
                  >展开/折叠
                  </el-button>
                </el-col>
              </el-row>
            </div>
            <div class="table-container__body" ref="tableRef">
              <el-table
                  v-if="refreshTable"
                  v-loading="tableLoading"
                  :data="list"
                  row-key="id"
                  :default-expand-all="isExpandAll"
                  :tree-props="{ children: 'children', hasChildren: 'hasChildren' }"
                  :max-height="tableMaxHeight"
                  border
                  fit
                  style="width: 100%;"
              >
                <el-table-column prop="name" label="菜单名称"></el-table-column>
                <el-table-column prop="icon" label="图标" align="center" width="100">
                  <template #default="scope">
                    <svg-icon :icon-class="scope.row.icon"/>
                  </template>
                </el-table-column>
                <el-table-column prop="sort" label="排序" width="60"></el-table-column>
                <el-table-column prop="perms" label="权限标识"></el-table-column>
                <el-table-column prop="path" label="路由地址"></el-table-column>
                <el-table-column prop="component" label="组件路径"></el-table-column>
                <el-table-column prop="status" label="状态" width="80">
                  <template #default="scope">
                    <dict-tag :options="sys_normal_disable" :value="scope.row.status"/>
                  </template>
                </el-table-column>
                <el-table-column label="操作" align="center" width="240">
                  <template #default="scope">
                    <el-button link type="primary" icon="Edit" @click="onUpdate(scope.row)"
                               v-hasPermi="['system:menu:edit']">修改
                    </el-button>
                    <el-button link type="primary" icon="Plus" @click="onAdd(scope.row)"
                               v-hasPermi="['system:menu:add']">新增
                    </el-button>
                    <el-button link type="primary" icon="Delete" @click="onDelete(scope.row)"
                               v-hasPermi="['system:menu:remove']">删除
                    </el-button>
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <el-dialog :title="dialogTitle" v-model="isDialogOpen" width="800px" append-to-body>
      <el-form :model="formData" :rules="formRules" ref="formRef" label-width="110">
        <el-row>
          <el-col :span="24">
            <el-form-item label="上级菜单">
              <el-tree-select
                  v-model="formData.parentId"
                  :data="menuAll"
                  :props="{ value: 'id', label: 'name', children: 'children' }"
                  value-key="id"
                  placeholder="选择上级菜单"
                  check-strictly
              />
            </el-form-item>
          </el-col>
          <el-col :span="24">
            <el-form-item label="菜单类型" prop="menuType">
              <el-radio-group v-model="formData.menuType">
                <el-radio value="M">目录</el-radio>
                <el-radio value="C">菜单</el-radio>
                <el-radio value="F">按钮</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType !== 'F'">
            <el-form-item label="菜单图标" prop="icon">
              <el-popover
                  placement="bottom-start"
                  :width="540"
                  trigger="click"
              >
                <template #reference>
                  <el-input v-model="formData.icon" placeholder="点击选择图标" @blur="onBlurSelectIcon" readonly>
                    <template #prefix>
                      <svg-icon
                          v-if="formData.icon"
                          :icon-class="formData.icon"
                          class="el-input__icon"
                          style="height: 32px;width: 16px;"
                      />
                      <el-icon v-else style="height: 32px;width: 16px;">
                        <search/>
                      </el-icon>
                    </template>
                  </el-input>
                </template>
                <icon-select ref="iconSelectRef" @selected="onSelectedIcon" :active-icon="formData.icon"/>
              </el-popover>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="显示排序" prop="sort">
              <el-input-number v-model="formData.sort" controls-position="right" :min="0"/>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="菜单名称" prop="name">
              <el-input v-model="formData.name" placeholder="请输入菜单名称"/>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType === 'C'">
            <el-form-item prop="routeName">
              <template #label>
                        <span>
                           <el-tooltip
                               content="默认不填则和路由地址相同：如地址为：`user`，则名称为`User`（注意：因为router会删除名称相同路由，为避免名字的冲突，特殊情况下请自定义，保证唯一性）"
                               placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           路由名称
                        </span>
              </template>
              <el-input v-model="formData.routeName" placeholder="请输入路由名称"/>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType !== 'F'">
            <el-form-item>
              <template #label>
                        <span>
                           <el-tooltip content="选择是外链则路由地址需要以`http(s)://`开头" placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>是否外链
                        </span>
              </template>
              <el-radio-group v-model="formData.isFrame">
                <el-radio value="0">否</el-radio>
                <el-radio value="1">是</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType !== 'F'">
            <el-form-item prop="path">
              <template #label>
                        <span>
                           <el-tooltip content="访问的路由地址，如：`user`，如外网地址需内链访问则以`http(s)://`开头"
                                       placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           路由地址
                        </span>
              </template>
              <el-input v-model="formData.path" placeholder="请输入路由地址"/>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType === 'C'">
            <el-form-item prop="component">
              <template #label>
                        <span>
                           <el-tooltip content="访问的组件路径，如：`system/user/index`，默认在`views`目录下"
                                       placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           组件路径
                        </span>
              </template>
              <el-input v-model="formData.component" placeholder="请输入组件路径"/>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType !== 'M'">
            <el-form-item>
              <el-input v-model="formData.perms" placeholder="请输入权限标识" maxlength="100"/>
              <template #label>
                        <span>
                           <el-tooltip
                               content="控制器中定义的权限字符，如：@PreAuthorize(`@ss.hasPermi('system:user:list')`)"
                               placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           权限字符
                        </span>
              </template>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType === 'C'">
            <el-form-item>
              <el-input v-model="formData.query" placeholder="请输入路由参数" maxlength="255"/>
              <template #label>
                        <span>
                           <el-tooltip content='访问路由的默认传递参数，如：`{"id": 1, "name": "ry"}`' placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           路由参数
                        </span>
              </template>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType === 'C'">
            <el-form-item>
              <template #label>
                        <span>
                           <el-tooltip content="选择是则会被`keep-alive`缓存，需要匹配组件的`name`和地址保持一致"
                                       placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           是否缓存
                        </span>
              </template>
              <el-radio-group v-model="formData.isCache">
                <el-radio value="0">缓存</el-radio>
                <el-radio value="1">不缓存</el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="formData.menuType !== 'F'">
            <el-form-item>
              <template #label>
                        <span>
                           <el-tooltip content="选择隐藏则路由将不会出现在侧边栏，但仍然可以访问" placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           显示状态
                        </span>
              </template>
              <el-radio-group v-model="formData.visible">
                <el-radio
                    v-for="dict in sys_show_hide"
                    :key="dict.value"
                    :value="dict.value"
                >{{ dict.label }}
                </el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item>
              <template #label>
                        <span>
                           <el-tooltip content="选择停用则路由将不会出现在侧边栏，也不能被访问" placement="top">
                              <el-icon><question-filled/></el-icon>
                           </el-tooltip>
                           菜单状态
                        </span>
              </template>
              <el-radio-group v-model="formData.status">
                <el-radio
                    v-for="dict in sys_normal_disable"
                    :key="dict.value"
                    :value="dict.value"
                >{{ dict.label }}
                </el-radio>
              </el-radio-group>
            </el-form-item>
          </el-col>
        </el-row>
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

<script setup name="Menu">
// --- 框架工具等相关 ---
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import {ElMessage, ElMessageBox} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import SvgIcon from "@/components/SvgIcon/index.vue";
import IconSelect from "@/components/IconSelect/index.vue";
import {buildTree} from '@/utils'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc.js'

dayjs.extend(utc)

// --- api 相关 ----
import {
  getMenuAll,
  getMenu,
  createMenu,
  updateMenu,
  deleteMenu,
} from "@/api/system/menu.js"

// 表格相关
const tableRef = ref(null)
const tableMaxHeight = ref(0)
const tableLoading = ref(true)
const refreshTable = ref(true)

// 表格数据相关
const list = ref([])
const searchRef = ref(null)
const searchParams = ref({
  pageNumber: 1,
  pageSize: 20,
  name: null,
  status: null,
})

// 表单相关
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  name: [{required: true, message: "菜单名称不能为空", trigger: "blur"}],
  sort: [{required: true, message: "菜单顺序不能为空", trigger: "blur"}],
  path: [{required: true, message: "路由地址不能为空", trigger: "blur"}]
})
const formSubmitting = ref(false)
const isSubmitDisabled = computed(() => {
  // if (uploadCover.isUploading || formSubmitting.value) {
  //   return true
  // }
  // return false
  return formSubmitting.value
})
const menuAll = ref([])
const iconSelectRef = ref(null)
const isExpandAll = ref(false)

// 字典相关
const {sys_normal_disable, sys_show_hide} = useDict('sys_normal_disable', 'sys_show_hide')

/* 加载列表 */
async function loadList() {
  tableLoading.value = true
  try {
    const response = await getMenuAll(searchParams.value)
    const {success, data, message} = response
    if (success) {
      list.value = buildTree(data);
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
  const delIds = [row.id]
  try {
    await ElMessageBox.confirm('是否确认删除名称为 ' + row.name + ' 的数据项？')

    let response = await deleteMenu(delIds[0])

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

/* 重置表单  */
function reset() {
  formData.value = {
    id: null,
    parentId: 0,
    name: null,
    icon: null,
    menuType: "M",
    routeName: null,
    perms: null,
    sort: null,
    isFrame: "0",
    isCache: "0",
    visible: "0",
    status: "0"
  };
  formRef.value?.resetFields()
  menuAll.value = []
}

/* 取消表单 */
function onCancel() {
  isDialogOpen.value = false
  reset()
}

/* 新增 */
function onAdd(row) {
  reset()
  loadTreeSelect()
  if (row != null && row.id) {
    formData.value.parentId = row.id
  } else {
    formData.value.parentId = 0
  }
  isDialogOpen.value = true
  dialogTitle.value = '添加'
}

/* 修改 */
async function onUpdate(row) {
  reset()
  await loadTreeSelect()
  const MenuId = row.id
  try {
    const response = await getMenu(MenuId)
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
            ? updateMenu(formData.value)
            : createMenu(formData.value))
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

/* 展开/折叠操作 */
function onToggleExpandAll() {
  refreshTable.value = false;
  isExpandAll.value = !isExpandAll.value;
  nextTick(() => {
    refreshTable.value = true;
  });
}

/* 展示下拉图标 */
function onBlurSelectIcon() {
  iconSelectRef.value.reset();
}

/* 选择图标 */
function onSelectedIcon(name) {
  formData.value.icon = name;
}

/* 加载菜单下拉树结构 */
async function loadTreeSelect() {
  try {
    const response = await getMenuAll()
    const {data} = response
    const menu = {id: 0, name: "主类目", children: []}
    menu.children = buildTree(data)
    menuAll.value.push(menu)
  } catch (error) {
    // 可选：处理错误，比如提示用户
    console.error('加载列表失败:', error)
  } finally {
    // 无论成功或失败都会执行，如 loading 必须结束
    tableLoading.value = false
  }
}

/* 设置表格最大高度 */
function updateTableMaxHeight() {
  tableMaxHeight.value = tableRef.value.clientHeight
}

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

