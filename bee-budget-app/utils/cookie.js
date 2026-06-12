// 储存 setCookie("username", user, 365);
export const setCookie = (cname, cvalue, exdays) => {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));//为d设置一个形式为一串数字的cookie过期时间
    var expires = "expires="+d.toUTCString();//转化为世界时
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";//我们要的cookie主体
}
// 获取 getCookie("username");
export const getCookie = (cname) => {
    var name = cname + "=";
    var ca = document.cookie.split(';');//分割记录中已存入的cookie，如果有的话
    for(var i = 0; i < ca.length; i++) {//有三个，cname=cvalue;expires=~~~;path=/
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);//从下标为1开始，目的是清除字符串开头带有的空格
         }
        if (c.indexOf(name)  == 0) {
            return c.substring(name.length, c.length);//返回cvalue
         }
    }
    return "";
}
// 删除cookie
// cookie有效期到了之后就自动消失了
// 将有效期设置成昨天--该cookie就会自动删除
export const removeCookie = (cname) => {
    setCookie(cname, "", -1)
}