///<reference path="../modals/upload_images.ts"/>
///<reference path="../modals/create_album.ts"/>

namespace pages {

    export interface image_data {
        id: string;
        desc: string;
        alt: string;
    }

    export class album extends Bootstrap {

        get appName(): string {
            return "album";
        }

        private uploader: modals.WebUploader;

        protected init(): void {
            let vm = this;

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

            for (let menu of utils.getObsoletes()) {
                utils.removeElement(menu);
            }

            $ts.get(`/gallery/get_images?album_id=${modals.album_parent()}`, function (result: IMsg<image_data[]>) {
                if (result.code == 0) {
                    vm.show_album(<image_data[]>result.info);
                }
            });
        }

        public show_album(list: image_data[]) {
            let div = $ts("#animated-thumbnails-gallery").clear();

            for (let img of list) {
                let lbox = $ts("<img>", {
                    class: "img-responsive",
                    alt: img.alt,
                    src: `/gallery/image?id=${img.id}&q=thumbnail`
                });
                let link = $ts("<a>", {
                    class: "gallery-item",
                    "data-src": `/gallery/image?id=${img.id}&q=large`,
                    "data-sub-html": img.desc
                }).display(lbox);

                div.appendElement(link);
            }
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