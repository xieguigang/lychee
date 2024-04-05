///<reference path="./pages/album.ts" />
///<reference path="./pages/gallery.ts" />

namespace app {

    export function run() {
        Router.AddAppHandler(new pages.album());
        Router.AddAppHandler(new pages.gallery());

        Router.RunApp();
    }
}

$ts(app.run);