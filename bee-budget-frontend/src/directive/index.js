import hasRole from './permission/has-role.js'
import hasPermi from './permission/has-permi.js'
import clickOutside from './common/click-outside.js'

export default function directive(app){
  app.directive('hasRole', hasRole)
  app.directive('hasPermi', hasPermi)
  app.directive('clickOutside', clickOutside)
}
