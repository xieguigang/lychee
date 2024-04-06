///<reference path="../modals/upload_images.ts"/>

namespace pages {

    export class album extends Bootstrap {

        get appName(): string {
            return "album";
        }

        private uploader: modals.WebUploader;

        protected init(): void {
            this.uploader = modals.CreateWebUploaderUi();

            // 当有文件添加进来的时候
            this.uploader.on('fileQueued', file => modals.showFileInfo(file));
            // 文件上传过程中创建进度条实时显示。
            this.uploader.on('uploadProgress', (file, percentage) => modals.on_progress(file, percentage));
            // 文件上传成功，给item添加成功class, 用样式标记上传成功。
            this.uploader.on('uploadSuccess', (file, response) => modals.on_success(file, response));
            // 文件上传失败，显示上传出错。
            this.uploader.on('uploadError', file => modals.on_error(file));
            // 完成上传完了，成功或者失败，先删除进度条。
            this.uploader.on('uploadComplete', file => modals.on_complete(file));
        }

        public file_picker_onclick() {
            let inputs: DOMEnumerator<HTMLInputElement> = <any>$ts("$file");
            let file: HTMLInputElement = inputs.First;
            console.log(file);
            file.click();
        }

        public upload_onclick() {
            this.uploader.upload();
        }

        public create_onclick() {
            modals.create_album();
        }
    }
}