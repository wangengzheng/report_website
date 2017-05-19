$(function () {

    var indicator = $('<div class="indicator">>></div>').appendTo('body');
    $('.drag-item').draggable({
        revert: true,
        deltaX: 0,
        deltaY: 0
    }).droppable({
        onDragOver: function (e, source) {
            indicator.css({
                display: 'block',
                left: $(this).offset().left - 10,
                top: $(this).offset().top + $(this).outerHeight() - 5
            });
        },
        onDragLeave: function (e, source) {
            indicator.hide();
        },
        onDrop: function (e, source) {
            $(source).insertAfter(this);
            indicator.hide();
        }
    });


    //setup chang 1
    $('.left .item1').draggable({
        revert: true
      //  proxy: 'clone'
    });
    $('.right td.drop').droppable({
        accept: '.item1',
        onDragEnter: function () {

            $(this).addClass('over');
        },
        onDragLeave: function () {
            $(this).removeClass('over');
        },
        onDrop: function (e, source) {
            $(this).removeClass('over');
            if ($(source).hasClass('assigned')) {
                $(this).append(source);
            } else {
                var c = $(source).clone().addClass('assigned');
                $(this).empty().append(c);
                c.draggable({
                    revert: true
                });
            }
        }
    });
    $('.left').droppable({
        accept: '.assigned',
        onDragEnter: function (e, source) {
            $(source).addClass('trash');
        },
        onDragLeave: function (e, source) {
            $(source).removeClass('trash');
        },
        onDrop: function (e, source) {
            $(source).remove();
        }
    });

    $("#setupwindow").window('close');

    $("#setupquit").click(function () {
        $("#setupwindow").window('close');
    });

    //end of this 
    //var dg = $('#datagirdview'); remoteFilter: 'true',
    var dataGirdViewIsFirstLoading = true;
    $("#datagirdview").datagrid({
        url: '/Report/getJsonFromReport?report=' + $("#report").val(),
        onLoadSuccess: function (data) {
            if (data.total == 0) {
                $.messager.alert('提示', '没有找到符合条件的数据', 'question');
            } else {                
                if ($("#tab-tools-select").combobox('getValue') == 1) {  /*刷新*/
                    var onLoadSuccesstab = $('#报表tabs').tabs('getSelected');
                    var onLoadSuccessindex = $('#报表tabs').tabs('getTabIndex', onLoadSuccesstab);
                    if (onLoadSuccessindex == 0) {
                        //本页统计
                        $('#报表tabs').tabs('unselect', 0)
                        $('#报表tabs').tabs('select', 0)
                    } else {
                        //本表统计
                        $('#报表tabs').tabs('unselect', 1)
                        $('#报表tabs').tabs('select', 1)
                    }
                }
            }
        },
        rowStyler: function (index, row) {
            if (index % 2 == 0) {
                return 'background-color:#E0ECFF;'
            }
        },
        onLoadError: function () {
    
        }
    });
    // dg.datagrid('enableFilter');



    $("#openwindow").click(function () {
        $("#window").window('open');
    })

    $("#quite").click(function () {
        $("#window").window('close');
    })

    $("#formrest").click(function () {
        $("#ff").form('reset');
    });

    $.extend($.fn.validatebox.defaults.rules, {
        dateValid: {
            validator: function (value, param) {
                startTime = $(param[0]).datetimebox('getValue');

                var start = $.fn.datebox.defaults.parser(startTime);
                var end = $.fn.datebox.defaults.parser(value);
                varify = end >= start;
                return varify;
            },
            message: '结束时间要大于等于起始时间!'
        }
    });

    //设置窗口退出按钮
    $("#setupquite").click(function () {
        $('#setupwindow').window('close');
    });

    //点击重新设置
    $("#resetsetup").click(function () {
        $.post('/Report/RestSetUp/', {
            code: "81aa9b921d18c926cdb196067124d213"
        }, function (data) {
            if (data == "ok") {
                window.location.href = "/Report/Category?report=" + $("#report").val() + "&title=" + $("#title").val() + "&RestSetUp=RestSetUp";
            } else {
                $.messager.alert('提示', '当前无需重新设置!', 'question');
            }
        });
    });
    //  重新设置 window弹出
    var thisurl = window.location.href.toString();
    if (thisurl.indexOf("RestSetUp") != -1) {
        $("#setupwindow").window('open');
    }

    //顶部搜索事件表单事件  keydown
    $("#queryfromid").bind('keydown', function (e) {
        if (13 == e.keyCode) {
            submitsquery('queryfromid');
            // $(this).focus();
        }
    });

    //多条件窗口表单事件  keydown
    $("#ff").bind('keydown', function (e) {
        if (13 == e.keyCode) {
            submitForm('ff');

            $("#window").window('close');
        }
    });


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


    $("#helpButton").on('click', function () {
        $.messager.alert('温馨提示', '帮助文档完善中...!   ', 'question');
    })


    //顶部查询 订单状态(全部状态)
    $("#allStatus").on('click', function () {
        var $allStatus = $("#allStatus");
        if ($allStatus.prop('checked')) {
            //选中全部状态
            $("#queryfromid input[name='订单状态']").prop("checked", true);
        } else {
            $("#queryfromid input[name='订单状态']").prop("checked", false);
        }
    })

    $("#queryfromid input[name='订单状态']:checkbox").click(
        function () {
            var flag = true;
            $("#queryfromid input[name='订单状态']:checkbox:gt(0)").each(function () {
                if (!this.checked)
                    flag = false;
            });
            $('#allStatus').prop('checked', flag);
        })


    $(".datebox input[type='text']").on('click', function (e) {
        var $alink = $(this).parent().children("span").children("a");
        $alink.click();

    })


});                                                                             //end of jquery

