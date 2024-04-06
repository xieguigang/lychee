/// <reference path="../utils.ts" />

namespace pages {

    export class gallery extends Bootstrap {

        get appName(): string {
            return "gallery";
        }

        protected init(): void {
            utils.removeElement($ts("#menu_upload"));
        }

        public create_onclick() {
            modals.create_album();
        }
    }
}