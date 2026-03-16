
function JzuploadFile(inpFile, serverPath, callback) {
    var progress = inpFile.parents('.jzuploader').find('.progress-bar');
    var data = new FormData();
    if (inpFile.prop('files').length > 0) {

        data.append('File', inpFile.prop('files')[0]);
        data.append('Name', inpFile.prop('name'));
        $.ajax({
            url: serverPath,
            type: 'POST',
            data: data,
            cache: false,
            dataType: 'json',
            processData: false, // Don't process the files
            contentType: false, // Set content type to false as jQuery will tell the server its a query string request
            xhr: function () {
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) {
                    // For handling the progress of the upload
                    myXhr.upload.addEventListener('progress', function (e) {
                        if (e.lengthComputable) {
                            var nowPercent = (100 * e.loaded) / e.total;
                            progress.attr('aria-valuenow', e.loaded);
                            progress.css('width', nowPercent + '%');
                        }
                    }, false);
                }
                return myXhr;
            },
            success: function (data, textStatus, jqXHR) {
                callback(data);
                if (data.Status != 1) {
                    progress.attr('aria-valuenow', 0);
                    progress.css('width', 0 + '%');
                } else {
                    progress.toggleClass("bg-primary bg-success");
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                progress.attr('aria-valuenow', 0);
                progress.css('width', 0 + '%');
            }
        });
    }
}