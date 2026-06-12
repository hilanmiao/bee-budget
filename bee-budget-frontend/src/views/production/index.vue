<template>
  <div class="app-container">
    <div class="layout">

      <div class="layout-center">
        <el-card shadow="always" class="search">
          <el-form :model="searchParams" ref="searchRef" :inline="true" label-width="100px" @submit.prevent="onSearch">
            <el-form-item label="产品名称" prop="name">
              <el-input
                  v-model="searchParams.name"
                  placeholder="请输入产品名称"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item label="产品型号" prop="model">
              <el-input
                  v-model="searchParams.model"
                  placeholder="请输入产品型号"
                  clearable
                  style="width: 160px"
              />
            </el-form-item>
            <el-form-item label="产品状态" prop="status">
              <el-select
                  v-model="searchParams.status"
                  placeholder="产品状态"
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
                <el-table-column label="产品名称" key="name" prop="name"/>
                <el-table-column label="产品系列" align="center" key="series" prop="series">
                  <template #default="scope">
                    <span v-if="scope.row.series === '1'">流量仪表</span>
                    <span v-if="scope.row.series === '2'">显示仪表</span>
                    <span v-if="scope.row.series === '3'">压力液位</span>
                    <span v-if="scope.row.series === '4'">自控系统</span>
                  </template>
                </el-table-column>
                <el-table-column label="产品型号" align="center" key="model" prop="model"/>
                <el-table-column label="产品封面" align="center" width="160" key="cover" prop="cover">
                  <template #default="scope">
                    <el-image
                        style="width: 100px; height: 100px"
                        :src="BASE_URL + scope.row.cover"
                        :zoom-rate="1.2"
                        :max-scale="7"
                        :min-scale="0.2"
                        :initial-index="0"
                        :preview-src-list="[BASE_URL + scope.row.cover]"
                        :preview-teleported="true"
                        fit="contain"
                    />
                  </template>
                </el-table-column>
                <el-table-column label="状态" align="center" key="status">
                  <template #default="scope">
                    <el-switch
                        v-model="scope.row.status"
                        active-value="0"
                        inactive-value="1"
                        active-text="正常"
                        inactive-text="停用"
                        @change="onStatusChange(scope.row)"
                    ></el-switch>
                  </template>
                </el-table-column>
                <el-table-column label="备注" align="center" prop="remark"/>
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

    <el-dialog :title="dialogTitle" v-model="isDialogOpen" width="1200px" append-to-body>
      <div style="margin-bottom: 32px; display: flex; align-items: center; justify-content: space-between;">
        <el-steps style="margin-right: 16px;flex: 1;" :active="stepActive" finish-status="success">
          <el-step title="填写基础信息"/>
          <el-step title="填写产品说明"/>
          <el-step title="填写性能参数"/>
          <el-step title="填写外形尺寸"/>
          <el-step title="填写安装事项"/>
          <el-step title="配置产品特点"/>
        </el-steps>
        <div>
          <el-button type="primary" round @click="prevStep">上一步</el-button>
          <el-button type="primary" round @click="nextStep">下一步</el-button>
        </div>
      </div>

      <el-form :model="formData" :rules="formRules" ref="formRef" label-width="110">
        <el-row :gutter="24" v-show="stepActive === 0">
          <el-col :span="12">
            <el-form-item label="产品名称" prop="name">
              <el-input v-model="formData.name" placeholder="请输入产品名称"/>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品系列" prop="series">
              <el-select
                  v-model="formData.series"
                  placeholder="请选择产品系列"
              >
                <el-option label="流量仪表" value="1"/>
                <el-option label="显示仪表" value="2"/>
                <el-option label="压力液位" value="3"/>
                <el-option label="自控系统" value="4"/>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品型号" prop="model">
              <el-input v-model="formData.model" placeholder="请输入产品型号"/>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品特点" prop="characteristic">
              <el-input v-model="formData.characteristic" placeholder="每个特点不超过8个字，用中文逗号隔开"/>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品封面" prop="cover">
              <el-upload
                  class="avatar-uploader"
                  ref="uploadCoverRef"
                  :limit="1"
                  accept=".jpg,.jpeg,.png"
                  :headers="uploadCover.headers"
                  :action="uploadCover.url"
                  :disabled="uploadCover.isUploading"
                  :on-progress="onCoverUploadProgress"
                  :on-exceed="onCoverUploadExceed"
                  :on-success="onCoverUploadSuccess"
                  :on-error="onCoverUploadError"
                  :show-file-list="false"
              >
                <img v-if="formData.cover" :src="BASE_URL + formData.cover" class="avatar"/>
                <el-icon v-else class="avatar-uploader-icon">
                  <Plus/>
                </el-icon>
              </el-upload>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品价格" prop="price">
              <el-input-number v-model='formData.price' :min="0"/>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="3D模型" prop="modelThreeDimensional">
              <el-upload
                  class="list-uploader"
                  ref="uploadModelThreeDimensionalRef"
                  accept=".zip,.rar"
                  :limit="1"
                  :headers="uploadModelThreeDimensional.headers"
                  :action="uploadModelThreeDimensional.url"
                  :disabled="uploadModelThreeDimensional.isUploading"
                  :on-progress="onModelThreeDimensionalUploadProgress"
                  :on-exceed="onModelThreeDimensionalUploadExceed"
                  :on-success="onModelThreeDimensionalUploadSuccess"
                  :on-error="onModelThreeDimensionalUploadError"
                  :on-remove="onModelThreeDimensionalUploadRemove"
                  :file-list="formData.modelThreeDimensional"
              >
                <el-button type="primary">上传模型</el-button>
                <template #tip>
                  <div class="el-upload__tip">
                    仅支持上传1个并且不能超过500MB的压缩文件（.zip、.rar后缀）
                  </div>
                </template>
              </el-upload>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品相册" prop="album">
              <el-upload
                  ref="uploadAlbumRef"
                  multiple
                  :limit="9"
                  accept=".jpg,.jpeg,.png"
                  :headers="uploadAlbum.headers"
                  :action="uploadAlbum.url"
                  :disabled="uploadAlbum.isUploading"
                  :on-progress="onAlbumUploadProgress"
                  :on-success="onAlbumUploadSuccess"
                  :on-error="onAlbumUploadError"
                  list-type="picture-card"
                  :file-list="formData.album"
              >
                <template #tip>
                  <div class="el-upload__tip">
                    最多9张图片
                  </div>
                </template>
                <el-icon>
                  <Plus/>
                </el-icon>
                <template #file="{ file }">
                  <div>
                    <img class="el-upload-list__item-thumbnail" :src="BASE_URL + file.url" alt=""/>
                    <div class="el-upload-list__item-actions">
                      <span class="el-upload-list__item-preview" @click="onAlbumUploadPreview(file)">
                        <el-icon><zoom-in/></el-icon>
                      </span>
                      <span class="el-upload-list__item-delete" @click="onAlbumUploadRemove(file)">
                        <el-icon><Delete/></el-icon>
                      </span>
                    </div>
                  </div>
                </template>
              </el-upload>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品资料" prop="files">
              <el-upload
                  class="list-uploader"
                  ref="uploadFilesRef"
                  accept=""
                  multiple
                  :limit="9"
                  :headers="uploadFiles.headers"
                  :action="uploadFiles.url"
                  :disabled="uploadFiles.isUploading"
                  :on-progress="onFilesUploadProgress"
                  :on-remove="onFilesUploadRemove"
                  :on-success="onFilesUploadSuccess"
                  :on-error="onFilesUploadError"
                  :file-list="formData.files"
              >
                <el-button type="primary">上传资料</el-button>
                <template #tip>
                  <div class="el-upload__tip">
                    最多9个文件，单个文件不能超过500MB
                  </div>
                </template>
              </el-upload>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="产品状态" prop="status">
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
          <el-col :span="12">
            <el-form-item label="备注" prop="remark">
              <el-input v-model="formData.remark" type="textarea" placeholder="请输入内容"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="24" v-show="stepActive === 1">
          <el-col :span="24">
            <el-form-item label="产品说明（手机宽度）" prop="description" label-width="200">
              <!--                  <el-tiptap placeholder="从这里开始" v-model:content="content" :extensions="extensions" :locale="zh" :width="360" :height="780"/>-->
              <!--              <el-input v-model="formData.description" placeholder="请输入产品型号"/>-->

              <div style="border: 1px solid #ccc; width: 360px;" v-if="isDialogOpen">
                <Toolbar
                    style="border-bottom: 1px solid #ccc"
                    :editor="editorRef"
                    :defaultConfig="toolbarConfig"
                    :mode="mode"
                />
                <Editor
                    style="height: 480px; overflow-y: hidden;"
                    v-model="formData.description"
                    :defaultConfig="editorConfig"
                    :mode="mode"
                    @onCreated="handleCreated"
                />
              </div>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="24" v-show="stepActive === 2">
          <el-col :span="24">
            <el-form-item label="性能参数（手机宽度）" prop="params" label-width="200">
              <!--                  <el-tiptap placeholder="从这里开始" v-model:content="content" :extensions="extensions" :locale="zh" :width="360" :height="780"/>-->
              <!--              <el-input v-model="formData.description" placeholder="请输入产品型号"/>-->

              <div style="border: 1px solid #ccc; width: 360px;" v-if="isDialogOpen">
                <Toolbar
                    style="border-bottom: 1px solid #ccc"
                    :editor="editorRefParams"
                    :defaultConfig="toolbarConfig"
                    :mode="mode"
                />
                <Editor
                    style="height: 480px; overflow-y: hidden;"
                    v-model="formData.params"
                    :defaultConfig="editorConfig"
                    :mode="mode"
                    @onCreated="handleCreatedParams"
                />
              </div>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="24" v-show="stepActive === 3">
          <el-col :span="24">
            <el-form-item label="外形尺寸（手机宽度）" prop="size" label-width="200">
              <!--                  <el-tiptap placeholder="从这里开始" v-model:content="content" :extensions="extensions" :locale="zh" :width="360" :height="780"/>-->
              <!--              <el-input v-model="formData.description" placeholder="请输入产品型号"/>-->

              <div style="border: 1px solid #ccc; width: 360px;" v-if="isDialogOpen">
                <Toolbar
                    style="border-bottom: 1px solid #ccc"
                    :editor="editorRefSize"
                    :defaultConfig="toolbarConfig"
                    :mode="mode"
                />
                <Editor
                    style="height: 480px; overflow-y: hidden;"
                    v-model="formData.size"
                    :defaultConfig="editorConfig"
                    :mode="mode"
                    @onCreated="handleCreatedSize"
                />
              </div>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="24" v-show="stepActive === 4">
          <el-col :span="24">
            <el-form-item label="安装事项（手机宽度）" prop="install" label-width="200">
              <!--                  <el-tiptap placeholder="从这里开始" v-model:content="content" :extensions="extensions" :locale="zh" :width="360" :height="780"/>-->
              <!--              <el-input v-model="formData.description" placeholder="请输入产品型号"/>-->

              <div style="border: 1px solid #ccc; width: 360px;" v-if="isDialogOpen">
                <Toolbar
                    style="border-bottom: 1px solid #ccc"
                    :editor="editorRefInstall"
                    :defaultConfig="toolbarConfig"
                    :mode="mode"
                />
                <Editor
                    style="height: 480px; overflow-y: hidden;"
                    v-model="formData.install"
                    :defaultConfig="editorConfig"
                    :mode="mode"
                    @onCreated="handleCreatedInstall"
                />
              </div>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="24" v-show="stepActive === 5">
          <el-col :span="24">
            <el-form-item label="选型条件（临时）" prop="choose">
              <div class="choose">
                <div class="choose__item">
                  <div class="choose__label">测量任务：</div>
                  <el-checkbox-group v-model="formData.choose.measurementTask">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in measurementTaskList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">测量输出：</div>
                  <el-checkbox-group v-model="formData.choose.measurementOutput">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in measurementOutputList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">测量介质形态：</div>
                  <el-checkbox-group v-model="formData.choose.mediumState">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in mediumStateList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">安装位置：</div>
                  <el-checkbox-group v-model="formData.choose.installationPosition">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in installationPositionList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">过程连接：</div>
                  <el-checkbox-group v-model="formData.choose.processConnection">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in processConnectionList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">测量点：</div>
                  <el-checkbox-group v-model="formData.choose.measurementPoint">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in measurementPointList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">测量点特性：</div>
                  <el-checkbox-group v-model="formData.choose.measurementPointCharacteristics">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in measurementPointCharacteristicsList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">工作温度（℃）：</div>
                  <div>
                    <el-input-number size="small" v-model='formData.choose.operatingTemperature.min' :min="-200"
                                     :max="800"/>
                    <span style="padding: 0 16px;">~</span>
                    <el-input-number size="small" v-model='formData.choose.operatingTemperature.max' :min="0"
                                     :max="800"/>
                  </div>
                </div>
                <div class="choose__item">
                  <div class="choose__label">工作压力（MPa）：</div>
                  <div>
                    <el-input-number size="small" v-model='formData.choose.operatingPressure.min' :min="0" :max="50"/>
                    <span style="padding: 0 16px;">~</span>
                    <el-input-number size="small" v-model='formData.choose.operatingPressure.max' :min="0" :max="50"/>
                  </div>
                </div>
                <div class="choose__item">
                  <div class="choose__label">量程范围（mm/MPa）：</div>
                  <div>
                    <el-input-number size="small" v-model='formData.choose.measuringRange.min' :min="0" :max="10000"/>
                    <span style="padding: 0 16px;">~</span>
                    <el-input-number size="small" v-model='formData.choose.measuringRange.max' :min="0" :max="10000"/>
                  </div>
                </div>
                <div class="choose__item">
                  <div class="choose__label">介质特性：</div>
                  <el-checkbox-group v-model="formData.choose.mediumCharacteristics">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in mediumCharacteristicsList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">产品特性：</div>
                  <el-checkbox-group v-model="formData.choose.productCharacteristics">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in productCharacteristicsList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">输出信号：</div>
                  <el-checkbox-group v-model="formData.choose.outputSignal">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in outputSignalList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
                <div class="choose__item">
                  <div class="choose__label">认证：</div>
                  <el-checkbox-group v-model="formData.choose.certification">
                    <el-checkbox border size="small" :label="item.value"
                                 v-for="item in certificationList">
                      {{ item.label }}
                    </el-checkbox>
                  </el-checkbox-group>
                </div>
              </div>
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

    <el-dialog v-model="isPreviewDialogOpen" width="1200px" append-to-body>
      <el-image :src="previewImageUrl" fit="fill"/>
    </el-dialog>

  </div>
