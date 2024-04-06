declare namespace modals {
    const $: any;
    interface WebUploader {
        upload(): unknown;
        on(evt: string, arg1: (file: UploadFile, arg2?: any) => void): unknown;
    }
    interface UploadFile {
        size: number;
        name: string;
        id: string;
        type: string;
    }
    function CreateWebUploaderUi(): WebUploader;
    function showFileInfo(file: UploadFile): void;
    function on_progress(file: UploadFile, percentage: number): void;
    function on_success(file: UploadFile, response: any): void;
    function on_complete(file: UploadFile): void;
    function on_error(file: UploadFile): void;
}
declare namespace pages {
    interface image_data {
        id: string;
        desc: string;
        alt: string;
    }
    class album extends Bootstrap {
        get appName(): string;
        private uploader;
        protected init(): void;
        show_album(list: image_data[]): void;
        file_picker_onclick(): void;
        upload_onclick(): void;
        create_onclick(): void;
    }
}
declare namespace utils {
    function removeElement(node: HTMLElement): void;
    function getObsoletes(): HTMLElement[];
}
declare namespace pages {
    class gallery extends Bootstrap {
        get appName(): string;
        protected init(): void;
        create_onclick(): void;
    }
}
declare namespace pages {
    class login extends Bootstrap {
        get appName(): string;
        protected init(): void;
        login_onclick(): void;
    }
}
declare namespace app {
    function run(): void;
}
declare namespace modals {
    function album_parent(): string;
    function create_album(): void;
}
