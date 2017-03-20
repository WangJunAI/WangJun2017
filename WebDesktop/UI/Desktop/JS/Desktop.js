/// <reference path="E:\WangJunCode\WangJun\WangJun\WebDesktop\JS/Base/jquery-3.1.1.min.js" />
/// <reference path="E:\WangJunCode\WangJun\WangJun\WebDesktop\JS/Base/jquery-ui-1.12.1/jquery-ui.js" />

$(document).ready(function () {
    $('.desktop_dialog').draggable();
});

var DESKTOP = {
    
}

///创建一个对话框
DESKTOP.CreateDialog = function () {
    var id=
    var html ='<div class="desktop_dialog" id="test">'
                +'<div class="title"><a href="javascript:void(0)">浏览器</a><a href="javascript:void(0)">最小化</a><a href="javascript:void(0)">最大化</a><a href="javascript:void(0)" onclick="$(this).parents('.desktop_dialog').remove()">关闭</a></div>'
                +'<iframe style="border:none; width:100%;height:90%;" src="http://www.toutiao.com"></iframe>'
                +'</div>';
    $()
}