</template>

<script setup name="Production">
// --- 框架工具等相关 ---
import {reactive, ref, watch, computed, onMounted, nextTick} from 'vue'
import _ from "lodash";
import {ElMessage, ElMessageBox, genFileId} from 'element-plus'
import {useDict} from '@/composables/use-dict.js'
import dayjs from 'dayjs'

// --- api 相关 ----
import {
  getProductionPaged,
  getProduction,
  createProduction,
  updateProduction,
  deleteProduction,
  batchDeleteProduction,
  changeProductionStatus,
} from "@/api/production"

// --- 表格相关 ---
const tableRef = ref(null)
const tableMaxHeight = ref(0)
const tableDefaultSort = ref({prop: "createdAt", order: "descending"})
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
  name: null,
  model: null,
  status: null,
})

// --- 表单相关 ---
const isDialogOpen = ref(false)
const dialogTitle = ref("")
const formRef = ref(null)
const formData = ref({})
const formRules = ref({
  name: [{required: true, message: "产品名称不能为空", trigger: "blur"}],
  series: [{required: true, message: "产品系列不能为空", trigger: "blur"}],
  model: [{required: true, message: "产品型号不能为空", trigger: "blur"}],
  characteristic: [{required: true, message: "产品特点不能为空", trigger: "blur"}],
  price: [{required: true, message: "产品价格不能为空", trigger: "blur"}],
  cover: [{required: true, message: "产品封面不能为空", trigger: "blur"}],
  description: [{required: true, message: "产品说明不能为空", trigger: "blur"}],
  params: [{required: true, message: "性能参数不能为空", trigger: "blur"}],
  size: [{required: true, message: "外形尺寸不能为空", trigger: "blur"}],
  install: [{required: true, message: "安装事项不能为空", trigger: "blur"}],
})
const stepActive = ref(0)
const measurementTaskList = ref([
  {label: '物位', value: '1'},
  {label: '温度', value: '2'},
  {label: '压力', value: '3'},
  {label: '流量', value: '4'},
  {label: '分析', value: '5'},
  {label: '其他', value: '6'},
])
const measurementOutputList = ref([
  {label: '模拟量', value: '1'},
  {label: '界位', value: '2'},
  {label: '开关量', value: '3'},
])
const mediumStateList = ref([
  {label: '液体', value: '1'},
  {label: '气体', value: '2'},
  {label: '固体', value: '3'},
])
const installationPositionList = ref([
  {label: '顶装', value: '1'},
  {label: '底装', value: '2'},
  {label: '侧装', value: '3'},
])
const processConnectionList = ref([
  {label: '卡盘', value: '1'},
  {label: '法兰', value: '2'},
  {label: '螺纹', value: '3'},
  {label: '支架', value: '4'},
])
const measurementPointList = ref([
  {label: '竖井', value: '1'},
  {label: '罐体', value: '2'},
  {label: '排水渠', value: '3'},
  {label: '管道', value: '4'},
  {label: '旁通管', value: '5'},
  {label: '传送带', value: '6'},
  {label: '筒仓', value: '7'},
  {label: '堆料', value: '8'},
  {label: '斗仓', value: '9'},
])
const measurementPointCharacteristicsList = ref([
  {label: '带压', value: '1'},
  {label: '罐体内阻挡物', value: '2'},
  {label: '容器材料不导电', value: '3'},
])
// const operatingTemperatureList = ref({min: -200, max: 800}
// const operatingPressureList = ref({min: 0, max: 50})
// const measuringRangeList = ref({min: 0, max: 10000})
const mediumCharacteristicsList = ref([
  {label: '磨蚀', value: '1'},
  {label: '腐蚀', value: '2'},
  {label: '黏附', value: '3'},
  {label: '产生泡沫', value: '4'},
  {label: '变化的介质', value: '5'},
  {label: '不导电', value: '6'},
])
const productCharacteristicsList = ref([
  {label: '非接触式', value: '1'},
  {label: '接触式', value: '2'},
  {label: '带料调试', value: '3'},
  {label: '带显示', value: '4'},
])
const outputSignalList = ref([
  {label: '无源干接点', value: '1'},
  {label: 'NB-IoT', value: '2'},
  {label: 'LTE-M', value: '3'},
  {label: 'LoRa WAN', value: '4'},
  {label: '继电器', value: '5'},
  {label: 'Profibus PA', value: '6'},
  {label: 'SDI接口', value: '7'},
  {label: 'ModBus', value: '8'},
  {label: 'HART', value: '9'},
  {label: 'Foundation FeildBus', value: '10'},
  {label: '4-20mA', value: '11'},
  {label: '8-16mA', value: '12'},
  {label: 'NAMVR', value: '13'},
  {label: '晶体管', value: '14'},
])
const certificationList = ref([
  {label: 'SIL认证', value: '1'},
  {label: '防爆', value: '2'},
  {label: 'WHG许可证', value: '3'},
  {label: '卫生许可证', value: '4'},
])
const formSubmitting = ref(false)
const isSubmitDisabled = computed(() => {
  if (uploadCover.isUploading || uploadAlbum.isUploading || uploadModelThreeDimensional.isUploading
      || uploadFiles.isUploading || formSubmitting.value) {
    return true
  }
  return false
})

