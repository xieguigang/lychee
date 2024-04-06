namespace modals {

    export function album_parent() {
        const url = $ts.location.url;

        console.log("try to get current album reference from url:");
        console.log(url);

        if (url.path == "/gallery" || url.path == "/gallery/") {
            return "0";
        } else {
            return url.getArgument("id");
        }
    }

    export function create_album() {
        let album_name = $ts.value("#album-name");
        let desc = $ts.value("#desc-text");
        let parent_id = album_parent();
        let new_album = {
            name: album_name,
            description: desc,
            parent_id: parent_id
        };

        console.log("view of the arguments for new album:");
        console.log(new_album);

        if (Strings.Empty(album_name)) {

        }

        $ts.post("/gallery/new_album", new_album, function (result) {
            if (result.code == 0) {
                $goto(`/album?id=${result.info}`);
            } else {

            }
        });
    }
}