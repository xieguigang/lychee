namespace modals {

    export const $ = (<any>window).$;

    export interface WebUploader {
        upload(): unknown;
        on(evt: string, arg1: (file: UploadFile, arg2?: any) => void): unknown;
    }

    export interface UploadFile {
        size: number;
        name: string;
        id: string;
        type: string;
    }

    export function CreateWebUploaderUi(): WebUploader {
        return (<any>window).WebUploader.create({
            // 选完文件后，是否自动上传。
            auto: false,

            // swf文件路径
            swf: "/resources/js/webuploader/Uploader.swf",

            // 文件接收服务端。
            server: "/video/upload/",
            accept: {
                title: "Video Files",
                // extensions: "dat,asf,rm,ram,3gp,mov,m4v,dvix,dv,qt,divx,cpk,fli,flc,mod,mp4,wmv,flv,avi,mkv,vob,mpg,rmvb,mpeg,mov,mts",
            },

            // 选择文件的按钮。可选。
            // 内部根据当前运行是创建，可能是input元素，也可能是flash.
            pick: '#picker',
            // mulitple:true,//选择多个
            chunked: true,//开启分片上传
            chunkSize: 2 * 1024 * 1024,//分片大小，建议2M，其他可能需要设置
            threads: 3,//上传并发数

            method: 'POST',
        });
    }

    export function showFileInfo(file: UploadFile) {
        var $list = $ts("#thelist");
        var info_str: string = `
            <div id="${file.id}" class="item">
                <h4 class="info">${file.name}</h4>
                <p class="info">FileSize: ${Strings.Lanudry(file.size)}</p>
                <p class="state">Pending Upload ...</p>
            </div>`;

        console.log(file);

        // webuploader事件.当选择文件后，文件被加载到文件队列中，触发该事件。
        // 等效于 uploader.onFileueued = function(file){...} ，类似js的事件定义。
        $list.appendElement($ts("<div>").display(info_str));
    }

    export function on_progress(file: UploadFile, percentage: number) {
        var $li = $('#' + file.id);
        var $percent = $li.find('.progress .progress-bar');

        // 避免重复创建
        if (!$percent.length) {
            $percent = $('<div class="progress progress-striped active">' +
                '<div class="progress-bar" role="progressbar" style="width: 0%">' +
                '</div>' +
                '</div>').appendTo($li).find('.progress-bar');
        }

        $li.find('p.state').text(`Upload ... ${Strings.round(percentage * 100, 2)}%`);
        $percent.css('width', percentage * 100 + '%');
    }

    export function on_success(file: UploadFile, response: any) {
        let urls: { dir: string, name: string } = response.data;
        let info = {
            file: `${urls.dir}/${urls.name}`,
            name: $ts.baseName(file.name),
            size: file.size,
            type: file.type,
            collection: this.collection_id
        };

        $('#' + file.id).addClass('upload-state-done');

        console.log("video file upload success:");
        console.log(urls);

        // write database
        $ts.post("/video/save/", info, function () {

        });
    }

    export function on_complete(file: UploadFile) {
        // alert(file.id)
        // alert(file);
        $('#' + file.id).find('.progress').remove();
        $('#' + file.id).find('p.state').text('已上传');

        // // $('.layui-video-box').html(Help.videoHtml(url, key));
        // Help.video_read();

        // location.href="http://www.xiaosan.com/tp5/public/index.php/index/backstage/vioshow";
    }

    export function on_error(file: UploadFile) {
        $('#' + file.id).find('p.state').text('上传出错');
    }
}