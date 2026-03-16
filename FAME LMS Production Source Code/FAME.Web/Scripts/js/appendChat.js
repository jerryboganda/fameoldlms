
function appendMessage(data) {
    var file = ''
    if (data.FilePath.length > 2)
        file = `<br /> <a href="${data.FilePath}" target="_blank"><i class="material-icons font-size-24pt">attach_file</i><b>${data.FileName}</b></a>`
    var html = `
            <div class="chat-message-${data.side} pb-4">
                <div>
                    <img src="${data.Pic}" class="rounded-circle mr-1" alt="${data.name}" width="40" height="40">
                    <div class="text-muted small text-nowrap mt-2">${data.time}</div>
                </div>
                <div class="flex-shrink-1 bg-light rounded py-2 px-3 rep${data.ID || 0} ml-3">
                    <div class="font-weight-bold mb-1">${data.name}</div> ${data.message} ${file}
                </div>
            </div>
            `;
    $("#messages").append(html);
    $("#messages").animate({ scrollTop: 999999 }, 0);
    $("#NoMessage").remove();
}

function addCount(data) {
    var conv = $("a[data-id=" + data.convID + "]");
    var elCount = conv.find("#msgCount");
    var c = parseInt(elCount.text() || 0) + 1;
    elCount.text(c);
    //conv.insertBefore
}

function UpdateStatus(data) {
    $("#Conversations a").each(function (i, el) {
        if ($(el).data("friendid") != "") {
            if (data.indexOf($(el).data("friendid")) != -1)
                $(el).find("#Activestatus").html('<span class="fas fa-circle chat-online"></span> Online')
            else
                $(el).find("#Activestatus").html('<span class="fas fa-circle chat-offline"></span> Offline')
        }
    })
}

function appendFile(ID, FilePath) {
    var reply = $(".rep" + ID)
    var file = `<br /> <a href="/Images/Chat/${FilePath}" target="_blank"><i class="material-icons font-size-24pt">attach_file</i><b>${FilePath}</b></a>`
    reply.append(file);
}