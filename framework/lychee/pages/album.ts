namespace pages {

    export class album extends Bootstrap {

        get appName(): string {
            return "album";
        }

        protected init(): void {

        }

        public create_onclick() {
            modals.create_album();
        }
    }
}