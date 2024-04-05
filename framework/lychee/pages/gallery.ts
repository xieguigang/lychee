namespace pages {

    export class gallery extends Bootstrap {

        get appName(): string {
            return "gallery";
        }

        protected init(): void {

        }

        public create_onclick() {
            modals.create_album();
        }
    }
}