// --- 字典相关 ---
const {sys_normal_disable} = useDict('sys_normal_disable')

// --- 上传相关 ---
import {BASE_URL, BASE_API_URL} from '@/utils/constants.js'

const uploadPrefix = ref(BASE_API_URL)
const uploadCoverRef = ref(null)
const uploadCover = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-image"
})
const uploadAlbumRef = ref(null)
const uploadAlbum = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-image"
})
const uploadModelThreeDimensionalRef = ref(null)
const uploadModelThreeDimensional = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-and-unzip"
})
const uploadFilesRef = ref(null)
const uploadFiles = reactive({
  isUploading: false,
  // 设置上传的请求头部
  // headers: {Authorization: "Bearer " + getToken()},
  // 上传的地址
  url: uploadPrefix.value + "/file/upload-file"
})
const previewImageUrl = ref('')
const isPreviewDialogOpen = ref(false)

// --- 富文本相关 ---
import '@wangeditor/editor/dist/css/style.css' // 引入 css
import {Editor, Toolbar} from '@wangeditor/editor-for-vue'
import {getAppPaged} from "@/api/app/index.js";
// 编辑器实例，必须用 shallowRef
const editorRef = shallowRef()
const editorRefParams = shallowRef()
const editorRefSize = shallowRef()
const editorRefInstall = shallowRef()
const toolbarConfig = {}
const editorConfig = {placeholder: '请输入内容...', MENU_CONF: {}}
editorConfig.MENU_CONF['uploadImage'] = {
  server: uploadPrefix.value + "/file/upload-image",

  // form-data fieldName ，默认值 'wangeditor-uploaded-image'
  fieldName: 'file',

  // 单个文件的最大体积限制，默认为 2M
  maxFileSize: 10 * 1024 * 1024, // 10M

  // 最多可上传几个文件，默认为 100
  maxNumberOfFiles: 100,

  // 选择文件时的类型限制，默认为 ['image/*'] 。如不想限制，则设置为 []
  allowedFileTypes: ['image/*'],

  // 自定义上传参数，例如传递验证的 token 等。参数会被添加到 formData 中，一起上传到服务端。
  meta: {
    token: 'xxx',
    otherKey: 'yyy',
  },

  // 将 meta 拼接到 url 参数中，默认 false
  metaWithUrl: false,

  // 自定义增加 http  header
  headers: {
    Accept: 'text/x-json',
    otherKey: 'xxx',
  },

  // 跨域是否传递 cookie ，默认为 false
  withCredentials: true,

  // 超时时间，默认为 10 秒
  timeout: 5 * 1000, // 5 秒

  // 小于该值就插入 base64 格式（而不上传），默认为 0
  base64LimitSize: 5 * 1024, // 5kb

  // 自定义插入图片
  customInsert(res, insertFn) {
    // res 即服务端的返回结果
    console.log(999, res)
    if (res.code === 200 && res.success) {
      const url = BASE_URL + res.data
      const alt = ''
      const href = ''
      // 从 res 中找到 url alt href ，然后插入图片
      insertFn(url, alt, href)
    }
  },
  onFailed(file, res) {
    console.log(`${file.name} 上传失败`, res)
    proxy.$modal.msgError("上传失败")
  },
  onError(file, err, res) {
    console.log(`${file.name} 上传出错`, err, res)
    proxy.$modal.msgError("上传出错")
  },
}
editorConfig.MENU_CONF['uploadVideo'] = {
  server: uploadPrefix.value + "/file/upload-file",

  // form-data fieldName ，默认值 'wangeditor-uploaded-video'
  fieldName: 'file',

  // 单个文件的最大体积限制，默认为 10M
  maxFileSize: 50 * 1024 * 1024, // 50M

  // 最多可上传几个文件，默认为 5
  maxNumberOfFiles: 5,

  // 选择文件时的类型限制，默认为 ['video/*'] 。如不想限制，则设置为 []
  allowedFileTypes: ['video/*'],

  // 自定义上传参数，例如传递验证的 token 等。参数会被添加到 formData 中，一起上传到服务端。
  meta: {
    token: 'xxx',
    otherKey: 'yyy',
  },

  // 将 meta 拼接到 url 参数中，默认 false
  metaWithUrl: false,

  // 自定义增加 http  header
  headers: {
    Accept: 'text/x-json',
    otherKey: 'xxx',
  },

  // 跨域是否传递 cookie ，默认为 false
  withCredentials: true,

  // 超时时间，默认为 30 秒
  timeout: 15 * 1000, // 15 秒

  customInsert(res, insertFn) {
    // res 即服务端的返回结果
    const url = BASE_URL + res.data
    const poster = ''

    // 从 res 中找到 url poster ，然后插入视频
    insertFn(url, poster)
  },
}
const mode = ref('default')
const handleCreated = (editor) => {
  editorRef.value = editor // 记录 editor 实例，重要！
}
const handleCreatedParams = (editor) => {
  editorRefParams.value = editor // 记录 editor 实例，重要！
}
const handleCreatedSize = (editor) => {
  editorRefSize.value = editor // 记录 editor 实例，重要！
}
const handleCreatedInstall = (editor) => {
  editorRefInstall.value = editor // 记录 editor 实例，重要！
}

