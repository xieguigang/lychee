///<reference path="./pages/album.ts" />
///<reference path="./pages/gallery.ts" />
///<reference path="./pages/login.ts" />

namespace app {

    export function run() {
        Router.AddAppHandler(new pages.album());
        Router.AddAppHandler(new pages.gallery());
        Router.AddAppHandler(new pages.login());

        Router.RunApp();
    }
}

$ts(app.run);