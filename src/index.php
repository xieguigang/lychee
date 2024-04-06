<?php

include __DIR__ . "/../etc/bootstrap.php";

class App {

    /**
     * Lychee
     * 
     * @access *
     * @uses view
    */
    public function index() {
        View::Display();
    }

    /**
     * @access *
     * @uses view
    */
    public function album($id) {
        $pool = new Table("album");
        $album = $pool->where(["id" => $id])->find();
        $usr_session = accessController::has_user_session();
        $usr_ss = $usr_session ? "keeps" :"delete";

        if (Utils::isDbNull($album)) {
            RFC7231Error::err404("There is no album which its id equals to $id");
        }

        View::Display([
            "title" => $album["name"],
            "album_title" => $album["name"],
            "album_desc" => $album["note"],
            "usr_ss" => $usr_ss
        ]);
    }

    /**
     * Gallery
     * 
     * @access *
     * @uses view
    */
    public function gallery() {
        View::Display();
    }

    /**
     * Login
     * 
     * @access *
     * @uses view
    */
    public function login() {
        View::Display();
    }
}