declare namespace pages {
    class album extends Bootstrap {
        get appName(): string;
        protected init(): void;
        create_onclick(): void;
    }
}
declare namespace pages {
    class gallery extends Bootstrap {
        get appName(): string;
        protected init(): void;
        create_onclick(): void;
    }
}
declare namespace app {
    function run(): void;
}
declare namespace modals {
    function create_album(): void;
}
declare namespace modals {
    function upload_images(): void;
}
