declare namespace pages {
    class album extends Bootstrap {
        get appName(): string;
        protected init(): void;
    }
}
declare namespace pages {
    class gallery extends Bootstrap {
        get appName(): string;
        protected init(): void;
    }
}
declare namespace app {
    function run(): void;
}
declare namespace modals {
    function create_album(): void;
}