function gettabs() {
    var tabstotal = $("#subtotalcolumnname").val();
    var tabsreport = $("#report").val();
    var tabsUrl = '/home/thisTableSubtotalmorecolumn/?total=' + tabstotal + '&report=' + tabsreport;
    //让tabs默认选择第一个
    var index = $('#报表tabs').tabs('getTab', 0);
    $('#报表tabs').tabs('update', {
        tab: $('#报表tabs').tabs('getTab', 0),
        options: {
            title: '本页统计',
            href: tabsUrl // the new content URL
        }
    });
}

function Thistablesubtotal() {
    $.messager.show({
        title: '本biao小计',
        msg: '这里是本页小计的内容',
        showType: 'show'
    });
}
//导出数据
var saveIsLoading = false;
function save() {
    if (saveIsLoading) return;
    saveIsLoading = true;
    setTimeout(function () { saveIsLoading = false; }, 1500);
    var value = $("#report").val();
    if (value != "") {
        var grid = $('#datagirdview').datagrid('options');
        var datas = { report: value, sort: grid.sortName, order: grid.sortOrder, title: $("#title").val(), total: $("#subtotalcolumnname").val(), limitvalue: $("#ziduan").val(), __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() };
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

//多条件查询
var AdvancedQueryLoading = false;
function submitForm(formid) {
    if (AdvancedQueryLoading) return;
    AdvancedQueryLoading = true;
    setTimeout(function () { AdvancedQueryLoading = false; }, 1000);
    $.ajax({
        url: '/Report/AdvancedQuery',
        cache: false,
        type: 'POST',
        data: $("#" + formid).serialize(),
        success: function (data) {
            if (data == "ok") {
                $("#window").window('close');
                var report = $("#report").val();
                $("#datagirdview").datagrid({
                    url: '/Report/getJsonFromreport?report=' + report + "&isManyConditions=isManyConditions" + "&title=" + $("#title").val()
                });
            } else if (data == "no") {
                $.messager.alert('温馨提示', '没有找到符合条件的数据', 'question');
            }
        },
        error: function (data) {
            $.messager.alert('温馨提示', '数据出错请联系管理员!   ', 'question');
        }
    });
};

//手动刷新
function handRefresh()
{
    var onLoadSuccesstab = $('#报表tabs').tabs('getSelected');
    var onLoadSuccessindex = $('#报表tabs').tabs('getTabIndex', onLoadSuccesstab);
    if (onLoadSuccessindex == 0) {
        //本页统计
        $('#报表tabs').tabs('unselect', 0)
        $('#报表tabs').tabs('select', 0)
    } else {
        //本表统计
        $('#报表tabs').tabs('unselect', 1)
        $('#报表tabs').tabs('select', 1)
    }
}

//设置查询
function submitsetupForm(formid) {
    $.ajax({
        url: '/Report/SetUpQuery',
        cache: false,
        type: 'POST',
        data: $("#" + formid).serialize(),
        success: function (data) {
            if (data == "ok") {
                var report = $("#report").val();
                $("#setupwindow").window('close');
                window.location.href = "/Report/Category/?report=" + report + "&title=" + $("#title").val() + "&CustomQuery=CustomQuery";
            } else if (data == "nochange") {
                $("#setupwindow").window('close');
            }
        },
        error: function (data) {
            $.messager.alert('温馨提示', '数据出错请联系管理员!   ', 'question');
            $("#setupwindow").window('close');
        }
    });
};




//顶部查询
var SimpleQueryLoading = false;
function submitsquery(formid) {
    if (SimpleQueryLoading) return;
    SimpleQueryLoading = true;
    setTimeout(function () { SimpleQueryLoading = false; }, 1000);
    $.ajax({
        url: '/Report/SimpleQuery',
        cache: false,
        type: 'POST',
        data: $("#" + formid).serialize(),
        success: function (data) {
            if (data == "ok") {
                clearform("ff");
                var report = $("#report").val();
                $("#datagirdview").datagrid({
                    url: '/Report/getJsonFromReport?report=' + report + "&isManyConditions=isManyConditions"
                });
            } else if (data == "no") {
                $.messager.alert('温馨提示', '没有找到符合条件的数据', 'question');
            } else if (data == "error") {
                $.messager.alert('温馨提示', '出错了 请联系管理员!', 'question');
            }
        },
        error: function (data) {
            $.messager.alert('温馨提示', '数据出错请联系管理员!   ', 'question');
        }
    });
}


function myformatter(date) {
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    var d = date.getDate();
    return y + '-' + (m < 10 ? ('0' + m) : m) + '-' + (d < 10 ? ('0' + d) : d);
}

function myparser(s) {
    if (!s) return new Date();
    var ss = (s.split('-'));
    var y = parseInt(ss[0], 10);
    var m = parseInt(ss[1], 10);
    var d = parseInt(ss[2], 10);
    if (!isNaN(y) && !isNaN(m) && !isNaN(d)) {
        return new Date(y, m - 1, d);
    } else {
        return new Date();
    }
}


function detail() {
    var row = $('#datagirdview').datagrid('getSelected');
    if (row) {
        $('#window').window('open');
        $('#ff').form('load', row);


    } else {
        $.messager.alert('温馨提示', '查看详情必须选中一行数据!')
    }
};


function setup() {
    $('#setupwindow').window('open');
};

function disable(id) {
    $("#" + id).prop("disabled", false);
}

function enable(id) {
    $("#" + id).prop("disabled", true);

}