/* 加载列表 */
async function loadList() {
  tableLoading.value = true
  try {
    const response = await getProductionPaged(searchParams.value)
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
      response = await batchDeleteProduction(delIds)
    } else {
      response = await deleteProduction(delIds[0])
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
  const text = row.status === '0' ? '启用' : '停用'

  try {
    await ElMessageBox.confirm(`确认要 ${text} ${row.name} 吗?`)
    const response = await changeProductionStatus(row.id, row.status)

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
    name: null,
    series: null,
    model: null,
    characteristic: null,
    cover: null,
    modelThreeDimensional: [],
    album: [],
    files: [],
    description: null,
    params: null,
    size: null,
    install: null,
    price: 0,
    choose: {
      measurementTask: [],
      measurementOutput: [],
      mediumState: [],
      installationPosition: [],
      processConnection: [],
      measurementPoint: [],
      measurementPointCharacteristics: [],
      operatingTemperature: {min: -200, max: 800},
      operatingPressure: {min: 0, max: 50},
      measuringRange: {min: 0, max: 10000},
      mediumCharacteristics: [],
      productCharacteristics: [],
      outputSignal: [],
      certification: []
    },
    status: "0",
    remark: null
  };
  formRef.value?.resetFields()
  stepActive.value = 0
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
  const dictCategoryId = row.id
  try {
    const response = await getProduction(dictCategoryId)
    const {success, data, message} = response
    if (success) {
      formData.value = data
      formData.value.modelThreeDimensional = data.modelThreeDimensional?.length ? data.modelThreeDimensional.split(',').map(o => {
        return {name: o, url: o}
      }) : []
      formData.value.album = data.album?.length ? data.album.split(',').map(o => {
        return {name: o, url: o}
      }) : []
      formData.value.files = data.files?.length ? data.files.split(',').map(o => {
        return {name: o, url: o}
      }) : []
      if (data.choose) {
        formData.value.choose = JSON.parse(data.choose)
      }
      formData.value.description = data?.description?.replace(new RegExp('/uploads/', 'g'), BASE_URL + '/uploads/')
      formData.value.params = data?.params?.replace(new RegExp('/uploads/', 'g'), BASE_URL + '/uploads/')
      formData.value.size = data?.size?.replace(new RegExp('/uploads/', 'g'), BASE_URL + '/uploads/')
      formData.value.install = data?.install?.replace(new RegExp('/uploads/', 'g'), BASE_URL + '/uploads/')

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
        formDataClone.album = formData.value.album?.length ? formData.value.album.map(o => o.url).join(',') : null
        formDataClone.modelThreeDimensional = formData.value.modelThreeDimensional?.length ? formData.value.modelThreeDimensional.map(o => o.url).join(',') : null
        formDataClone.files = formData.value.files?.length ? formData.value.files.map(o => o.url).join(',') : null
        formDataClone.choose = JSON.stringify(formData.value.choose)
        // 替换富文本上传文件路径
        formDataClone.description = formData.value.description.replace(new RegExp(BASE_URL, 'g'), '')
        formDataClone.params = formData.value.params.replace(new RegExp(BASE_URL, 'g'), '')
        formDataClone.size = formData.value.size.replace(new RegExp(BASE_URL, 'g'), '')
        formDataClone.install = formData.value.install.replace(new RegExp(BASE_URL, 'g'), '')
        const response = await (formDataClone.id !== null
            ? updateProduction(formDataClone)
            : createProduction(formDataClone))
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

function prevStep() {
  if (stepActive.value !== 0) {
    stepActive.value--
  }
}

function nextStep() {
  if (stepActive.value < 5) {
    stepActive.value++
  }
}

/* 封面上传中处理 */
const onCoverUploadProgress = (event, file, fileList) => {
  uploadCover.isUploading = true
}

/* 封面覆盖前一个 */
const onCoverUploadExceed = (files, uploadFiles) => {
  uploadCoverRef.value.clearFiles()
  const file = files[0]
  file.uid = genFileId()
  uploadCoverRef.value.handleStart(file)
  uploadCoverRef.value.submit()
}

/* 封面上传成功处理 */
const onCoverUploadSuccess = (response, uploadFile, uploadFiles) => {
  uploadCover.isUploading = false
  const {code, success, data, message} = response
  if (code === 200 && success) {
    formData.value.cover = data
    // ElMessageBox.alert("上传文件成功！")
  } else {
    ElMessageBox.alert(message)
  }
}

/* 封面上传失败处理 */
const onCoverUploadError = (error, uploadFile, uploadFiles) => {
  console.log(error, uploadFile, uploadFiles)
  uploadCover.isUploading = false
}

/* 相册上传中处理 */
const onAlbumUploadProgress = (evt, uploadFile, uploadFiles) => {
  uploadAlbum.isUploading = true
}

/* 相册上传成功处理 */
const onAlbumUploadSuccess = (response, uploadFile, uploadFiles) => {
  console.log(uploadFile, uploadFiles)
  uploadAlbum.isUploading = false
  const {code, success, data, message} = response
  if (code === 200 && success) {
    // formData.value.file_path = data.file_path
    uploadFile.url = data
    formData.value.album.push({name: uploadFile.name, url: data})
  } else {
    ElMessageBox.alert(message)
  }
}

/* 相册上传失败处理 */
const onAlbumUploadError = (error, uploadFile, uploadFiles) => {
  console.log(error, uploadFile, uploadFiles)
  uploadAlbum.isUploading = false
}

/* 预览相册图片 */
const onAlbumUploadPreview = (uploadFile) => {
  previewImageUrl.value = BASE_URL + uploadFile.url
  isPreviewDialogOpen.value = true
}

/* 删除相册图片 */
const onAlbumUploadRemove = (uploadFile) => {
  const index = formData.value.album.findIndex(item => item.uid === uploadFile.uid || item.name === uploadFile.name)
  if (index !== -1) {
    formData.value.album.splice(index, 1)
  }
}

/* 3d模型上传中处理 */
const onModelThreeDimensionalUploadProgress = (evt, uploadFile, uploadFiles) => {
  uploadModelThreeDimensional.isUploading = true
}

/* 3d模型覆盖前一个 */
const onModelThreeDimensionalUploadExceed = (files, uploadFiles) => {
  uploadModelThreeDimensionalRef.value.clearFiles()
  const file = files[0]
  file.uid = genFileId()
  uploadModelThreeDimensionalRef.value.handleStart(file)
  uploadModelThreeDimensionalRef.value.submit()
}

/* 3d模型上传成功处理 */
const onModelThreeDimensionalUploadSuccess = (response, uploadFile, uploadFiles) => {
  uploadModelThreeDimensional.isUploading = false
  const {code, success, data, message} = response
  if (code === 200 && success) {
    uploadFile.url = data
    // formData.value.modelThreeDimensional.push({name: uploadFile.name, url: data})
    formData.value.modelThreeDimensional = [{name: uploadFile.name, url: data}]
    // ElMessageBox.alert("上传文件成功！")
  } else {
    ElMessageBox.alert(message)
  }
}

/* 3d模型上传失败处理 */
const onModelThreeDimensionalUploadError = (error, uploadFile, uploadFiles) => {
  console.log(error, uploadFile, uploadFiles)
  uploadModelThreeDimensional.isUploading = false
}

/* 3d模型删除处理 */
const onModelThreeDimensionalUploadRemove = (uploadFile, uploadFiles) => {
  formData.value.modelThreeDimensional = formData.value.modelThreeDimensional.filter(o => o.url !== uploadFile.url)
}

/* 资料上传中处理 */
const onFilesUploadProgress = (evt, uploadFile, uploadFilesOri) => {
  uploadFiles.isUploading = true
}

/* 资料上传成功处理 */
const onFilesUploadSuccess = (response, uploadFile, uploadFilesOri) => {
  console.log(uploadFile, uploadFilesOri)
  uploadFiles.isUploading = false
  const {code, success, data, message} = response
  if (code === 200 && success) {
    // formData.value.file_path = data.file_path
    uploadFile.url = data
    formData.value.files.push({name: uploadFile.name, url: data})
  } else {
    ElMessageBox.alert(message)
  }
}

/* 资料上传失败处理 */
const onFilesUploadError = (error, uploadFile, uploadFilesOri) => {
  console.log(error, uploadFile, uploadFilesOri)
  uploadFiles.isUploading = false
}

/* 资料删除处理 */
const onFilesUploadRemove = (uploadFile) => {
  formData.value.files = formData.value.files.filter(o => o.url !== uploadFile.url)
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

// 组件销毁时，也及时销毁编辑器
onBeforeUnmount(() => {
  const editor = editorRef.value
  const editorParams = editorRefParams.value
  const editorSize = editorRefSize.value
  const editorInstall = editorRefInstall.value
  if (editor) {
    editor.destroy()
  }
  if (editorParams) {
    editorParams.destroy()
  }
  if (editorSize) {
    editorSize.destroy()
  }
  if (editorInstall) {
    editorInstall.destroy()
  }
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

.choose {
  flex: 1;
  display: flex;
  flex-direction: column;

  &__item {
    display: flex;
    padding-bottom: 8px;
  }

  &__label {
    padding-right: 16px;
    color: var(--el-text-color-primary);
    line-height: normal;
  }

  .el-checkbox-group {
    flex: 1;

    .el-checkbox {
      margin-bottom: 8px;
    }
  }
}

.el-form {
  ::v-deep video {
    width: 100%;
  }
}

</style>
<style lang="scss" scoped>
.avatar-uploader .avatar {
  width: 178px;
  //height: 178px;
  height: auto;
  display: block;
}
</style>
<style lang="scss">
.avatar-uploader .el-upload {
  border: 1px dashed var(--el-border-color);
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: var(--el-transition-duration-fast);
}

.avatar-uploader .el-upload:hover {
  border-color: var(--el-color-primary);
}

.el-icon.avatar-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  width: 178px;
  height: 178px;
  text-align: center;
}
</style>
