var serverPath = '/FileManager/StMain/';
var addressBar = $('#current-path');
var bdy = $(document.body);
const updateBtn = $('#update');
const updateIcon = updateBtn.find('> i');
var SelectedItemPath;
var SelectedItemId;


function getCurrentPath() {
    return addressBar.text();
}

function simplifyMimeType(mime) {
    if (mime === 'txt' || mime.includes('text')) {
        return 'txt';
    } else if (mime.includes('javascript')) {
        return 'javascript';
    } else if (mime.includes('mp4')) {
        return 'mp4';
    }
    return 'file';
}

function update() {

    // Show Loader
    toggle_loader();
    // SPIN update icon
    updateIcon.toggleClass('fa-spin');
    $.get(serverPath + 'Update?path=' + getCurrentPath(), function (res) {
        // Hide Loader
        toggle_loader();
        updateIcon.toggleClass('fa-spin');
        if (res) {
            $("#kt_file_manager_items_counter").text(res.Items.length + " items")
            var itemsWrapper = $('#items-wrapper');
            var rows = ``;
            res.Items.forEach(function (value, index, array) {
                var src = '';
                var path = value['Path'] + '/' + value['Name'];
                var realPath = path.replace('ROOT', '/File-Repository');
                if (value['MimeType'] !== null && value['MimeType'].includes('image')) {
                    src = value['Path'].replace('ROOT', '/File-Repository') + '/' + value['Name'];
                } else {
                    src = '/Areas/FileManager/Assets/img/file-types/' + (value['IsFolder'] ? 'folder' : simplifyMimeType(value['MimeType'])) + '.png';
                }
                rows += ` <tr class="item ${value['IsFolder'] ? 'folder' : 'file'}"
                            data-uid="${value['Id']}"
                            data-name="${value['Name']}"
                            data-mime-type="${value['MimeType']}"
                            data-path="${path}" >
                        <td>
                            <div class="form-check form-check-sm form-check-custom form-check-solid">
                                <input class="form-check-input" type="checkbox" value="1" />
                            </div>
                        </td>
                        <td class="item">
                            <div class="d-flex align-items-center">
                                <img src="${src}" class="img-fileIcon" alt="">
                                <a href="javascript:;" class="text-gray-800 text-hover-primary">&emsp; ${value['Name']}</a>
                            </div>
                        </td>
                        <td>-</td>
                        <td>${new Date(parseInt(value['CDate'].substr(6))).toLocaleString()}</td>
                        <td class="text-end">
                            <button type="button" class="btn btn-sm preview btn-icon btn-light btn-active-light-primary">
                                <i class="fa fa-eye" style="font-size:12px"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-icon btndownload btn-light btn-active-light-primary">
                                <i class="fa fa-download" style="font-size:12px"></i>
                            </button>
                        </td>
                    </tr> ` ;
            });
            itemsWrapper.html(rows);
        }
    });
}

$('#items-wrapper').on('dblclick', '.item', function () {
    var item = $(this);
    if (item.hasClass('folder')) {
        addressBar.text(item.attr('data-path'));
        update();
    }
});
$('.go-back').click(function () {
    var currentAddress = addressBar.text();
    if (currentAddress === 'ROOT') {
        return;
    }
    currentAddress = currentAddress.split('/');
    currentAddress.pop();
    addressBar.text(currentAddress.join('/'));
    update();
});
$('.Select-Item').click(function () {
    parent.img(SelectedItemPath);
});
function token(msg) {
    alert(msg);
    return SelectedItemPath;
}
updateBtn.click(function () {
    update();
});


var x = 0;
bdy.on('click', '.item.file .custom-checkbox', function (e) {
    e.stopPropagation();
    console.log('1-start');
    //disable select 
    if ($('.Select-Item').length) {
        if (x === 0) {
            if ($(this).children(".custom-control-input").is(":checked")) {//true
                $(".Select-Item").css("display", "none"); //alert('true');
                console.log('2-checkbox is checked');

            } else {//false 
                console.log('3-checkbox unchecked');

                var item = $(this).children(".custom-control-input");
                console.log(item);
                var Checkboxid = $("#ch-" + $(this).parent().attr('data-uid') + "").attr('id');
                var itemfileid = $(this).parent();
                var type = $(this).parent().hasClass('folder') ? 'Folder' : 'File';
                console.log(type);
                console.log(Checkboxid);
                console.log($('#' + Checkboxid + '').is(':checked'));
                console.log($('#' + Checkboxid + ''));
                console.log(itemfileid.attr('data-path'));
                $(".custom-control-input").prop("checked", false);
                $("#" + Checkboxid).attr('checked', 'checked');
                if (type === 'File') {
                    $(".Select-Item").css("display", "block");
                } else { $(".Select-Item").css("display", "none"); }

                SelectedItemPath = $(this).parent().attr('data-path').replace('ROOT', '/File-Repository');
                SelectedItemId = $(this).parent().attr('data-uid');
                console.log(SelectedItemId + ' - ' + SelectedItemPath);

            }
            x = 1;
        } else x = 0;
    }

});

