var AntIcon = {
    info: '<span class="anticon anticon-info-circle" role="img"><svg focusable="false" width="1em" height="1em" fill="currentColor" style="pointer-events:none;" xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 1024 1024"><path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z"></path><path d="M464 336a48 48 0 1 0 96 0 48 48 0 1 0-96 0zm72 112h-48c-4.4 0-8 3.6-8 8v272c0 4.4 3.6 8 8 8h48c4.4 0 8-3.6 8-8V456c0-4.4-3.6-8-8-8z"></path></svg></span>',
    success: '<span class="anticon anticon-check-circle" role="img"><svg focusable="false" width="1em" height="1em" fill="currentColor" style="pointer-events:none;" xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 1024 1024"><path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm193.5 301.7l-210.6 292a31.8 31.8 0 0 1-51.7 0L318.5 484.9c-3.8-5.3 0-12.7 6.5-12.7h46.9c10.2 0 19.9 4.9 25.9 13.3l71.2 98.8 157.2-218c6-8.3 15.6-13.3 25.9-13.3H699c6.5 0 10.3 7.4 6.5 12.7z"></path></svg></span>',
    error: '<span class="anticon anticon-close-circle" role="img"><svg focusable="false" width="1em" height="1em" fill="currentColor" style="pointer-events:none;" xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 1024 1024"><path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm165.4 618.2l-66-.3L512 563.4l-99.3 118.4-66.1.3c-4.4 0-8-3.5-8-8 0-1.9.7-3.7 1.9-5.2l130.1-155L340.5 359a8.32 8.32 0 0 1-1.9-5.2c0-4.4 3.6-8 8-8l66.1.3L512 464.6l99.3-118.4 66-.3c4.4 0 8 3.5 8 8 0 1.9-.7 3.7-1.9 5.2L553.5 514l130 155c1.2 1.5 1.9 3.3 1.9 5.2 0 4.4-3.6 8-8 8z"></path></svg></span>',
    warning: '<span class="anticon anticon-exclamation-circle" role="img"><svg focusable="false" width="1em" height="1em" fill="currentColor" style="pointer-events: none;" xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 1024 1024"><path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z"></path><path d="M464 688a48 48 0 1 0 96 0 48 48 0 1 0-96 0zm24-112h48c4.4 0 8-3.6 8-8V296c0-4.4-3.6-8-8-8h-48c-4.4 0-8 3.6-8 8v272c0 4.4 3.6 8 8 8z"></path></svg></span>',
    question: '<span class="anticon anticon-question-circle" role="img"><svg focusable="false" width="1em" height="1em" fill="currentColor" style="pointer-events: none;" xmlns="http://www.w3.org/2000/svg" class="icon" viewBox="0 0 1024 1024"><path d="M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm0 820c-205.4 0-372-166.6-372-372s166.6-372 372-372 372 166.6 372 372-166.6 372-372 372z"></path><path d="M623.6 316.7C593.6 290.4 554 276 512 276s-81.6 14.5-111.6 40.7C369.2 344 352 380.7 352 420v7.6c0 4.4 3.6 8 8 8h48c4.4 0 8-3.6 8-8V420c0-44.1 43.1-80 96-80s96 35.9 96 80c0 31.1-22 59.6-56.1 72.7-21.2 8.1-39.2 22.3-52.1 40.9-13.1 19-19.9 41.8-19.9 64.9V620c0 4.4 3.6 8 8 8h48c4.4 0 8-3.6 8-8v-22.7a48.3 48.3 0 0 1 30.9-44.8c59-22.7 97.1-74.7 97.1-132.5.1-39.3-17.1-76-48.3-103.3zM472 732a40 40 0 1 0 80 0 40 40 0 1 0-80 0z"></path></svg></span>'
};
var AntBlazor = {
    get: function (url) {
        return fetch(url);
    },
    post: function (url, data) {
        var token = localStorage.getItem('Known-Token');
        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Known-Token': token },
            body: JSON.stringify(data)
        });
    },
    showAlert: function (text) {
        var content = AntIcon.info;
        content += '<p>' + text + '</p>';
        var html = `<div tabindex="-1" class="ant-modal-wrap" role="dialog">
            <div role="document" class="ant-modal ant-modal-confirm ant-modal-confirm-info" style="width:416px;">
                <div class="ant-modal-content">
                    <div class="ant-modal-body">
                        <div class="ant-modal-confirm-body-wrapper">
                            <div class="ant-modal-confirm-body">{content}</div>
                            <div class="ant-modal-confirm-btns">
                                <button class="ant-btn ant-btn-primary">确定</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>`.replace('{content}', content);
        var mask = $('<div class="ant-modal-mask"></div>').appendTo($('body'));
        var elem = $(html).appendTo($('body'));
        elem.find('button').click(function () { mask.remove(); elem.remove(); });
    },
    showConfirm: function (text, action) {
        var content = AntIcon.question;
        content += '<p>' + text + '</p>';
        var html = `<div tabindex="-1" class="ant-modal-wrap" role="dialog">
            <div role="document" class="ant-modal ant-modal-confirm ant-modal-confirm-confirm" style="width:416px;">
                <div class="ant-modal-content">
                    <div class="ant-modal-body">
                        <div class="ant-modal-confirm-body-wrapper">
                            <div class="ant-modal-confirm-body">{content}</div>
                            <div class="ant-modal-confirm-btns">
                                <button class="ant-btn ant-btn-primary">确定</button>
                                <button class="ant-btn ant-btn-default">取消</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>`.replace('{content}', content);
        var mask = $('<div class="ant-modal-mask"></div>').appendTo($('body'));
        var elem = $(html).appendTo($('body'));
        elem.find('.ant-btn-primary').click(function () { action && action(); mask.remove(); elem.remove(); });
        elem.find('.ant-btn-default').click(function () { mask.remove(); elem.remove(); });
    },
    showMessage: function (type, text) {
        var style = '';
        var content = '';
        text = '<span>' + text + '</span>';
        if (type == 'error') {
            style = ' ant-message-error';
            content = AntIcon.error;
        } else if (type == 'success') {
            style = ' ant-message-success';
            content = AntIcon.success;
        }
        content += text;
        var html = `<div class="ant-message">
            <div class="ant-message-notice ant-move-up-enter ant-move-up-enter-active ant-move-up">
                <div class="ant-message-notice-content">
                    <div class="ant-message-custom-content{style}">{content}</div>
                </div>
            </div>
        </div>`.replace('{style}', style).replace('{content}', content);
        var elem = $(html).appendTo($('body'));
        setTimeout(function () { elem.remove(); }, 3000);
    },
    showModal: function (id) {
        modal(id, 1);
        $('#' + id + ' .ant-modal-close').click(function () { modal(id, 0); });
        function modal(id, state) {
            if (state == 1)
                $('#' + id).show();
            else
                $('#' + id).hide();
        }
    },
    validate: function (id) {
        var isValid = true;
        $('#' + id + ' [aria-required]').each(function (idx, elm) {
            var parent = $(elm).parent('.ant-input-affix-wrapper');
            parent.removeClass('ant-input-affix-wrapper-status-error');
            var value = $.trim($(elm).val());
            if (value === '') {
                parent.addClass('ant-input-affix-wrapper-status-error');
                isValid = isValid && false;
            }
        });
        return isValid;
    },
    getForm: function (id) {
        var isValid = AntBlazor.validate(id);
        var data = {};
        if (isValid) {
            $('#' + id).find('input,textarea').each(function (idx, elm) {
                var elmId = $(elm).attr('id');
                data[elmId] = $.trim($(elm).val());
            });
        }
        return { IsValid: isValid, Data: data };
    }
};