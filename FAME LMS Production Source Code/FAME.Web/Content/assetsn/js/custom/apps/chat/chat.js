"use strict";
var chat = $.connection.chatHub;
var msgCont = document.querySelector('[data-kt-element="msgcontainer"]');
var msgDiv = document.querySelector('[data-kt-element="messages"]');
var tempIn = msgDiv.querySelector('[data-kt-element="template-in"]');
var KTAppChat = function () {
    var send = function (e) {
        var message = document.querySelector('[data-kt-element="input"]');
        var File = document.getElementById('File');
        var file = '';
        if (File.files.length > 0) file = `<br /> <a href="${$("#urlFile").attr("href")}" target="_blank"><i class="bi bi-paperclip fs-3"></i><b>${File.files[0].name}</b></a>`
        if (0 !== message.value.length || file != "") {
            var o;
            var tempOut = msgDiv.querySelector('[data-kt-element="template-out"]');

            chat.server.send($("#ConversationID").attr("data-convid"), message.value).fail(function (error) {
                console.log("Error handler called: " + error);
            });

            (o = tempOut.cloneNode(!0)).classList.remove("d-none"),
                o.querySelector('[data-kt-element="message-text"]').innerHTML = message.value + file, message.value = "",
                msgCont.appendChild(o),
                msgDiv.scrollTop = msgDiv.scrollHeight

        }
    };
    return {
        init: function (t) {
            ! function (t) {
                $.connection.hub.start().done(function () {
                    t && (KTUtil.on(t, '[data-kt-element="input"]', "keydown", (function (n) {
                        if (13 == n.keyCode) return send(t), n.preventDefault(), !1
                    })), KTUtil.on(t, '[data-kt-element="send"]', "click", (function (n) {
                        send(t)
                    })))
                });
            }(t)
        }
    }
}();


KTUtil.onDOMContentLoaded((function () {
    KTAppChat.init(document.querySelector("#kt_chat_messenger")), KTAppChat.init(document.querySelector("#kt_drawer_chat_messenger"))
}));


chat.client.AttachFile = function (ID, File) { appendFile(ID, File) }
chat.client.UploadFiles = function (ReplyID) {
    var formdata = new FormData();
    var File = document.getElementById('File');
    if (File.files.length > 0) {
        var sfilename = File.files[0].name;
        formdata.append(sfilename, File.files[0]);
        $.ajax({
            url: '/Chat/UploadFile/' + ReplyID,
            type: "POST",
            contentType: false,
            processData: false,
            data: formdata,
            success: function (result) {
                $("#urlFile").hide().attr("href", "");
                $("#File").val("").change;
            },
            error: function (err) {
                alert(err.statusText);
            }
        });
    }
};
chat.client.UpdateStatus = function (data) { UpdateStatus(data); };


function appendFile(ID, FilePath) {
    var reply = $(".rep" + ID)
    var file = `<br /> <a href="/Images/Chat/${FilePath}" target="_blank"><i class="bi bi-paperclip fs-3"></i><b>${FilePath}</b></a>`
    reply.append(file);
}
chat.client.addNewMessageToPage = function (data) {
    var file = '';
    if (data.FilePath.length > 2)
        file = `<br /> <a href="${data.FilePath}" target="_blank"><i class="bi bi-paperclip fs-3"></i><b>${data.FileName}</b></a>`
    var convID = $("#ConversationID").attr("data-convid");
    var o;
    if (convID == data.convID)
        (o = tempIn.cloneNode(!0)).classList.remove("d-none"),
            o.querySelector('[data-kt-element="message-text"]').innerHTML = data.message + file,
            o.querySelector('[data-kt-element="message-text"]').classList.add('rep' + data.ID),
            o.querySelector('[data-kt-element="pic-src"]').src = data.Pic,
            o.querySelector('[data-kt-element="sender-name"]').innerText = data.name,
            msgCont.appendChild(o),
            msgDiv.scrollTop = msgDiv.scrollHeight

};

$(document).on("click", "#btnLoadMore", function () {
    var LoadAbove = $(this).data("id");
    var convID = $("#ConversationID").attr("data-convid");
    var div = $(this);
    $.ajax({
        url: "/Chat/LoadMore/" + LoadAbove + "?ConvID=" + convID,
        success: function (data) {
            $(data).prependTo(msgCont)
            div.remove();
        }
    })
})


$(document).on("change", "#File", function (e) {
    var File = document.getElementById('File');
    if (File.files[0].size > 20971520) {
        File.value = '';
        Swal.fire({
            position: 'top-end',
            icon: 'warning',
            title: 'You Cannot Upload File More than 20MB',
            showConfirmButton: false,
            timer: 1500
        })
        $("#urlFile").hide().attr("href", "");
        $("#File").val("").change;
        return;
    }
    else {
        var Url = window.URL.createObjectURL(File.files[0]);
        $("#urlFile").show().attr("href", Url);
        $("#nameFile").text(File.files[0].name);
    }

})


function UpdateStatus(data) {
    $(".friendDiv").each(function (i, el) {
        if ($(el).data("friendid") != "") {
            if (data.indexOf($(el).data("friendid")) != -1)
                $(el).find(".userOnline").show();
            else
                $(el).find(".userOnline").hide();

        }
    })
}