bdy.on('click', '.item.file', function (e) {
    var item = $(this);
    var type = item.hasClass('folder') ? 'Folder' : 'File';
    $(".custom-control-input").prop("checked", false);
    $("#ch-" + item.attr('data-uid') + "").prop("checked", true);
    if ($('.Select-Item').length) {
        if (type === 'File') {
            $(".Select-Item").css("display", "block");
        } else { $(".Select-Item").css("display", "none"); }
    }
    SelectedItemPath = item.attr('data-path').replace('ROOT', '/File-Repository');
    SelectedItemId = item.attr('data-uid');
});


bdy.on('click', 'button.btndownload', function (e) {
    e.preventDefault();
    var item = $(this).parents('.item');
    var type = item.hasClass('folder') ? 'Folder' : 'File';
    window.open(serverPath + 'Download/' + item.attr('data-uid'), '');
});
bdy.on('click', 'button.info', function (e) {
    e.preventDefault();
    var item = $(this).parents('.item');
    var type = item.hasClass('folder') ? 'Folder' : 'File';
    alertify.alert('<a href="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class="download" download>' + item.attr('data-name') + '</a> ' + type + ' Information',
        '<dl class="dl-horizontal dt-30">' +
        '   <dt>Id</dt>' +
        '   <dd>' + item.attr('data-uid') + '</dd>' +
        '   <dt>Name</dt>' +
        '   <dd>' + item.attr('data-name') + '</dd>' +
        '   <dt>Mime Type</dt>' +
        '   <dd>' + item.attr('data-mime-type') + '</dd>' +
        '   <dt>Path</dt>' +
        '   <dd>' + item.attr('data-path') + '</dd>' +
        '   <dt>Creation Date</dt>' +
        '   <dd>' + item.attr('data-CDate') + '</dd>' +
        '   <dt>Modification Date</dt>' +
        '   <dd>' + item.attr('data-MDate') + '</dd>' +
        '   <dt>Download</dt>' +
        '   <dd> <a href = "' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class= "download" download > <i class="fa fa-download fa-fw"></i></a> </dd>' +
        '</dl>',
        function (evt, value) {
        }
    );
});
bdy.on('click', 'button.preview', function (e) {
    e.preventDefault();
    var item = $(this).parents('.item');
    var type = item.hasClass('folder') ? 'Folder' : 'File';
    var Filemime = item.attr('data-mime-type');
    if (type === 'File') {
        if (Filemime.includes('video')) {
            alertify.alert('<a href="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class="download" download ><i class="fal fa-download fa-fw"></i>' + item.attr('data-name') + '</a> ' + type + ' preview',
                '<video controls id="myVideo" width="100%" height="100%"><source src="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" type="video/mp4"></video>',
                function (evt, value) {
                }
            );
        }
        else if (Filemime.includes('image')) {
            alertify.alert('<a href="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class="download" download >' + item.attr('data-name') + '</a> ' + type + ' preview',
                '<div><img class="img-thumbnail" ' +
                'src = "' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" ></div>',
                function (evt, value) {
                }
            );

        }
        else if (Filemime.includes('txt')) {
            alertify.alert('<a href="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class="download" download >' + item.attr('data-name') + '</a> ' + type + ' preview',
                '<div><iframe src="/filemanager/main/_Edit/' + item.attr('data-uid') + '" frameborder="0" style="overflow:hidden;min-height:600px; height:100%;width:100%"></iframe> </div> ',
                function (evt, value) {
                }
            );
        }
        else {
            alertify.alert('<a href="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class="download" download >' + item.attr('data-name') + '</a> ' + type + ' preview',
                '<div><a href="' + item.attr('data-path').replace('ROOT', '/File-Repository') + '" class="download" download><i class="fal fa-download fa-fw"></i>Not Suported! click to download</a></div> ',
                function (evt, value) {
                }
            );
        }
    }

});
$(document).ready(function () {
    update();
});