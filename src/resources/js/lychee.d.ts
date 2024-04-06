declare namespace pages {
    class album extends Bootstrap {
        get appName(): string;
        protected init(): void;
        create_onclick(): void;
    }
}
declare namespace utils {
    function removeElement(node: HTMLElement): void;
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
    function create_album(): void;
}
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
    function upload_images(): void;
    function CreateWebUploaderUi(): WebUploader;
}
