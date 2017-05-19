//会话过期
var ajax_conf = {};
/** 
* 设置全局AJAX请求默认选项
* 主要设置了AJAX请求遇到Session过期的情况 
*/
$.ajaxSetup({
    complete: function (data) {
        //  var json = $.parseJSON(data.responseText);
        if (data.responseText.indexOf("\"Redirect\":\"\/login\/index\"") > 0) {
            var top = ajax_conf.getTopWinow();
            top.location.href = "/login/index"
        } else if (data.responseText.indexOf("\"Message\":\"sqlError\"") > 0) {
            $.messager.alert('温馨提示', '错误的 SQL 输入 ！', 'question');
        }
    }
});

/** 
* 在页面中任何嵌套层次的窗口中获取顶层窗口 
* @return 当前页面的顶层窗口对象 
*/
ajax_conf.getTopWinow = function () {
    var p = window;
    while (p != p.parent) {
        p = p.parent;
    }
    return p;
}


$(function () {
    var $formTextarea = $(".validatebox-text");
    var $formSqlValue = $("input[name='sqlValue']");
    $("#SQLSearch .textbox-addon").attr('style', 'right:0px; background-color: #EAE8E8; padding: 1px;white-space:wrap;');

    $("#SQLSearch .textbox-addon a").attr('style', 'width: 76px; height: 98px;');

    $("#SQLSearch .icon-search").on('click', function () {
        submitForm($formSqlValue.val());
    });

    $formTextarea.keydown(function (e) {
        if (e.keyCode == 13 && e.ctrlKey) {
            submitForm($formSqlValue.val());
        }
    });

    setInterval(function () { if ($(".messager-button").length <= 0) $formTextarea.focus(); }, 1000);

    var dataGirdViewIsFirstLoading = true;
    $("#datagirdview").datagrid({
        url: '/Admin/God/getJsonFromReport',
        onLoadSuccess: function (data) {
            if (data.total == 0) {
                $.messager.alert('提示', '没有找到符合条件的数据', 'question');
            }
        },
        rowStyler: function (index, row) {
            if (index % 2 == 0) {
                return 'background-color:#E0ECFF;'
            }
        }
    });

    function submitForm(sqlValue) {
        var isTrue = false;
        var option = {
            url: '/Admin/God/CheckSQLSuccess',
            data: {
                sqlValue: sqlValue,
                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
            },
            type: 'POST',
            async: false,
            success: function (data) {
                if (data != 'True') {
                    if (data == 'False') {
                        console.log('受影响的行: 0   ' + data);
                        $.messager.alert('提示', ' 当前查找结果为空 请重新再试 ');
                    }
                    else if (data == 'Error') {
                        console.error('error    ' + data);
                        $.messager.alert('提示', ' 请输入正确的 SQL 语句 ');
                    }
                } else {
                    isTrue = true;
                }
            },
            error: function (data) {
                console.error('error    '+data);
                $.messager.alert('提示', '请输入正确的 SQL 语句');
            }
        };
        $.ajax(option);
        if (isTrue) {
            $("#SQLSearchForm").submit();
        }
    }
});

//导出数据
var saveIsLoading = false;
function save() {
    if (saveIsLoading) return;
    saveIsLoading = true;
    setTimeout(function () { saveIsLoading = false; }, 1500);
    var value = $("#report").val();
    if (value != "") {
        var grid = $('#datagirdview').datagrid('options');
        var datas = { report: value, sort: grid.sortName, order: grid.sortOrder, title: "自定义", total: "", limitvalue: "", __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() };
        $.ajax({
            url: '/Export/DownLoad',
            cache: false,
            type: 'POST',
            data: datas,
            success: function (data) {
                window.location.href = data;
            },
            error: function (data) {
                $.messager.alert('温馨提示', '数据出错请联系管理员!   ', 'question');
            }
        });
    }
    else {
        $.messager.alert('提示', '出错了请联系管理员!', 'question');
    }
}
